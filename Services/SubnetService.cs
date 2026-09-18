using IPAM_WPF_App.DataAccess;
using IPAM_WPF_App.Models;

namespace IPAM_WPF_App.Services;

public class SubnetService
{
    private readonly SubnetDataAccess _subnetDataAccess;

    public SubnetService()
    {
        _subnetDataAccess = new SubnetDataAccess();
    }

    public List<Subnet> GetAll()
    {
        return _subnetDataAccess.GetAll();
    }

    public bool Add(string name, string beschreibung)
    {
        name = name.Trim();
        beschreibung = beschreibung.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        _subnetDataAccess.Add(name, beschreibung);
        return true;
    }

    public void Update(Subnet subnet)
    {
        subnet.Name = subnet.Name.Trim();
        subnet.Beschreibung = subnet.Beschreibung.Trim();

        if (string.IsNullOrWhiteSpace(subnet.Name))
        {
            throw new ArgumentException("Der Subnet-Name darf nicht leer sein.");
        }

        _subnetDataAccess.Update(subnet);
    }

    public void Delete(int subnetId)
    {
        _subnetDataAccess.Delete(subnetId);
    }
}
