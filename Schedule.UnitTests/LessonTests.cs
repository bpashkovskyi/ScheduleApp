using FluentAssertions;
using ScheduleApp.Models;
using ScheduleApp.Models.Enums;
using Xunit;

namespace Schedule.UnitTests;

public sealed class LessonTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldSetProperties()
    {
        // Arrange
        var lesson = new Lesson
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
        lesson.Object.Should().Be("Test Object");
        lesson.Date.Should().Be("2025-01-15");
        lesson.Comment.Should().Be("Test Comment");
        lesson.LessonNumber.Should().Be("1");
        lesson.LessonName.Should().Be("Test Lesson");
        lesson.LessonTime.Should().Be("09:00-10:30");
        lesson.LessonDescription.Should().Be("Test Description");
    }

    [Fact]
    public void Constructor_WithNullData_ShouldSetPropertiesToNull()
    {
        // Arrange
        var lesson = new Lesson();

        // Assert
        lesson.Object.Should().BeNull();
        lesson.Date.Should().BeNull();
        lesson.Comment.Should().BeNull();
        lesson.LessonNumber.Should().BeNull();
        lesson.LessonName.Should().BeNull();
        lesson.LessonTime.Should().BeNull();
        lesson.LessonDescription.Should().BeNull();
    }

    [Theory]
    [InlineData("(Л)", LessonType.Lecture)]
    [InlineData("(Лаб)", LessonType.Laboratory)]
    [InlineData("(Пр)", LessonType.Practical)]
    [InlineData("(Зал)", LessonType.Credit)]
    [InlineData("(КЕкз)", LessonType.ExamConsultation)]
    [InlineData("(Екз)", LessonType.Exam)]
    public void LessonType_WithValidTypeInDescription_ShouldReturnCorrectType(string typeMarker, LessonType expectedType)
    {
        // Arrange
        var lesson = new Lesson
        {
            LessonDescription = $"Test Lesson {typeMarker} Description"
        };

        // Act
        var result = lesson.LessonType;

        // Assert
        result.Should().Be(expectedType);
    }

    [Fact]
    public void LessonType_WithNoTypeInDescription_ShouldReturnDefault()
    {
        // Arrange
        var lesson = new Lesson
        {
            LessonDescription = "Test Lesson Description"
        };

        // Act
        var result = lesson.LessonType;

        // Assert
        result.Should().Be(default(LessonType));
    }

    [Fact]
    public void LessonType_WithNullDescription_ShouldReturnDefault()
    {
        // Arrange
        var lesson = new Lesson
        {
            LessonDescription = null
        };

        // Act
        var result = lesson.LessonType;

        // Assert
        result.Should().Be(default(LessonType));
    }

    [Fact]
    public void LessonType_WithEmptyDescription_ShouldReturnDefault()
    {
        // Arrange
        var lesson = new Lesson
        {
            LessonDescription = ""
        };

        // Act
        var result = lesson.LessonType;

        // Assert
        result.Should().Be(default(LessonType));
    }

    [Fact]
    public void LessonType_WithWhitespaceDescription_ShouldReturnDefault()
    {
        // Arrange
        var lesson = new Lesson
        {
            LessonDescription = "   "
        };

        // Act
        var result = lesson.LessonType;

        // Assert
        result.Should().Be(default(LessonType));
    }

    [Theory]
    [InlineData("мз-", HourType.Hourly)]
    [InlineData("з-", HourType.PartTime)]
    public void HourType_WithValidTypeInDescription_ShouldReturnCorrectType(string typeMarker, HourType expectedType)
    {
        // Arrange
        var lesson = new Lesson
        {
            LessonDescription = $"Test Lesson {typeMarker} Description"
        };

        // Act
        var result = lesson.HourType;

        // Assert
        result.Should().Be(expectedType);
    }

    [Fact]
    public void HourType_WithNoTypeInDescription_ShouldReturnFullTime()
    {
        // Arrange
        var lesson = new Lesson
        {
            LessonDescription = "Test Lesson Description"
        };

        // Act
        var result = lesson.HourType;

        // Assert
        result.Should().Be(HourType.FullTime);
    }

    [Fact]
    public void HourType_WithNullDescription_ShouldReturnFullTime()
    {
        // Arrange
        var lesson = new Lesson
        {
            LessonDescription = null
        };

        // Act
        var result = lesson.HourType;

        // Assert
        result.Should().Be(HourType.FullTime);
    }

    [Fact]
    public void HourType_WithEmptyDescription_ShouldReturnFullTime()
    {
        // Arrange
        var lesson = new Lesson
        {
            LessonDescription = ""
        };

        // Act
        var result = lesson.HourType;

        // Assert
        result.Should().Be(HourType.FullTime);
    }

    [Fact]
    public void HourType_WithWhitespaceDescription_ShouldReturnFullTime()
    {
        // Arrange
        var lesson = new Lesson
        {
            LessonDescription = "   "
        };

        // Act
        var result = lesson.HourType;

        // Assert
        result.Should().Be(HourType.FullTime);
    }

    [Fact]
    public void HourType_WithBothHourlyAndPartTimeMarkers_ShouldPrioritizeHourly()
    {
        // Arrange
        var lesson = new Lesson
        {
            LessonDescription = "Test Lesson (мз-) (з-) Description"
        };

        // Act
        var result = lesson.HourType;

        // Assert
        result.Should().Be(HourType.Hourly);
    }

    [Fact]
    public void HourType_WithPartTimeButNotHourly_ShouldReturnPartTime()
    {
        // Arrange
        var lesson = new Lesson
        {
            LessonDescription = "Test Lesson (з-) Description"
        };

        // Act
        var result = lesson.HourType;

        // Assert
        result.Should().Be(HourType.PartTime);
    }

    [Fact]
    public void HourType_WithNeitherMarker_ShouldReturnFullTime()
    {
        // Arrange
        var lesson = new Lesson
        {
            LessonDescription = "Test Lesson Description"
        };

        // Act
        var result = lesson.HourType;

        // Assert
        result.Should().Be(HourType.FullTime);
    }

    [Fact]
    public void LessonType_WithMultipleTypeMarkers_ShouldReturnFirstMatch()
    {
        // Arrange
        var lesson = new Lesson
        {
            LessonDescription = "Test Lesson (Лаб) (Л) Description"
        };

        // Act
        var result = lesson.LessonType;

        // Assert
        result.Should().Be(LessonType.Laboratory);
    }

    [Fact]
    public void LessonType_WithComplexDescription_ShouldExtractCorrectType()
    {
        // Arrange
        var lesson = new Lesson
        {
            LessonDescription = "Системний аналіз інформаційних процесів (Л) ІС-22-1  1.А14.ауд. 03.03.2025 12:50-14:10"
        };

        // Act
        var result = lesson.LessonType;

        // Assert
        result.Should().Be(LessonType.Lecture);
    }

    [Fact]
    public void HourType_WithComplexDescription_ShouldExtractCorrectType()
    {
        // Arrange
        var lesson = new Lesson
        {
            LessonDescription = "Системний аналіз інформаційних процесів (Л) ІС-22-1 мз-1.А14.ауд. 03.03.2025 12:50-14:10"
        };

        // Act
        var result = lesson.HourType;

        // Assert
        result.Should().Be(HourType.Hourly);
    }

    [Fact]
    public void Properties_CanBeModified_ShouldUpdateValues()
    {
        // Arrange
        var lesson = new Lesson
        {
            Object = "Initial Object",
            Date = "2025-01-15"
        };

        // Act
        lesson.Object = "Updated Object";
        lesson.Date = "2025-01-16";

        // Assert
        lesson.Object.Should().Be("Updated Object");
        lesson.Date.Should().Be("2025-01-16");
    }

    [Fact]
    public void ComputedProperties_ShouldUpdateWhenDescriptionChanges()
    {
        // Arrange
        var lesson = new Lesson
        {
            LessonDescription = "Test Lesson (Л) Description"
        };

        // Act & Assert - Initial state
        lesson.LessonType.Should().Be(LessonType.Lecture);
        lesson.HourType.Should().Be(HourType.FullTime);

        // Act - Change description
        lesson.LessonDescription = "Test Lesson (Лаб) (мз-) Description";

        // Assert - Updated state
        lesson.LessonType.Should().Be(LessonType.Laboratory);
        lesson.HourType.Should().Be(HourType.Hourly);
    }

    [Fact]
    public void EdgeCase_WithVeryLongDescription_ShouldHandleCorrectly()
    {
        // Arrange
        var longDescription = new string('A', 10000) + "(Л)";
        var lesson = new Lesson
        {
            LessonDescription = longDescription
        };

        // Act
        var lessonType = lesson.LessonType;
        var hourType = lesson.HourType;

        // Assert
        lessonType.Should().Be(LessonType.Lecture);
        hourType.Should().Be(HourType.FullTime);
    }

    [Fact]
    public void EdgeCase_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var lesson = new Lesson
        {
            LessonDescription = "Test Lesson (Л) !@#$%^&*()_+-=[]{}|;':\",./<>? Description"
        };

        // Act
        var result = lesson.LessonType;

        // Assert
        result.Should().Be(LessonType.Lecture);
    }

    [Fact]
    public void EdgeCase_WithUnicodeCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var lesson = new Lesson
        {
            LessonDescription = "Тест урок (Л) українською мовою"
        };

        // Act
        var result = lesson.LessonType;

        // Assert
        result.Should().Be(LessonType.Lecture);
    }
}
