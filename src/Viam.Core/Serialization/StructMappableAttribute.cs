using System;
using System.Collections.Generic;
using System.Text;

namespace Viam.Core.Serialization
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class StructMappableAttribute : Attribute
    {
    }
}
