using Microsoft.AspNetCore.Identity;

namespace Projekt.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateTimeStarted { get; set; }
        public DateTime DateTimeEnded { get; set; }
        public int AmountOfSpace { get; set; }
        public int AmountOfOccupiedSpace { get; set; } = 0;
    }
}
