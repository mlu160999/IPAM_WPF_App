using IPAM_WPF_App.Models;

namespace IPAM_WPF_App.Dtos;

public class IPAdresseDto
{
    public int Id { get; set; }
    public string Adresse { get; set; } = string.Empty;
    public IPStatus Status { get; set; }
}