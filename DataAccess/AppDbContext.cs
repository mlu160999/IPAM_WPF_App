using IPAM_WPF_App.Models;
using Microsoft.EntityFrameworkCore;

namespace IPAM_WPF_App.DataAccess;

public class AppDbContext : DbContext
{
    public const string ConnectionString = "Data Source=app.db";

    public DbSet<Subnet> Subnets { get; set; } = null!;
    public DbSet<IPAdresse> IPAdressen { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subnet>().ToTable("Subnet");

        modelBuilder.Entity<IPAdresse>(entity =>
        {
            entity.ToTable("IPAdresse", table =>
                table.HasCheckConstraint("CK_IPAdresse_Status", "\"Status\" IN (0, 1, 2)"));

            entity.HasIndex(a => new { a.SubnetId, a.Adresse }).IsUnique();

            entity.HasOne(a => a.Subnet)
                  .WithMany(s => s.IPAdressen)
                  .HasForeignKey(a => a.SubnetId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}