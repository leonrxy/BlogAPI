using System.Security.Claims;
using BlogAPI.Models;
using Microsoft.AspNetCore.Identity;

public class RoleSyncMiddleware
{
    private readonly RequestDelegate _next;

    public RoleSyncMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, UserManager<Users> userManager)
    {
        if (context.Request.Path.StartsWithSegments("/api/auth/login", StringComparison.OrdinalIgnoreCase))
    {
        await _next(context);
        return;
    }
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(userId);
            var tokenUpdate = DateTime.Parse(context.User.FindFirstValue("lastRoleUpdate"));

            if (user != null && user.LastRoleUpdate > tokenUpdate)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token is no longer valid due to role change.");
                return;
            }
        }

        await _next(context);
    }
}
