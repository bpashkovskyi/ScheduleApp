using FastEndpoints;
using Schedule.Web.Services;

namespace Schedule.Web.Endpoints.Proxy;

public sealed class ProxyEndpoint : Endpoint<ProxyRequest, string>
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
        Description(description => description
            .WithName("Proxy")
            .WithTags("Schedule")
            .WithSummary("Proxy request to external schedule API")
            .WithDescription("Forwards requests to the external schedule API and returns the response"));
    }

    public override async Task HandleAsync(ProxyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Q))
            {
                AddError(proxyRequest => proxyRequest.Q, "Query parameter 'q' is required");
                await SendErrorsAsync(400, cancellationToken);
                return;
            }

            var response = await _scheduleService.GetProxyDataAsync(request.Q);
            await SendStringAsync(response, 200, "application/json", cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error proxying request to external API");
            AddError("An error occurred while processing the request");
            await SendErrorsAsync(500, cancellationToken);
        }
    }
}
