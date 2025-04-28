using System;
using Viam.Component.Camera.V1;
using Viam.Core.Resources;

namespace Viam.Core
{
    public class ResourceException(string? message = null) : Exception(message);

    public class ResourceRegistrationNotFoundException : ResourceException;

    public class ResourceCreatorTypException(SubType subType, Model model) : ResourceException($"Resource creator registration for {subType} and {model} is not of type {typeof(ResourceCreatorRegistration)}");

    public class ResourceCreatorRegistrationNotFoundException : ResourceException;

    public class ResourceAlreadyRegisteredException : ResourceException;

    public class ResourceNotFoundException(string name) : Exception($"No resource with name {name} found");

    public class MissingDependencyException(string? dependencyName = null)
        : Exception($"Unable to find dependency {dependencyName ?? "null"} in dependency list. Did you forget to add to the \"Depends on\" list?");

    public class ImageFormatNotSupportedException(string imageFormat)
        : Exception($"Image of format {imageFormat} is not supported")
    {
        public ImageFormatNotSupportedException(MimeType mimeType):this(mimeType.ToString()){}
        public ImageFormatNotSupportedException(Format protoImageFormat):this(protoImageFormat.ToString()){}
    }
}
