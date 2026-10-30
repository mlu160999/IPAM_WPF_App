using IPAM_WPF_App.Models;

namespace IPAM_WPF_App.DataAccess;

public class SubnetDataAccess
{
    public List<Subnet> GetAll()
    {
        using var context = new AppDbContext();
        return context.Subnets
            .OrderBy(s => s.Name)
            .ToList();
    }

    public int Add(string name, string beschreibung)
    {
        using var context = new AppDbContext();

        var subnet = new Subnet
        {
            Name = name,
            Beschreibung = beschreibung,
            Erstellt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        context.Subnets.Add(subnet);
        context.SaveChanges();

        return subnet.Id;
    }

    public void Update(Subnet subnet)
    {
        using var context = new AppDbContext();
        var vorhanden = context.Subnets.Find(subnet.Id);
        if (vorhanden == null) return;

        vorhanden.Name = subnet.Name;
        vorhanden.Beschreibung = subnet.Beschreibung;
        context.SaveChanges();
    }

    public void Delete(int id)
    {
        using var context = new AppDbContext();
        var subnet = context.Subnets.Find(id);
        if (subnet == null) return;

        context.Subnets.Remove(subnet);
        context.SaveChanges();
    }
}