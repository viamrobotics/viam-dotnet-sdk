using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viam.Common.V1;
using Viam.Core.Resources;

namespace Viam.Core.Utils
{
    internal static class Extensions
    {
        public static string Join(this ViamResourceName[] resourceNames) => string.Join(",", resourceNames.Select(x => x.ToString()));
        public static string Join(this IEnumerable<ResourceName> resourceNames) => string.Join(",", resourceNames.Select(x => x.ToString()));
    }
}
