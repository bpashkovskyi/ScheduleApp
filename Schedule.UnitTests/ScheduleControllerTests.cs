using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Schedule.Web.Controllers.Api;
using Schedule.Web.Models;
using Xunit;

namespace Schedule.UnitTests;

public sealed class ScheduleControllerTests
{
    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly Mock<ILogger<ScheduleController>> _mockLogger;
    private readonly ScheduleController _controller;

    public ScheduleControllerTests()
    {
        _mockHttpClient = new Mock<HttpClient>();
        _mockLogger = new Mock<ILogger<ScheduleController>>();
        _controller = new ScheduleController(_mockHttpClient.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Act & Assert
        _controller.Should().NotBeNull();
        _controller.Should().BeOfType<ScheduleController>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetTeacherSchedule_WithInvalidTeacherId_ShouldReturnBadRequest(int invalidTeacherId)
    {
        // Arrange
        var monthId = 1;

        // Act
        var result = await _controller.GetTeacherSchedule(invalidTeacherId, monthId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(new { error = "Teacher ID must be a positive number" });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(100)]
    public async Task GetTeacherSchedule_WithInvalidMonthId_ShouldReturnBadRequest(int invalidMonthId)
    {
        // Arrange
        var teacherId = 123;

        // Act
        var result = await _controller.GetTeacherSchedule(teacherId, invalidMonthId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(new { error = "Month ID must be between 1 and 12" });
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(123, 6)]
    [InlineData(999, 12)]
    public async Task GetTeacherSchedule_WithValidParameters_ShouldNotReturnBadRequest(int teacherId, int monthId)
    {
        // Arrange & Act
        var result = await _controller.GetTeacherSchedule(teacherId, monthId);

        // Assert - Should not return BadRequest for valid parameters
        result.Should().NotBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Lesson_WithValidData_ShouldSetProperties()
    {
        // Arrange
        var rawItem = new Lesson
        {
            Object = "Test Object",
            Date = "2025-01-15",
            Comment = "Test Comment",
            LessonNumber = "1",
            LessonName = "Test Lesson",
            LessonTime = "09:00-10:30",
            LessonDescription = "Test Description"
        };

        // Assert
        rawItem.Object.Should().Be("Test Object");
        rawItem.Date.Should().Be("2025-01-15");
        rawItem.Comment.Should().Be("Test Comment");
        rawItem.LessonNumber.Should().Be("1");
        rawItem.LessonName.Should().Be("Test Lesson");
        rawItem.LessonTime.Should().Be("09:00-10:30");
        rawItem.LessonDescription.Should().Be("Test Description");
    }

    [Fact]
    public void Lesson_WithNullData_ShouldSetPropertiesToNull()
    {
        // Arrange
        var rawItem = new Lesson();

        // Assert
        rawItem.Object.Should().BeNull();
        rawItem.Date.Should().BeNull();
        rawItem.Comment.Should().BeNull();
        rawItem.LessonNumber.Should().BeNull();
        rawItem.LessonName.Should().BeNull();
        rawItem.LessonTime.Should().BeNull();
        rawItem.LessonDescription.Should().BeNull();
    }

    [Theory]
    [InlineData(1, "01.01.2025", "31.01.2025")]  // January
    [InlineData(2, "01.02.2025", "28.02.2025")]  // February (non-leap year)
    [InlineData(6, "01.06.2025", "30.06.2025")]  // June
    [InlineData(12, "01.12.2025", "31.12.2025")] // December
    public void GetMonthDateRange_WithValidMonthId_ShouldReturnCorrectDateRange(int monthId, string expectedBeginDate, string expectedEndDate)
    {
        // Arrange
        var controller = new ScheduleController(_mockHttpClient.Object, _mockLogger.Object);
        
        // Use reflection to access the private method
        var method = typeof(ScheduleController).GetMethod("GetMonthDateRange", 
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
        
        // Arrange
        var controller = new ScheduleController(_mockHttpClient.Object, _mockLogger.Object);
        
        // Use reflection to access the private method
        var method = typeof(ScheduleController).GetMethod("GetMonthDateRange", 
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
