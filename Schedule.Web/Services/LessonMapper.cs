using Schedule.Web.Models;
using Schedule.Web.Models.Api;

namespace Schedule.Web.Services;

public sealed class LessonMapper : ILessonMapper
{
    public LessonResponse ToResponse(Lesson lesson)
    {
        if (lesson is null)
            return new LessonResponse();

        return new LessonResponse
        {
            Object = lesson.Object,
            Date = lesson.Date,
            Comment = lesson.Comment,
            LessonNumber = lesson.LessonNumber,
            LessonName = lesson.LessonName,
            LessonTime = lesson.LessonTime,
            LessonDescription = lesson.LessonDescription
        };
    }

    public List<LessonResponse> ToResponseList(IEnumerable<Lesson> lessons)
    {
        if (lessons is null)
            return new List<LessonResponse>();

        return lessons.Select(ToResponse).ToList();
    }

    public Lesson ToDomain(LessonResponse response)
    {
        if (response is null)
            return new Lesson();

        return new Lesson
        {
            Object = response.Object,
            Date = response.Date,
            Comment = response.Comment,
            LessonNumber = response.LessonNumber,
            LessonName = response.LessonName,
            LessonTime = response.LessonTime,
            LessonDescription = response.LessonDescription
        };
    }

    public List<Lesson> ToDomainList(IEnumerable<LessonResponse> responses)
    {
        if (responses is null)
            return new List<Lesson>();

        return responses.Select(ToDomain).ToList();
    }
}
