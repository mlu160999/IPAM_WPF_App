using Microsoft.Data.Sqlite;

namespace IPAM_WPF_App;

public class IPAdresse
{
    public int Id { get; set; }
    public string Titel { get; set; } = string.Empty;
    public int Erledigt { get; set; }
    public int ProjektId { get; set; }

    public string Status => Erledigt switch
    {
        0 => "Frei",
        1 => "Reserviert",
        2 => "Zugewiesen",
        _ => "Unbekannt"
    };
}