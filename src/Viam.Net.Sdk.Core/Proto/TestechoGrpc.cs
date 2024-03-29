// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: component/testecho/v1/testecho.proto
// </auto-generated>
#pragma warning disable 0414, 1591, 8981, 0612
#region Designer generated code

using grpc = global::Grpc.Core;

namespace Viam.Component.Testecho.V1 {
  public static partial class TestEchoService
  {
    static readonly string __ServiceName = "viam.component.testecho.v1.TestEchoService";

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static void __Helper_SerializeMessage(global::Google.Protobuf.IMessage message, grpc::SerializationContext context)
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (message is global::Google.Protobuf.IBufferMessage)
      {
        context.SetPayloadLength(message.CalculateSize());
        global::Google.Protobuf.MessageExtensions.WriteTo(message, context.GetBufferWriter());
        context.Complete();
        return;
      }
      #endif
      context.Complete(global::Google.Protobuf.MessageExtensions.ToByteArray(message));
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static class __Helper_MessageCache<T>
    {
      public static readonly bool IsBufferMessage = global::System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(global::Google.Protobuf.IBufferMessage)).IsAssignableFrom(typeof(T));
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static T __Helper_DeserializeMessage<T>(grpc::DeserializationContext context, global::Google.Protobuf.MessageParser<T> parser) where T : global::Google.Protobuf.IMessage<T>
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (__Helper_MessageCache<T>.IsBufferMessage)
      {
        return parser.ParseFrom(context.PayloadAsReadOnlySequence());
      }
      #endif
      return parser.ParseFrom(context.PayloadAsNewBuffer());
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Viam.Component.Testecho.V1.EchoRequest> __Marshaller_viam_component_testecho_v1_EchoRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Viam.Component.Testecho.V1.EchoRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Viam.Component.Testecho.V1.EchoResponse> __Marshaller_viam_component_testecho_v1_EchoResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Viam.Component.Testecho.V1.EchoResponse.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Viam.Component.Testecho.V1.EchoMultipleRequest> __Marshaller_viam_component_testecho_v1_EchoMultipleRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Viam.Component.Testecho.V1.EchoMultipleRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Viam.Component.Testecho.V1.EchoMultipleResponse> __Marshaller_viam_component_testecho_v1_EchoMultipleResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Viam.Component.Testecho.V1.EchoMultipleResponse.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Viam.Component.Testecho.V1.EchoBiDiRequest> __Marshaller_viam_component_testecho_v1_EchoBiDiRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Viam.Component.Testecho.V1.EchoBiDiRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Viam.Component.Testecho.V1.EchoBiDiResponse> __Marshaller_viam_component_testecho_v1_EchoBiDiResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Viam.Component.Testecho.V1.EchoBiDiResponse.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Viam.Component.Testecho.V1.StopRequest> __Marshaller_viam_component_testecho_v1_StopRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Viam.Component.Testecho.V1.StopRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Viam.Component.Testecho.V1.StopResponse> __Marshaller_viam_component_testecho_v1_StopResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Viam.Component.Testecho.V1.StopResponse.Parser));

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::Viam.Component.Testecho.V1.EchoRequest, global::Viam.Component.Testecho.V1.EchoResponse> __Method_Echo = new grpc::Method<global::Viam.Component.Testecho.V1.EchoRequest, global::Viam.Component.Testecho.V1.EchoResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "Echo",
        __Marshaller_viam_component_testecho_v1_EchoRequest,
        __Marshaller_viam_component_testecho_v1_EchoResponse);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::Viam.Component.Testecho.V1.EchoMultipleRequest, global::Viam.Component.Testecho.V1.EchoMultipleResponse> __Method_EchoMultiple = new grpc::Method<global::Viam.Component.Testecho.V1.EchoMultipleRequest, global::Viam.Component.Testecho.V1.EchoMultipleResponse>(
        grpc::MethodType.ServerStreaming,
        __ServiceName,
        "EchoMultiple",
        __Marshaller_viam_component_testecho_v1_EchoMultipleRequest,
        __Marshaller_viam_component_testecho_v1_EchoMultipleResponse);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::Viam.Component.Testecho.V1.EchoBiDiRequest, global::Viam.Component.Testecho.V1.EchoBiDiResponse> __Method_EchoBiDi = new grpc::Method<global::Viam.Component.Testecho.V1.EchoBiDiRequest, global::Viam.Component.Testecho.V1.EchoBiDiResponse>(
        grpc::MethodType.DuplexStreaming,
        __ServiceName,
        "EchoBiDi",
        __Marshaller_viam_component_testecho_v1_EchoBiDiRequest,
        __Marshaller_viam_component_testecho_v1_EchoBiDiResponse);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::Viam.Component.Testecho.V1.StopRequest, global::Viam.Component.Testecho.V1.StopResponse> __Method_Stop = new grpc::Method<global::Viam.Component.Testecho.V1.StopRequest, global::Viam.Component.Testecho.V1.StopResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "Stop",
        __Marshaller_viam_component_testecho_v1_StopRequest,
        __Marshaller_viam_component_testecho_v1_StopResponse);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::Viam.Component.Testecho.V1.TestechoReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of TestEchoService</summary>
    [grpc::BindServiceMethod(typeof(TestEchoService), "BindService")]
    public abstract partial class TestEchoServiceBase
    {
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task<global::Viam.Component.Testecho.V1.EchoResponse> Echo(global::Viam.Component.Testecho.V1.EchoRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task EchoMultiple(global::Viam.Component.Testecho.V1.EchoMultipleRequest request, grpc::IServerStreamWriter<global::Viam.Component.Testecho.V1.EchoMultipleResponse> responseStream, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task EchoBiDi(grpc::IAsyncStreamReader<global::Viam.Component.Testecho.V1.EchoBiDiRequest> requestStream, grpc::IServerStreamWriter<global::Viam.Component.Testecho.V1.EchoBiDiResponse> responseStream, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task<global::Viam.Component.Testecho.V1.StopResponse> Stop(global::Viam.Component.Testecho.V1.StopRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Client for TestEchoService</summary>
    public partial class TestEchoServiceClient : grpc::ClientBase<TestEchoServiceClient>
    {
      /// <summary>Creates a new client for TestEchoService</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public TestEchoServiceClient(grpc::ChannelBase channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for TestEchoService that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public TestEchoServiceClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected TestEchoServiceClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected TestEchoServiceClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Viam.Component.Testecho.V1.EchoResponse Echo(global::Viam.Component.Testecho.V1.EchoRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Echo(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Viam.Component.Testecho.V1.EchoResponse Echo(global::Viam.Component.Testecho.V1.EchoRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_Echo, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Viam.Component.Testecho.V1.EchoResponse> EchoAsync(global::Viam.Component.Testecho.V1.EchoRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return EchoAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Viam.Component.Testecho.V1.EchoResponse> EchoAsync(global::Viam.Component.Testecho.V1.EchoRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_Echo, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncServerStreamingCall<global::Viam.Component.Testecho.V1.EchoMultipleResponse> EchoMultiple(global::Viam.Component.Testecho.V1.EchoMultipleRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return EchoMultiple(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncServerStreamingCall<global::Viam.Component.Testecho.V1.EchoMultipleResponse> EchoMultiple(global::Viam.Component.Testecho.V1.EchoMultipleRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncServerStreamingCall(__Method_EchoMultiple, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncDuplexStreamingCall<global::Viam.Component.Testecho.V1.EchoBiDiRequest, global::Viam.Component.Testecho.V1.EchoBiDiResponse> EchoBiDi(grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return EchoBiDi(new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncDuplexStreamingCall<global::Viam.Component.Testecho.V1.EchoBiDiRequest, global::Viam.Component.Testecho.V1.EchoBiDiResponse> EchoBiDi(grpc::CallOptions options)
      {
        return CallInvoker.AsyncDuplexStreamingCall(__Method_EchoBiDi, null, options);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Viam.Component.Testecho.V1.StopResponse Stop(global::Viam.Component.Testecho.V1.StopRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Stop(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Viam.Component.Testecho.V1.StopResponse Stop(global::Viam.Component.Testecho.V1.StopRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_Stop, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Viam.Component.Testecho.V1.StopResponse> StopAsync(global::Viam.Component.Testecho.V1.StopRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return StopAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Viam.Component.Testecho.V1.StopResponse> StopAsync(global::Viam.Component.Testecho.V1.StopRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_Stop, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected override TestEchoServiceClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new TestEchoServiceClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    public static grpc::ServerServiceDefinition BindService(TestEchoServiceBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_Echo, serviceImpl.Echo)
          .AddMethod(__Method_EchoMultiple, serviceImpl.EchoMultiple)
          .AddMethod(__Method_EchoBiDi, serviceImpl.EchoBiDi)
          .AddMethod(__Method_Stop, serviceImpl.Stop).Build();
    }

    /// <summary>Register service method with a service binder with or without implementation. Useful when customizing the service binding logic.
    /// Note: this method is part of an experimental API that can change or be removed without any prior notice.</summary>
    /// <param name="serviceBinder">Service methods will be bound by calling <c>AddMethod</c> on this object.</param>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    public static void BindService(grpc::ServiceBinderBase serviceBinder, TestEchoServiceBase serviceImpl)
    {
      serviceBinder.AddMethod(__Method_Echo, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Viam.Component.Testecho.V1.EchoRequest, global::Viam.Component.Testecho.V1.EchoResponse>(serviceImpl.Echo));
      serviceBinder.AddMethod(__Method_EchoMultiple, serviceImpl == null ? null : new grpc::ServerStreamingServerMethod<global::Viam.Component.Testecho.V1.EchoMultipleRequest, global::Viam.Component.Testecho.V1.EchoMultipleResponse>(serviceImpl.EchoMultiple));
      serviceBinder.AddMethod(__Method_EchoBiDi, serviceImpl == null ? null : new grpc::DuplexStreamingServerMethod<global::Viam.Component.Testecho.V1.EchoBiDiRequest, global::Viam.Component.Testecho.V1.EchoBiDiResponse>(serviceImpl.EchoBiDi));
      serviceBinder.AddMethod(__Method_Stop, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Viam.Component.Testecho.V1.StopRequest, global::Viam.Component.Testecho.V1.StopResponse>(serviceImpl.Stop));
    }

  }
}
#endregion
