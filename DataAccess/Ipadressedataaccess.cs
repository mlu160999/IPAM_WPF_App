using IPAM_WPF_App.Models;
using Microsoft.Data.Sqlite;

namespace IPAM_WPF_App.DataAccess;

public class IPAdresseDataAccess
{
    public List<IPAdresse> GetBySubnetId(int subnetId)
    {
        var addresses = new List<IPAdresse>();

        using var connection = DbInitializer.CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Adresse, Status, SubnetId
            FROM IPAdresse
            WHERE SubnetId = $subnetId
            ORDER BY Adresse;";
        command.Parameters.AddWithValue("$subnetId", subnetId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            addresses.Add(new IPAdresse
            {
                Id = reader.GetInt32(0),
                Adresse = reader.GetString(1),
                Status = (IpStatus)reader.GetInt32(2),
                SubnetId = reader.GetInt32(3)
            });
        }

        return addresses;
    }

    public bool TryAdd(string adresse, int subnetId)
    {
        try
        {
            using var connection = DbInitializer.CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO IPAdresse (Adresse, Status, SubnetId)
                VALUES ($adresse, $status, $subnetId);";

            command.Parameters.AddWithValue("$adresse", adresse);
            command.Parameters.AddWithValue("$status", (int)IpStatus.Frei);
            command.Parameters.AddWithValue("$subnetId", subnetId);
            command.ExecuteNonQuery();
            return true;
        }
        catch (SqliteException ex) when (ex.SqliteExtendedErrorCode == 2067)
        {
            return false;
        }
    }

    public void Update(IPAdresse ipAdresse)
    {
        using var connection = DbInitializer.CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE IPAdresse
            SET Adresse = $adresse,
                Status = $status,
                SubnetId = $subnetId
            WHERE Id = $id;";

        command.Parameters.AddWithValue("$adresse", ipAdresse.Adresse);
        command.Parameters.AddWithValue("$status", (int)ipAdresse.Status);
        command.Parameters.AddWithValue("$subnetId", ipAdresse.SubnetId);
        command.Parameters.AddWithValue("$id", ipAdresse.Id);
        command.ExecuteNonQuery();
    }

    public void UpdateStatus(int id, IpStatus neuerStatus)
    {
        using var connection = DbInitializer.CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE IPAdresse
            SET Status = $status
            WHERE Id = $id;";

        command.Parameters.AddWithValue("$status", (int)neuerStatus);
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = DbInitializer.CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM IPAdresse WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }
}