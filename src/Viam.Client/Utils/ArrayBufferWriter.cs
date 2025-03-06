using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viam.Client.Utils
{
    // This is a terrible implementation of IBufferWriter<T> that is only used for net462 and needs to be fixed.
    public class MyArrayBufferWriter<T> : IBufferWriter<T>
    {
        private List<T> _buffer = new List<T>();

        public void Advance(int count)
        {
            _buffer.RemoveRange(0, count);
        }

        public Memory<T> GetMemory(int sizeHint = 0)
        {
            if (sizeHint > 0)
            {
                _buffer.Capacity = sizeHint;
            }

            return _buffer.ToArray();
        }

        public Span<T> GetSpan(int sizeHint = 0)
        {
            return GetMemory(sizeHint).Span;
        }

        public T[] ToArray() => _buffer.ToArray();
    }
}
