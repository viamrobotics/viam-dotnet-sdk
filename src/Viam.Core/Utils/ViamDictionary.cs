using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Viam.Serialization;

namespace Viam.Core.Utils
{
    public class ViamDictionary(IDictionary<string, object?> dict) : IViamSerializable<ViamDictionary>, IDictionary<string, object?>
    {
        public Dictionary<string, object?> ToDictionary() => new(dict);
        public static ViamDictionary FromDictionary(IDictionary<string, object?> dict) => new(dict);
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => dict.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public void Add(KeyValuePair<string, object?> item) => dict.Add(item);
        public void Clear() => dict.Clear();
        public bool Contains(KeyValuePair<string, object?> item) => dict.Contains(item);
        public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex) => dict.CopyTo(array, arrayIndex);
        public bool Remove(KeyValuePair<string, object?> item) => dict.Remove(item.Key);
        public int Count => dict.Count;
        public bool IsReadOnly => dict.IsReadOnly;
        public void Add(string key, object? value) => dict.Add(key, value);
        public bool ContainsKey(string key) => dict.ContainsKey(key);
        public bool Remove(string key) => dict.Remove(key);
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value) => dict.TryGetValue(key, out value);
        public object? this[string key] { get => dict[key]; set => dict[key] = value; }
        public ICollection<string> Keys => dict.Keys;
        public ICollection<object?> Values => dict.Values;
    }
}
