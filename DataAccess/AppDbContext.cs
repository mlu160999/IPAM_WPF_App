using IPAM_WPF_App.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.Json;

namespace IPAM_WPF_App.DataAccess;

public class AppDbContext : DbContext
{
    public static string ConnectionString => ReadConnectionString();

    private static string ReadConnectionString()
    {
        string path = Path.Combine(
            AppContext.BaseDirectory, "appsettings.json");

        if (!File.Exists(path))
        {
            throw new InvalidOperationException(
                "Lokale appsettings.json fehlt. Vorlage kopieren " +
                "und ConnectionStrings:Ipam eintragen.");
        }

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("ConnectionStrings", out var strings)
            || strings.ValueKind != JsonValueKind.Object
            || !strings.TryGetProperty("Ipam", out var value)
            || value.ValueKind != JsonValueKind.String)
        {
            throw new InvalidOperationException(
                "ConnectionStrings:Ipam fehlt oder ist kein Text.");
        }

        string connectionString = value.GetString()!;

        if (string.IsNullOrWhiteSpace(connectionString)
            || connectionString.Contains("<LOKALER_DB_PFAD>"))
        {
            throw new InvalidOperationException(
                "Lokalen SQLite-Pfad in appsettings.json eintragen.");
        }

        return connectionString;
    }

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
            // Die erlaubten Werte werden aus dem Enum IPStatus gelesen statt als 0, 1, 2 fest codiert.
            var erlaubteStatusWerte = string.Join(", ", Enum.GetValues<IPStatus>().Select(s => (int)s));
            entity.ToTable("IPAdresse", table =>
                table.HasCheckConstraint("CK_IPAdresse_Status", $"\"Status\" IN ({erlaubteStatusWerte})"));

            entity.HasIndex(a => new { a.SubnetId, a.Adresse }).IsUnique();

            entity.HasOne(a => a.Subnet)
                  .WithMany(s => s.IPAdressen)
                  .HasForeignKey(a => a.SubnetId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}