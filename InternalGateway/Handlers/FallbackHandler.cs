using System.Net;


namespace InternalGateway.Handlers;

public class FallbackHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            return await base.SendAsync(request, cancellationToken);
        }
        catch (Exception)
        {
            var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent(
                    "{\"message\":\"Service temporarily unavailable. Please try again later (Fallback response).\"}",
                    System.Text.Encoding.UTF8,
                    "application/json")
            };
            response.Headers.Add("X-Fallback", "true");
            return response;
        }
    }
}