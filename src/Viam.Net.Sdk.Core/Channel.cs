using Grpc.Core;
using Grpc.Net.Client;
using System;

namespace Viam.Net.Sdk.Core
{
    public abstract class Channel : ChannelBase, IDisposable
    {
        protected Channel(string target) : base(target)
        {

        }

        public abstract void Dispose();

        // Note(erd): this is a smelly hack
        internal class Wrapped : Channel
        {
            GrpcChannel _channel;

            public Wrapped(GrpcChannel channel) : base("doesnotmatter")
            {
                _channel = channel;
            }

            public override CallInvoker CreateCallInvoker()
            {
                return _channel.CreateCallInvoker();
            }

            public override void Dispose()
            {
                _channel.Dispose();
            }
        }
    }

}