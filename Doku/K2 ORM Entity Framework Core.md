# K2 -- ORM / Entity Framework Core

## Ziel

Die DataAccess-Schicht der IPAM-WPF-App wurde von rohem ADO.NET
(`SqliteConnection`, `SqliteCommand`, SQL-Strings) auf
**Entity Framework Core** umgestellt.\
Die Funktionalität bleibt gleich. UI und Service-Schicht wurden
**nicht verändert**, nur das Innere der DataAccess-Klassen.

**UI → Services → DataAccess → AppDbContext (EF Core) → SQLite**

## Projektstruktur nach K2

``` text
IPAM_WPF_App/
├── DataAccess/
│   ├── AppDbContext.cs          (neu)
│   ├── DbInitializer.cs         (geändert)
│   ├── IPAdresseDataAccess.cs   (geändert)
│   └── SubnetDataAccess.cs      (geändert)
├── Migrations/                  (neu, von EF Core generiert)
│   ├── <Zeitstempel>_InitialDbCreation.cs
│   ├── <Zeitstempel>_InitialDbCreation.Designer.cs
│   └── AppDbContextModelSnapshot.cs
├── Models/
│   ├── IPAdresse.cs             (geändert)
│   ├── IPStatus.cs
│   └── Subnet.cs                (geändert)
├── Services/                    (unverändert)
│   ├── ApplicationService.cs
│   ├── IPAdresseService.cs
│   └── SubnetService.cs
├── App.xaml / App.xaml.cs       (unverändert)
├── MainWindow.xaml / .xaml.cs   (unverändert)
└── IPAM_WPF_App.csproj          (geändert)
```

## Durchgeführte Änderungen

### 1. NuGet-Pakete (`IPAM_WPF_App.csproj`)

| Paket | Version | Zweck |
|---|---|---|
| `Microsoft.EntityFrameworkCore.Sqlite` | 10.0.10 | EF Core mit SQLite-Provider |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.10 | Wird für Migrations benötigt (nur Entwicklungszeit) |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.10 | Stellt `Add-Migration` / `Update-Database` in der Paket-Manager-Konsole bereit |
| `SQLitePCLRaw.lib.e_sqlite3` | 3.53.3 | Fixiert eine aktuelle SQLite-Engine (siehe unten) |

-   `Microsoft.Data.Sqlite` wurde entfernt, weil EF Core es selbst
    mitbringt.
-   `Design` und `Tools` sind mit `PrivateAssets=all` eingebunden und
    werden nicht mit der App ausgeliefert.
-   Das Paket `Tools` steht nicht in der Aufgabenstellung, ohne es
    kennt die Paket-Manager-Konsole den Befehl `Add-Migration` aber
    nicht.

**Sicherheitslücke NU1903:** EF Core bringt standardmässig die
SQLite-Engine `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 mit. Diese hat eine
bekannte Sicherheitslücke (GHSA-2m69-gcr7-jv3q, Speicherfehler in
SQLite vor 3.50.2). Durch die explizite Referenz auf Version 3.53.3
wird eine aktuelle Engine verwendet und die Warnung verschwindet. Die
Warnung hat ausserdem `Add-Migration` in der Paket-Manager-Konsole
abbrechen lassen.

### 2. `DataAccess/AppDbContext.cs` (neu)

Zentrale Klasse für den Datenbankzugriff, erbt von `DbContext`.

-   **Connection-String** `Data Source=app.db` liegt neu hier (vorher in
    `DbInitializer`).
-   **`DbSet<Subnet> Subnets`** und **`DbSet<IPAdresse> IPAdressen`**:
    je ein DbSet pro Entity, darüber wird mit LINQ abgefragt.
-   **`OnConfiguring`**: verbindet den Context mit SQLite (`UseSqlite`).
-   **`OnModelCreating`** (Fluent API): bildet alle Regeln des
    bisherigen `CREATE TABLE` nach, damit sich die Datenbank gleich
    verhält:

| Bisher (SQL) | Jetzt (Fluent API) |
|---|---|
| Tabellenname `Subnet` / `IPAdresse` | `ToTable("Subnet")` / `ToTable("IPAdresse")` |
| `CHECK (Status IN (0, 1, 2))` | `HasCheckConstraint("CK_IPAdresse_Status", ...)` |
| `UNIQUE (SubnetId, Adresse)` | `HasIndex(a => new { a.SubnetId, a.Adresse }).IsUnique()` |
| `FOREIGN KEY ... ON DELETE CASCADE` | `HasOne(...).WithMany(...).HasForeignKey(...).OnDelete(Cascade)` |

Ohne `ToTable` würde EF Core die Tabellen nach den DbSets benennen
(`Subnets`, `IPAdressen`).

### 3. `DataAccess/DbInitializer.cs` (geändert)

-   Vorher: `SqliteConnection`, `PRAGMA foreign_keys`, `CREATE TABLE`
    und eine Umbenennungslogik für alte Spalten.
-   Jetzt: nur noch `context.Database.Migrate()`.

`Migrate()` führt beim App-Start alle noch nicht angewendeten
Migrations aus. Beim ersten Start wird die Datenbank erstellt, bei
späteren Änderungen werden nur neue Migrations nachgespielt. EF Core
merkt sich in der Tabelle `__EFMigrationsHistory`, was schon
angewendet wurde.

**Warum `Migrate()` beim Start statt nur `Update-Database`?**\
`Update-Database` erstellt die `app.db` im Projektordner. Die App
läuft aber aus `bin\Debug\net10.0-windows\` und sucht die Datei dort.
Mit `Migrate()` entsteht die Datenbank immer dort, wo die App sie
braucht.

Die Klasse heisst weiterhin `DbInitializer`, damit der
`ApplicationService` unverändert bleibt.

### 4. `DataAccess/SubnetDataAccess.cs` und `IPAdresseDataAccess.cs` (geändert)

Die **Methodensignaturen (Name, Parameter, Rückgabewert) sind
unverändert** (Aufgabe B3.8). Jede Methode folgt demselben Ablauf:
**Context erstellen → mit Daten arbeiten → `SaveChanges()`**, mit
`using` für jeden Context (Aufgabe B3.9).

| Bisher (ADO.NET) | Jetzt (EF Core / LINQ) |
|---|---|
| `SELECT ... WHERE SubnetId = $id ORDER BY Adresse` | `.Where(a => a.SubnetId == id).OrderBy(a => a.Adresse).ToList()` |
| `INSERT` + `SELECT last_insert_rowid()` | `.Add(subnet)` + `SaveChanges()`, danach ist `subnet.Id` gesetzt |
| `UPDATE ... SET ...` | Objekt mit `Find()` laden, Properties ändern, `SaveChanges()` |
| `DELETE FROM ...` | `Find()` + `Remove()` + `SaveChanges()` |
| `SqliteException` mit Code 2067 abfangen | Vorher mit `.Any(...)` prüfen, ob die Adresse existiert |

Bewusste Entscheidungen:

-   **`Update()` lädt das Objekt und setzt einzelne Felder**, statt
    `context.Update()` zu verwenden. So werden wie bisher nur Name und
    Beschreibung geändert, `Erstellt` bleibt unverändert.
-   **`TryAdd()` prüft Duplikate mit `Any()`**. Mit EF Core käme der
    Datenbankfehler verpackt als `DbUpdateException`, das Abfangen wäre
    umständlich und an SQLite-Fehlercodes gebunden. Der UNIQUE-Index in
    der Datenbank bleibt als Sicherheitsnetz bestehen.
-   Enums (`IPStatus`) werden von EF Core automatisch als Zahl
    gespeichert, die `(int)`-Casts sind entfallen.
-   Verbindungen und `PRAGMA foreign_keys` verwaltet EF Core selbst,
    `CreateOpenConnection()` ist entfallen.

### 5. `Models/Subnet.cs` und `Models/IPAdresse.cs` (geändert)

Neue **Navigation Properties** für die 1:n-Beziehung:

``` csharp
// Subnet.cs: ein Subnet hat viele IP-Adressen
public ICollection<IPAdresse> IPAdressen { get; set; } = new List<IPAdresse>();

// IPAdresse.cs: eine IP-Adresse gehört zu einem Subnet
public Subnet? Subnet { get; set; }
```

-   Werden in `AppDbContext` für `HasOne` / `WithMany` verwendet.
-   Sind standardmässig leer (`null` bzw. leere Liste), solange sie
    nicht mit `.Include(...)` mitgeladen werden (siehe Lazy Loading /
    N+1 in Kapitel 2.6).
-   Grundlage für die K2-Vertiefung (`Include`) und die DTOs in K3
    (z. B. `SubnetName = a.Subnet.Name`).

### 6. Migration `InitialDbCreation` (neu)

Erstellt mit der Paket-Manager-Konsole:

``` text
Add-Migration InitialDbCreation
```

-   `<Zeitstempel>_InitialDbCreation.cs`: erstellt die Tabellen
    `Subnet` und `IPAdresse` inkl. Fremdschlüssel (Cascade), CHECK und
    Unique-Index.
-   `<Zeitstempel>_InitialDbCreation.Designer.cs`: Metadaten zur
    Migration.
-   `AppDbContextModelSnapshot.cs`: aktueller Stand des Modells. EF
    Core vergleicht bei der nächsten Migration damit.

Prüfung der generierten Migration gegen das bisherige Schema (Aufgabe
B2.6): Tabellen `Subnet` und `IPAdresse`, Spalten, Fremdschlüssel mit
`ReferentialAction.Cascade`, CHECK-Constraint auf `Status` und
Unique-Index auf `(SubnetId, Adresse)` sind enthalten.

**Hinweis:** Die alte `app.db` aus K0/K1 musste gelöscht werden. Sie
wurde mit rohem SQL erstellt und hat keine Tabelle
`__EFMigrationsHistory`. `Migrate()` hätte sonst versucht, die
Tabellen erneut anzulegen ("table already exists"). Beim nächsten
Start wurde die Datenbank aus der Migration neu erstellt.

### Unverändert

`Services/*`, `MainWindow.xaml(.cs)`, `App.xaml(.cs)` und
`Models/IPStatus.cs` wurden nicht angepasst. Das zeigt, dass durch die
Schichtentrennung aus K1 nur die DataAccess-Schicht umgebaut werden
musste.

## Kontrolle (B4)

-   App getestet: Subnet hinzufügen/löschen, IP hinzufügen, Duplikat
    wird erkannt, Status-Workflow Frei → Reserviert → Zugewiesen →
    Frei, IP löschen, beim Löschen eines Subnets werden die IPs
    mitgelöscht, Daten bleiben nach Neustart erhalten.
-   **Kontrollfrage:** Im Ordner `DataAccess` gibt es keine Treffer mehr
    für `SqliteConnection` oder `SqliteCommand`.

## Übersicht: Serviceklasse → DataAccess

Unverändert gegenüber K1, nur die Umsetzung in DataAccess ist neu:

| Serviceklasse | Service-Methode | Ruft auf in DataAccess | Umsetzung mit EF Core |
|---|---|---|---|
| ApplicationService | InitializeDatabase() | DbInitializer.Initialize() | `Database.Migrate()` |
| SubnetService | GetAll() | SubnetDataAccess.GetAll() | `OrderBy` + `ToList` |
| SubnetService | Add(name, beschreibung) | SubnetDataAccess.Add(...) | `Add` + `SaveChanges` |
| SubnetService | Update(subnet) | SubnetDataAccess.Update(subnet) | `Find` + Felder setzen + `SaveChanges` |
| SubnetService | Delete(subnetId) | SubnetDataAccess.Delete(id) | `Find` + `Remove` + `SaveChanges` |
| IPAdresseService | GetBySubnetId(subnetId) | IPAdresseDataAccess.GetBySubnetId(...) | `Where` + `OrderBy` + `ToList` |
| IPAdresseService | Add(ipText, subnetId) | IPAdresseDataAccess.TryAdd(...) | `Any` + `Add` + `SaveChanges` |
| IPAdresseService | AdvanceStatus(ipAdresse) | IPAdresseDataAccess.UpdateStatus(...) | `Find` + Status setzen + `SaveChanges` |
| IPAdresseService | Delete(ipAdresseId) | IPAdresseDataAccess.Delete(id) | `Find` + `Remove` + `SaveChanges` |

## Git / GitHub

Branch: **`feature/k2-efcore`** (von `master`)

Commits in Schritten:

1.  `K2: Ergänzung der EF Core Packages`
2.  `K2: Zentraler Connection String`
3.  `K2: DB Context Anbindung mittels APPDbContext in Models und
    DataAccess für IPAdresse und Subnet`
4.  `K2: Migration InitialDbCreation erstellt und SQLite-Engine auf
    3.53.3 fixiert`

Anschliessend Merge Request `feature/k2-efcore` → `master` mit Review
durch ein Gruppenmitglied.

## Stolpersteine

-   **Visual Studio im Ordner-Modus ("Folder View"):** Die NuGet-Pakete
    wurden nicht wiederhergestellt und der Build schlug fehl
    (`EntityFrameworkCore does not exist`). Lösung: Projekt über
    *File → Open → Project/Solution* mit der `.csproj` öffnen.
-   **Warnung NU1903 bricht `Add-Migration` ab:** siehe
    Sicherheitslücke oben.
-   **Alte `app.db`:** musste gelöscht werden (siehe Migration).

## Ergebnis

Die App funktioniert unverändert, verwendet aber keine SQL-Strings mehr.
Der Datenzugriff läuft über `AppDbContext`, LINQ und Migrations.