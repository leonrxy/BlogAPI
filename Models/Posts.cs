using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Models
{
    [Index(nameof(Slug), IsUnique = true)]
    public class Posts
    {
        public int Id { get; set; }
        [Required]
        public string Slug { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}