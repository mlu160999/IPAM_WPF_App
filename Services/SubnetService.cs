using IPAM_WPF_App.DataAccess;
using IPAM_WPF_App.Models;

namespace IPAM_WPF_App.Services;

public class SubnetService
{
    private readonly IRepository<Subnet> _repository;

    public SubnetService(IRepository<Subnet> repository)
    {
        _repository = repository;
    }

    public List<Subnet> GetAll()
    {
        return _repository.GetAll();
    }

    public bool Add(string name, string beschreibung)
    {
        name = name.Trim();
        beschreibung = beschreibung.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        _repository.Add(new Subnet
        {
            Name = name,
            Beschreibung = beschreibung,
            Erstellt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        });
        return true;
    }

    public void Update(Subnet subnet)
    {
        string name = subnet.Name.Trim();
        string beschreibung = subnet.Beschreibung.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Der Subnet-Name darf nicht leer sein.");
        }

        var vorhanden = _repository.GetById(subnet.Id);
        if (vorhanden == null) return;

        vorhanden.Name = name;
        vorhanden.Beschreibung = beschreibung;
        _repository.Update(vorhanden);
    }

    public void Delete(int subnetId)
    {
        _repository.Delete(subnetId);
    }
}