using BackendEgitimiYeni.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendEgitimiYeni.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<Document> Documents { get; set; }

    public DbSet<DocumentChunk> DocumentChunks { get; set; }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        if (Database.IsNpgsql())
        {
            modelBuilder.HasPostgresExtension("vector");

            modelBuilder.Entity<DocumentChunk>()
                .Property(x => x.Embedding)
                .HasColumnType("vector(768)");
        }
        else
        {
            modelBuilder.Entity<DocumentChunk>()
                .Ignore(x => x.Embedding);
        }

        modelBuilder.Entity<Document>()
            .HasMany(x => x.Chunks)
            .WithOne(x => x.Document)
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}