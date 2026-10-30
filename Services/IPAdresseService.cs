using IPAM_WPF_App.DataAccess;
using IPAM_WPF_App.Dtos;
using IPAM_WPF_App.Models;
using System.Net;

namespace IPAM_WPF_App.Services;

public enum IPAdresseAddResult
{
    Erfolgreich,
    Ungueltig,
    BereitsVorhanden
}

public class IPAdresseService
{
    private readonly IIPAdresseRepository _repository;

    public IPAdresseService(IIPAdresseRepository repository)
    {
        _repository = repository;
    }

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

    public void AdvanceStatus(int ipAdresseId)
    {
        var ipAdresse = _repository.GetById(ipAdresseId);
        if (ipAdresse == null) return;

        ipAdresse.Status = ipAdresse.Status switch
        {
            IPStatus.Frei => IPStatus.Reserviert,
            IPStatus.Reserviert => IPStatus.Zugewiesen,
            _ => IPStatus.Frei
        };

        _repository.Update(ipAdresse);
    }

    public void Delete(int ipAdresseId)
    {
        _repository.Delete(ipAdresseId);
    }
}