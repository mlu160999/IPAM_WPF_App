using IPAM_WPF_App.Models;
using IPAM_WPF_App.Services;
using System.Windows;
using System.Windows.Controls;

namespace IPAM_WPF_App;

public partial class MainWindow : Window
{
    private readonly ApplicationService _applicationService;
    private readonly SubnetService _subnetService;
    private readonly IPAdresseService _ipAdresseService;

    public MainWindow()
    {
        InitializeComponent();

        _applicationService = new ApplicationService();
        _subnetService = new SubnetService();
        _ipAdresseService = new IPAdresseService();

        _applicationService.InitializeDatabase();
        LoadSubnets();
    }

    private void LoadSubnets()
    {
        int? selectedSubnetId =
            (SubnetDataGrid.SelectedItem as Subnet)?.Id;

        SubnetDataGrid.ItemsSource = _subnetService.GetAll();

        if (selectedSubnetId.HasValue)
        {
            foreach (Subnet subnet in SubnetDataGrid.Items)
            {
                if (subnet.Id == selectedSubnetId.Value)
                {
                    SubnetDataGrid.SelectedItem = subnet;
                    break;
                }
            }
        }
    }

    private void LoadIpAdressen(int subnetId)
    {
        IpAdresseDataGrid.ItemsSource =
            _ipAdresseService.GetBySubnetId(subnetId);
    }

    private void SubnetDataGrid_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (SubnetDataGrid.SelectedItem is not Subnet subnet)
        {
            IpAdresseDataGrid.ItemsSource = null;
            SelectedSubnetTextBlock.Text =
                "Bitte zuerst ein Subnet auswählen.";
            return;
        }

        SelectedSubnetTextBlock.Text =
            $"Ausgewähltes Subnet: {subnet.Name}";
        LoadIpAdressen(subnet.Id);
    }

    private void AddSubnetButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        bool wurdeHinzugefuegt = _subnetService.Add(
            SubnetNameTextBox.Text,
            SubnetBeschreibungTextBox.Text);

        if (!wurdeHinzugefuegt)
        {
            MessageBox.Show(
                "Bitte einen Namen oder ein CIDR für das Subnet eingeben.",
                "Eingabe fehlt",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        SubnetNameTextBox.Clear();
        SubnetBeschreibungTextBox.Clear();
        LoadSubnets();
    }

    private void DeleteSubnetButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (SubnetDataGrid.SelectedItem is not Subnet subnet)
        {
            MessageBox.Show(
                "Bitte zuerst ein Subnet auswählen.",
                "Keine Auswahl",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        MessageBoxResult result = MessageBox.Show(
            $"Subnet '{subnet.Name}' wirklich löschen?\n" +
            "Alle zugehörigen IP-Adressen werden ebenfalls gelöscht.",
            "Subnet löschen",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        _subnetService.Delete(subnet.Id);
        IpAdresseDataGrid.ItemsSource = null;
        SelectedSubnetTextBlock.Text =
            "Bitte zuerst ein Subnet auswählen.";
        LoadSubnets();
    }

    private void AddIpAdresseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (SubnetDataGrid.SelectedItem is not Subnet subnet)
        {
            MessageBox.Show(
                "Bitte zuerst ein Subnet auswählen.",
                "Keine Auswahl",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        IpAdresseAddResult result = _ipAdresseService.Add(
            IpAdresseTextBox.Text,
            subnet.Id);

        switch (result)
        {
            case IpAdresseAddResult.Ungueltig:
                MessageBox.Show(
                    "Bitte eine gültige IPv4- oder IPv6-Adresse eingeben.",
                    "Ungültige IP-Adresse",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;

            case IpAdresseAddResult.BereitsVorhanden:
                MessageBox.Show(
                    "Diese IP-Adresse existiert in diesem Subnet bereits.",
                    "Doppelter Eintrag",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;

            case IpAdresseAddResult.Erfolgreich:
                IpAdresseTextBox.Clear();
                LoadIpAdressen(subnet.Id);
                break;
        }
    }

    private void AdvanceStatusButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (SubnetDataGrid.SelectedItem is not Subnet subnet)
        {
            MessageBox.Show(
                "Bitte zuerst ein Subnet auswählen.",
                "Keine Auswahl",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        if (IpAdresseDataGrid.SelectedItem is not IPAdresse ipAdresse)
        {
            MessageBox.Show(
                "Bitte zuerst eine IP-Adresse auswählen.",
                "Keine Auswahl",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        _ipAdresseService.AdvanceStatus(ipAdresse);
        LoadIpAdressen(subnet.Id);
    }

    private void DeleteIpAdresseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (SubnetDataGrid.SelectedItem is not Subnet subnet)
        {
            MessageBox.Show(
                "Bitte zuerst ein Subnet auswählen.",
                "Keine Auswahl",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        if (IpAdresseDataGrid.SelectedItem is not IPAdresse ipAdresse)
        {
            MessageBox.Show(
                "Bitte zuerst eine IP-Adresse auswählen.",
                "Keine Auswahl",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        _ipAdresseService.Delete(ipAdresse.Id);
        LoadIpAdressen(subnet.Id);
    }
}
