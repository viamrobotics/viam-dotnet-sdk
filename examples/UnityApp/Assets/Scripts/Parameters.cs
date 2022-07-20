using Proto.Rpc.V1;
public class Parameters
{
    public static readonly string SignalingServer = "https://app.viam.com";
    public static readonly Credentials SignalingCredentials = new Credentials
    {
        Type = "robot-location-secret",
        Payload = "<SECRET>"
    };
    public static readonly string Host = "<HOST>";
}
