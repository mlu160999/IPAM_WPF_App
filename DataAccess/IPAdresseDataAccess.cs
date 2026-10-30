using IPAM_WPF_App.Models;

namespace IPAM_WPF_App.DataAccess;

public class IPAdresseDataAccess
{
    public List<IPAdresse> GetBySubnetId(int subnetId)
    {
        using var context = new AppDbContext();
        return context.IPAdressen
            .Where(a => a.SubnetId == subnetId)
            .OrderBy(a => a.Adresse)
            .ToList();
    }

    public bool TryAdd(string adresse, int subnetId)
    {
        using var context = new AppDbContext();

        bool existiertBereits = context.IPAdressen
            .Any(a => a.SubnetId == subnetId && a.Adresse == adresse);

        if (existiertBereits)
        {
            return false;
        }

        context.IPAdressen.Add(new IPAdresse
        {
            Adresse = adresse,
            Status = IPStatus.Frei,
            SubnetId = subnetId
        });
        context.SaveChanges();
        return true;
    }

    public void Update(IPAdresse ipAdresse)
    {
        using var context = new AppDbContext();
        var vorhanden = context.IPAdressen.Find(ipAdresse.Id);
        if (vorhanden == null) return;

        vorhanden.Adresse = ipAdresse.Adresse;
        vorhanden.Status = ipAdresse.Status;
        vorhanden.SubnetId = ipAdresse.SubnetId;
        context.SaveChanges();
    }

    public void UpdateStatus(int id, IPStatus neuerStatus)
    {
        using var context = new AppDbContext();
        var ipAdresse = context.IPAdressen.Find(id);
        if (ipAdresse == null) return;

        ipAdresse.Status = neuerStatus;
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
}