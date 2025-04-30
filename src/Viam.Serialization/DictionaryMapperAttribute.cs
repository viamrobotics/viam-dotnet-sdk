using System;
using System.Collections.Generic;
using System.Text;

namespace Viam.Serialization
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class DictionaryMapperAttribute : Attribute
    {
    }
}
