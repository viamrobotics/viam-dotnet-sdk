# Sample Unity App

This sample app demonstrates visualizing an arm using Unity forward kinematics and joint angles coming from status updates.

## Setup

* `make setup`
* Edit ./Assets/Scripts/ViamComponent.cs and update `<HOST>` and `<SECRET>` with values from your robot on https://app.viam.com.

## Running

* Open this directory in Unity editor
* It will probably fail to open, but do not open in safe mode and delete protobuf and microsoft logging extensions from packages.config if added, and then delete the two corresponding packages in Assests/Packages.
* You will need to open the SampleScene and may need to attach a ViamComponent (in Scripts) as a Component to the Container GameObject.

