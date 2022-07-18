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
    List<GameObject> leftJoints = new List<GameObject>();
    List<double> leftJointVals = new List<double>();
    List<GameObject> rightJoints = new List<GameObject>();
    List<double> rightJointVals = new List<double>();

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
                    var robotClient = new RobotService.RobotServiceClient(chan);

                    Debug.Log(await robotClient.ResourceNamesAsync(new ResourceNamesRequest()));


                    var initialStatus = await robotClient.GetStatusAsync(new GetStatusRequest
                    {
                        ResourceNames = {
                            leftArmResourceName,
                            rightArmResourceName
                        }
                    });

                    leftJointVals = initialStatus.Status[0].Status_.Fields["joint_positions"].StructValue.Fields["values"].ListValue.Values.Clone().Select(obj => obj.NumberValue).ToList();
                    rightJointVals = initialStatus.Status[1].Status_.Fields["joint_positions"].StructValue.Fields["values"].ListValue.Values.Clone().Select(obj => obj.NumberValue).ToList();

                    this.ready = true;

                    var statusRespStream = robotClient.StreamStatus(new StreamStatusRequest
                    {
                        ResourceNames = {
                            leftArmResourceName,
                            rightArmResourceName
                        },
                        Every = Duration.FromTimeSpan(TimeSpan.FromSeconds(1.0 / 60))
                    });

                    while (true)
                    {
                        if (!(await statusRespStream.ResponseStream.MoveNext()))
                        {
                            break;
                        }
                        var cur = statusRespStream.ResponseStream.Current;

                        for (int i = 0; i < cur.Status.Count; i++)
                        {
                            var status = cur.Status[i];
                            if (status.Name.Equals(leftArmResourceName))
                            {
                                leftJointVals = status.Status_.Fields["joint_positions"].StructValue.Fields["values"].ListValue.Values.Clone().Select(obj => obj.NumberValue).ToList();
                            }
                            else
                            {
                                rightJointVals = status.Status_.Fields["joint_positions"].StructValue.Fields["values"].ListValue.Values.Clone().Select(obj => obj.NumberValue).ToList();
                            }
                        }
                    }
                }
            }
        });
    }

    void setArmStartingPositions(List<GameObject> joints, int len, bool isLeft)
    {

        GameObject last = null;
        for (int i = 0; i < len; i++)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.transform.parent = last?.transform;
            cylinder.transform.localPosition = new Vector3(0, 2, 0);
            if (i == 0)
            {
                cylinder.transform.localPosition = new Vector3(isLeft ? 10 : 0, 2, 0);
            }

            joints.Add(cylinder);
            last = cylinder;
        }
    }

    void Update()
    {

        if (!ready)
        {
            return;
        }

        if (!initialSet)
        {
            setArmStartingPositions(leftJoints, leftJointVals.Count, true);
            setArmStartingPositions(rightJoints, rightJointVals.Count, false);


            initialSet = true;
        }

        for (int i = 0; i < leftJoints.Count; i++)
        {
            // TODO(erd): make thread safe
            leftJoints[i].transform.rotation = Quaternion.Euler((float)leftJointVals[i], 0, 0);
        }

        for (int i = 0; i < rightJoints.Count; i++)
        {
            // TODO(erd): make thread safe
            rightJoints[i].transform.rotation = Quaternion.Euler((float)rightJointVals[i], 0, 0);
        }
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
