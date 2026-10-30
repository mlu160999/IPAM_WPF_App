using IPAM_WPF_App.Models;

namespace IPAM_WPF_App.DataAccess;

public class IPAdresseRepository : IIPAdresseRepository
{
    public List<IPAdresse> GetAll()
    {
        using var context = new AppDbContext();
        return context.IPAdressen
            .OrderBy(a => a.Adresse)
            .ToList();
    }

    public IPAdresse? GetById(int id)
    {
        using var context = new AppDbContext();
        return context.IPAdressen.Find(id);
    }

    public void Add(IPAdresse entity)
    {
        using var context = new AppDbContext();
        context.IPAdressen.Add(entity);
        context.SaveChanges();
    }

    public void Update(IPAdresse entity)
    {
        using var context = new AppDbContext();
        context.IPAdressen.Update(entity);
        context.SaveChanges();
    }

    public void Delete(int id)
    {
        using var context = new AppDbContext();
        var ipAdresse = context.IPAdressen.Find(id);
        if (ipAdresse == null) return;

        context.IPAdressen.Remove(ipAdresse);
        context.SaveChanges();
    }


    public List<IPAdresse> GetBySubnetId(int subnetId)
    {
        using var context = new AppDbContext();
        return context.IPAdressen
            .Where(a => a.SubnetId == subnetId)
            .OrderBy(a => a.Adresse)
            .ToList();
    }

    public bool ExistsInSubnet(string adresse, int subnetId)
    {
        using var context = new AppDbContext();
        return context.IPAdressen
            .Any(a => a.SubnetId == subnetId && a.Adresse == adresse);
    }
}