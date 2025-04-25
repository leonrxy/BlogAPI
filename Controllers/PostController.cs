using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogAPI.DBContext;
using BlogAPI.Models;
using System.Text.Json.Serialization;
using BlogAPI.Helper;
using BlogAPI.Models.DTO;
using Microsoft.AspNetCore.Authorization;

namespace TefaTodoList.Controllers;

[Authorize]
[Route("api/posts")]
[ApiController]
public class PostsController : ControllerBase
{
    private readonly AppDbContext db;

    public PostsController(AppDbContext db)
    {
        this.db = db;
    }

    [HttpGet]
    public async Task<ActionResult> GetPosts([FromQuery] PaginationParams param)
    {
        var postsQuery = db.Posts.OrderByDescending(t => t.UpdatedAt).AsQueryable();
        var result = await PaginationHelper.PaginateAsync(postsQuery, param);
        return ApiResponse.Success(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Posts>> GetPosts(int id)
    {
        var Posts = await db.Posts.FindAsync(id);
        if (Posts == null)
        {
            return ApiResponse.NotFound("Post Not Found");
        }

        return ApiResponse.Success(Posts);
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<Posts>> GetPostBySlug(string slug)
    {
        var post = await db.Posts
            .FirstOrDefaultAsync(p => p.Slug == slug);
        if (post == null)
        {
            return ApiResponse.NotFound("Post Not Found");
        }

        return ApiResponse.Success(post);
    }

    [HttpPut("{id}")]
    // [Authorize(Roles = "superadmin, admin")]
    public async Task<IActionResult> PutPosts(int id, [FromForm] PostsDTO PostsDTO)
    {
        var existingPosts = await db.Posts.FindAsync(id);

        if (existingPosts == null)
        {
            return ApiResponse.NotFound("Post Not Found");
        }

        existingPosts.Title = PostsDTO.Title;
        existingPosts.Slug = PostsDTO.Slug;
        existingPosts.Content = PostsDTO.Content;
        existingPosts.UpdatedAt = DateTime.Now;

        if (PostsDTO.CoverImage != null)
        {
            // Tentukan folder tempat file disimpan
            var uploadsFolder = Path.Combine("wwwroot", "uploads", "posts");

            // Cek apakah folder ada, jika belum buat
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Hapus cover image lama (jika ada)
            if (!string.IsNullOrEmpty(existingPosts.CoverImagePath))
            {
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingPosts.CoverImagePath);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath); // Hapus file lama
                }
            }

            // Buat nama file baru yang unik
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(PostsDTO.CoverImage.FileName);

            // Tentukan path untuk menyimpan file baru
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Simpan file baru
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await PostsDTO.CoverImage.CopyToAsync(stream);
            }

            // Simpan path file baru dalam database
            existingPosts.CoverImagePath = Path.Combine("uploads", "posts", uniqueFileName).Replace("\\", "/");
        }

        try
        {
            await db.SaveChangesAsync();
            return ApiResponse.Success(null, "update");
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PostsExists(id))
            {
                return ApiResponse.NotFound("Post Not Found");
            }
            else
            {
                return StatusCode(500, ApiResponse.Error("An error occurred while updating the Post"));
            }
        }
    }

    [HttpPost]
    [Authorize(Roles = "superadmin, admin")]
    public async Task<ActionResult<Posts>> PostPosts([FromForm] PostsDTO PostsDTO)
    {
        var existingPost = await db.Posts
            .FirstOrDefaultAsync(p => p.Slug == PostsDTO.Slug);
        if (existingPost != null)
        {
            return ApiResponse.Error("Validation failed", ApiResponse.Errors(
                ("Slug", "Slug must be unique")
            ));
        }

        string? savedFileName = null;
        if (PostsDTO.CoverImage != null)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "posts");
            // var uploadsFolder = "/home/leonardusreka/docker/uploads-blog";
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(PostsDTO.CoverImage.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await PostsDTO.CoverImage.CopyToAsync(stream);
            }

            var relativePath = Path.Combine("uploads", "posts", uniqueFileName).Replace("\\", "/");
            savedFileName = relativePath;
        }

        var post = new Posts
        {
            Title = PostsDTO.Title,
            Slug = PostsDTO.Slug,
            Content = PostsDTO.Content,
            CreatedAt = DateTime.Now,

            CoverImagePath = savedFileName
        };
        db.Posts.Add(post);
        await db.SaveChangesAsync();
        return ApiResponse.Success(post, "insert");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePosts(int id)
    {
        var Posts = await db.Posts.FindAsync(id);
        if (Posts == null)
        {
            return ApiResponse.NotFound("Post Not Found");
        }

        db.Posts.Remove(Posts);
        await db.SaveChangesAsync();
        return ApiResponse.Success(null, "delete");
    }

    private bool PostsExists(int id)
    {
        return db.Posts.Any(e => e.Id == id);
    }
}