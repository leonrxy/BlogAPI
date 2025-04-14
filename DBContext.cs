using Microsoft.EntityFrameworkCore;
using BlogAPI.Models;

namespace BlogAPI.DBContext
{
    public class AppDbContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
