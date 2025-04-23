using Microsoft.EntityFrameworkCore;
using BlogAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BlogAPI.DBContext
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Posts>()
                .HasIndex(u => u.Slug)
                .IsUnique();
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Posts> Posts { get; set; } = null!;
    }
}
