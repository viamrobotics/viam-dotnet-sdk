using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Viam.Common.V1;

namespace Viam.Net.Sdk.Core.Resources
{
    public abstract class ResourceBase
    {
        public static ResourceName GetResourceName(SubType subtype, string name) =>
            new()
            {
                Name = name,
                Namespace = subtype.Namespace,
                Type = subtype.ResourceType,
                Subtype = subtype.ResourceSubType
            };

        public abstract ValueTask<IDictionary<string, object?>> DoCommandAsync(IDictionary<string, object> command,
                                                                         TimeSpan? timeout = null);
    }
}
