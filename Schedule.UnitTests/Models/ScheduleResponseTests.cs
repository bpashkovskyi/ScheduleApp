using FluentAssertions;
using Schedule.Web.Models.Api;
using Xunit;

namespace Schedule.UnitTests.Models;

public sealed class ScheduleResponseTests
{
    [Fact]
    public void ScheduleResponse_WithValidData_ShouldSetProperties()
    {
        // Arrange
        var scheduleResponse = new ScheduleResponse
        {
            ScheduleExport = new ScheduleExport
            {
                RozItems = new List<LessonResponse>
                {
                    new LessonResponse
                    {
                        Object = "Test Teacher",
                        Date = "01.01.2025",
                        LessonNumber = "1",
                        LessonName = "Test Lesson",
                        LessonTime = "09:00-10:30"
                    }
                },
                Code = "0"
            }
        };

        // Assert
        scheduleResponse.ScheduleExport.Should().NotBeNull();
        scheduleResponse.ScheduleExport.RozItems.Should().HaveCount(1);
        scheduleResponse.ScheduleExport.Code.Should().Be("0");
        scheduleResponse.ScheduleExport.RozItems[0].Object.Should().Be("Test Teacher");
    }

    [Fact]
    public void ScheduleResponse_WithEmptyData_ShouldSetDefaultValues()
    {
        // Arrange
        var scheduleResponse = new ScheduleResponse();

        // Assert
        scheduleResponse.ScheduleExport.Should().NotBeNull();
        scheduleResponse.ScheduleExport.RozItems.Should().BeEmpty();
        scheduleResponse.ScheduleExport.Code.Should().BeEmpty();
    }

    [Fact]
    public void ScheduleExport_WithValidData_ShouldSetProperties()
    {
        // Arrange
        var scheduleExport = new ScheduleExport
        {
            RozItems = new List<LessonResponse>
            {
                new LessonResponse { Object = "Teacher 1" },
                new LessonResponse { Object = "Teacher 2" }
            },
            Code = "1"
        };

        // Assert
        scheduleExport.RozItems.Should().HaveCount(2);
        scheduleExport.Code.Should().Be("1");
        scheduleExport.RozItems[0].Object.Should().Be("Teacher 1");
        scheduleExport.RozItems[1].Object.Should().Be("Teacher 2");
    }

    [Fact]
    public void ScheduleExport_WithEmptyData_ShouldSetDefaultValues()
    {
        // Arrange
        var scheduleExport = new ScheduleExport();

        // Assert
        scheduleExport.RozItems.Should().BeEmpty();
        scheduleExport.Code.Should().BeEmpty();
    }
}
