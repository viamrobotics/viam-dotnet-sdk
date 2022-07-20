using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Net.Http;

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Proto.Api.Component.Arm.V1;
using Proto.Api.Robot.V1;
using Proto.Api.Service.Motion.V1;
using Proto.Rpc.V1;
using UnityEngine;
using Viam.Net.Sdk.Core;

public class ViamComponent : MonoBehaviour
{
    private GameObject leftArm;
    private GameObject leftLine;
    private GameObject rightArm;
    private GameObject rightLine;
    private Proto.Api.Common.V1.Pose leftPose;
    private Proto.Api.Common.V1.Pose rightPose;
    private bool ready = false;
    private bool initialSet = false;

    private readonly Proto.Api.Common.V1.ResourceName leftArmResourceName = new Proto.Api.Common.V1.ResourceName()
    {
        Namespace = "rdk",
        Type = "component",
        Subtype = "arm",
        Name = "left_arm"
    };
    private readonly Proto.Api.Common.V1.ResourceName rightArmResourceName = new Proto.Api.Common.V1.ResourceName()
    {
        Namespace = "rdk",
        Type = "component",
        Subtype = "arm",
        Name = "right_arm"
    };

    void Start()
    {
        UnitySystemConsoleRedirector.Redirect();
        var logger = NLog.LogManager.GetCurrentClassLogger();

        Task.Run(async () =>
        {
            try
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
                            SignalingCredentials = Parameters.SignalingCredentials
                        }
                    };


                    using (var chan = await dialer.DialWebRTCAsync(Parameters.SignalingServer, Parameters.Host, dialOpts))
                    {
                        var motionClient = new MotionService.MotionServiceClient(chan);

                        while (true)
                        {
                            await setArmPosesMotion(motionClient);
                            if (!ready)
                            {
                                ready = true;
                            }

                            await Task.Delay(TimeSpan.FromSeconds(1.0 / 60));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        });
    }

    async Task setArmPosesMotion(MotionService.MotionServiceClient motionClient)
    {
        var resp = await motionClient.GetPoseAsync(new GetPoseRequest { ComponentName = leftArmResourceName });
        leftPose = resp.Pose.Pose;
        Debug.Log(resp.Pose);

        resp = await motionClient.GetPoseAsync(new GetPoseRequest { ComponentName = rightArmResourceName });
        rightPose = resp.Pose.Pose;
    }

    void Update()
    {

        if (!ready)
        {
            return;
        }

        if (!initialSet)
        {
            leftArm = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftArm.name = "Left Arm";
            leftArm.transform.localScale = new Vector3(.1F, .1F, .1F);
            leftLine = new GameObject("Pointer");
            leftLine.transform.parent = leftArm.transform;
            var lineRenderer = leftLine.AddComponent(typeof(LineRenderer)) as LineRenderer;
            lineRenderer.startWidth = .1F;
            lineRenderer.endWidth = .1F;
            lineRenderer.useWorldSpace = false;

            rightArm = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightArm.name = "Right Arm";
            rightArm.transform.localScale = new Vector3(.1F, .1F, .1F);
            rightLine = new GameObject("Pointer");
            rightLine.transform.parent = rightArm.transform;
            lineRenderer = rightLine.AddComponent(typeof(LineRenderer)) as LineRenderer;
            lineRenderer.startWidth = .1F;
            lineRenderer.endWidth = .1F;
            lineRenderer.useWorldSpace = false;

            initialSet = true;
        }

        leftArm.transform.localPosition = new Vector3((float)leftPose.X/1e3F, (float)leftPose.Y/1e3F, (float)leftPose.Z/1e3F);
        leftLine.transform.rotation = ovToQuat(leftPose);

        rightArm.transform.localPosition = new Vector3((float)rightPose.X/1e3F, (float)rightPose.Y/1e3F, (float)rightPose.Z/1e3F);
        rightLine.transform.rotation = ovToQuat(rightPose);
    }

    private Quaternion ovToQuat(Proto.Api.Common.V1.Pose pose)
    {
        var lat = Math.Acos(pose.OZ);
        var lon = 0F;
        if (1 - Math.Abs(pose.OZ) > 0.0001)
        {
            lon = (float)Math.Atan2(pose.OY, pose.OX);
        }
        var theta = pose.Theta;

        var cosLon = Math.Cos(lon);
        var cosLat = Math.Cos(lat);
        var cosThe = Math.Cos(theta);

        var sinLon = Math.Sin(lon);
        var sinLat = Math.Sin(lat);
        var sinThe = Math.Sin(theta);

        var quat = new Quaternion();
        quat.w = (float)((cosLon * cosLat * cosThe) - (sinLon * cosLat * sinThe));
        quat.x = (float)((cosLon * sinLat * sinThe) - (sinLon * sinLat * cosThe));
        quat.y = (float)((cosLon * sinLat * cosThe) + (sinLon * sinLat * sinThe));
        quat.z = (float)((sinLon * cosLat * cosThe) + (cosLon * cosLat * sinThe));
        return quat.normalized;
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
