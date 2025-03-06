using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Viam.App.V1;
using Viam.Common.V1;
using Viam.Core.Utils;

namespace Viam.Core.App
{
    public sealed class AppClient(ILogger<AppClient> logger, AppService.AppServiceClient client)
    {
        private string? _orgId;

        private async ValueTask<string?> GetOrganizationId(CancellationToken cancellationToken = default)
        {
            if (_orgId is not null)
                return _orgId;

            var organizations = await ListOrganizations(cancellationToken)
                                    .ConfigureAwait(false);

            var org = organizations.FirstOrDefault();
            if (org?.Id is not null)
            {
                _orgId = org.Id;
            }

            return _orgId;
        }

        private async ValueTask<Authorization> CreateAuthorization(string identityId,
                                                                   string identityType,
                                                                   string role,
                                                                   string resourceType,
                                                                   string resourceId,
                                                                   CancellationToken cancellationToken = default)
        {
            var orgId = await GetOrganizationId(cancellationToken)
                            .ConfigureAwait(false);

            return new Authorization()
            {
                AuthorizationType = "role",
                IdentityId = identityId,
                IdentityType = identityType,
                AuthorizationId = $"{resourceType}_{role}",
                ResourceType = resourceType,
                ResourceId = resourceId,
                OrganizationId = orgId
            };
        }

        private ValueTask<Authorization> CreateAuthorizationForNewApiKey(ApiKeyAuthorization auth)
        {
            return CreateAuthorization("", "api-key", auth.Role, auth.ResourceType, auth.ResourceId);
        }

        public async ValueTask<Organization> CreateOrganization(string name,
                                                                CancellationToken cancellationToken = default)
        {
            var result = await client
                               .CreateOrganizationAsync(new CreateOrganizationRequest() { Name = name },
                                                        cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.Organization;
        }

        public async ValueTask<Organization[]> ListOrganizations(CancellationToken cancellationToken = default)
        {
            var result = await client
                               .ListOrganizationsAsync(new ListOrganizationsRequest(),
                                                       cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.Organizations.ToArray();
        }

        public async ValueTask<OrgDetails[]> ListOrganizationsByUser(string userId,
                                                                     CancellationToken cancellationToken = default)
        {
            var result = await client.ListOrganizationsByUserAsync(
                                                   new ListOrganizationsByUserRequest() { UserId = userId },
                                                   cancellationToken: cancellationToken)
                                               .ConfigureAwait(false);

            return result.Orgs.ToArray();
        }

        public async ValueTask<Organization> GetOrganization(string? orgId = null,
                                                             CancellationToken cancellationToken = default)
        {
            var result = await client
                               .GetOrganizationAsync(new GetOrganizationRequest() { OrganizationId = orgId },
                                                     cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.Organization;
        }

        public async ValueTask<bool> GetOrganizationNamespaceAvailability(
            string publicNamespace,
            CancellationToken cancellationToken = default)
        {
            var result = await client.GetOrganizationNamespaceAvailabilityAsync(
                                                   new GetOrganizationNamespaceAvailabilityRequest()
                                                   {
                                                       PublicNamespace = publicNamespace
                                                   },
                                                   cancellationToken: cancellationToken)
                                               .ConfigureAwait(false);

            return result.Available;
        }

        public async ValueTask<Organization> UpdateOrganization(string? name = null,
                                                                string? publicNamespace = null,
                                                                string? region = null,
                                                                string? cid = null,
                                                                CancellationToken cancellationToken = default)
        {
            var result = await client
                               .UpdateOrganizationAsync(
                                   new UpdateOrganizationRequest()
                                   {
                                       OrganizationId = _orgId,
                                       Name = name,
                                       PublicNamespace = publicNamespace,
                                       Cid = cid,
                                       Region = region
                                   },
                                   cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.Organization;
        }

        public async ValueTask DeleteOrganization(string orgId, CancellationToken cancellationToken = default)
        {
            await client.DeleteOrganizationAsync(new DeleteOrganizationRequest() { OrganizationId = orgId },
                                                           cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);
        }

        public async ValueTask<OrganizationMember[]> ListOrganizationMembers(
            CancellationToken cancellationToken = default)
        {
            var result = await client.ListOrganizationMembersAsync(new ListOrganizationMembersRequest()
            {
                OrganizationId =
                                                                                     await GetOrganizationId(
                                                                                             cancellationToken)
                                                                                         .ConfigureAwait(false)
            },
                                                                             cancellationToken: cancellationToken)
                                               .ConfigureAwait(false);

            return result.Members.ToArray();
        }

        public async ValueTask CreateOrganizationInvite(string email,
                                                        Authorization[]? authorizations = null,
                                                        bool sendEmailInvite = true,
                                                        CancellationToken cancellationToken = default)
        {
            await client.CreateOrganizationInviteAsync(new CreateOrganizationInviteRequest()
            {
                OrganizationId = await GetOrganizationId()
                                                                         .ConfigureAwait(false),
                Email = email,
                Authorizations = { authorizations },
                SendEmailInvite = sendEmailInvite
            },
                                                                 cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);
        }

        public async ValueTask UpdateOrganizationInviteAuthorizations(string email,
                                                                      Authorization[]? addAuthorizations,
                                                                      Authorization[]? removeAuthorizations,
                                                                      CancellationToken cancellationToken = default)
        {
            await client.UpdateOrganizationInviteAuthorizationsAsync(
                                      new UpdateOrganizationInviteAuthorizationsRequest()
                                      {
                                          OrganizationId = await GetOrganizationId()
                                                               .ConfigureAwait(false),
                                          Email = email,
                                          AddAuthorizations = { addAuthorizations },
                                          RemoveAuthorizations = { removeAuthorizations }
                                      },
                                      cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);
        }

        public async ValueTask DeleteOrganizationMember(string userId, CancellationToken cancellationToken = default)
        {
            await client.DeleteOrganizationMemberAsync(new DeleteOrganizationMemberRequest()
            {
                OrganizationId = await GetOrganizationId()
                                                                         .ConfigureAwait(false),
                UserId = userId
            },
                                                                 cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);
        }

        public async ValueTask DeleteOrganizationInvite(string email, CancellationToken cancellationToken = default)
        {
            await client.DeleteOrganizationInviteAsync(new DeleteOrganizationInviteRequest()
            {
                OrganizationId = await GetOrganizationId()
                                                                         .ConfigureAwait(false),
                Email = email
            },
                                                                 cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);
        }

        public async ValueTask<OrganizationInvite> ResendOrganizationInvite(
            string email,
            CancellationToken cancellationToken = default)
        {
            var result = await client.ResendOrganizationInviteAsync(new ResendOrganizationInviteRequest()
            {
                OrganizationId =
                                                                                      await GetOrganizationId()
                                                                                          .ConfigureAwait(false),
                Email = email
            },
                                                                              cancellationToken: cancellationToken)
                                               .ConfigureAwait(false);

            return result.Invite;
        }

        public async ValueTask<Location> CreateLocation(string name,
                                                        string? parentLocationId = null,
                                                        CancellationToken cancellationToken = default)
        {
            var result = await client.CreateLocationAsync(new CreateLocationRequest()
            {
                OrganizationId = await GetOrganizationId()
                                                                            .ConfigureAwait(false),
                Name = name,
                ParentLocationId = parentLocationId
            },
                                                                    cancellationToken: cancellationToken)
                                               .ConfigureAwait(false);

            return result.Location;
        }

        public async ValueTask<Location> GetLocation(string locationId, CancellationToken cancellationToken = default)
        {
            var result = await client.GetLocationAsync(new GetLocationRequest() { LocationId = locationId },
                                                                 cancellationToken: cancellationToken)
                                               .ConfigureAwait(false);

            return result.Location;
        }

        public async ValueTask<Location> UpdateLocation(string locationId,
                                                        string? name = null,
                                                        string? parentLocationId = null,
                                                        CancellationToken cancellationToken = default)
        {
            var result = await client.UpdateLocationAsync(new UpdateLocationRequest()
            {
                LocationId = locationId,
                Name = name,
                ParentLocationId = parentLocationId
            },
                                                                    cancellationToken: cancellationToken)
                                               .ConfigureAwait(false);

            return result.Location;
        }

        public async ValueTask<Location[]> ListLocations(CancellationToken cancellationToken = default)
        {
            var result = await client.ListLocationsAsync(new ListLocationsRequest()
            {
                OrganizationId =
                                                                           await GetOrganizationId(cancellationToken)
                                                                               .ConfigureAwait(false)
            },
                                                                   cancellationToken: cancellationToken)
                                               .ConfigureAwait(false);

            return result.Locations.ToArray();
        }

        public async ValueTask ShareLocation(string locationId,
                                             string orgId,
                                             CancellationToken cancellationToken = default)
        {
            await client
                  .ShareLocationAsync(new ShareLocationRequest() { LocationId = locationId, OrganizationId = orgId },
                                      cancellationToken: cancellationToken)
                  .ConfigureAwait(false);
        }

        public async ValueTask UnshareLocation(string locationId,
                                               string orgId,
                                               CancellationToken cancellationToken = default)
        {
            await client
                  .UnshareLocationAsync(
                      new UnshareLocationRequest() { LocationId = locationId, OrganizationId = orgId },
                      cancellationToken: cancellationToken)
                  .ConfigureAwait(false);
        }

        public async ValueTask<LocationAuth> LocationAuth(string? locationId = null,
                                                          CancellationToken cancellationToken = default)
        {
            var result = await client.LocationAuthAsync(new LocationAuthRequest() { LocationId = locationId },
                                                                  cancellationToken: cancellationToken)
                                               .ConfigureAwait(false);

            return result.Auth;
        }

        public async ValueTask<LocationAuth> CreateLocationSecret(string? locationId = null,
                                                                  CancellationToken cancellationToken = default)
        {
            var result = await client.CreateLocationSecretAsync(
                                                   new CreateLocationSecretRequest() { LocationId = locationId },
                                                   cancellationToken: cancellationToken)
                                               .ConfigureAwait(false);

            return result.Auth;
        }

        public async ValueTask DeleteLocationSecret(string secretId,
                                                    string? locationId = null,
                                                    CancellationToken cancellationToken = default)
        {
            await client.DeleteLocationSecretAsync(
                                      new DeleteLocationSecretRequest()
                                      {
                                          SecretId = secretId,
                                          LocationId = locationId
                                      },
                                      cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);
        }

        public async ValueTask<Viam.App.V1.Robot> GetRobot(string robotId,
                                                           CancellationToken cancellationToken = default)
        {
            var result = await client
                               .GetRobotAsync(new GetRobotRequest() { Id = robotId },
                                              cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.Robot;
        }

        public async ValueTask<RoverRentalRobot[]> GetRoverRentalRobots(CancellationToken cancellationToken = default)
        {
            var result = await client
                               .GetRoverRentalRobotsAsync(new GetRoverRentalRobotsRequest(),
                                                          cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.Robots.ToArray();
        }

        public async ValueTask<RobotPart[]> GetRobotParts(string robotId, CancellationToken cancellationToken = default)
        {
            var result = await client
                               .GetRobotPartsAsync(new GetRobotPartsRequest() { RobotId = robotId },
                                                   cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.Parts.ToArray();
        }

        public async ValueTask<RobotPart> GetRobotPart(string partId, CancellationToken cancellationToken = default)
        {
            var result = await client
                               .GetRobotPartAsync(new GetRobotPartRequest() { Id = partId },
                                                  cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.Part;
        }

        public async ValueTask<LogEntry[]> GetRobotPartLogs(string robotPartId,
                                                            string? filter = null,
                                                            int numLogEntries = 100,
                                                            CancellationToken cancellationToken = default)
        {
            var logCount = 0;
            var logs = new LogEntry[numLogEntries];
            string? pageToken = null;
            while (logCount < numLogEntries)
            {
                (var logEntries, pageToken) = await GetRobotParLogs(robotPartId, filter, pageToken, cancellationToken)
                                                  .ConfigureAwait(false);

                if (logEntries.Length == 0)
                    break;

                logEntries.CopyTo(logs, logCount);
                logCount += logEntries.Length;
                if (pageToken is null)
                    break;
            }

            return logs[..logCount];
        }

        private async ValueTask<(LogEntry[], string?)> GetRobotParLogs(string robotPartId,
                                                                       string? filter,
                                                                       string? pageToken,
                                                                       CancellationToken cancellationToken)
        {
            var result = await client
                               .GetRobotPartLogsAsync(
                                   new GetRobotPartLogsRequest()
                                   {
                                       Id = robotPartId,
                                       Filter = filter,
                                       PageToken = pageToken
                                   },
                                   cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return (result.Logs.ToArray(), result.NextPageToken);
        }

        public ValueTask TailRobotPartLogs(string robotPartId,
                                           string? filter = null,
                                           CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<RobotPartHistoryEntry[]> GetRobotPartHistory(
            string robotPartId,
            CancellationToken cancellationToken = default)
        {
            var result = await client
                               .GetRobotPartHistoryAsync(new GetRobotPartHistoryRequest() { Id = robotPartId },
                                                         cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.History.ToArray();
        }

        public async ValueTask<RobotPart> UpdateRobotPart(string robotPartId,
                                                          string name,
                                                          IDictionary<string, object?>? robotConfig = null,
                                                          CancellationToken cancellationToken = default)
        {
            var result = await client
                               .UpdateRobotPartAsync(
                                   new UpdateRobotPartRequest()
                                   {
                                       Id = robotPartId,
                                       Name = name,
                                       RobotConfig = robotConfig.ToStruct(),
                                   },
                                   cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.Part;
        }

        public async ValueTask<string> NewRobotPart(string robotId,
                                                    string partName,
                                                    CancellationToken cancellationToken = default)
        {
            var result = await client
                               .NewRobotPartAsync(new NewRobotPartRequest() { RobotId = robotId, PartName = partName },
                                                  cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.PartId;
        }

        public async ValueTask DeleteRobotPart(string robotPartId, CancellationToken cancellationToken = default)
        {
            await client.DeleteRobotPartAsync(new DeleteRobotPartRequest() { PartId = robotPartId },
                                                        cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);
        }

        public async ValueTask MarkPartAsMain(string robotPartId, CancellationToken cancellationToken = default)
        {
            await client.MarkPartAsMainAsync(new MarkPartAsMainRequest() { PartId = robotPartId },
                                                       cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);
        }

        public async ValueTask MarkPartForRestart(string robotPartId, CancellationToken cancellationToken = default)
        {
            await client.MarkPartForRestartAsync(new MarkPartForRestartRequest() { PartId = robotPartId },
                                                           cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);
        }

        public async ValueTask<RobotPart> CreateRobotPartSecret(string robotPartId,
                                                                CancellationToken cancellationToken = default)
        {
            var result = await client.CreateRobotPartSecretAsync(
                                                   new CreateRobotPartSecretRequest() { PartId = robotPartId },
                                                   cancellationToken: cancellationToken)
                                               .ConfigureAwait(false);

            return result.Part;
        }

        public async ValueTask DeleteRobotPartSecret(string secretId,
                                                     string robotPartId,
                                                     CancellationToken cancellationToken = default)
        {
            await client.DeleteRobotPartSecretAsync(
                                      new DeleteRobotPartSecretRequest() { SecretId = secretId, PartId = robotPartId },
                                      cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);
        }

        public async ValueTask<Viam.App.V1.Robot[]> ListRobots(string? locationId = null,
                                                               CancellationToken cancellationToken = default)
        {
            var result = await client
                               .ListRobotsAsync(new ListRobotsRequest() { LocationId = locationId },
                                                cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.Robots.ToArray();
        }

        public async ValueTask<string> NewRobot(string name,
                                                string? locationId = null,
                                                CancellationToken cancellationToken = default)
        {
            var result = await client
                               .NewRobotAsync(new NewRobotRequest() { Name = name, Location = locationId },
                                              cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.Id;
        }

        public async ValueTask<Viam.App.V1.Robot> UpdateRobot(string robotId,
                                                              string name,
                                                              string? locationId = null,
                                                              CancellationToken cancellationToken = default)
        {
            var result = await client
                               .UpdateRobotAsync(
                                   new UpdateRobotRequest() { Id = robotId, Name = name, Location = locationId },
                                   cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.Robot;
        }

        public async ValueTask DeleteRobot(string robotId, CancellationToken cancellationToken = default)
        {
            await client
                  .DeleteRobotAsync(new DeleteRobotRequest() { Id = robotId }, cancellationToken: cancellationToken)
                  .ConfigureAwait(false);
        }

        public async ValueTask<Fragment[]> ListFragments(bool showPublic = true,
                                                         CancellationToken cancellationToken = default)
        {
            var result = await client
                               .ListFragmentsAsync(new ListFragmentsRequest() { ShowPublic = showPublic },
                                                   cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.Fragments.ToArray();

        }

        public async ValueTask<Fragment> GetFragment(string fragmentId, CancellationToken cancellationToken = default)
        {
            var result = await client
                               .GetFragmentAsync(new GetFragmentRequest() { Id = fragmentId },
                                                 cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.Fragment;
        }

        public async ValueTask<Fragment> CreateFragment(string name,
                                                        IDictionary<string, object?>? config = null,
                                                        CancellationToken cancellationToken = default)
        {
            var result = await client
                               .CreateFragmentAsync(
                                   new CreateFragmentRequest() { Name = name, Config = config.ToStruct() },
                                   cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.Fragment;
        }

        public async ValueTask<Fragment> UpdateFragment(string fragmentId,
                                                        string name,
                                                        IDictionary<string, object?>? config = null,
                                                        CancellationToken cancellationToken = default)
        {
            var result = await client
                               .UpdateFragmentAsync(
                                   new UpdateFragmentRequest()
                                   {
                                       Id = fragmentId,
                                       Name = name,
                                       Config = config.ToStruct()
                                   },
                                   cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.Fragment;
        }

        public async ValueTask DeleteFragment(string fragmentId, CancellationToken cancellationToken = default)
        {
            await client.DeleteFragmentAsync(new DeleteFragmentRequest() { Id = fragmentId },
                                                       cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);
        }

        public async ValueTask AddRole(string identityId,
                                       string role,
                                       string resourceType,
                                       string resourceId,
                                       CancellationToken cancellationToken = default)
        {
            if (role is not ("owner" or "operator"))
                throw new ArgumentOutOfRangeException(nameof(role), role, "Acceptable values are owner and operator");

            if (resourceType is not ("organization" or "location" or "robot"))
                throw new ArgumentOutOfRangeException(nameof(resourceType),
                                                      resourceType,
                                                      "Acceptable values are organization, location, or robot");

            var orgId = await GetOrganizationId(cancellationToken)
                            .ConfigureAwait(false);

            await client.AddRoleAsync(new AddRoleRequest()
            {
                Authorization = new Authorization()
                {
                    AuthorizationType = "role",
                    IdentityId = identityId,
                    IdentityType = "",
                    AuthorizationId = $"{resourceType}_{role}",
                    ResourceType = resourceType,
                    ResourceId = resourceId,
                    OrganizationId = orgId
                }
            },
                                                cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);
        }

        public async ValueTask RemoveRole(string identityId,
                                          string role,
                                          string resourceType,
                                          string resourceId,
                                          CancellationToken cancellationToken = default)
        {
            if (role is not ("owner" or "operator"))
                throw new ArgumentOutOfRangeException(nameof(role), role, "Acceptable values are owner and operator");

            if (resourceType is not ("organization" or "location" or "robot"))
                throw new ArgumentOutOfRangeException(nameof(resourceType),
                                                      resourceType,
                                                      "Acceptable values are organization, location, or robot");

            var orgId = await GetOrganizationId(cancellationToken)
                            .ConfigureAwait(false);

            await client.RemoveRoleAsync(new RemoveRoleRequest()
            {
                Authorization = new Authorization()
                {
                    AuthorizationType = "role",
                    IdentityId = identityId,
                    IdentityType = "",
                    AuthorizationId = $"{resourceType}_{role}",
                    ResourceType = resourceType,
                    ResourceId = resourceId,
                    OrganizationId = orgId
                }
            },
                                                   cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);
        }

        public async ValueTask<Authorization[]> ListAuthorizations(string[]? resourceIds = null,
                                                                   CancellationToken cancellationToken = default)
        {
            var orgId = await GetOrganizationId(cancellationToken)
                            .ConfigureAwait(false);

            var result = await client
                               .ListAuthorizationsAsync(
                                   new ListAuthorizationsRequest()
                                   {
                                       OrganizationId = orgId,
                                       ResourceIds = { resourceIds }
                                   },
                                   cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.Authorizations.ToArray();
        }

        public async ValueTask<AuthorizedPermissions[]> CheckPermissions(
            AuthorizedPermissions[] permissions,
            CancellationToken cancellationToken = default)
        {
            var result = await client
                               .CheckPermissionsAsync(new CheckPermissionsRequest() { Permissions = { permissions } },
                                                      cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.AuthorizedPermissions.ToArray();
        }

        public async ValueTask<(string Id, string Url)> CreateModule(string name,
                                                                     CancellationToken cancellationToken = default)
        {
            var result = await client
                               .CreateModuleAsync(new CreateModuleRequest() { Name = name },
                                                  cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return (result.ModuleId, result.Url);
        }

        public async ValueTask<Uri> UpdateModule(string moduleId,
                                                 Uri url,
                                                 string description,
                                                 string entrypoint,
                                                 bool isPublic = false,
                                                 Model[]? models = null,
                                                 CancellationToken cancellationToken = default)
        {
            var result = await client.UpdateModuleAsync(new UpdateModuleRequest()
            {
                ModuleId = moduleId,
                Url = url.ToString(),
                Description = description,
                Entrypoint = entrypoint,
                Visibility = isPublic
                                                                          ? Visibility.Public
                                                                          : Visibility.Private,
                Models = { models }
            },
                                                                  cancellationToken: cancellationToken)
                                               .ConfigureAwait(false);

            return new Uri(result.Url);
        }

        public async ValueTask<Uri> UploadModuleFile(ModuleFileInfo? moduleFileInfo,
                                                     ReadOnlyMemory<byte> file,
                                                     CancellationToken cancellationToken = default)
        {
            var request = new UploadModuleFileRequest() { ModuleFileInfo = moduleFileInfo };
            var requestFile = new UploadModuleFileRequest { File = Google.Protobuf.ByteString.CopyFrom(file.Span) };
            using var upload = client.UploadModuleFile();
#if NET6_0_OR_GREATER
            await upload.RequestStream.WriteAsync(request, cancellationToken);
            await upload.RequestStream.WriteAsync(requestFile, cancellationToken);
#elif NETFRAMEWORK
            await upload.RequestStream.WriteAsync(request);
            await upload.RequestStream.WriteAsync(requestFile);
#else
            throw new PlatformNotSupportedException();
#endif
            var response = await upload.ResponseAsync.ConfigureAwait(false);
            return new Uri(response.Url);
        }

        public async ValueTask<Viam.App.V1.Module> GetModule(string moduleId,
                                                             CancellationToken cancellationToken = default)
        {
            var result = await client
                               .GetModuleAsync(new GetModuleRequest() { ModuleId = moduleId },
                                               cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.Module;
        }

        public async ValueTask<Viam.App.V1.Module[]> ListModules(CancellationToken cancellationToken = default)
        {
            var result = await client
                               .ListModulesAsync(new ListModulesRequest(), cancellationToken: cancellationToken)
                               .ConfigureAwait(false);

            return result.Modules.ToArray();
        }

        public async ValueTask<(string Id, string Key)> CreateKey(ApiKeyAuthorization[] authorizations,
                                                                  string? name = null,
                                                                  CancellationToken cancellationToken = default)
        {
            if (name == null)
                name = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

            var auths = await Task.WhenAll(authorizations.Select(async x => await CreateAuthorizationForNewApiKey(x)
                                                                                .ConfigureAwait(false)));

            var result = await client.CreateKeyAsync(new CreateKeyRequest()
            {
                Name = name,
                Authorizations = { auths }
            })
                                               .ConfigureAwait(false);

            return (result.Id, result.Key);
        }

        public async ValueTask<(string Id, string Key)> CreateKeyFromExistingKeyAuthorizations(string id, CancellationToken cancellationToken = default)
        {
            var result = await client.CreateKeyFromExistingKeyAuthorizationsAsync(new CreateKeyFromExistingKeyAuthorizationsRequest() { Id = id }, cancellationToken: cancellationToken).ConfigureAwait(false);
            return (result.Id, result.Key);
        }

        public async ValueTask<APIKeyWithAuthorizations[]> ListKeys(CancellationToken cancellationToken)
        {
            var result = await client
                   .ListKeysAsync(new ListKeysRequest(), cancellationToken: cancellationToken)
                   .ConfigureAwait(false);

            return result.ApiKeys.ToArray();
        }
    }

    public record ApiKeyAuthorization(string Role, string ResourceType, string ResourceId);
}


