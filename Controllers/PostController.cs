using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogAPI.DBContext;
using BlogAPI.Models;
using System.Text.Json.Serialization;
using BlogAPI.Helper;
using BlogAPI.Models.DTO;

namespace TefaTodoList.Controllers;

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
       var postsQuery =  db.Posts.OrderByDescending(t => t.UpdatedAt).AsQueryable();
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
       return ApiResponse.Success( post);
   }
   [HttpPut("{id}")]
   public async Task<IActionResult> PutPosts(int id, PostsDTO PostsDTO)
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
   public async Task<ActionResult<Posts>> PostPosts(PostsDTO PostsDTO)
   {
        var existingPost = await db.Posts
        .FirstOrDefaultAsync(p => p.Slug == PostsDTO.Slug);
        if (existingPost != null)
        {
            return ApiResponse.Error("Validation failed", ApiResponse.Errors(
                ("Slug", "Slug must be unique")
            ));
        }
       var Posts = new Posts
       {
           Title = PostsDTO.Title,
           Slug = PostsDTO.Slug,
           Content = PostsDTO.Content,
           CreatedAt = DateTime.Now
       };
       db.Posts.Add(Posts);
       await db.SaveChangesAsync();
       return ApiResponse.Success(Posts, "insert");
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