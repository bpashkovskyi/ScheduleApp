using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Schedule.Web.Services;

namespace Schedule.Web.Endpoints.Proxy;

public sealed class ProxyEndpoint : Endpoint<ProxyRequest, ProxyResponse>
{
    private readonly IScheduleService _scheduleService;
    private readonly ILogger<ProxyEndpoint> _logger;

    public ProxyEndpoint(IScheduleService scheduleService, ILogger<ProxyEndpoint> logger)
    {
        _scheduleService = scheduleService;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/api/schedule/proxy");
        AllowAnonymous();
        Description(d => d
            .WithName("Proxy")
            .WithTags("Schedule")
            .WithSummary("Proxy request to external schedule API")
            .WithDescription("Forwards requests to the external schedule API and returns the response"));
    }

    public override async Task HandleAsync(ProxyRequest req, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrEmpty(req.Q))
            {
                AddError(r => r.Q, "Query parameter 'q' is required");
                await SendErrorsAsync(400, ct);
                return;
            }

            var response = await _scheduleService.GetProxyDataAsync(req.Q);
            await SendAsync(new ProxyResponse { Content = response }, cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying request to external API");
            AddError("An error occurred while processing the request");
            await SendErrorsAsync(500, ct);
        }
    }
}
