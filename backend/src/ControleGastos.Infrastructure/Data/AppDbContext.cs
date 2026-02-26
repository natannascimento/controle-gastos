using ControleGastos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControleGastos.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Person> People { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Category> Categories { get; set; }
    
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
    }
}
