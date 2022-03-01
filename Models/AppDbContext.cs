using AppMVC.Models.Blog;
using AppMVC.Models.Product;
using AppMVC.Models.Contacts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AppMVC.Models
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Custome to remove prefix "AspNet"
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Slug)
                    .IsUnique();
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasIndex(p => p.Slug)
                    .IsUnique();
            });

            // Set primary key is PostId and CategoryId
            modelBuilder.Entity<PostCategory>(entity =>
            {
                entity.HasKey(p => new { p.PostId, p.CategoryId });
            });

            modelBuilder.Entity<PCategory>(entity =>
            {
                entity.HasIndex(c => c.Slug)
                    .IsUnique();
            });

            modelBuilder.Entity<ProductModel>(entity =>
            {
                entity.HasIndex(p => p.Slug)
                    .IsUnique();
            });

            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.HasKey(p => new { p.ProductId, p.CategoryId });
            });
        }

        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostCategory> PostCategories { get; set; }

        public DbSet<PCategory> PCategories { get; set; }
        public DbSet<ProductModel> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }

    }
}