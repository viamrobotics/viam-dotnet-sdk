using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Viam.App.V1;
using Viam.Core.Utils;

namespace Viam.Core.App
{
    public class BillingClient(ILogger<BillingClient> logger, BillingService.BillingServiceClient client)
    {
        public async ValueTask<GetCurrentMonthUsageResponse> GetCurrentMonthUsageAsync(string orgId,
            TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            return await client.GetCurrentMonthUsageAsync(new GetCurrentMonthUsageRequest() { OrgId = orgId },
                    deadline: timeout.ToDeadline(),
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        // Should this just return a byte array?
        public async IAsyncEnumerable<ReadOnlyMemory<byte>> GetInvoicePdf(string invoiceId, string orgId,
            TimeSpan? timeout = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var result = client.GetInvoicePdf(new GetInvoicePdfRequest() { Id = invoiceId, OrgId = orgId },
                deadline: timeout.ToDeadline(),
                cancellationToken: cancellationToken);

            while (await result.ResponseStream.MoveNext(cancellationToken))
            {
                yield return result.ResponseStream.Current.Chunk.Memory;
            }
        }

        public async ValueTask<GetInvoicesSummaryResponse> GetInvoicesSummary(string orgId, TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            return await client.GetInvoicesSummaryAsync(new GetInvoicesSummaryRequest() { OrgId = orgId },
                    deadline: timeout.ToDeadline(),
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        public async ValueTask<GetOrgBillingInformationResponse> GetOrgBillingInformation(string orgId,
            TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            return await client.GetOrgBillingInformationAsync(new GetOrgBillingInformationRequest() { OrgId = orgId },
                    deadline: timeout.ToDeadline(),
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }
}