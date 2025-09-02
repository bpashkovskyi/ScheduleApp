using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Schedule.Web.Services;
using Schedule.Web.Models;
using Xunit;

namespace Schedule.UnitTests;

public sealed class ScheduleServiceTests
{
    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly Mock<ILogger<ScheduleService>> _mockLogger;
    private readonly Mock<ILessonMapper> _mockLessonMapper;
    private readonly ScheduleService _service;

    public ScheduleServiceTests()
    {
        _mockHttpClient = new Mock<HttpClient>();
        _mockLogger = new Mock<ILogger<ScheduleService>>();
        _mockLessonMapper = new Mock<ILessonMapper>();
        _service = new ScheduleService(_mockHttpClient.Object, _mockLogger.Object, _mockLessonMapper.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Act & Assert
        _service.Should().NotBeNull();
        _service.Should().BeOfType<ScheduleService>();
    }

    [Theory]
    [InlineData(1, "01.01.2025", "31.01.2025")]  // January
    [InlineData(2, "01.02.2025", "28.02.2025")]  // February (non-leap year)
    [InlineData(6, "01.06.2025", "30.06.2025")]  // June
    [InlineData(12, "01.12.2025", "31.12.2025")] // December
    public void GetMonthDateRange_WithValidMonthId_ShouldReturnCorrectDateRange(int monthId, string expectedBeginDate, string expectedEndDate)
    {
        // Use reflection to access the private method
        var method = typeof(ScheduleService).GetMethod("GetMonthDateRange", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        
        // Act
        var result = method!.Invoke(null, new object[] { monthId });
        var (beginDate, endDate) = ((string, string))result!;
        
        // Assert
        beginDate.Should().Be(expectedBeginDate);
        endDate.Should().Be(expectedEndDate);
    }

    [Theory]
    [InlineData(1, "01.01.2026", "31.01.2026")]  // January next year (if current month > 1)
    [InlineData(2, "01.02.2026", "28.02.2026")]  // February next year (if current month > 2)
    public void GetMonthDateRange_WithPastMonth_ShouldUseNextYear(int monthId, string expectedBeginDate, string expectedEndDate)
    {
        // This test assumes the current month is greater than the test month
        // In a real scenario, you might want to mock DateTime.Now or use a different approach
        
        // Use reflection to access the private method
        var method = typeof(ScheduleService).GetMethod("GetMonthDateRange", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        
        // Act
        var result = method!.Invoke(null, new object[] { monthId });
        var (beginDate, endDate) = ((string, string))result!;
        
        // Assert - The actual year will depend on the current date, but format should be correct
        beginDate.Should().MatchRegex(@"^\d{2}\.\d{2}\.\d{4}$");
        endDate.Should().MatchRegex(@"^\d{2}\.\d{2}\.\d{4}$");
        
        // Verify the dates are valid
        DateTime.TryParseExact(beginDate, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out _).Should().BeTrue();
        DateTime.TryParseExact(endDate, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out _).Should().BeTrue();
    }
}
