using Schedule.Web.Models.Enums;

namespace Schedule.Web.Models
{
    public class Lesson
    {
        public string Object { get; set; } = string.Empty;
        
        public string Date { get; set; } = string.Empty;
        
        public string Comment { get; set; } = string.Empty;
        
        public string LessonNumber { get; set; } = string.Empty;
        
        public string LessonName { get; set; } = string.Empty;
        
        public string LessonTime { get; set; } = string.Empty;
        
        public string LessonDescription { get; set; } = string.Empty;

        // Computed property for LessonType
        public LessonType LessonType
        {
            get
            {
                if (string.IsNullOrWhiteSpace(LessonDescription))
                {
                    return default;
                }

                if (LessonDescription.Contains("(Лаб)", StringComparison.OrdinalIgnoreCase))
                {
                    return LessonType.Laboratory;
                }

                if (LessonDescription.Contains("(Пр)", StringComparison.OrdinalIgnoreCase))
                {
                    return LessonType.Practical;
                }

                if (LessonDescription.Contains("(Зал)", StringComparison.OrdinalIgnoreCase))
                {
                    return LessonType.Credit;
                }

                if (LessonDescription.Contains("(КЕкз)", StringComparison.OrdinalIgnoreCase))
                {
                    return LessonType.ExamConsultation;
                }

                if (LessonDescription.Contains("(Екз)", StringComparison.OrdinalIgnoreCase))
                {
                    return LessonType.Exam;
                }

                if (LessonDescription.Contains("(Л)", StringComparison.OrdinalIgnoreCase))
                {
                    return LessonType.Lecture;
                }

                return default; // Return default instead of throwing exception
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

                if (LessonDescription.Contains("мз-", StringComparison.OrdinalIgnoreCase))
                {
                    return HourType.Hourly;
                }

                if (LessonDescription.Contains("з-", StringComparison.OrdinalIgnoreCase))
                {
                    return HourType.PartTime;
                }

                return HourType.FullTime;
            }
        }

        public bool HalfLesson => !string.IsNullOrWhiteSpace(LessonDescription) && 
                                 LessonDescription.Contains("півпара", StringComparison.OrdinalIgnoreCase);

        public bool Substitution => !string.IsNullOrWhiteSpace(LessonDescription) && 
                                  LessonDescription.Contains("Увага! Заміна", StringComparison.OrdinalIgnoreCase);

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
