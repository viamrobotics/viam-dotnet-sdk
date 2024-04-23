using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Viam.Core.Utils;
using Viam.Core.Resources;
using Viam.Robot.V1;

using grpcRobotService = Viam.Robot.V1.RobotService;
namespace Viam.ModularResources.Controllers
{
    public class RobotService(ResourceManager manager) : grpcRobotService.RobotServiceBase
    {
        public readonly ResourceManager Manager = manager;

        public override Task<Robot.V1.ResourceNamesResponse> ResourceNames(Robot.V1.ResourceNamesRequest request, ServerCallContext context)
        {
            var resp = new Robot.V1.ResourceNamesResponse();
            resp.Resources.AddRange(Manager.GetResourceNames());
            return Task.FromResult(resp);
        }

        public override Task<Robot.V1.GetStatusResponse> GetStatus(Robot.V1.GetStatusRequest request, ServerCallContext context)
        {
            var resp = new GetStatusResponse();
            foreach (var resource in request.ResourceNames)
            {
                var status = Manager.GetResource(resource).GetStatus();
                var s = new Viam.Robot.V1.Status { Name = resource, Status_ = new Struct() };
                if (status.LastReconfigured.HasValue)
                    s.LastReconfigured = Timestamp.FromDateTime(status.LastReconfigured.Value);

                s.LastReconfigured = null;

                resp.Status.Add(s);
            }

            return Task.FromResult(resp);
        }

        public override async Task StreamStatus(Robot.V1.StreamStatusRequest request, IServerStreamWriter<Robot.V1.StreamStatusResponse> responseStream, ServerCallContext context)
        {
            while (true)
            {
                var resp = new StreamStatusResponse();
                foreach (var resource in request.ResourceNames)
                {
                    var status = Manager.GetResource(resource).GetStatus();
                    var s = new Viam.Robot.V1.Status { Name = resource, Status_ = new Struct() };
                    if (status.LastReconfigured.HasValue)
                        s.LastReconfigured = Timestamp.FromDateTime(status.LastReconfigured.Value);

                    s.LastReconfigured = null;

                    resp.Status.Add(s);
                }
                await responseStream.WriteAsync(resp);
            }
        }

        public override async Task<Robot.V1.StopAllResponse> StopAll(Robot.V1.StopAllRequest request, ServerCallContext context)
        {
            await Manager.GetResources()
                         .Select(r => r.StopResource())
                         .WhenAll();
            var resp = new StopAllResponse();
            return resp;
        }

        public override Task<Robot.V1.GetOperationsResponse> GetOperations(Robot.V1.GetOperationsRequest request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }

        public override Task<Robot.V1.ResourceRPCSubtypesResponse> ResourceRPCSubtypes(Robot.V1.ResourceRPCSubtypesRequest request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }

        public override Task<Robot.V1.CancelOperationResponse> CancelOperation(Robot.V1.CancelOperationRequest request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }

        public override Task<Robot.V1.BlockForOperationResponse> BlockForOperation(Robot.V1.BlockForOperationRequest request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }

        public override Task<Robot.V1.FrameSystemConfigResponse> FrameSystemConfig(Robot.V1.FrameSystemConfigRequest request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }

        public override Task<Robot.V1.TransformPoseResponse> TransformPose(Robot.V1.TransformPoseRequest request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }

        public override Task<Robot.V1.DiscoverComponentsResponse> DiscoverComponents(Robot.V1.DiscoverComponentsRequest request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }

        public override Task<Robot.V1.GetSessionsResponse> GetSessions(Robot.V1.GetSessionsRequest request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }

        public override Task<Robot.V1.StartSessionResponse> StartSession(Robot.V1.StartSessionRequest request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }

        public override Task<Robot.V1.SendSessionHeartbeatResponse> SendSessionHeartbeat(Robot.V1.SendSessionHeartbeatRequest request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }

        public override Task<Robot.V1.TransformPCDResponse> TransformPCD(Robot.V1.TransformPCDRequest request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }

        public override Task<Robot.V1.LogResponse> Log(Robot.V1.LogRequest request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }

        public override Task<Robot.V1.GetCloudMetadataResponse> GetCloudMetadata(Robot.V1.GetCloudMetadataRequest request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }
    }
}
