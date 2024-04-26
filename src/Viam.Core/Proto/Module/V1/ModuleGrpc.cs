// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: module/v1/module.proto
// </auto-generated>
#pragma warning disable 0414, 1591, 8981, 0612
#region Designer generated code

using grpc = global::Grpc.Core;

namespace Viam.Module.V1 {
  /// <summary>
  /// ModuleService deals with modular resource registration
  /// </summary>
  public static partial class ModuleService
  {
    static readonly string __ServiceName = "viam.module.v1.ModuleService";

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
    static readonly grpc::Marshaller<global::Viam.Module.V1.AddResourceRequest> __Marshaller_viam_module_v1_AddResourceRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Viam.Module.V1.AddResourceRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Viam.Module.V1.AddResourceResponse> __Marshaller_viam_module_v1_AddResourceResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Viam.Module.V1.AddResourceResponse.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Viam.Module.V1.ReconfigureResourceRequest> __Marshaller_viam_module_v1_ReconfigureResourceRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Viam.Module.V1.ReconfigureResourceRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Viam.Module.V1.ReconfigureResourceResponse> __Marshaller_viam_module_v1_ReconfigureResourceResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Viam.Module.V1.ReconfigureResourceResponse.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Viam.Module.V1.RemoveResourceRequest> __Marshaller_viam_module_v1_RemoveResourceRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Viam.Module.V1.RemoveResourceRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Viam.Module.V1.RemoveResourceResponse> __Marshaller_viam_module_v1_RemoveResourceResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Viam.Module.V1.RemoveResourceResponse.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Viam.Module.V1.ReadyRequest> __Marshaller_viam_module_v1_ReadyRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Viam.Module.V1.ReadyRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Viam.Module.V1.ReadyResponse> __Marshaller_viam_module_v1_ReadyResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Viam.Module.V1.ReadyResponse.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Viam.Module.V1.ValidateConfigRequest> __Marshaller_viam_module_v1_ValidateConfigRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Viam.Module.V1.ValidateConfigRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::Viam.Module.V1.ValidateConfigResponse> __Marshaller_viam_module_v1_ValidateConfigResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Viam.Module.V1.ValidateConfigResponse.Parser));

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::Viam.Module.V1.AddResourceRequest, global::Viam.Module.V1.AddResourceResponse> __Method_AddResource = new grpc::Method<global::Viam.Module.V1.AddResourceRequest, global::Viam.Module.V1.AddResourceResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "AddResource",
        __Marshaller_viam_module_v1_AddResourceRequest,
        __Marshaller_viam_module_v1_AddResourceResponse);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::Viam.Module.V1.ReconfigureResourceRequest, global::Viam.Module.V1.ReconfigureResourceResponse> __Method_ReconfigureResource = new grpc::Method<global::Viam.Module.V1.ReconfigureResourceRequest, global::Viam.Module.V1.ReconfigureResourceResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "ReconfigureResource",
        __Marshaller_viam_module_v1_ReconfigureResourceRequest,
        __Marshaller_viam_module_v1_ReconfigureResourceResponse);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::Viam.Module.V1.RemoveResourceRequest, global::Viam.Module.V1.RemoveResourceResponse> __Method_RemoveResource = new grpc::Method<global::Viam.Module.V1.RemoveResourceRequest, global::Viam.Module.V1.RemoveResourceResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "RemoveResource",
        __Marshaller_viam_module_v1_RemoveResourceRequest,
        __Marshaller_viam_module_v1_RemoveResourceResponse);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::Viam.Module.V1.ReadyRequest, global::Viam.Module.V1.ReadyResponse> __Method_Ready = new grpc::Method<global::Viam.Module.V1.ReadyRequest, global::Viam.Module.V1.ReadyResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "Ready",
        __Marshaller_viam_module_v1_ReadyRequest,
        __Marshaller_viam_module_v1_ReadyResponse);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::Viam.Module.V1.ValidateConfigRequest, global::Viam.Module.V1.ValidateConfigResponse> __Method_ValidateConfig = new grpc::Method<global::Viam.Module.V1.ValidateConfigRequest, global::Viam.Module.V1.ValidateConfigResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "ValidateConfig",
        __Marshaller_viam_module_v1_ValidateConfigRequest,
        __Marshaller_viam_module_v1_ValidateConfigResponse);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::Viam.Module.V1.ModuleReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of ModuleService</summary>
    [grpc::BindServiceMethod(typeof(ModuleService), "BindService")]
    public abstract partial class ModuleServiceBase
    {
      /// <summary>
      /// AddResource tells a module about a new resource to handle
      /// </summary>
      /// <param name="request">The request received from the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>The response to send back to the client (wrapped by a task).</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task<global::Viam.Module.V1.AddResourceResponse> AddResource(global::Viam.Module.V1.AddResourceRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      /// <summary>
      /// ReconfigureResource tells a module to reconfigure an existing resource
      /// </summary>
      /// <param name="request">The request received from the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>The response to send back to the client (wrapped by a task).</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task<global::Viam.Module.V1.ReconfigureResourceResponse> ReconfigureResource(global::Viam.Module.V1.ReconfigureResourceRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      /// <summary>
      /// RemoveResource tells a module to close/stop a component/service and remove it
      /// </summary>
      /// <param name="request">The request received from the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>The response to send back to the client (wrapped by a task).</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task<global::Viam.Module.V1.RemoveResourceResponse> RemoveResource(global::Viam.Module.V1.RemoveResourceRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      /// <summary>
      /// Ready determines if the server is started and ready to recieve resource configurations.
      /// </summary>
      /// <param name="request">The request received from the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>The response to send back to the client (wrapped by a task).</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task<global::Viam.Module.V1.ReadyResponse> Ready(global::Viam.Module.V1.ReadyRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      /// <summary>
      /// ValidateConfig determines whether the given config is valid and registers/returns implicit
      /// dependencies.
      /// </summary>
      /// <param name="request">The request received from the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>The response to send back to the client (wrapped by a task).</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task<global::Viam.Module.V1.ValidateConfigResponse> ValidateConfig(global::Viam.Module.V1.ValidateConfigRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Client for ModuleService</summary>
    public partial class ModuleServiceClient : grpc::ClientBase<ModuleServiceClient>
    {
      /// <summary>Creates a new client for ModuleService</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public ModuleServiceClient(grpc::ChannelBase channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for ModuleService that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public ModuleServiceClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected ModuleServiceClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected ModuleServiceClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      /// <summary>
      /// AddResource tells a module about a new resource to handle
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The response received from the server.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Viam.Module.V1.AddResourceResponse AddResource(global::Viam.Module.V1.AddResourceRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return AddResource(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// AddResource tells a module about a new resource to handle
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The response received from the server.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Viam.Module.V1.AddResourceResponse AddResource(global::Viam.Module.V1.AddResourceRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_AddResource, null, options, request);
      }
      /// <summary>
      /// AddResource tells a module about a new resource to handle
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Viam.Module.V1.AddResourceResponse> AddResourceAsync(global::Viam.Module.V1.AddResourceRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return AddResourceAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// AddResource tells a module about a new resource to handle
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Viam.Module.V1.AddResourceResponse> AddResourceAsync(global::Viam.Module.V1.AddResourceRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_AddResource, null, options, request);
      }
      /// <summary>
      /// ReconfigureResource tells a module to reconfigure an existing resource
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The response received from the server.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Viam.Module.V1.ReconfigureResourceResponse ReconfigureResource(global::Viam.Module.V1.ReconfigureResourceRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return ReconfigureResource(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// ReconfigureResource tells a module to reconfigure an existing resource
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The response received from the server.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Viam.Module.V1.ReconfigureResourceResponse ReconfigureResource(global::Viam.Module.V1.ReconfigureResourceRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_ReconfigureResource, null, options, request);
      }
      /// <summary>
      /// ReconfigureResource tells a module to reconfigure an existing resource
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Viam.Module.V1.ReconfigureResourceResponse> ReconfigureResourceAsync(global::Viam.Module.V1.ReconfigureResourceRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return ReconfigureResourceAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// ReconfigureResource tells a module to reconfigure an existing resource
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Viam.Module.V1.ReconfigureResourceResponse> ReconfigureResourceAsync(global::Viam.Module.V1.ReconfigureResourceRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_ReconfigureResource, null, options, request);
      }
      /// <summary>
      /// RemoveResource tells a module to close/stop a component/service and remove it
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The response received from the server.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Viam.Module.V1.RemoveResourceResponse RemoveResource(global::Viam.Module.V1.RemoveResourceRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return RemoveResource(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// RemoveResource tells a module to close/stop a component/service and remove it
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The response received from the server.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Viam.Module.V1.RemoveResourceResponse RemoveResource(global::Viam.Module.V1.RemoveResourceRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_RemoveResource, null, options, request);
      }
      /// <summary>
      /// RemoveResource tells a module to close/stop a component/service and remove it
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Viam.Module.V1.RemoveResourceResponse> RemoveResourceAsync(global::Viam.Module.V1.RemoveResourceRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return RemoveResourceAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// RemoveResource tells a module to close/stop a component/service and remove it
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Viam.Module.V1.RemoveResourceResponse> RemoveResourceAsync(global::Viam.Module.V1.RemoveResourceRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_RemoveResource, null, options, request);
      }
      /// <summary>
      /// Ready determines if the server is started and ready to recieve resource configurations.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The response received from the server.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Viam.Module.V1.ReadyResponse Ready(global::Viam.Module.V1.ReadyRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Ready(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// Ready determines if the server is started and ready to recieve resource configurations.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The response received from the server.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Viam.Module.V1.ReadyResponse Ready(global::Viam.Module.V1.ReadyRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_Ready, null, options, request);
      }
      /// <summary>
      /// Ready determines if the server is started and ready to recieve resource configurations.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Viam.Module.V1.ReadyResponse> ReadyAsync(global::Viam.Module.V1.ReadyRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return ReadyAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// Ready determines if the server is started and ready to recieve resource configurations.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Viam.Module.V1.ReadyResponse> ReadyAsync(global::Viam.Module.V1.ReadyRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_Ready, null, options, request);
      }
      /// <summary>
      /// ValidateConfig determines whether the given config is valid and registers/returns implicit
      /// dependencies.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The response received from the server.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Viam.Module.V1.ValidateConfigResponse ValidateConfig(global::Viam.Module.V1.ValidateConfigRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return ValidateConfig(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// ValidateConfig determines whether the given config is valid and registers/returns implicit
      /// dependencies.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The response received from the server.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Viam.Module.V1.ValidateConfigResponse ValidateConfig(global::Viam.Module.V1.ValidateConfigRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_ValidateConfig, null, options, request);
      }
      /// <summary>
      /// ValidateConfig determines whether the given config is valid and registers/returns implicit
      /// dependencies.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
      /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
      /// <param name="cancellationToken">An optional token for canceling the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Viam.Module.V1.ValidateConfigResponse> ValidateConfigAsync(global::Viam.Module.V1.ValidateConfigRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return ValidateConfigAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      /// <summary>
      /// ValidateConfig determines whether the given config is valid and registers/returns implicit
      /// dependencies.
      /// </summary>
      /// <param name="request">The request to send to the server.</param>
      /// <param name="options">The options for the call.</param>
      /// <returns>The call object.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Viam.Module.V1.ValidateConfigResponse> ValidateConfigAsync(global::Viam.Module.V1.ValidateConfigRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_ValidateConfig, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected override ModuleServiceClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new ModuleServiceClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    public static grpc::ServerServiceDefinition BindService(ModuleServiceBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_AddResource, serviceImpl.AddResource)
          .AddMethod(__Method_ReconfigureResource, serviceImpl.ReconfigureResource)
          .AddMethod(__Method_RemoveResource, serviceImpl.RemoveResource)
          .AddMethod(__Method_Ready, serviceImpl.Ready)
          .AddMethod(__Method_ValidateConfig, serviceImpl.ValidateConfig).Build();
    }

    /// <summary>Register service method with a service binder with or without implementation. Useful when customizing the service binding logic.
    /// Note: this method is part of an experimental API that can change or be removed without any prior notice.</summary>
    /// <param name="serviceBinder">Service methods will be bound by calling <c>AddMethod</c> on this object.</param>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    public static void BindService(grpc::ServiceBinderBase serviceBinder, ModuleServiceBase serviceImpl)
    {
      serviceBinder.AddMethod(__Method_AddResource, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Viam.Module.V1.AddResourceRequest, global::Viam.Module.V1.AddResourceResponse>(serviceImpl.AddResource));
      serviceBinder.AddMethod(__Method_ReconfigureResource, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Viam.Module.V1.ReconfigureResourceRequest, global::Viam.Module.V1.ReconfigureResourceResponse>(serviceImpl.ReconfigureResource));
      serviceBinder.AddMethod(__Method_RemoveResource, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Viam.Module.V1.RemoveResourceRequest, global::Viam.Module.V1.RemoveResourceResponse>(serviceImpl.RemoveResource));
      serviceBinder.AddMethod(__Method_Ready, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Viam.Module.V1.ReadyRequest, global::Viam.Module.V1.ReadyResponse>(serviceImpl.Ready));
      serviceBinder.AddMethod(__Method_ValidateConfig, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Viam.Module.V1.ValidateConfigRequest, global::Viam.Module.V1.ValidateConfigResponse>(serviceImpl.ValidateConfig));
    }

  }
}
#endregion