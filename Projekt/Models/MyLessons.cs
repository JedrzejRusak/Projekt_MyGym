using Microsoft.AspNetCore.Identity;

namespace Projekt.Models
{
    public class MyLessons
    {
        public int Id { get; set; }
        public int? LessonsId { get; set; }
        public virtual Lesson? Lessons { get; set; }
        public string UserId { get; set; }
        public virtual IdentityUser? User { get; set; }

    }
}
