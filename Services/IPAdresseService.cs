using IPAM_WPF_App.DataAccess;
using IPAM_WPF_App.Dtos;
using IPAM_WPF_App.Models;
using System.Net;

namespace IPAM_WPF_App.Services;

/// <summary>
/// Geschäftslogik für IP-Adressen: Validierung, Duplikatprüfung und Status-Workflow.
/// </summary>
public class IPAdresseService
{
    private readonly IIPAdresseRepository _repository;

    public IPAdresseService(IIPAdresseRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Lädt alle IP-Adressen eines Subnets für die Anzeige.
    /// </summary>
    /// <param name="subnetId">Id des Subnets.</param>
    /// <returns>Die IP-Adressen als DTOs, sortiert nach Adresse.</returns>
    public List<IPAdresseDto> GetBySubnetId(int subnetId)
    {
        return _repository.GetBySubnetId(subnetId)
            .Select(a => new IPAdresseDto
            {
                Id = a.Id,
                Adresse = a.Adresse,
                Status = a.Status
            })
            .ToList();
    }

    /// <summary>
    /// Fügt einem Subnet eine neue IP-Adresse mit Status Frei hinzu.
    /// </summary>
    /// <param name="ipText">Die eingegebene IPv4- oder IPv6-Adresse.</param>
    /// <param name="subnetId">Id des Subnets.</param>
    /// <returns>Ob die Adresse hinzugefügt wurde oder warum nicht.</returns>
    public IPAdresseAddResult Add(string ipText, int subnetId)
    {
        ipText = ipText.Trim();

        if (!IPAddress.TryParse(ipText, out _))
        {
            return IPAdresseAddResult.Ungueltig;
        }

        if (_repository.ExistsInSubnet(ipText, subnetId))
        {
            return IPAdresseAddResult.BereitsVorhanden;
        }

        _repository.Add(new IPAdresse
        {
            Adresse = ipText,
            Status = IPStatus.Frei,
            SubnetId = subnetId
        });

        return IPAdresseAddResult.Erfolgreich;
    }

    /// <summary>
    /// Schaltet den Status einer IP-Adresse eine Stufe weiter.
    /// Existiert die Id nicht, passiert nichts.
    /// </summary>
    /// <param name="ipAdresseId">Id der IP-Adresse.</param>
    public void AdvanceStatus(int ipAdresseId)
    {
        var ipAdresse = _repository.GetById(ipAdresseId);
        if (ipAdresse == null)
        {
            return;
        }

        ipAdresse.Status = IPStatusWorkflow.GetNextStatus(ipAdresse.Status);
        _repository.Update(ipAdresse);
    }

    /// <summary>
    /// Löscht eine IP-Adresse.
    /// </summary>
    /// <param name="ipAdresseId">Id der IP-Adresse.</param>
    public void Delete(int ipAdresseId)
    {
        _repository.Delete(ipAdresseId);
    }
}
