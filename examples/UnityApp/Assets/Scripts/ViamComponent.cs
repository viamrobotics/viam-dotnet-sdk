using UnityEngine;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using System.Net.Http;
using Proto.Api.Robot.V1;
using Proto.Api.Service.Motion.V1;
using Viam.Net.Sdk.Core;
using Proto.Rpc.V1;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Text;
using System.Linq;

public class ViamComponent : MonoBehaviour
{
    GameObject leftArm;
    GameObject leftLine;
    GameObject rightArm;
    GameObject rightLine;
    Proto.Api.Common.V1.Pose leftPose;
    Proto.Api.Common.V1.Pose rightPose;
    bool ready = false;
    bool initialSet = false;

    private readonly Proto.Api.Common.V1.ResourceName leftArmResourceName = new Proto.Api.Common.V1.ResourceName() { Namespace = "rdk", Type = "component", Subtype = "arm", Name = "left_arm" };
    private readonly Proto.Api.Common.V1.ResourceName rightArmResourceName = new Proto.Api.Common.V1.ResourceName() { Namespace = "rdk", Type = "component", Subtype = "arm", Name = "right_arm" };

    async void Start()
    {
        UnitySystemConsoleRedirector.Redirect();
        var logger = NLog.LogManager.GetCurrentClassLogger();

        await Task.Run(async () =>
        {
            using (var dialer = new Dialer(logger))
            {
                var dialOpts = new DialOptions
                {
                    ChannelOptions = new GrpcChannelOptions
                    {
                        HttpHandler = new GrpcWebHandler(new HttpClientHandler())
                    },
                    WebRTCOptions = new DialWebRTCOptions
                    {
                        SignalingCredentials = new Credentials { Type = "robot-location-secret", Payload = "<SECRET>" }
                    }
                };


                using (var chan = await dialer.DialWebRTCAsync("https://app.viam.com", "<HOST>.viam.cloud", dialOpts))
                {
                    var motionClient = new MotionService.MotionServiceClient(chan);
                    await setArmPoses(motionClient);
                    ready = true;

                    while (true)
                    {
                        await setArmPoses(motionClient);
                        await Task.Delay(TimeSpan.FromSeconds(1.0 / 60));

                    }
                }
            }
        });
    }

    async Task setArmPoses(MotionService.MotionServiceClient motionClient)
    {
        var resp = await motionClient.GetPoseAsync(new GetPoseRequest { ComponentName = leftArmResourceName });
        leftPose = resp.Pose.Pose;

        resp = await motionClient.GetPoseAsync(new GetPoseRequest { ComponentName = rightArmResourceName });
        rightPose = resp.Pose.Pose;
    }

    void Update()
    {

        if (!ready) { return; }

        if (!initialSet)
        {
            leftArm = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftLine = new GameObject("ov");
            leftLine.transform.parent = leftArm.transform;
            var lineRenderer = leftLine.AddComponent(typeof(LineRenderer)) as LineRenderer;
            lineRenderer.startWidth = .1F;
            lineRenderer.endWidth = .1F;
            lineRenderer.useWorldSpace = false;

            rightArm = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightLine = new GameObject("ov");
            rightLine.transform.parent = rightArm.transform;
            lineRenderer = rightLine.AddComponent(typeof(LineRenderer)) as LineRenderer;
            lineRenderer.startWidth = .1F;
            lineRenderer.endWidth = .1F;
            lineRenderer.useWorldSpace = false;

            initialSet = true;
        }

        leftArm.transform.localPosition = new Vector3((float)leftPose.X, (float)leftPose.Y, (float)leftPose.Z);
        leftLine.transform.rotation = Quaternion.Euler((float)leftPose.OX, (float)leftPose.OY, (float)leftPose.OZ);

        rightArm.transform.localPosition = new Vector3((float)rightPose.X, (float)rightPose.Y, (float)rightPose.Z);
        rightLine.transform.rotation = Quaternion.Euler((float)rightPose.OX, (float)rightPose.OY, (float)rightPose.OZ);
    }
}

// from https://www.jacksondunstan.com/articles/2986
public static class UnitySystemConsoleRedirector
{
    private class UnityTextWriter : TextWriter
    {
        private StringBuilder buffer = new StringBuilder();

        public override void Flush()
        {
            Debug.Log(buffer.ToString());
            buffer.Length = 0;
        }

        public override void Write(string value)
        {
            buffer.Append(value);
            if (value != null)
            {
                var len = value.Length;
                if (len > 0)
                {
                    var lastChar = value[len - 1];
                    if (lastChar == '\n')
                    {
                        Flush();
                    }
                }
            }
        }

        public override void Write(char value)
        {
            buffer.Append(value);
            if (value == '\n')
            {
                Flush();
            }
        }

        public override void Write(char[] value, int index, int count)
        {
            Write(new string(value, index, count));
        }

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }
    }

    public static void Redirect()
    {
        Console.SetOut(new UnityTextWriter());
    }
}
