namespace IPAM_WPF_App.Models;

public class IPAdresse
{
    public int Id { get; set; }
    public string Adresse { get; set; } = string.Empty;
    public IPStatus Status { get; set; } = IPStatus.Frei;

    public int SubnetId { get; set; }
    public Subnet? Subnet { get; set; }
}