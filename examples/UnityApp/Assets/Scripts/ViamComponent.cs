using UnityEngine;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using System.Net.Http;
using Proto.Api.Robot.V1;
using Viam.Net.Sdk.Core;
using Proto.Rpc.V1;
using Grpc.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Text;

public class ViamComponent : MonoBehaviour
{
    List<GameObject> joints = new List<GameObject>();
    List<double> jointVals = new List<double>();

    async void Start()
    {
        GameObject last = null;
        for (int i = 0; i < 6; i++)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.transform.parent = last?.transform;
            cylinder.transform.localPosition = new Vector3(0, 2, 0);

            joints.Add(cylinder);
            jointVals.Add(0);
            last = cylinder;
        }

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

                    var statusRespStream = robotClient.StreamStatus(new StreamStatusRequest
                    {
                        ResourceNames = {
                        new Proto.Api.Common.V1.ResourceName() { Namespace = "rdk", Type = "component", Subtype = "arm", Name = "arm1" } }
                    });

                    while (true)
                    {
                        if (!(await statusRespStream.ResponseStream.MoveNext()))
                        {
                            break;
                        }
                        var cur = statusRespStream.ResponseStream.Current;
                        var jointVals = cur.Status[0].Status_.Fields["joint_positions"].StructValue.Fields["values"].ListValue.Values.Clone();
                        for (int i = 0; i < joints.Count; i++)
                        {
                            // TODO(erd): make thread safe
                            this.jointVals[i] = jointVals[i].NumberValue;
                        }
                    }
                }
            }
        });
    }

    void Update()
    {
        for (int i = 0; i < joints.Count; i++)
        {
            // TODO(erd): make thread safe
            joints[i].transform.rotation = Quaternion.Euler((float)jointVals[i], 0, 0);
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

