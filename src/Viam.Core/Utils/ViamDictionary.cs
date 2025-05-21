using System.Collections.Generic;
using Viam.Serialization;

namespace Viam.Core.Utils
{
    public class ViamDictionary(IDictionary<string, object?> dict)
        : Dictionary<string, object?>(dict), IViamSerializable<ViamDictionary>
    {
        public Dictionary<string, object?> ToDictionary() => new(this);
        public static ViamDictionary FromDictionary(IDictionary<string, object?> dict) => new(dict);
    }
}
