using IPAM_WPF_App.DataAccess;

namespace IPAM_WPF_App.Services;

public class ApplicationService
{
    public void InitializeDatabase()
    {
        DbInitializer.Initialize();
    }
}
