using System;

using Viam.Component.Camera.V1;
using Viam.Contracts.Resources;
using Viam.Core.Resources;

namespace Viam.Core
{
    public class ViamClientException(string? message = null) : Exception(message);
    public class ResourceException(string? message = null) : ViamClientException(message);

    public class ResourceRegistrationNotFoundException : ResourceException;

    public class ResourceCreatorRegistrationNotFoundException : ResourceException;

    public class ResourceAlreadyRegisteredException : ResourceException;

    public class ResourceNotFoundException(string name) : ViamClientException($"No resource with name {name} found");

    public class ComponentNotFoundException(ViamResourceName resourceName) : ViamClientException($"No component with name {resourceName} found");

    public class MissingDependencyException(string? dependencyName = null)
        : ViamClientException(
            $"Unable to find dependency {dependencyName ?? "null"} in dependency list. Did you forget to add to the \"Depends on\" list?");

    public class ImageFormatNotSupportedException(string imageFormat)
        : ViamClientException($"Image of format {imageFormat} is not supported")
    {
        public ImageFormatNotSupportedException(MimeType mimeType) : this(mimeType.ToString())
        {
        }

        public ImageFormatNotSupportedException(Format protoImageFormat) : this(protoImageFormat.ToString())
        {
        }
    }
}