using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Viam.Core.Grpc
{
    public record UnixDomainSocketsConnectionFactory(EndPoint endPoint, ILogger logger)
    {
        public async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext ctx,
                                                    CancellationToken cancellationToken = default)
        {
            var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

            try
            {
                logger.LogTrace("Connecting to {EndPoint}", endPoint);
                await socket.ConnectAsync(endPoint, cancellationToken).ConfigureAwait(false);
                logger.LogTrace("Connected to {EndPoint}", endPoint);
                return new NetworkStream(socket, true);
            }
            catch
            {
                logger.LogTrace("An error occurred while connecting to {EndPoint}", endPoint);
                socket.Dispose();
                throw;
            }
        }
    }
}
