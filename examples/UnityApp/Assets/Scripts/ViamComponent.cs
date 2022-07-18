using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using System.Net.Http;
using Proto.Api.Robot.V1;
using Viam.Net.Sdk.Core;
using Proto.Rpc.V1;
using Proto.Rpc.Webrtc.V1;
using Proto.Rpc.Examples.Echo.V1;
using Grpc.Core;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Text;

public class ViamComponent : MonoBehaviour
{
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
                    Insecure = true,
                    ChannelOptions = new GrpcChannelOptions
                    {
                        HttpHandler = new GrpcWebHandler(new HttpClientHandler())
                    },
                    WebRTCOptions = new DialWebRTCOptions
                    {
                        SignalingInsecure = true,
                        SignalingCredentials = new Credentials { Type = "api-key", Payload = "sosecret" }
                    }
                };


                using (var chan = await dialer.DialWebRTCAsync("http://localhost:8080", "something-unique", dialOpts))
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
                        Debug.Log(statusRespStream.ResponseStream.Current);
                    }
                }
            }
        });
    }

    void Update()
    {
    }
}

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

