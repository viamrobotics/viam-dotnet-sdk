using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Viam.Core.Utils
{
    internal static class Extensions
    {
        private static string ToLogFormat(this object? obj) =>
            obj switch
            {
                IDictionary<string, object?> dict => $"dict: [{dict.ToLogFormat()}]",
                IEnumerable<object> objects       => ToLogFormat(objects),
                not null                          => $"{obj.GetType().Name}: [{obj}]",
                _                                 => "null"
            };

        private static string ToLogFormat(this IEnumerable<object?>? objects) =>
            objects == null
                ? "null"
                : string.Join(", ", objects.Select(x => x.ToLogFormat()));

        public static string ToLogFormat(this object?[]? objects)
        {
            if (objects == null)
                return "null";

            var arr = ArrayPool<string>.Shared.Rent(objects.Length);
            try
            {
                var i = 0;
                foreach (var o in objects)
                {
                    arr[i] = o.ToLogFormat();
                    i++;
                }
                return i == 0
                           ? "{}"
                           : string.Join(", ", arr[..i]);
            }
            finally
            {
                ArrayPool<string>.Shared.Return(arr);
            }
        }

        public static string ToLogFormat(this IDictionary<string, object?> dict) =>
            string.Join(", ", dict.Select(kvp => $"{kvp.Key}:{kvp.Value.ToLogFormat()}"));
    }
}
