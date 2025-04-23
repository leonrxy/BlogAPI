using Microsoft.AspNetCore.Identity;

namespace BlogAPI.Models
{
    public class Users: IdentityUser
    {
        public string FullName { get; set; }
        public DateTime LastRoleUpdate { get; set; } = DateTime.UtcNow;
    }
}