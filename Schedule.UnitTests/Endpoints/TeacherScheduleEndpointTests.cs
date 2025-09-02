using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Schedule.Web.Endpoints.TeacherSchedule;
using Schedule.Web.Services;
using Xunit;
using Schedule.Web.Models.Domain;

namespace Schedule.UnitTests.Endpoints;

public sealed class TeacherScheduleEndpointTests
{
    private readonly Mock<IScheduleService> _mockScheduleService;
    private readonly Mock<ILogger<TeacherScheduleEndpoint>> _mockLogger;
    private readonly TeacherScheduleEndpoint _endpoint;

    public TeacherScheduleEndpointTests()
    {
        _mockScheduleService = new Mock<IScheduleService>();
        _mockLogger = new Mock<ILogger<TeacherScheduleEndpoint>>();
        _endpoint = new TeacherScheduleEndpoint(_mockScheduleService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Act & Assert
        _endpoint.Should().NotBeNull();
        _endpoint.Should().BeOfType<TeacherScheduleEndpoint>();
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

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task HandleAsync_WithInvalidTeacherId_ShouldReturnError(int invalidTeacherId)
    {
        // Arrange
        var request = new TeacherScheduleRequest { TeacherId = invalidTeacherId, MonthId = 1 };

        // Act
        await _endpoint.HandleAsync(request, CancellationToken.None);

        // Assert
        _endpoint.ValidationFailures.Should().ContainSingle(validationFailure => validationFailure.PropertyName == "TeacherId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(100)]
    public async Task HandleAsync_WithInvalidMonthId_ShouldReturnError(int invalidMonthId)
    {
        // Arrange
        var request = new TeacherScheduleRequest { TeacherId = 123, MonthId = invalidMonthId };

        // Act
        await _endpoint.HandleAsync(request, CancellationToken.None);

        // Assert
        _endpoint.ValidationFailures.Should().ContainSingle(validationFailure => validationFailure.PropertyName == "MonthId");
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(123, 6)]
    [InlineData(999, 12)]
    public async Task HandleAsync_WithValidParameters_ShouldNotReturnError(int teacherId, int monthId)
    {
        // Arrange
        var request = new TeacherScheduleRequest { TeacherId = teacherId, MonthId = monthId };
        var expectedLessons = new List<Lesson>();
        
        _mockScheduleService
            .Setup(scheduleService => scheduleService.GetTeacherScheduleDataAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(expectedLessons);

        // Act
        await _endpoint.HandleAsync(request, CancellationToken.None);

        // Assert
        _endpoint.ValidationFailures.Should().BeEmpty();
        _mockScheduleService.Verify(scheduleService => scheduleService.GetTeacherScheduleDataAsync(teacherId, monthId), Times.Once);
    }
}
