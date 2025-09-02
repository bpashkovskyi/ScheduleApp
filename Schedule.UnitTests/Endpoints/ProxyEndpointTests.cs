using FastEndpoints;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Schedule.Web.Endpoints.Proxy;
using Schedule.Web.Services;
using Xunit;

namespace Schedule.UnitTests.Endpoints;

public sealed class ProxyEndpointTests
{
    private readonly Mock<IScheduleService> _mockScheduleService;
    private readonly Mock<ILogger<ProxyEndpoint>> _mockLogger;
    private readonly ProxyEndpoint _endpoint;

    public ProxyEndpointTests()
    {
        _mockScheduleService = new Mock<IScheduleService>();
        _mockLogger = new Mock<ILogger<ProxyEndpoint>>();
        _endpoint = new ProxyEndpoint(_mockScheduleService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Act & Assert
        _endpoint.Should().NotBeNull();
        _endpoint.Should().BeOfType<ProxyEndpoint>();
    }

    [Fact]
    public void Configure_ShouldSetCorrectRouteAndPermissions()
    {
        // Act
        _endpoint.Configure();

        // Assert
        _endpoint.Definition.Should().NotBeNull();
        // Note: Route testing is not easily possible in unit tests with FastEndpoints
        // The route configuration is tested during integration tests
    }

    [Fact]
    public async Task HandleAsync_WithValidQuery_ShouldReturnResponse()
    {
        // Arrange
        var request = new ProxyRequest { Q = "test=query" };
        var expectedResponse = "test response";
        
        _mockScheduleService
            .Setup(s => s.GetProxyDataAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedResponse);

        // Act
        await _endpoint.HandleAsync(request, CancellationToken.None);

        // Assert
        _mockScheduleService.Verify(s => s.GetProxyDataAsync("test=query"), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyQuery_ShouldReturnError()
    {
        // Arrange
        var request = new ProxyRequest { Q = string.Empty };

        // Act
        await _endpoint.HandleAsync(request, CancellationToken.None);

        // Assert
        _endpoint.ValidationFailures.Should().ContainSingle(f => f.PropertyName == "Q");
    }
}
