namespace Viam.Core.Grpc
{
    public record Credentials(string Type, string Payload, string AuthEntity)
    {
        public Credentials ToSignalingCredentials(string authEntity) => new(Type, Payload, authEntity);
        public override string ToString() => $"{Type}:{AuthEntity}:<redacted>";
    }

    public record ApiCredentials(string apiKey, string apiKeyId) : Credentials("api-key", apiKey, apiKeyId)
    {
        public override string ToString() => $"{Type}:{AuthEntity}:<redacted>";
    }
}