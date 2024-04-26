// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: proto/rpc/webrtc/v1/signaling.proto
// </auto-generated>
#pragma warning disable 0414, 1591, 8981, 0612
#region Designer generated code

using grpc = global::Grpc.Core;

namespace Proto.Rpc.Webrtc.V1 {
  /// <summary>
  /// A SignalingService provides the means to have one client "call" another
  /// client using the Session Description Protocol (SDP).
  /// </summary>
  public static partial class SignalingService
  {
    static readonly string __ServiceName = "proto.rpc.webrtc.v1.SignalingService";

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
    static readonly grpc::Marshaller<global::Proto.Rpc.Webrtc.V1.CallRequest> __Marshaller_proto_rpc_webrtc_v1_CallRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Proto.Rpc.Webrtc.V1.CallRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Proto.Rpc.Webrtc.V1.CallResponse> __Marshaller_proto_rpc_webrtc_v1_CallResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Proto.Rpc.Webrtc.V1.CallResponse.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Proto.Rpc.Webrtc.V1.CallUpdateRequest> __Marshaller_proto_rpc_webrtc_v1_CallUpdateRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Proto.Rpc.Webrtc.V1.CallUpdateRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Proto.Rpc.Webrtc.V1.CallUpdateResponse> __Marshaller_proto_rpc_webrtc_v1_CallUpdateResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Proto.Rpc.Webrtc.V1.CallUpdateResponse.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Proto.Rpc.Webrtc.V1.AnswerResponse> __Marshaller_proto_rpc_webrtc_v1_AnswerResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Proto.Rpc.Webrtc.V1.AnswerResponse.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Proto.Rpc.Webrtc.V1.AnswerRequest> __Marshaller_proto_rpc_webrtc_v1_AnswerRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Proto.Rpc.Webrtc.V1.AnswerRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigRequest> __Marshaller_proto_rpc_webrtc_v1_OptionalWebRTCConfigRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigResponse> __Marshaller_proto_rpc_webrtc_v1_OptionalWebRTCConfigResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigResponse.Parser));

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::Proto.Rpc.Webrtc.V1.CallRequest, global::Proto.Rpc.Webrtc.V1.CallResponse> __Method_Call = new grpc::Method<global::Proto.Rpc.Webrtc.V1.CallRequest, global::Proto.Rpc.Webrtc.V1.CallResponse>(
        grpc::MethodType.ServerStreaming,
        __ServiceName,
        "Call",
        __Marshaller_proto_rpc_webrtc_v1_CallRequest,
        __Marshaller_proto_rpc_webrtc_v1_CallResponse);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::Proto.Rpc.Webrtc.V1.CallUpdateRequest, global::Proto.Rpc.Webrtc.V1.CallUpdateResponse> __Method_CallUpdate = new grpc::Method<global::Proto.Rpc.Webrtc.V1.CallUpdateRequest, global::Proto.Rpc.Webrtc.V1.CallUpdateResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "CallUpdate",
        __Marshaller_proto_rpc_webrtc_v1_CallUpdateRequest,
        __Marshaller_proto_rpc_webrtc_v1_CallUpdateResponse);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::Proto.Rpc.Webrtc.V1.AnswerResponse, global::Proto.Rpc.Webrtc.V1.AnswerRequest> __Method_Answer = new grpc::Method<global::Proto.Rpc.Webrtc.V1.AnswerResponse, global::Proto.Rpc.Webrtc.V1.AnswerRequest>(
        grpc::MethodType.DuplexStreaming,
        __ServiceName,
        "Answer",
        __Marshaller_proto_rpc_webrtc_v1_AnswerResponse,
        __Marshaller_proto_rpc_webrtc_v1_AnswerRequest);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigRequest, global::Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigResponse> __Method_OptionalWebRTCConfig = new grpc::Method<global::Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigRequest, global::Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "OptionalWebRTCConfig",
        __Marshaller_proto_rpc_webrtc_v1_OptionalWebRTCConfigRequest,
        __Marshaller_proto_rpc_webrtc_v1_OptionalWebRTCConfigResponse);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::Proto.Rpc.Webrtc.V1.SignalingReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of SignalingService</summary>
    [grpc::BindServiceMethod(typeof(SignalingService), "BindService")]
    public abstract partial class SignalingServiceBase
    {
      /// <summary>
      /// Call makes an offer to a client that it expects an answer to. The host
      /// of the client in question should be identified in the rpc-host metadata
      /// field.
      /// Note: Based on how this is a server streaming responnse to the caller,
      /// we do not have a good way of knowing if the caller has disappeared.
      /// Depending on answerer timeouts and concurrency limits, this can result in
      /// hangs on the answerer waiting for a connection to establish, which in turn
      /// can result in the caller waiting for an answerer to be listening.
      /// </summary>
      /// <param name="request">The request received from the client.</param>
      /// <param name="responseStream">Used for sending responses back to the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>A task indicating completion of the handler.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task Call(global::Proto.Rpc.Webrtc.V1.CallRequest request, grpc::IServerStreamWriter<global::Proto.Rpc.Webrtc.V1.CallResponse> responseStream, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      /// <summary>
      /// CallUpdate is used to send additional info in relation to a Call.
      /// The host of the client for the call in question should be identified
      /// in the rpc-host metadata field.
      /// In a world where https://github.com/grpc/grpc-web/issues/24 is fixed,
      /// this should be removed in favor of a bidirectional stream on Call.
      /// </summary>
      /// <param name="request">The request received from the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>The response to send back to the client (wrapped by a task).</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task<global::Proto.Rpc.Webrtc.V1.CallUpdateResponse> CallUpdate(global::Proto.Rpc.Webrtc.V1.CallUpdateRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      /// <summary>
      /// Answer sets up an answering service where the caller answers call offers
      /// and responds with answers.
      /// The host(s) to answer for should be in the rpc-host metadata field.
      /// </summary>
      /// <param name="requestStream">Used for reading requests from the client.</param>
      /// <param name="responseStream">Used for sending responses back to the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>A task indicating completion of the handler.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task Answer(grpc::IAsyncStreamReader<global::Proto.Rpc.Webrtc.V1.AnswerResponse> requestStream, grpc::IServerStreamWriter<global::Proto.Rpc.Webrtc.V1.AnswerRequest> responseStream, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      /// <summary>
      /// OptionalWebRTCConfig returns any WebRTC configuration the caller may want to use.
      /// The host to get a config for must be in the rpc-host metadata field.
      /// </summary>
      /// <param name="request">The request received from the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>The response to send back to the client (wrapped by a task).</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task<global::Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigResponse> OptionalWebRTCConfig(global::Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Client for SignalingService</summary>
    public partial class SignalingServiceClient : grpc::ClientBase<SignalingServiceClient>
    {
      /// <summary>Creates a new client for SignalingService</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public SignalingServiceClient(grpc::ChannelBase channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for SignalingService that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public SignalingServiceClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected SignalingServiceClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected SignalingServiceClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      /// <summary>
      /// Call makes an offer to a client that it expects an answer to. The host
      /// of the client in question should be identified in the rpc-host metadata
      /// field.
      /// Note: Based on how this is a server streaming responnse to the caller,
      /// we do not have a good way of knowing if the caller has disappeared.
      /// Depending on answerer timeouts and concurrency limits, this can result in
      /// hangs on the answerer waiting for a connection to establish, which in turn
      /// can result in the caller waiting for an answerer to be listening.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncServerStreamingCall<global::Proto.Rpc.Webrtc.V1.CallResponse> Call(global::Proto.Rpc.Webrtc.V1.CallRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Call(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// Call makes an offer to a client that it expects an answer to. The host
      /// of the client in question should be identified in the rpc-host metadata
      /// field.
      /// Note: Based on how this is a server streaming responnse to the caller,
      /// we do not have a good way of knowing if the caller has disappeared.
      /// Depending on answerer timeouts and concurrency limits, this can result in
      /// hangs on the answerer waiting for a connection to establish, which in turn
      /// can result in the caller waiting for an answerer to be listening.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncServerStreamingCall<global::Proto.Rpc.Webrtc.V1.CallResponse> Call(global::Proto.Rpc.Webrtc.V1.CallRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncServerStreamingCall(__Method_Call, null, options, request);
      }
      /// <summary>
      /// CallUpdate is used to send additional info in relation to a Call.
      /// The host of the client for the call in question should be identified
      /// in the rpc-host metadata field.
      /// In a world where https://github.com/grpc/grpc-web/issues/24 is fixed,
      /// this should be removed in favor of a bidirectional stream on Call.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The response received from the server.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Proto.Rpc.Webrtc.V1.CallUpdateResponse CallUpdate(global::Proto.Rpc.Webrtc.V1.CallUpdateRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return CallUpdate(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// CallUpdate is used to send additional info in relation to a Call.
      /// The host of the client for the call in question should be identified
      /// in the rpc-host metadata field.
      /// In a world where https://github.com/grpc/grpc-web/issues/24 is fixed,
      /// this should be removed in favor of a bidirectional stream on Call.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The response received from the server.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Proto.Rpc.Webrtc.V1.CallUpdateResponse CallUpdate(global::Proto.Rpc.Webrtc.V1.CallUpdateRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_CallUpdate, null, options, request);
      }
      /// <summary>
      /// CallUpdate is used to send additional info in relation to a Call.
      /// The host of the client for the call in question should be identified
      /// in the rpc-host metadata field.
      /// In a world where https://github.com/grpc/grpc-web/issues/24 is fixed,
      /// this should be removed in favor of a bidirectional stream on Call.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Proto.Rpc.Webrtc.V1.CallUpdateResponse> CallUpdateAsync(global::Proto.Rpc.Webrtc.V1.CallUpdateRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return CallUpdateAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// CallUpdate is used to send additional info in relation to a Call.
      /// The host of the client for the call in question should be identified
      /// in the rpc-host metadata field.
      /// In a world where https://github.com/grpc/grpc-web/issues/24 is fixed,
      /// this should be removed in favor of a bidirectional stream on Call.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Proto.Rpc.Webrtc.V1.CallUpdateResponse> CallUpdateAsync(global::Proto.Rpc.Webrtc.V1.CallUpdateRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_CallUpdate, null, options, request);
      }
      /// <summary>
      /// Answer sets up an answering service where the caller answers call offers
      /// and responds with answers.
      /// The host(s) to answer for should be in the rpc-host metadata field.
      /// </summary>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncDuplexStreamingCall<global::Proto.Rpc.Webrtc.V1.AnswerResponse, global::Proto.Rpc.Webrtc.V1.AnswerRequest> Answer(grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Answer(new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// Answer sets up an answering service where the caller answers call offers
      /// and responds with answers.
      /// The host(s) to answer for should be in the rpc-host metadata field.
      /// </summary>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncDuplexStreamingCall<global::Proto.Rpc.Webrtc.V1.AnswerResponse, global::Proto.Rpc.Webrtc.V1.AnswerRequest> Answer(grpc::CallOptions options)
      {
        return CallInvoker.AsyncDuplexStreamingCall(__Method_Answer, null, options);
      }
      /// <summary>
      /// OptionalWebRTCConfig returns any WebRTC configuration the caller may want to use.
      /// The host to get a config for must be in the rpc-host metadata field.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The response received from the server.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigResponse OptionalWebRTCConfig(global::Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return OptionalWebRTCConfig(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// OptionalWebRTCConfig returns any WebRTC configuration the caller may want to use.
      /// The host to get a config for must be in the rpc-host metadata field.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The response received from the server.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigResponse OptionalWebRTCConfig(global::Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_OptionalWebRTCConfig, null, options, request);
      }
      /// <summary>
      /// OptionalWebRTCConfig returns any WebRTC configuration the caller may want to use.
      /// The host to get a config for must be in the rpc-host metadata field.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigResponse> OptionalWebRTCConfigAsync(global::Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return OptionalWebRTCConfigAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// OptionalWebRTCConfig returns any WebRTC configuration the caller may want to use.
      /// The host to get a config for must be in the rpc-host metadata field.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigResponse> OptionalWebRTCConfigAsync(global::Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_OptionalWebRTCConfig, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected override SignalingServiceClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new SignalingServiceClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    public static grpc::ServerServiceDefinition BindService(SignalingServiceBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_Call, serviceImpl.Call)
          .AddMethod(__Method_CallUpdate, serviceImpl.CallUpdate)
          .AddMethod(__Method_Answer, serviceImpl.Answer)
          .AddMethod(__Method_OptionalWebRTCConfig, serviceImpl.OptionalWebRTCConfig).Build();
    }

    /// <summary>Register service method with a service binder with or without implementation. Useful when customizing the service binding logic.
    /// Note: this method is part of an experimental API that can change or be removed without any prior notice.</summary>
    /// <param name="serviceBinder">Service methods will be bound by calling <c>AddMethod</c> on this object.</param>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    public static void BindService(grpc::ServiceBinderBase serviceBinder, SignalingServiceBase serviceImpl)
    {
      serviceBinder.AddMethod(__Method_Call, serviceImpl == null ? null : new grpc::ServerStreamingServerMethod<global::Proto.Rpc.Webrtc.V1.CallRequest, global::Proto.Rpc.Webrtc.V1.CallResponse>(serviceImpl.Call));
      serviceBinder.AddMethod(__Method_CallUpdate, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Proto.Rpc.Webrtc.V1.CallUpdateRequest, global::Proto.Rpc.Webrtc.V1.CallUpdateResponse>(serviceImpl.CallUpdate));
      serviceBinder.AddMethod(__Method_Answer, serviceImpl == null ? null : new grpc::DuplexStreamingServerMethod<global::Proto.Rpc.Webrtc.V1.AnswerResponse, global::Proto.Rpc.Webrtc.V1.AnswerRequest>(serviceImpl.Answer));
      serviceBinder.AddMethod(__Method_OptionalWebRTCConfig, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigRequest, global::Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigResponse>(serviceImpl.OptionalWebRTCConfig));
    }

  }
}
#endregion