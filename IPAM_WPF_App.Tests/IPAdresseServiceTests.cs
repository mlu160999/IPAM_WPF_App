using IPAM_WPF_App.Models;
using IPAM_WPF_App.Services;
using IPAM_WPF_App.Tests.Fakes;

namespace IPAM_WPF_App.Tests;

public class IPAdresseServiceTests
{
    private const int SubnetId = 1;
    private const int AnderesSubnetId = 2;

    // --- Add ---

    [Fact]
    public void Add_GueltigeNeueAdresse_GibtErfolgreichZurueckUndSpeichertMitStatusFrei()
    {
        // Arrange
        var repository = new FakeIPAdresseRepository();
        var service = new IPAdresseService(repository);

        // Act
        var resultat = service.Add("10.10.10.25", SubnetId);

        // Assert
        Assert.Equal(IPAdresseAddResult.Erfolgreich, resultat);
        var gespeichert = Assert.Single(repository.GespeicherteIPAdressen);
        Assert.Equal("10.10.10.25", gespeichert.Adresse);
        Assert.Equal(IPStatus.Frei, gespeichert.Status);
        Assert.Equal(SubnetId, gespeichert.SubnetId);
    }

    [Fact]
    public void Add_GueltigeIPv6Adresse_GibtErfolgreichZurueck()
    {
        // Arrange
        var service = new IPAdresseService(new FakeIPAdresseRepository());

        // Act
        var resultat = service.Add("fe80::1", SubnetId);

        // Assert
        Assert.Equal(IPAdresseAddResult.Erfolgreich, resultat);
    }

    [Fact]
    public void Add_UngueltigeAdresse_GibtUngueltigZurueckUndSpeichertNichts()
    {
        // Arrange
        var repository = new FakeIPAdresseRepository();
        var service = new IPAdresseService(repository);

        // Act
        var resultat = service.Add("keine-ip", SubnetId);

        // Assert
        Assert.Equal(IPAdresseAddResult.Ungueltig, resultat);
        Assert.Empty(repository.GespeicherteIPAdressen);
    }

    [Fact]
    public void Add_LeereEingabe_GibtUngueltigZurueck()
    {
        // Arrange
        var service = new IPAdresseService(new FakeIPAdresseRepository());

        // Act
        var resultat = service.Add("   ", SubnetId);

        // Assert
        Assert.Equal(IPAdresseAddResult.Ungueltig, resultat);
    }

    [Fact]
    public void Add_AdresseExistiertImGleichenSubnet_GibtBereitsVorhandenZurueck()
    {
        // Arrange
        var repository = new FakeIPAdresseRepository(
            new IPAdresse { Adresse = "10.10.10.25", SubnetId = SubnetId });
        var service = new IPAdresseService(repository);

        // Act
        var resultat = service.Add("10.10.10.25", SubnetId);

        // Assert
        Assert.Equal(IPAdresseAddResult.BereitsVorhanden, resultat);
        Assert.Single(repository.GespeicherteIPAdressen);
    }

    [Fact]
    public void Add_GleicheAdresseInAnderemSubnet_GibtErfolgreichZurueck()
    {
        // Arrange
        var repository = new FakeIPAdresseRepository(
            new IPAdresse { Adresse = "10.10.10.25", SubnetId = AnderesSubnetId });
        var service = new IPAdresseService(repository);

        // Act
        var resultat = service.Add("10.10.10.25", SubnetId);

        // Assert
        Assert.Equal(IPAdresseAddResult.Erfolgreich, resultat);
        Assert.Equal(2, repository.GespeicherteIPAdressen.Count);
    }

    [Fact]
    public void Add_AdresseMitLeerzeichen_SpeichertGetrimmteAdresse()
    {
        // Arrange
        var repository = new FakeIPAdresseRepository();
        var service = new IPAdresseService(repository);

        // Act
        service.Add("  10.10.10.25  ", SubnetId);

        // Assert
        Assert.Equal("10.10.10.25", repository.GespeicherteIPAdressen[0].Adresse);
    }

    // --- AdvanceStatus ---

    [Theory]
    [InlineData(IPStatus.Frei, IPStatus.Reserviert)]
    [InlineData(IPStatus.Reserviert, IPStatus.Zugewiesen)]
    [InlineData(IPStatus.Zugewiesen, IPStatus.Frei)]
    public void AdvanceStatus_ExistierendeAdresse_SchaltetStatusWeiter(
        IPStatus statusVorher, IPStatus erwarteterStatus)
    {
        // Arrange
        var ipAdresse = new IPAdresse { Adresse = "10.10.10.25", Status = statusVorher, SubnetId = SubnetId };
        var repository = new FakeIPAdresseRepository(ipAdresse);
        var service = new IPAdresseService(repository);

        // Act
        service.AdvanceStatus(ipAdresse.Id);

        // Assert
        Assert.Equal(erwarteterStatus, repository.GetById(ipAdresse.Id)!.Status);
    }

    [Fact]
    public void AdvanceStatus_NichtExistierendeId_AendertNichts()
    {
        // Arrange
        var ipAdresse = new IPAdresse { Adresse = "10.10.10.25", Status = IPStatus.Frei, SubnetId = SubnetId };
        var repository = new FakeIPAdresseRepository(ipAdresse);
        var service = new IPAdresseService(repository);

        // Act
        service.AdvanceStatus(999);

        // Assert
        Assert.Equal(IPStatus.Frei, repository.GetById(ipAdresse.Id)!.Status);
    }

    // --- GetBySubnetId / Delete ---

    [Fact]
    public void GetBySubnetId_MehrereSubnets_LiefertNurAdressenDesSubnets()
    {
        // Arrange
        var repository = new FakeIPAdresseRepository(
            new IPAdresse { Adresse = "10.10.10.2", SubnetId = SubnetId },
            new IPAdresse { Adresse = "10.10.10.1", SubnetId = SubnetId },
            new IPAdresse { Adresse = "192.168.1.1", SubnetId = AnderesSubnetId });
        var service = new IPAdresseService(repository);

        // Act
        var resultat = service.GetBySubnetId(SubnetId);

        // Assert
        Assert.Equal(2, resultat.Count);
        Assert.Equal("10.10.10.1", resultat[0].Adresse);
        Assert.Equal("10.10.10.2", resultat[1].Adresse);
    }

    [Fact]
    public void Delete_ExistierendeId_EntferntAdresse()
    {
        // Arrange
        var ipAdresse = new IPAdresse { Adresse = "10.10.10.25", SubnetId = SubnetId };
        var repository = new FakeIPAdresseRepository(ipAdresse);
        var service = new IPAdresseService(repository);

        // Act
        service.Delete(ipAdresse.Id);

        // Assert
        Assert.Empty(repository.GespeicherteIPAdressen);
    }
}
