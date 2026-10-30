using IPAM_WPF_App.Models;

namespace IPAM_WPF_App.DataAccess;

public interface IIPAdresseRepository : IRepository<IPAdresse>
{
    List<IPAdresse> GetBySubnetId(int subnetId);
    bool ExistsInSubnet(string adresse, int subnetId);
}