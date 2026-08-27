using Microsoft.Data.Sqlite;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace IPAM_WPF_App;

public partial class MainWindow : Window
{
    private readonly SubnetRepository _subnetRepository;
    private readonly IPAdresseRepository _ipAdresseRepository;

    public MainWindow()
    {
        InitializeComponent();

        DbInitializer.Initialize();

        _subnetRepository = new SubnetRepository();
        _ipAdresseRepository = new IPAdresseRepository();

        LoadSubnets();
    }

    private void LoadSubnets()
    {
        int? selectedSubnetId = (SubnetDataGrid.SelectedItem as Subnet)?.Id;

        SubnetDataGrid.ItemsSource = _subnetRepository.GetAll();

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
        IpAdresseDataGrid.ItemsSource = _ipAdresseRepository.GetBySubnetId(subnetId);
    }

    private void SubnetDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SubnetDataGrid.SelectedItem is not Subnet subnet)
        {
            IpAdresseDataGrid.ItemsSource = null;
            SelectedSubnetTextBlock.Text = "Bitte zuerst ein Subnet auswählen.";
            return;
        }

        SelectedSubnetTextBlock.Text = $"Ausgewähltes Subnet: {subnet.Name}";
        LoadIpAdressen(subnet.Id);
    }

    private void AddSubnetButton_Click(object sender, RoutedEventArgs e)
    {
        string name = SubnetNameTextBox.Text.Trim();
        string beschreibung = SubnetBeschreibungTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Bitte einen Namen oder ein CIDR für das Subnet eingeben.",
                "Eingabe fehlt", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _subnetRepository.Add(name, beschreibung);

        SubnetNameTextBox.Clear();
        SubnetBeschreibungTextBox.Clear();
        LoadSubnets();
    }

    private void DeleteSubnetButton_Click(object sender, RoutedEventArgs e)
    {
        if (SubnetDataGrid.SelectedItem is not Subnet subnet)
        {
            MessageBox.Show("Bitte zuerst ein Subnet auswählen.",
                "Keine Auswahl", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        MessageBoxResult result = MessageBox.Show(
            $"Subnet '{subnet.Name}' wirklich löschen?\nAlle zugehörigen IP-Adressen werden ebenfalls gelöscht.",
            "Subnet löschen",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        _subnetRepository.Delete(subnet.Id);
        IpAdresseDataGrid.ItemsSource = null;
        SelectedSubnetTextBlock.Text = "Bitte zuerst ein Subnet auswählen.";
        LoadSubnets();
    }

    private void AddIpAdresseButton_Click(object sender, RoutedEventArgs e)
    {
        if (SubnetDataGrid.SelectedItem is not Subnet subnet)
        {
            MessageBox.Show("Bitte zuerst ein Subnet auswählen.",
                "Keine Auswahl", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        string ipText = IpAdresseTextBox.Text.Trim();

        if (!IPAddress.TryParse(ipText, out _))
        {
            MessageBox.Show("Bitte eine gültige IPv4- oder IPv6-Adresse eingeben.",
                "Ungültige IP-Adresse", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            _ipAdresseRepository.Add(ipText, subnet.Id);
            IpAdresseTextBox.Clear();
            LoadIpAdressen(subnet.Id);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            MessageBox.Show("Diese IP-Adresse existiert in diesem Subnet bereits.",
                "Doppelter Eintrag", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void AdvanceStatusButton_Click(object sender, RoutedEventArgs e)
    {
        if (SubnetDataGrid.SelectedItem is not Subnet subnet)
        {
            MessageBox.Show("Bitte zuerst ein Subnet auswählen.",
                "Keine Auswahl", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (IpAdresseDataGrid.SelectedItem is not IPAdresse ipAdresse)
        {
            MessageBox.Show("Bitte zuerst eine IP-Adresse auswählen.",
                "Keine Auswahl", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        int neuerStatus = ipAdresse.Erledigt switch
        {
            0 => 1, // Frei -> Reserviert
            1 => 2, // Reserviert -> Zugewiesen
            _ => 0  // Zugewiesen -> Frei
        };

        _ipAdresseRepository.UpdateStatus(ipAdresse.Id, neuerStatus);
        LoadIpAdressen(subnet.Id);
    }

    private void DeleteIpAdresseButton_Click(object sender, RoutedEventArgs e)
    {
        if (SubnetDataGrid.SelectedItem is not Subnet subnet)
        {
            MessageBox.Show("Bitte zuerst ein Subnet auswählen.",
                "Keine Auswahl", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (IpAdresseDataGrid.SelectedItem is not IPAdresse ipAdresse)
        {
            MessageBox.Show("Bitte zuerst eine IP-Adresse auswählen.",
                "Keine Auswahl", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _ipAdresseRepository.Delete(ipAdresse.Id);
        LoadIpAdressen(subnet.Id);
    }
}
