using FluentAssertions;
using Schedule.Web.Models;
using Schedule.Web.Models.Enums;
using Xunit;

namespace Schedule.UnitTests.Models;

public sealed class LessonTests
{
    [Fact]
    public void Lesson_WithValidData_ShouldSetProperties()
    {
        // Arrange
        var lesson = new Lesson
        {
            Object = "Test Object",
            Date = "01.01.2025",
            Comment = "Test Comment",
            LessonNumber = "1",
            LessonName = "Test Lesson",
            LessonTime = "09:00-10:30",
            LessonDescription = "Test Description"
        };

        // Assert
        lesson.Object.Should().Be("Test Object");
        lesson.Date.Should().Be("01.01.2025");
        lesson.Comment.Should().Be("Test Comment");
        lesson.LessonNumber.Should().Be("1");
        lesson.LessonName.Should().Be("Test Lesson");
        lesson.LessonTime.Should().Be("09:00-10:30");
        lesson.LessonDescription.Should().Be("Test Description");
    }

    [Fact]
    public void Lesson_WithDefaultData_ShouldSetPropertiesToEmptyString()
    {
        // Arrange
        var lesson = new Lesson();

        // Assert
        lesson.Object.Should().BeEmpty();
        lesson.Date.Should().BeEmpty();
        lesson.Comment.Should().BeEmpty();
        lesson.LessonNumber.Should().BeEmpty();
        lesson.LessonName.Should().BeEmpty();
        lesson.LessonTime.Should().BeEmpty();
        lesson.LessonDescription.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Програмування (Лаб) КІ-25-1", LessonType.Laboratory)]
    [InlineData("Програмування (Пр) КІ-25-1", LessonType.Practical)]
    [InlineData("Програмування (Зал) КІ-25-1", LessonType.Credit)]
    [InlineData("Програмування (КЕкз) КІ-25-1", LessonType.ExamConsultation)]
    [InlineData("Програмування (Екз) КІ-25-1", LessonType.Exam)]
    [InlineData("Програмування (Л) КІ-25-1", LessonType.Lecture)]
    [InlineData("", default(LessonType))]
    [InlineData(null, default(LessonType))]
    public void LessonType_WithDifferentDescriptions_ShouldReturnCorrectType(string description, LessonType expectedType)
    {
        // Arrange
        var lesson = new Lesson { LessonDescription = description };

        // Act & Assert
        lesson.LessonType.Should().Be(expectedType);
    }

    [Theory]
    [InlineData("мз-25-1", HourType.Hourly)]
    [InlineData("з-25-1", HourType.PartTime)]
    [InlineData("", HourType.FullTime)]
    [InlineData(null, HourType.FullTime)]
    public void HourType_WithDifferentDescriptions_ShouldReturnCorrectType(string description, HourType expectedType)
    {
        // Arrange
        var lesson = new Lesson { LessonDescription = description };

        // Act & Assert
        lesson.HourType.Should().Be(expectedType);
    }

    [Theory]
    [InlineData("півпара", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void HalfLesson_WithDifferentDescriptions_ShouldReturnCorrectValue(string description, bool expected)
    {
        // Arrange
        var lesson = new Lesson { LessonDescription = description };

        // Act & Assert
        lesson.HalfLesson.Should().Be(expected);
    }

    [Theory]
    [InlineData("Увага! Заміна", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void Substitution_WithDifferentDescriptions_ShouldReturnCorrectValue(string description, bool expected)
    {
        // Arrange
        var lesson = new Lesson { LessonDescription = description };

        // Act & Assert
        lesson.Substitution.Should().Be(expected);
    }

    [Theory]
    [InlineData("Увага! Заміна", 0)]
    [InlineData("півпара", 1)]
    [InlineData("", 2)]
    [InlineData(null, 2)]
    public void LessonHours_WithDifferentDescriptions_ShouldReturnCorrectHours(string description, int expectedHours)
    {
        // Arrange
        var lesson = new Lesson { LessonDescription = description };

        // Act & Assert
        lesson.LessonHours.Should().Be(expectedHours);
    }
}
