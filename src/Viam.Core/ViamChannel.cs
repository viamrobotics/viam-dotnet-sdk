using System;
using Grpc.Core;

namespace Viam.Core
{
    public abstract class ViamChannel(string remote) : ChannelBase(remote), IDisposable
    {
        protected abstract CallInvoker GetCallInvoker();
        public abstract void Dispose();
        public override CallInvoker CreateCallInvoker() => GetCallInvoker();
        public override string ToString() => Target;
    }
}
