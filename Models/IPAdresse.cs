namespace IPAM_WPF_App.Models;

public class IPAdresse
{
    public int Id { get; set; }
    public string Adresse { get; set; } = string.Empty;
    public IpStatus Status { get; set; } = IpStatus.Frei;
    public int SubnetId { get; set; }
}