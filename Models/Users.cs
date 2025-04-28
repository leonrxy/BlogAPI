using Microsoft.AspNetCore.Identity;

namespace BlogAPI.Models
{
    public class Users: IdentityUser
    {
        public string FullName { get; set; }
        public DateTime LastRoleUpdate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}