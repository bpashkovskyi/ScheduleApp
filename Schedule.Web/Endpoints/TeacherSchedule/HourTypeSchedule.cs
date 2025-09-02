using System.Text.Json.Serialization;

namespace Schedule.Web.Endpoints.TeacherSchedule;

public sealed class HourTypeSchedule
{
    [JsonPropertyName("lectures")]
    public List<LessonItem> Lectures { get; set; } = new();

    [JsonPropertyName("practice")]
    public List<LessonItem> Practice { get; set; } = new();

    [JsonPropertyName("labs")]
    public List<LessonItem> Labs { get; set; } = new();

    [JsonPropertyName("credits")]
    public List<LessonItem> Credits { get; set; } = new();

    [JsonPropertyName("exam_consultations")]
    public List<LessonItem> ExamConsultations { get; set; } = new();

    [JsonPropertyName("exams")]
    public List<LessonItem> Exams { get; set; } = new();
}