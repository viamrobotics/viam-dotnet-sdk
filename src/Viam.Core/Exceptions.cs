using System;

namespace Viam.Core
{
    public class ResourceRegistrationNotFoundException : Exception;

    public class ResourceCreatorRegistrationNotFoundException : Exception;

    public class ResourceNotFoundException(string name) : Exception($"No resource with name {name} found");
}
