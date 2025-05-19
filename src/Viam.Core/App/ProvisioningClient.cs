using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Viam.Provisioning.V1;
using CloudConfig = Viam.Provisioning.V1.CloudConfig;

namespace Viam.Core.App
{
    public class ProvisioningClient(
        ILogger<ProvisioningClient> logger,
        ProvisioningService.ProvisioningServiceClient client)
    {
        public async ValueTask<NetworkInfo[]> GetNetworkList(CancellationToken cancellationToken = default)
        {
            var response = await client
                .GetNetworkListAsync(new GetNetworkListRequest(), cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.Networks.ToArray();
        }

        // TODO: this seems wrong, how does the API know what machine we're talking about?
        public async ValueTask<GetSmartMachineStatusResponse> GetSmartMachineStatus(
            CancellationToken cancellationToken = default)
        {
            return await client
                .GetSmartMachineStatusAsync(new GetSmartMachineStatusRequest() { },
                    cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async ValueTask SetNetworkCredentials(string networkType, string ssid, string psk,
            CancellationToken cancellationToken = default)
        {
            await client
                .SetNetworkCredentialsAsync(
                    new SetNetworkCredentialsRequest() { Type = networkType, Ssid = ssid, Psk = psk },
                    cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async ValueTask SetSmartMachineCredentials(CloudConfig cloudConfig,
            CancellationToken cancellationToken = default)
        {
            await client
                .SetSmartMachineCredentialsAsync(new SetSmartMachineCredentialsRequest() { Cloud = cloudConfig },
                    cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}