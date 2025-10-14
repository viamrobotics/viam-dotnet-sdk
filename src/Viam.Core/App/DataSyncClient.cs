using Google.Protobuf;

using Microsoft.Extensions.Logging;

using System;
using System.Threading.Tasks;

using Viam.App.Datasync.V1;

namespace Viam.Core.App
{
    public class DataSyncClient(ILogger<DataSyncClient> logger, DataSyncService.DataSyncServiceClient client)
    {
        public const int ChunkSize = (1 << 16) - 1; // 64 KiB
        public async Task<string> UploadFile(ReadOnlyMemory<byte> data, string componentName, string componentType, string fileName, string fileExtension, string methodName, string partId, string[] tags)
        {
            logger.LogDebug("Preparing file upload");
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

            var uploadRequest = client.FileUpload();
            logger.LogTrace("Created upload request");
            await uploadRequest.RequestStream.WriteAsync(new FileUploadRequest() { Metadata = uploadMetadata });
            logger.LogTrace("Sent metadata");
            for (var i = 0; i < data.Length; i += ChunkSize)
            {
                var end = Math.Min(i + ChunkSize, data.Length);
                var chunk = data[i..end];
                logger.LogTrace("Uploading file chunk {Start}..{End}", i, end);
                var fileData = new FileData { Data = ByteString.CopyFrom(chunk.Span) };
                await uploadRequest.RequestStream.WriteAsync(new FileUploadRequest { FileContents = fileData });
            }

            logger.LogTrace("Wrote file data to upload request");
            var uploadResponse = await uploadRequest;
            logger.LogDebug("Received upload response with BinaryDataId {BinaryDataId}", uploadResponse.BinaryDataId);
            return uploadResponse.BinaryDataId;
        }
    }
}
