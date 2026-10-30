using Microsoft.EntityFrameworkCore;

namespace IPAM_WPF_App.DataAccess;

public static class DbInitializer
{

    public static void Initialize()
    {
        using var context = new AppDbContext();
        context.Database.Migrate();
    }
}