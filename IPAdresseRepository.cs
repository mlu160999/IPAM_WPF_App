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

public class IPAdresseRepository
{
    private const string ConnectionString = "Data Source=app.db";

    private static SqliteConnection CreateOpenConnection()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var pragmaCommand = connection.CreateCommand();
        pragmaCommand.CommandText = "PRAGMA foreign_keys = ON;";
        pragmaCommand.ExecuteNonQuery();

        return connection;
    }

    public List<IPAdresse> GetBySubnetId(int subnetId)
    {
        var addresses = new List<IPAdresse>();

        using var connection = CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Titel, Erledigt, ProjektId
            FROM IPAdresse
            WHERE ProjektId = $subnetId
            ORDER BY Titel;";
        command.Parameters.AddWithValue("$subnetId", subnetId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            addresses.Add(new IPAdresse
            {
                Id = reader.GetInt32(0),
                Titel = reader.GetString(1),
                Erledigt = reader.GetInt32(2),
                ProjektId = reader.GetInt32(3)
            });
        }

        return addresses;
    }

    public int Add(string titel, int subnetId)
    {
        using var connection = CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO IPAdresse (Titel, Erledigt, ProjektId)
            VALUES ($titel, 0, $subnetId);
            SELECT last_insert_rowid();";

        command.Parameters.AddWithValue("$titel", titel);
        command.Parameters.AddWithValue("$subnetId", subnetId);

        return Convert.ToInt32((long)command.ExecuteScalar()!);
    }

    public void Update(IPAdresse ipAdresse)
    {
        using var connection = CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE IPAdresse
            SET Titel = $titel,
                Erledigt = $erledigt,
                ProjektId = $projektId
            WHERE Id = $id;";

        command.Parameters.AddWithValue("$titel", ipAdresse.Titel);
        command.Parameters.AddWithValue("$erledigt", ipAdresse.Erledigt);
        command.Parameters.AddWithValue("$projektId", ipAdresse.ProjektId);
        command.Parameters.AddWithValue("$id", ipAdresse.Id);
        command.ExecuteNonQuery();
    }

    public void UpdateStatus(int id, int neuerStatus)
    {
        using var connection = CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE IPAdresse
            SET Erledigt = $status
            WHERE Id = $id;";

        command.Parameters.AddWithValue("$status", neuerStatus);
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM IPAdresse WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }
}
