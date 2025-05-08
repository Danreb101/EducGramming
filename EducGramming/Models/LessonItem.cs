using System;

namespace EducGramming.Models
{
    public class LessonItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string VideoUrl { get; set; }
        public string Content { get; set; }
        public string Language { get; set; } // "C#" or "Java"
        public string Type { get; set; } // "Lesson", "Video", or "Quiz"
        public bool IsCompleted { get; set; }
    }
} 