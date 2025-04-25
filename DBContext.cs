using BlogAPI.Helper;
using Microsoft.EntityFrameworkCore;
using BlogAPI.Models;
using Microsoft.AspNetCore.Identity;
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

            // Snake_case untuk tabel ASP.NET Identity
            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("users");
                foreach (var property in entity.Metadata.GetProperties())
                {
                    property.SetColumnName(ConvertHelper.ToSnakeCase(property.Name));
                }
            });

            modelBuilder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable("roles");
                foreach (var property in entity.Metadata.GetProperties())
                {
                    property.SetColumnName(ConvertHelper.ToSnakeCase(property.Name));
                }
            });

            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("user_roles");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("user_claims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("user_logins");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("role_claims");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("user_tokens");

            // Snake_case untuk properti di Identity entitas lainnya
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Ganti nama tabel ke snake_case
                entityType.SetTableName(ConvertHelper.ToSnakeCase(entityType.GetTableName()));

                // Ganti nama kolom ke snake_case
                foreach (var property in entityType.GetProperties())
                {
                    property.SetColumnName(ConvertHelper.ToSnakeCase(property.Name));
                }

                // Ganti nama foreign key ke snake_case
                foreach (var fk in entityType.GetForeignKeys())
                {
                    foreach (var prop in fk.Properties)
                    {
                        prop.SetColumnName(ConvertHelper.ToSnakeCase(prop.Name));
                    }
                }

                // Ganti nama key/index ke snake_case
                foreach (var key in entityType.GetKeys())
                {
                    key.SetName(ConvertHelper.ToSnakeCase(key.GetName()));
                }

                foreach (var index in entityType.GetIndexes())
                {
                    index.SetDatabaseName(ConvertHelper.ToSnakeCase(index.GetDatabaseName()));
                }
            }
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Posts> Posts { get; set; } = null!;
    }
}