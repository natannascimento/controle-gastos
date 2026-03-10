using ControleGastos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControleGastos.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Person> People { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasMany(p => p.Transactions)
                .WithOne(t => t.Person)
                .HasForeignKey(t => t.PersonId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(p => p.BirthDate)
                .IsRequired()
                .HasColumnType("date");

            entity.HasOne(p => p.User)
                .WithOne(u => u.Person)
                .HasForeignKey<User>(u => u.PersonId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(400);

            entity.Property(t => t.Date)
                .IsRequired();
            entity.Property(t => t.Date)
                .HasColumnType("timestamp without time zone");

            entity.Property(t => t.Value)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            
            entity.Property(t => t.PersonId)
                .IsRequired();
            
            entity.Property(t => t.CategoryId)
                .IsRequired();
        });
        
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Description)
                .IsRequired()
                .HasMaxLength(400);

            entity.Property(c => c.Purpose)
                .IsRequired()
                .HasConversion<int>();

            entity.HasMany(c => c.Transactions)
                .WithOne(t => t.Category)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(320);

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(u => u.AuthProvider)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(u => u.GoogleSub)
                .HasMaxLength(200);

            entity.HasIndex(u => u.GoogleSub)
                .IsUnique()
                .HasFilter("\"GoogleSub\" IS NOT NULL");

            entity.Property(u => u.PasswordHash)
                .HasMaxLength(500);

            entity.HasIndex(u => u.PersonId)
                .IsUnique()
                .HasFilter("\"PersonId\" IS NOT NULL");

            entity.Property(u => u.CreatedAt)
                .IsRequired()
                .HasColumnType("timestamp without time zone");

            entity.Property(u => u.UpdatedAt)
                .IsRequired()
                .HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);

            entity.Property(rt => rt.TokenHash)
                .IsRequired()
                .HasMaxLength(128);

            entity.HasIndex(rt => rt.TokenHash)
                .IsUnique();

            entity.Property(rt => rt.ExpiresAt)
                .IsRequired()
                .HasColumnType("timestamp without time zone");

            entity.Property(rt => rt.CreatedAt)
                .IsRequired()
                .HasColumnType("timestamp without time zone");

            entity.Property(rt => rt.RevokedAt)
                .HasColumnType("timestamp without time zone");

            entity.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
