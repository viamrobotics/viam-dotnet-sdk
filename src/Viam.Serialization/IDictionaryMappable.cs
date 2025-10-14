using Google.Protobuf.WellKnownTypes;

namespace Viam.Serialization
{
    public interface IDictionaryMappable<out TSelf>
        where TSelf : IDictionaryMappable<TSelf>
    {
        public static abstract TSelf FromDictionary(IDictionary<string, object?> dictionary);
        public Dictionary<string, object?> ToDictionary();
    }
}
