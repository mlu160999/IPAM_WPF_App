using IPAM_WPF_App.Models;
using IPAM_WPF_App.Services;
using IPAM_WPF_App.Tests.Fakes;
using System.Globalization;

namespace IPAM_WPF_App.Tests;

public class SubnetServiceTests
{
    // --- Add ---

    [Fact]
    public void Add_GueltigerName_GibtTrueZurueckUndSpeichertSubnet()
    {
        // Arrange
        var repository = new FakeSubnetRepository();
        var service = new SubnetService(repository);

        // Act
        bool resultat = service.Add("10.10.10.0/24", "Server-Netz");

        // Assert
        Assert.True(resultat, "Subnet mit gültigem Namen wurde nicht erstellt");
        var gespeichert = Assert.Single(repository.GespeicherteSubnets);
        Assert.Equal("10.10.10.0/24", gespeichert.Name);
        Assert.Equal("Server-Netz", gespeichert.Beschreibung);
    }

    [Fact]
    public void Add_LeererName_GibtFalseZurueckUndSpeichertNichts()
    {
        // Arrange
        var repository = new FakeSubnetRepository();
        var service = new SubnetService(repository);

        // Act
        bool resultat = service.Add("   ", "Beschreibung");

        // Assert
        Assert.False(resultat, "Subnet ohne Namen wurde fälschlicherweise erstellt");
        Assert.Empty(repository.GespeicherteSubnets);
    }

    [Fact]
    public void Add_NameMitLeerzeichen_SpeichertGetrimmtenNamen()
    {
        // Arrange
        var repository = new FakeSubnetRepository();
        var service = new SubnetService(repository);

        // Act
        service.Add("  Server-Netz  ", "  Test  ");

        // Assert
        var gespeichert = repository.GespeicherteSubnets[0];
        Assert.Equal("Server-Netz", gespeichert.Name);
        Assert.Equal("Test", gespeichert.Beschreibung);
    }

    [Fact]
    public void Add_GueltigerName_SetztErstelltImErwartetenFormat()
    {
        // Arrange
        var repository = new FakeSubnetRepository();
        var service = new SubnetService(repository);

        // Act
        service.Add("Server-Netz", string.Empty);

        // Assert
        string erstellt = repository.GespeicherteSubnets[0].Erstellt;
        bool istGueltigesDatum = DateTime.TryParseExact(
            erstellt, SubnetService.ErstelltFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        Assert.True(istGueltigesDatum, $"Erstellt '{erstellt}' hat nicht das Format {SubnetService.ErstelltFormat}");
    }

    // --- Update ---

    [Fact]
    public void Update_LeererName_WirftArgumentException()
    {
        // Arrange
        var subnet = new Subnet { Name = "Server-Netz", Erstellt = "2026-01-01 08:00:00" };
        var service = new SubnetService(new FakeSubnetRepository(subnet));
        var geaendert = new Subnet { Id = subnet.Id, Name = "  " };

        // Act und Assert
        Assert.Throws<ArgumentException>(() => service.Update(geaendert));
    }

    [Fact]
    public void Update_NeuerName_AendertNameUndLaesstErstelltUnveraendert()
    {
        // Arrange
        var subnet = new Subnet { Name = "Alt", Beschreibung = "Alt", Erstellt = "2026-01-01 08:00:00" };
        var repository = new FakeSubnetRepository(subnet);
        var service = new SubnetService(repository);
        var geaendert = new Subnet { Id = subnet.Id, Name = "Neu", Beschreibung = "Neu", Erstellt = "falsch" };

        // Act
        service.Update(geaendert);

        // Assert
        var gespeichert = repository.GetById(subnet.Id)!;
        Assert.Equal("Neu", gespeichert.Name);
        Assert.Equal("2026-01-01 08:00:00", gespeichert.Erstellt);
    }
}
