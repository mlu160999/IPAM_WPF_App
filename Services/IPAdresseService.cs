using IPAM_WPF_App.DataAccess;
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
    private readonly IPAdresseDataAccess _ipAdresseDataAccess;

    public IPAdresseService()
    {
        _ipAdresseDataAccess = new IPAdresseDataAccess();
    }

    public List<IPAdresse> GetBySubnetId(int subnetId)
    {
        return _ipAdresseDataAccess.GetBySubnetId(subnetId);
    }

    public IPAdresseAddResult Add(string ipText, int subnetId)
    {
        ipText = ipText.Trim();

        if (!IPAddress.TryParse(ipText, out _))
        {
            return IPAdresseAddResult.Ungueltig;
        }

        bool wurdeHinzugefuegt =
            _ipAdresseDataAccess.TryAdd(ipText, subnetId);

        if (!wurdeHinzugefuegt)
        {
            return IPAdresseAddResult.BereitsVorhanden;
        }

        return IPAdresseAddResult.Erfolgreich;
    }

    public void AdvanceStatus(IPAdresse ipAdresse)
    {
        IPStatus neuerStatus = ipAdresse.Status switch
        {
            IPStatus.Frei => IPStatus.Reserviert,
            IPStatus.Reserviert => IPStatus.Zugewiesen,
            _ => IPStatus.Frei
        };

        _ipAdresseDataAccess.UpdateStatus(ipAdresse.Id, neuerStatus);
    }

    public void Delete(int ipAdresseId)
    {
        _ipAdresseDataAccess.Delete(ipAdresseId);
    }
}
