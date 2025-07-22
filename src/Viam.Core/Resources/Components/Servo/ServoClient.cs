using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Viam.Common.V1;
using Viam.Component.Servo.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components.Servo
{
    public class ServoClient(ViamResourceName resourceName, ViamChannel channel, ILogger<ServoClient> logger)
        : ComponentBase<ServoClient, Component.Servo.V1.ServoService.ServoServiceClient>(resourceName,
                new Component.Servo.V1.ServoService.ServoServiceClient(channel)),
            IServoClient
    {
        public static SubType SubType = SubType.FromRdkComponent("servo");

        public static IServoClient FromRobot(IMachineClient client, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return client.GetComponent<IServoClient>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => Stop();

        public override async ValueTask<Dictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name, command]);
                var res = await Client
                    .DoCommandAsync(
                        new DoCommandRequest() { Name = ResourceName.Name, Command = command.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var response = res.Result.ToDictionary();
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask Move(uint angle,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default,
            [CallerMemberName] string? caller = null)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name, angle]);
                await Client.MoveAsync(new MoveRequest() { Name = Name, AngleDeg = angle, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<uint> GetPosition(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default,
            [CallerMemberName] string? caller = null)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetPositionAsync(
                        new GetPositionRequest() { Name = Name, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess(results: res.PositionDeg);
                return res.PositionDeg;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<bool> IsMoving(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default,
            [CallerMemberName] string? caller = null)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.IsMovingAsync(new IsMovingRequest() { Name = ResourceName.Name },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess(results: res.IsMoving);
                return res.IsMoving;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask Stop(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default,
            [CallerMemberName] string? caller = null)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                await Client.StopAsync(new StopRequest() { Name = Name, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<Geometry[]> GetGeometries(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default,
            [CallerMemberName] string? caller = null)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetGeometriesAsync(
                        new GetGeometriesRequest() { Name = Name, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var geometries = res.Geometries.ToArray();
                logger.LogMethodInvocationSuccess(results: geometries);
                return geometries;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }
    }
}