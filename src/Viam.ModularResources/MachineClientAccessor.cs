using Viam.Core.Clients;

namespace Viam.ModularResources
{
    public sealed class MachineClientAccessor : IMachineClientAccessor
    {
        private readonly TaskCompletionSource<MachineClientBase> _tcs =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool IsReady => _tcs.Task.IsCompleted;

        public MachineClientBase? TryGet() => _tcs.Task.IsCompleted ? _tcs.Task.Result : null;

        public ValueTask<MachineClientBase> GetAsync(CancellationToken ct = default)
        {
            if (_tcs.Task.IsCompleted) return new ValueTask<MachineClientBase>(_tcs.Task.Result);
            if (ct.CanBeCanceled) return new ValueTask<MachineClientBase>(_tcs.Task.WaitAsync(ct));
            return new ValueTask<MachineClientBase>(_tcs.Task);
        }

        public bool TrySet(MachineClientBase client)
        {
            // succeed only once
            return _tcs.TrySetResult(client);
        }
    }

    public interface IMachineClientAccessor
    {
        bool IsReady { get; }
        MachineClientBase? TryGet();
        ValueTask<MachineClientBase> GetAsync(CancellationToken ct = default); // await readiness
        bool TrySet(MachineClientBase client); // set once
    }
}
