using Google.Protobuf.WellKnownTypes;
using Google.Protobuf.Collections;

namespace Viam.Serialization
{
    public interface IStructMappable<out TSelf>
        where TSelf : IStructMappable<TSelf>
    {
        public static abstract TSelf FromProto(Struct s);
        public static abstract TSelf FromProto(MapField<string, Value> s);
        public Struct ToStruct();
        public MapField<string, Value> ToMapField();
    }
}
