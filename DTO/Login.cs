using System.ComponentModel;

public class LoginDto
{
    [DefaultValue("superadmin@mail.com")]
    public string Username { get; set; }
    [DefaultValue("Admin123!")]
    public string Password { get; set; }
}
