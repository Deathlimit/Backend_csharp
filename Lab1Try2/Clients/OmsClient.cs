using Common;
using Lab1Try2.DAL.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Lab1Try2.Clients
{
    public class OmsClient(HttpClient client, ILogger<OmsClient> logger)
    {
        public async Task<V1AuditLogOrderResponse> LogOrder(V1AuditLogOrderRequest request, CancellationToken token)
        {
            var msg = await client.PostAsync("api/v1/audit-log-order", new StringContent(request.ToJson(), Encoding.UTF8, "application/json"), token);
            if (msg.IsSuccessStatusCode)
            {
                var content = await msg.Content.ReadAsStringAsync(cancellationToken: token);
                return content.FromJson<V1AuditLogOrderResponse>();
            }

            throw new HttpRequestException();
        }
    }
}