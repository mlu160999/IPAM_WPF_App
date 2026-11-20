using IPAM_WPF_App.DataAccess;
using IPAM_WPF_App.Models;

namespace IPAM_WPF_App.Tests.Fakes;

/// <summary>
/// Speicherinterne Implementierung von IIPAdresseRepository für Tests.
/// Arbeitet mit einer Liste statt mit einer echten Datenbank.
/// </summary>
public class FakeIPAdresseRepository : IIPAdresseRepository
{
    private readonly List<IPAdresse> _ipAdressen = new();
    private int _naechsteId = 1;

    public FakeIPAdresseRepository(params IPAdresse[] startDaten)
    {
        foreach (var ipAdresse in startDaten)
        {
            Add(ipAdresse);
        }
    }

    // Für Asserts: direkter Blick auf den gespeicherten Zustand.
    public IReadOnlyList<IPAdresse> GespeicherteIPAdressen => _ipAdressen;

    public List<IPAdresse> GetAll()
    {
        return _ipAdressen.OrderBy(a => a.Adresse).ToList();
    }

    public IPAdresse? GetById(int id)
    {
        return _ipAdressen.FirstOrDefault(a => a.Id == id);
    }

    public void Add(IPAdresse entity)
    {
        if (entity.Id == 0)
        {
            entity.Id = _naechsteId;
        }

        _naechsteId = Math.Max(_naechsteId, entity.Id) + 1;
        _ipAdressen.Add(entity);
    }

    public void Update(IPAdresse entity)
    {
        int index = _ipAdressen.FindIndex(a => a.Id == entity.Id);
        if (index >= 0)
        {
            _ipAdressen[index] = entity;
        }
    }

    public void Delete(int id)
    {
        _ipAdressen.RemoveAll(a => a.Id == id);
    }

    public List<IPAdresse> GetBySubnetId(int subnetId)
    {
        return _ipAdressen
            .Where(a => a.SubnetId == subnetId)
            .OrderBy(a => a.Adresse)
            .ToList();
    }

    public bool ExistsInSubnet(string adresse, int subnetId)
    {
        return _ipAdressen.Any(a => a.SubnetId == subnetId && a.Adresse == adresse);
    }
}
