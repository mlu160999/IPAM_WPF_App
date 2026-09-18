using IPAM_WPF_App.DataAccess;
using IPAM_WPF_App.Models;
using System.Net;

namespace IPAM_WPF_App.Services;

public enum IpAdresseAddResult
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

    public IpAdresseAddResult Add(string ipText, int subnetId)
    {
        ipText = ipText.Trim();

        if (!IPAddress.TryParse(ipText, out _))
        {
            return IpAdresseAddResult.Ungueltig;
        }

        bool wurdeHinzugefuegt =
            _ipAdresseDataAccess.TryAdd(ipText, subnetId);

        if (!wurdeHinzugefuegt)
        {
            return IpAdresseAddResult.BereitsVorhanden;
        }

        return IpAdresseAddResult.Erfolgreich;
    }

    public void AdvanceStatus(IPAdresse ipAdresse)
    {
        int neuerStatus = ipAdresse.Erledigt switch
        {
            0 => 1,
            1 => 2,
            _ => 0
        };

        _ipAdresseDataAccess.UpdateStatus(ipAdresse.Id, neuerStatus);
    }

    public void Delete(int ipAdresseId)
    {
        _ipAdresseDataAccess.Delete(ipAdresseId);
    }
}
