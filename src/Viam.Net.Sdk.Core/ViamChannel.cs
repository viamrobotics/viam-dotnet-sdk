using System;
using Grpc.Core;

namespace Viam.Net.Sdk.Core
{
    public abstract class ViamChannel() : ChannelBase("ignore"), IDisposable
    {
        protected abstract CallInvoker GetCallInvoker();
        public abstract void Dispose();
        public override CallInvoker CreateCallInvoker() => GetCallInvoker();
    }
}
