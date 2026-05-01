namespace Viam.Serialization
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class StructMappableAttribute : Attribute
    {
    }
}