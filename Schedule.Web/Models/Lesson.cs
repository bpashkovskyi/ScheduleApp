using ScheduleApp.Models.Enums;

namespace ScheduleApp.Models
{
    public class Lesson
    {
        public string Object { get; set; }
        public string Date { get; set; }
        public string Comment { get; set; }
        public string LessonNumber { get; set; }
        public string LessonName { get; set; }
        public string LessonTime { get; set; }
        public string LessonDescription { get; set; }

        // Computed property for LessonType
        public LessonType LessonType
        {
            get
            {
                if (string.IsNullOrWhiteSpace(LessonDescription))
                {
                    return default;
                }

                if (LessonDescription.Contains("(Лаб)"))
                {
                    return LessonType.Laboratory;
                }

                if (LessonDescription.Contains("(Пр)"))
                {
                    return LessonType.Practical;
                }

                if (LessonDescription.Contains("(Зал)"))
                {
                    return LessonType.Credit;
                }

                if (LessonDescription.Contains("(КЕкз)"))
                {
                    return LessonType.ExamConsultation;
                }

                if (LessonDescription.Contains("(Екз)"))
                {
                    return LessonType.Exam;
                }

                if (LessonDescription.Contains("(Л)"))
                {
                    return LessonType.Lecture;
                }

                throw new InvalidOperationException(
                    $"Lesson Type is not recognized from description: {LessonDescription}");
            }
        }

        // Computed property for HourType
        public HourType HourType
        {
            get
            {
                if (string.IsNullOrWhiteSpace(LessonDescription))
                {
                    return HourType.FullTime;
                }

                if (LessonDescription.Contains("мз-"))
                {
                    return HourType.Hourly;
                }

                if (LessonDescription.Contains("з-"))
                {
                    return HourType.PartTime;
                }

                return HourType.FullTime;
            }
        }

        public bool HalfLesson => LessonDescription.Contains("півпара");

        public bool Substitution => LessonDescription.Contains("Увага! Заміна"); // todo: double check

        public int LessonHours
        {
            get
            {
                if (Substitution)
                {
                    return 0;
                }

                if (HalfLesson)
                {
                    return 1;
                }

                return 2;
            }
        }

    }
}
