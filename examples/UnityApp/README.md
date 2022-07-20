# TODOs
* figure out linting
* stylecop in vscode
* how to invoke from command line?
* view all warnings
* cleaning up channel with shutdownasync properly?
* need to time out properly on everything?? cancellation tokens
* catch exceptions in On* delegate/actions/events
* resolve TODOs
* rename Viam.net.Sdk.Core to Viam.Net.Sdk?

# Sample Unity App

This sample app demonstrates visualizing an arm using Unity forward kinematics and joint angles coming from status updates.

## Setup

* `make setup`
* Create ./Assets/Scripts/Parameters.cs and update `<HOST>` and `<SECRET>` with values from your robot on https://app.viam.com:

```golang
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

```

## Running

* Open this directory in Unity editor
* It will probably fail to open, but do not open in safe mode and delete protobuf and microsoft logging extensions from packages.config if added, and then delete the two corresponding packages in Assests/Packages.
* You will need to open the SampleScene and may need to attach a ViamComponent (in Scripts) as a Component to the Container GameObject.

