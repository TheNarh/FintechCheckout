using FintechCheckout.Models;
using Microsoft.EntityFrameworkCore;

namespace FintechCheckout.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Transaction>()
        .HasIndex(t => t.Reference)
        .IsUnique();

    modelBuilder.Entity<Transaction>()
        .Property(t => t.Status)
        .HasConversion<string>();
}
}

