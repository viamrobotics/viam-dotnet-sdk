using Google.Protobuf;

using Microsoft.Extensions.Logging;

using System;
using System.Threading.Tasks;

using Viam.App.Datasync.V1;

namespace Viam.Core.App
{
    public class DataSyncClient(ILogger<DataSyncClient> logger, DataSyncService.DataSyncServiceClient client)
    {
        public async Task<string> UploadFile(ReadOnlyMemory<byte> data, string componentName, string componentType, string fileName, string fileExtension, string methodName, string partId, string[] tags)
        {
            var uploadMetadata = new UploadMetadata()
            {
                ComponentName = componentName,
                ComponentType = componentType,
                FileExtension = fileExtension,
                FileName = fileName,
                MethodName = methodName,
                PartId = partId,
                Type = DataType.File
            };
            uploadMetadata.Tags.AddRange(tags);

            logger.LogDebug("Uploading a file of size {FileSize} with metadata {@FileMetadata}", data.Length,
                uploadMetadata);

            var fileData = new FileData() { Data = ByteString.CopyFrom(data.Span) };

            var uploadRequest = client.FileUpload();
            logger.LogDebug("Opened upload request");
            await uploadRequest.RequestStream.WriteAsync(new FileUploadRequest(){Metadata = uploadMetadata, FileContents = fileData});
            logger.LogDebug("Wrote file data to upload request");
            var uploadResponse = await uploadRequest;
            logger.LogDebug("Received upload response with BinaryDataId {BinaryDataId}", uploadResponse.BinaryDataId);
            return uploadResponse.BinaryDataId;
        }
    }
}
