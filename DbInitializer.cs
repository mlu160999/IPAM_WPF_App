using Microsoft.Data.Sqlite;

namespace IPAM_WPF_App;

public static class DbInitializer
{
    private const string ConnectionString = "Data Source=app.db";

    public static void Initialize()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using (var pragmaCommand = connection.CreateCommand())
        {
            pragmaCommand.CommandText = "PRAGMA foreign_keys = ON;";
            pragmaCommand.ExecuteNonQuery();
        }

        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Subnet (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Beschreibung TEXT,
                Erstellt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS IPAdresse (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Titel TEXT NOT NULL,
                Erledigt INTEGER NOT NULL DEFAULT 0
                    CHECK (Erledigt IN (0, 1, 2)),
                ProjektId INTEGER NOT NULL,
                FOREIGN KEY (ProjektId) REFERENCES Subnet(Id) ON DELETE CASCADE,
                UNIQUE (ProjektId, Titel)
            );
        ";
        command.ExecuteNonQuery();
    }
}
