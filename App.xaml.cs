using IPAM_WPF_App.DataAccess;
using IPAM_WPF_App.Services;
using System.Windows;

namespace IPAM_WPF_App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var applicationService = new ApplicationService();
        applicationService.InitializeDatabase();

        var subnetService = new SubnetService(new SubnetRepository());
        var ipAdresseService = new IPAdresseService(new IPAdresseRepository());

        var mainWindow = new MainWindow(subnetService, ipAdresseService);
        mainWindow.Show();
    }
}