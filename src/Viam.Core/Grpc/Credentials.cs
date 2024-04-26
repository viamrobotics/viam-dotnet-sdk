namespace Viam.Core.Options
{
    public record Credentials(string Type, string Payload, string AuthEntity)
    {
        public Credentials ToSignalingCredentials(string authEntity) => new(Type, Payload, authEntity);
    }

    public record ApiCredentials(string apiKey, string apiKeyId) : Credentials("api-key", apiKey, apiKeyId);
}
