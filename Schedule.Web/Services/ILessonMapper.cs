using Schedule.Web.Models;
using Schedule.Web.Models.Api;

namespace Schedule.Web.Services;

public interface ILessonMapper
{
    LessonResponse ToResponse(Lesson lesson);
    List<LessonResponse> ToResponseList(IEnumerable<Lesson> lessons);
    Lesson ToDomain(LessonResponse response);
    List<Lesson> ToDomainList(IEnumerable<LessonResponse> responses);
}
