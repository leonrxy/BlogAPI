using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;

public enum UserRole
{
    [EnumMember(Value = "user")] user,
    [EnumMember(Value = "admin")] admin,
    [EnumMember(Value = "superadmin")] superadmin
}

public class UsersDTO
{
    [FromForm] public string email { get; set; }
    [FromForm] public string password { get; set; }
    [FromForm] public string full_name { get; set; }

    [FromForm]
    [BindProperty(Name = "role")]
    public UserRole role { get; set; }
}