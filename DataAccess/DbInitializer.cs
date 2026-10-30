using Microsoft.Data.Sqlite;

namespace IPAM_WPF_App.DataAccess;

public static class DbInitializer
{
    public const string ConnectionString = "Data Source=app.db";

    public static SqliteConnection CreateOpenConnection()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var pragmaCommand = connection.CreateCommand();
        pragmaCommand.CommandText = "PRAGMA foreign_keys = ON;";
        pragmaCommand.ExecuteNonQuery();

        return connection;
    }

    public static void Initialize()
    {
        using var connection = CreateOpenConnection();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Subnet (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Beschreibung TEXT,
                    Erstellt TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS IPAdresse (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Adresse TEXT NOT NULL,
                    Status INTEGER NOT NULL DEFAULT 0
                        CHECK (Status IN (0, 1, 2)),
                    SubnetId INTEGER NOT NULL,
                    FOREIGN KEY (SubnetId) REFERENCES Subnet(Id) ON DELETE CASCADE,
                    UNIQUE (SubnetId, Adresse)
                );";
            command.ExecuteNonQuery();
        }

        RenameColumnIfExists(connection, "IPAdresse", "Titel", "Adresse");
        RenameColumnIfExists(connection, "IPAdresse", "Erledigt", "Status");
        RenameColumnIfExists(connection, "IPAdresse", "ProjektId", "SubnetId");
    }

    private static void RenameColumnIfExists(
        SqliteConnection connection,
        string table,
        string oldName,
        string newName)
    {
        using var checkCommand = connection.CreateCommand();
        checkCommand.CommandText =
            $"SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name = $name;";
        checkCommand.Parameters.AddWithValue("$name", oldName);

        long count = (long)checkCommand.ExecuteScalar()!;
        if (count == 0)
        {
            return;
        }

        using var renameCommand = connection.CreateCommand();
        renameCommand.CommandText =
            $"ALTER TABLE {table} RENAME COLUMN {oldName} TO {newName};";
        renameCommand.ExecuteNonQuery();
    }
}