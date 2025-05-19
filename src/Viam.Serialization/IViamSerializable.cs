namespace Viam.Serialization
{
    public interface IViamSerializable<out TSelf>
    where TSelf : IViamSerializable<TSelf>
    {
        Dictionary<string, object?> ToDictionary();
        static abstract TSelf FromDictionary(IDictionary<string, object?> dict);
    }
}
