using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Viam.Core.Utils
{
    internal static class ValueTaskExtensions
    {
        // https://stackoverflow.com/a/63141544
        public static async ValueTask<T[]> WhenAll<T>(this ValueTask<T>[] tasks)
        {
            ArgumentNullException.ThrowIfNull(tasks);
            if (tasks.Length == 0)
                return Array.Empty<T>();

            var internalArray = ArrayPool<ValueTask<T>>.Shared.Rent(tasks.Length);
            try
            {
                // We don't allocate the list if no task throws
                List<Exception>? exceptions = null;

                var results = new T[tasks.Length];
                while (internalArray.Length > 0)
                {
                    for (var i = 0; i < internalArray.Length; i++)
                    {
                        if (internalArray[i].IsCompleted == false)
                            continue;
                        try
                        {
                            results[i] = await internalArray[i]
                                             ;
                        }
                        catch (Exception ex)
                        {
                            exceptions ??= new(internalArray.Length);
                            exceptions.Add(ex);
                        }
                    }
                }

                return exceptions is null
                           ? results
                           : throw new AggregateException(exceptions);
            }
            finally
            {
                ArrayPool<ValueTask<T>>.Shared.Return(internalArray);
            }
        }

        public static async ValueTask WhenAll(this IEnumerable<ValueTask> tasks)
        {
            ArgumentNullException.ThrowIfNull(tasks);
            // We are going to enumerate this often, so it makes sense
            var internalTasks = tasks.ToList();
            if (internalTasks.Count == 0)
                return;

            // We don't allocate the list if no task throws
            List<Exception>? exceptions = null;

            while (internalTasks.Count > 0)
            {
                for (var i = 0; i < internalTasks.Count; i++)
                {
                    if (internalTasks[i].IsCompleted == false)
                        continue;

                    try
                    {
                        await internalTasks[i]
                            ;
                        internalTasks.RemoveAt(i);
                    }
                    catch (Exception ex)
                    {
                        exceptions ??= [];
                        exceptions.Add(ex);
                    }
                }
            }

            if (exceptions is not null)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
