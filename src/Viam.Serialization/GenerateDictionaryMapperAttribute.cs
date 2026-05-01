namespace Viam.Serialization
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class GenerateDictionaryMapperAttribute : Attribute
    {
    }
}