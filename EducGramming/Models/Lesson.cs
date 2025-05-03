namespace EducGramming.Models
{
    public class Lesson
    {
        public int Number { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string VideoUrl { get; set; }
        public bool IsCompleted { get; set; }
        public TimeSpan Duration { get; set; }
    }
} 