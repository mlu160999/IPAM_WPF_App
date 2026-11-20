using IPAM_WPF_App.Models;
using IPAM_WPF_App.Services;

namespace IPAM_WPF_App.Tests;

public class IPStatusWorkflowTests
{
    [Fact]
    public void GetNextStatus_Frei_GibtReserviertZurueck()
    {
        // Act
        var resultat = IPStatusWorkflow.GetNextStatus(IPStatus.Frei);

        // Assert
        Assert.Equal(IPStatus.Reserviert, resultat);
    }

    [Fact]
    public void GetNextStatus_Reserviert_GibtZugewiesenZurueck()
    {
        // Act
        var resultat = IPStatusWorkflow.GetNextStatus(IPStatus.Reserviert);

        // Assert
        Assert.Equal(IPStatus.Zugewiesen, resultat);
    }

    [Fact]
    public void GetNextStatus_Zugewiesen_GibtFreiZurueck()
    {
        // Act
        var resultat = IPStatusWorkflow.GetNextStatus(IPStatus.Zugewiesen);

        // Assert
        Assert.Equal(IPStatus.Frei, resultat);
    }

    [Fact]
    public void GetNextStatus_UnbekannterStatus_WirftArgumentOutOfRangeException()
    {
        // Arrange
        var unbekannterStatus = (IPStatus)99;

        // Act und Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => IPStatusWorkflow.GetNextStatus(unbekannterStatus));
    }
}
