using IPAM_WPF_App.Dtos;
using IPAM_WPF_App.Models;
using IPAM_WPF_App.Services;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace IPAM_WPF_App;

public partial class MainWindow : Window
{
    private const string KeineAuswahlTitel = "Keine Auswahl";
    private const string SubnetAuswaehlenText = "Bitte zuerst ein Subnet auswählen.";
    private const string IPAdresseAuswaehlenText = "Bitte zuerst eine IP-Adresse auswählen.";

    private readonly SubnetService _subnetService;
    private readonly IPAdresseService _ipAdresseService;

    public MainWindow(
        SubnetService subnetService,
        IPAdresseService ipAdresseService)
    {
        InitializeComponent();

        _subnetService = subnetService;
        _ipAdresseService = ipAdresseService;

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

    private void ResetIpAdressenAnzeige()
    {
        IpAdresseDataGrid.ItemsSource = null;
        SelectedSubnetTextBlock.Text = SubnetAuswaehlenText;
    }

    // Liefert das ausgewählte Subnet oder zeigt einen Hinweis, wenn keines ausgewählt ist.
    private bool TryGetSelectedSubnet([NotNullWhen(true)] out Subnet? subnet)
    {
        subnet = SubnetDataGrid.SelectedItem as Subnet;
        if (subnet == null)
        {
            ShowInfo(SubnetAuswaehlenText);
        }

        return subnet != null;
    }

    // Liefert die ausgewählte IP-Adresse oder zeigt einen Hinweis, wenn keine ausgewählt ist.
    private bool TryGetSelectedIPAdresse([NotNullWhen(true)] out IPAdresseDto? ipAdresse)
    {
        ipAdresse = IpAdresseDataGrid.SelectedItem as IPAdresseDto;
        if (ipAdresse == null)
        {
            ShowInfo(IPAdresseAuswaehlenText);
        }

        return ipAdresse != null;
    }

    private static void ShowInfo(string text)
    {
        MessageBox.Show(text, KeineAuswahlTitel, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private static void ShowWarning(string text, string titel)
    {
        MessageBox.Show(text, titel, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void SubnetDataGrid_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (SubnetDataGrid.SelectedItem is not Subnet subnet)
        {
            ResetIpAdressenAnzeige();
            return;
        }

        SelectedSubnetTextBlock.Text = $"Ausgewähltes Subnet: {subnet.Name}";
        LoadIpAdressen(subnet.Id);
    }

    private void AddSubnetButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        bool istHinzugefuegt = _subnetService.Add(
            SubnetNameTextBox.Text,
            SubnetBeschreibungTextBox.Text);

        if (!istHinzugefuegt)
        {
            ShowWarning(
                "Bitte einen Namen oder ein CIDR für das Subnet eingeben.",
                "Eingabe fehlt");
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
        if (!TryGetSelectedSubnet(out var subnet))
        {
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
        ResetIpAdressenAnzeige();
        LoadSubnets();
    }

    private void AddIpAdresseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!TryGetSelectedSubnet(out var subnet))
        {
            return;
        }

        IPAdresseAddResult result = _ipAdresseService.Add(
            IpAdresseTextBox.Text,
            subnet.Id);

        switch (result)
        {
            case IPAdresseAddResult.Ungueltig:
                ShowWarning(
                    "Bitte eine gültige IPv4- oder IPv6-Adresse eingeben.",
                    "Ungültige IP-Adresse");
                return;

            case IPAdresseAddResult.BereitsVorhanden:
                ShowWarning(
                    "Diese IP-Adresse existiert in diesem Subnet bereits.",
                    "Doppelter Eintrag");
                return;

            case IPAdresseAddResult.Erfolgreich:
                IpAdresseTextBox.Clear();
                LoadIpAdressen(subnet.Id);
                break;
        }
    }

    private void AdvanceStatusButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!TryGetSelectedSubnet(out var subnet)
            || !TryGetSelectedIPAdresse(out var ipAdresse))
        {
            return;
        }

        _ipAdresseService.AdvanceStatus(ipAdresse.Id);
        LoadIpAdressen(subnet.Id);
    }

    private void DeleteIpAdresseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!TryGetSelectedSubnet(out var subnet)
            || !TryGetSelectedIPAdresse(out var ipAdresse))
        {
            return;
        }

        _ipAdresseService.Delete(ipAdresse.Id);
        LoadIpAdressen(subnet.Id);
    }
}
