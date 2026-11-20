using IPAM_WPF_App.DataAccess;
using IPAM_WPF_App.Models;

namespace IPAM_WPF_App.Tests.Fakes;

/// <summary>
/// Speicherinterne Implementierung von IRepository&lt;Subnet&gt; für Tests.
/// </summary>
public class FakeSubnetRepository : IRepository<Subnet>
{
    private readonly List<Subnet> _subnets = new();
    private int _naechsteId = 1;

    public FakeSubnetRepository(params Subnet[] startDaten)
    {
        foreach (var subnet in startDaten)
        {
            Add(subnet);
        }
    }

    // Für Asserts: direkter Blick auf den gespeicherten Zustand.
    public IReadOnlyList<Subnet> GespeicherteSubnets => _subnets;

    public List<Subnet> GetAll()
    {
        return _subnets.OrderBy(s => s.Name).ToList();
    }

    public Subnet? GetById(int id)
    {
        return _subnets.FirstOrDefault(s => s.Id == id);
    }

    public void Add(Subnet entity)
    {
        if (entity.Id == 0)
        {
            entity.Id = _naechsteId;
        }

        _naechsteId = Math.Max(_naechsteId, entity.Id) + 1;
        _subnets.Add(entity);
    }

    public void Update(Subnet entity)
    {
        int index = _subnets.FindIndex(s => s.Id == entity.Id);
        if (index >= 0)
        {
            _subnets[index] = entity;
        }
    }

    public void Delete(int id)
    {
        _subnets.RemoveAll(s => s.Id == id);
    }
}
