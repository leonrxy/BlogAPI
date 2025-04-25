 using Microsoft.AspNetCore.Mvc;

 public class PostsDTO
    {
        [FromForm]
        public string Slug { get; set; }
        [FromForm]
        public string Title { get; set; }
        [FromForm]
        public string Content { get; set; }
        [FromForm]
        public IFormFile? CoverImage { get; set; }
    }