using DocumentIntelligence.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentIntelligence.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasMany(user => user.Documents)
            .WithOne(document => document.User)
            .HasForeignKey(document => document.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}