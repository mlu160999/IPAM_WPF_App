using IPAM_WPF_App.DataAccess;
using IPAM_WPF_App.Models;

namespace IPAM_WPF_App.Services;

/// <summary>
/// Geschäftslogik für Subnets: Eingabeprüfung und Erstelldatum.
/// </summary>
public class SubnetService
{
    // Format, in dem das Erstelldatum in der Datenbank gespeichert wird.
    public const string ErstelltFormat = "yyyy-MM-dd HH:mm:ss";

    private readonly IRepository<Subnet> _repository;

    public SubnetService(IRepository<Subnet> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Lädt alle Subnets, sortiert nach Name.
    /// </summary>
    /// <returns>Alle Subnets.</returns>
    public List<Subnet> GetAll()
    {
        return _repository.GetAll();
    }

    /// <summary>
    /// Erstellt ein neues Subnet mit dem aktuellen Datum als Erstelldatum.
    /// </summary>
    /// <param name="name">Name oder CIDR des Subnets.</param>
    /// <param name="beschreibung">Optionale Beschreibung.</param>
    /// <returns>True, wenn das Subnet erstellt wurde; false bei leerem Namen.</returns>
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
            Erstellt = DateTime.Now.ToString(ErstelltFormat)
        });
        return true;
    }

    /// <summary>
    /// Ändert Name und Beschreibung eines bestehenden Subnets.
    /// Das Erstelldatum bleibt unverändert.
    /// </summary>
    /// <param name="subnet">Subnet mit Id und den neuen Werten.</param>
    /// <exception cref="ArgumentException">Wenn der Name leer ist.</exception>
    public void Update(Subnet subnet)
    {
        string name = subnet.Name.Trim();
        string beschreibung = subnet.Beschreibung.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Der Subnet-Name darf nicht leer sein.");
        }

        var vorhanden = _repository.GetById(subnet.Id);
        if (vorhanden == null)
        {
            return;
        }

        vorhanden.Name = name;
        vorhanden.Beschreibung = beschreibung;
        _repository.Update(vorhanden);
    }

    /// <summary>
    /// Löscht ein Subnet inklusive seiner IP-Adressen.
    /// </summary>
    /// <param name="subnetId">Id des Subnets.</param>
    public void Delete(int subnetId)
    {
        _repository.Delete(subnetId);
    }
}
