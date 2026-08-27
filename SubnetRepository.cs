using Microsoft.Data.Sqlite;

namespace IPAM_WPF_App;

public class Subnet
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Beschreibung { get; set; } = string.Empty;
    public string Erstellt { get; set; } = string.Empty;
}

public class SubnetRepository
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

    public List<Subnet> GetAll()
    {
        var subnets = new List<Subnet>();

        using var connection = CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Beschreibung, Erstellt
            FROM Subnet
            ORDER BY Name;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            subnets.Add(new Subnet
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Beschreibung = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Erstellt = reader.GetString(3)
            });
        }

        return subnets;
    }

    public int Add(string name, string beschreibung)
    {
        using var connection = CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Subnet (Name, Beschreibung, Erstellt)
            VALUES ($name, $beschreibung, $erstellt);
            SELECT last_insert_rowid();";

        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$beschreibung", beschreibung);
        command.Parameters.AddWithValue("$erstellt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        return Convert.ToInt32((long)command.ExecuteScalar()!);
    }

    public void Update(Subnet subnet)
    {
        using var connection = CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Subnet
            SET Name = $name,
                Beschreibung = $beschreibung
            WHERE Id = $id;";

        command.Parameters.AddWithValue("$name", subnet.Name);
        command.Parameters.AddWithValue("$beschreibung", subnet.Beschreibung);
        command.Parameters.AddWithValue("$id", subnet.Id);
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Subnet WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }
}
