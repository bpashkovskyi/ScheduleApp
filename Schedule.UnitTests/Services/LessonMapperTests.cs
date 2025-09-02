using FluentAssertions;

using Schedule.Web.Models.Api;
using Schedule.Web.Models.Domain;
using Schedule.Web.Services;

using Xunit;

namespace Schedule.UnitTests.Services;

public sealed class LessonMapperTests
{
    private readonly LessonMapper _mapper;

    public LessonMapperTests()
    {
        _mapper = new LessonMapper();
    }

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Act & Assert
        _mapper.Should().NotBeNull();
        _mapper.Should().BeOfType<LessonMapper>();
    }

    [Fact]
    public void ToResponse_WithValidLesson_ShouldMapCorrectly()
    {
        // Arrange
        var lesson = new Lesson
        {
            Object = "Test Teacher",
            Date = "01.01.2025",
            Comment = "Test Comment",
            LessonNumber = "1",
            LessonName = "Test Lesson",
            LessonTime = "09:00-10:30",
            LessonDescription = "Test Description"
        };

        // Act
        var result = _mapper.ToResponse(lesson);

        // Assert
        result.Should().NotBeNull();
        result.Object.Should().Be("Test Teacher");
        result.Date.Should().Be("01.01.2025");
        result.Comment.Should().Be("Test Comment");
        result.LessonNumber.Should().Be("1");
        result.LessonName.Should().Be("Test Lesson");
        result.LessonTime.Should().Be("09:00-10:30");
        result.LessonDescription.Should().Be("Test Description");
    }

    [Fact]
    public void ToResponse_WithNullLesson_ShouldReturnEmptyResponse()
    {
        // Act
        var result = _mapper.ToResponse(null);

        // Assert
        result.Should().NotBeNull();
        result.Object.Should().BeEmpty();
        result.Date.Should().BeEmpty();
        result.Comment.Should().BeEmpty();
        result.LessonNumber.Should().BeEmpty();
        result.LessonName.Should().BeEmpty();
        result.LessonTime.Should().BeEmpty();
        result.LessonDescription.Should().BeEmpty();
    }

    [Fact]
    public void ToResponseList_WithValidLessons_ShouldMapCorrectly()
    {
        // Arrange
        var lessons = new List<Lesson>
        {
            new Lesson { Object = "Teacher 1", Date = "01.01.2025" },
            new Lesson { Object = "Teacher 2", Date = "02.01.2025" }
        };

        // Act
        var result = _mapper.ToResponseList(lessons);

        // Assert
        result.Should().HaveCount(2);
        result[0].Object.Should().Be("Teacher 1");
        result[1].Object.Should().Be("Teacher 2");
    }

    [Fact]
    public void ToResponseList_WithNullLessons_ShouldReturnEmptyList()
    {
        // Act
        var result = _mapper.ToResponseList(null);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void ToDomain_WithValidResponse_ShouldMapCorrectly()
    {
        // Arrange
        var response = new LessonResponse
        {
            Object = "Test Teacher",
            Date = "01.01.2025",
            Comment = "Test Comment",
            LessonNumber = "1",
            LessonName = "Test Lesson",
            LessonTime = "09:00-10:30",
            LessonDescription = "Test Description"
        };

        // Act
        var result = _mapper.ToDomain(response);

        // Assert
        result.Should().NotBeNull();
        result.Object.Should().Be("Test Teacher");
        result.Date.Should().Be("01.01.2025");
        result.Comment.Should().Be("Test Comment");
        result.LessonNumber.Should().Be("1");
        result.LessonName.Should().Be("Test Lesson");
        result.LessonTime.Should().Be("09:00-10:30");
        result.LessonDescription.Should().Be("Test Description");
    }

    [Fact]
    public void ToDomain_WithNullResponse_ShouldReturnEmptyLesson()
    {
        // Act
        var result = _mapper.ToDomain(null);

        // Assert
        result.Should().NotBeNull();
        result.Object.Should().BeEmpty();
        result.Date.Should().BeEmpty();
        result.Comment.Should().BeEmpty();
        result.LessonNumber.Should().BeEmpty();
        result.LessonName.Should().BeEmpty();
        result.LessonTime.Should().BeEmpty();
        result.LessonDescription.Should().BeEmpty();
    }

    [Fact]
    public void ToDomainList_WithValidResponses_ShouldMapCorrectly()
    {
        // Arrange
        var responses = new List<LessonResponse>
        {
            new LessonResponse { Object = "Teacher 1", Date = "01.01.2025" },
            new LessonResponse { Object = "Teacher 2", Date = "02.01.2025" }
        };

        // Act
        var result = _mapper.ToDomainList(responses);

        // Assert
        result.Should().HaveCount(2);
        result[0].Object.Should().Be("Teacher 1");
        result[1].Object.Should().Be("Teacher 2");
    }

    [Fact]
    public void ToDomainList_WithNullResponses_ShouldReturnEmptyList()
    {
        // Act
        var result = _mapper.ToDomainList(null);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
