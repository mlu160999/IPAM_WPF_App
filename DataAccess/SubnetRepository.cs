using IPAM_WPF_App.Models;

namespace IPAM_WPF_App.DataAccess;

public class SubnetRepository : IRepository<Subnet>
{
    public List<Subnet> GetAll()
    {
        using var context = new AppDbContext();
        return context.Subnets
            .OrderBy(s => s.Name)
            .ToList();
    }

    public Subnet? GetById(int id)
    {
        using var context = new AppDbContext();
        return context.Subnets.Find(id);
    }

    public void Add(Subnet entity)
    {
        using var context = new AppDbContext();
        context.Subnets.Add(entity);
        context.SaveChanges();
    }

    public void Update(Subnet entity)
    {
        using var context = new AppDbContext();
        context.Subnets.Update(entity);
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