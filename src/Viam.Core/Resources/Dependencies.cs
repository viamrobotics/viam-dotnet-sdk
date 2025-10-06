using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Viam.Core.Resources
{
    public class Dependencies(IDictionary<ViamResourceName, IResourceBase> dependencies)
        : IDictionary<ViamResourceName, IResourceBase>
    {
        public IEnumerator<KeyValuePair<ViamResourceName, IResourceBase>> GetEnumerator()
        {
            return dependencies.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)dependencies).GetEnumerator();
        }

        public void Add(KeyValuePair<ViamResourceName, IResourceBase> item)
        {
            dependencies.Add(item);
        }

        public void Clear()
        {
            dependencies.Clear();
        }

        public bool Contains(KeyValuePair<ViamResourceName, IResourceBase> item)
        {
            return dependencies.Contains(item);
        }

        public void CopyTo(KeyValuePair<ViamResourceName, IResourceBase>[] array, int arrayIndex)
        {
            dependencies.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<ViamResourceName, IResourceBase> item)
        {
            return dependencies.Remove(item);
        }

        public int Count => dependencies.Count;

        public bool IsReadOnly => dependencies.IsReadOnly;

        public void Add(ViamResourceName key, IResourceBase value)
        {
            dependencies.Add(key, value);
        }

        public bool ContainsKey(ViamResourceName key)
        {
            return dependencies.ContainsKey(key);
        }

        public bool Remove(ViamResourceName key)
        {
            return dependencies.Remove(key);
        }

        public bool TryGetValue(ViamResourceName key, [MaybeNullWhen(false)] out IResourceBase value)
        {
            return dependencies.TryGetValue(key, out value);
        }

        public IResourceBase this[ViamResourceName key]
        {
            get => dependencies[key];
            set => dependencies[key] = value;
        }

        public ICollection<ViamResourceName> Keys => dependencies.Keys;

        public ICollection<IResourceBase> Values => dependencies.Values;
    }
}
