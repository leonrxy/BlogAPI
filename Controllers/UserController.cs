using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogAPI.DBContext;
using BlogAPI.Models;
using System.Text.Json.Serialization;
using BlogAPI.Helper;
using BlogAPI.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace TefaTodoList.Controllers;

// [Authorize]
[Route("api/users")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly AppDbContext db;
    private readonly UserManager<Users> _userManager;

    public UsersController(UserManager<Users> userManager, AppDbContext db)
    {
        this.db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult> GetUsers([FromQuery] PaginationParams param)
    {
        var UsersQuery = db.Users.OrderByDescending(t => t.UpdatedAt).AsQueryable();
        var result = await PaginationHelper.PaginateAsync(UsersQuery, param);
        return ApiResponse.Success(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Users>> GetUsers(string id)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null)
        {
            return ApiResponse.NotFound("User Not Found");
        }
        
        var result = new 
        {
            user.Id,
            user.UserName,
            user.FullName,
            user.Email,
            user.LastRoleUpdate,
            user.CreatedAt,
            user.UpdatedAt,
            Roles = await _userManager.GetRolesAsync(user)
        };

        return ApiResponse.Success(result);
    }

    [HttpPost()]
    // [Authorize(Roles = "superadmin, admin")]
    public async Task<ActionResult<Users>> PostUsers([FromForm] UsersDTO UsersDTO)
    {
        var existingUser = await db.Users
            .FirstOrDefaultAsync(p => p.UserName == UsersDTO.email);
        if (existingUser != null)
        {
            return ApiResponse.Error("Validation failed", ApiResponse.Errors(
                ("Email", "Email is Already Registered")
            ));
        }

        var user = new Users
        {
            UserName = UsersDTO.email,
            Email = UsersDTO.email,
            FullName = UsersDTO.full_name,
        };
        var result = await _userManager.CreateAsync(user, UsersDTO.password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, UsersDTO.role.ToString());
            return ApiResponse.Success(user, "insert");
        }
        else
        {
            return BadRequest(result.Errors);
        }
    }

    [HttpPut("{id}")]
    // [Authorize(Roles = "superadmin, admin")]
    public async Task<IActionResult> PutUsers(string id, [FromForm] UsersDTO UsersDTO)
    {
        var existingUsers = await db.Users.FindAsync(id);

        if (existingUsers == null)
        {
            return ApiResponse.NotFound("User Not Found");
        }

        existingUsers.UserName = UsersDTO.email;
        existingUsers.Email = UsersDTO.email;
        existingUsers.FullName = UsersDTO.full_name;
        existingUsers.UpdatedAt = DateTime.Now;

        if (UsersDTO.role != null)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                // Menghapus role lama
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                // Menambahkan role baru
                await _userManager.AddToRoleAsync(user, UsersDTO.role.ToString());
            }
            else
            {
                return NotFound("User not found");
            }
        }


        try
        {
            await db.SaveChangesAsync();
            return ApiResponse.Success(null, "update");
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UsersExists(id))
            {
                return ApiResponse.NotFound("User Not Found");
            }
            else
            {
                return StatusCode(500, ApiResponse.Error("An error occurred while updating the User"));
            }
        }
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUsers(string id)
    {
        var Users = await db.Users.FindAsync(id);
        if (Users == null)
        {
            return ApiResponse.NotFound("User Not Found");
        }

        db.Users.Remove(Users);
        await db.SaveChangesAsync();
        return ApiResponse.Success(null, "delete");
    }

    private bool UsersExists(string id)
    {
        return db.Users.Any(e => e.Id == id);
    }
}