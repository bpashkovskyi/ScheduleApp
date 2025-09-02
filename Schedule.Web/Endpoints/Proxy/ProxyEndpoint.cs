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
            await SendAsync(new ProxyResponse { Content = response }, cancellation: cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error proxying request to external API");
            AddError("An error occurred while processing the request");
            await SendErrorsAsync(500, cancellationToken);
        }
    }
}
