namespace Viam.Client.WebRTC
{
    internal class ManualResetEventSlim<T> : ManualResetEventSlim
    {
        private T? _value;
        private Exception? _exception;

        public void SetResult(T value)
        {
            if (_exception != null)
                throw new InvalidOperationException("An exception has already been set.", _exception);
            _value = value;
            base.Set();
        }

        public new T Wait()
        {
            base.Wait();
            return ReturnValueOrThrow();
        }

        public new T Wait(int millisecondsTimeout)
        {
            base.Wait(millisecondsTimeout);
            return ReturnValueOrThrow();
        }

        public new T Wait(TimeSpan timeout)
        {
            base.Wait(timeout);
            return ReturnValueOrThrow();
        }

        public new T Wait(CancellationToken cancellationToken)
        {
            base.Wait(cancellationToken);
            return ReturnValueOrThrow();
        }

        public new T Wait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            base.Wait(millisecondsTimeout, cancellationToken);
            return ReturnValueOrThrow();
        }

        public new T Wait(TimeSpan timeout, CancellationToken cancellationToken)
        {
            base.Wait(timeout, cancellationToken);
            return ReturnValueOrThrow();
        }

        public void SetException(Exception exception)
        {
            _exception = exception;
            base.Set();
        }

        private T ReturnValueOrThrow()
        {
            if (_exception != null)
                throw new AggregateException(_exception);
            return _value!;
        }
    }
}
