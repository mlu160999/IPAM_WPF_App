using Microsoft.Data.Sqlite;

namespace IPAM_WPF_App;

public class Subnet
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Beschreibung { get; set; } = string.Empty;
    public string Erstellt { get; set; } = string.Empty;
}