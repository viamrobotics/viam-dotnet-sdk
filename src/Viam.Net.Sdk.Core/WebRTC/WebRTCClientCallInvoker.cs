using System;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Viam.Net.Sdk.Core.WebRTC
{
    public class WebRTCClientCallInvoker(WebRTCClientChannel channel, ILogger logger) : CallInvoker
    {
        private WebRTCClientStream<TRequest, TResponse> MakeStream<TRequest, TResponse>(Method<TRequest, TResponse> method) where TResponse : class
        {
            var stream = channel.NextStreamId();
            if (channel.Streams.ContainsKey(stream.Id))
            {
                throw new Exception("channel already exists with id");
            }
            if (channel.Streams.Count > WebRTCClientChannel.MaxStreamCount)
            {
                throw new Exception("stream limit hit");
            }
            var activeStream = new WebRTCClientStream<TRequest, TResponse>(method, stream, channel, (id) => channel.RemoveStreamById(stream.Id), logger);
            channel.Streams.Add(stream.Id, activeStream);
            return activeStream;
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string? host,
            CallOptions options,
            TRequest request) =>
            throw new NotImplementedException();

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string? host,
            CallOptions options,
            TRequest request) =>
            MakeStream(method).UnaryCall(options, request);

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string? host,
            CallOptions options,
            TRequest request) =>
            MakeStream(method)
                .ServerStreamingCall(options, request);

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string? host,
            CallOptions options) => MakeStream(method)
            .AsyncClientStreamingCall(options);

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string? host,
            CallOptions options) => MakeStream(method)
            .DuplexStreamingCall(options);
    }
}
