# K1 -- Schichtenarchitektur und Git-Änderungen

## Ziel

Die bestehende IPAM-WPF-App wurde für K1 in eine
**Schichtenarchitektur** umgebaut.\
Die Funktionalität bleibt grundsätzlich gleich, aber die
Verantwortlichkeiten sind nun sauber getrennt:

**UI → Services → DataAccess → SQLite**

## Neue Projektstruktur

``` text
IPAM_WPF_App/
├── Models/
│   ├── IPAdresse.cs
│   ├── IpStatus.cs
│   └── Subnet.cs
├── Services/
│   ├── ApplicationService.cs
│   ├── IPAdresseService.cs
│   └── SubnetService.cs
├── DataAccess/
│   ├── DbInitializer.cs
│   ├── IPAdresseDataAccess.cs
│   └── SubnetDataAccess.cs
├── App.xaml
└── MainWindow.xaml
```

## Durchgeführte Änderungen

-   Die alten Klassen `SubnetRepository.cs` und `IPAdresseRepository.cs`
    wurden entfernt.
-   Die Datenbankzugriffe wurden in die Klassen unter `DataAccess/`
    verschoben.
-   Die Datenmodelle `Subnet` und `IPAdresse` befinden sich unter
    `Models/`.
-   Die Business-Logik wurde in `Services/` ausgelagert.
-   `IPAdresseService` übernimmt unter anderem die IP-Validierung und
    den Status-Workflow.
-   Die UI greift nicht mehr direkt auf die Datenbank zu, sondern
    verwendet die Services.
-   Die Namespaces wurden entsprechend angepasst:
    -   `IPAM_WPF_App.Models`
    -   `IPAM_WPF_App.Services`
    -   `IPAM_WPF_App.DataAccess`
-   Nach der Umstrukturierung wurde die Solution neu gebaut und die
    Anwendung erfolgreich getestet.

## Nachbesserungen nach Review

-   Datei `IPAddresseService.cs` in `IPAdresseService.cs` umbenannt
    (Dateiname = Klassenname).
-   Connection-String nur noch zentral in `DbInitializer.ConnectionString`;
    die DataAccess-Klassen öffnen Verbindungen über
    `DbInitializer.CreateOpenConnection()`.
-   Spalten von `IPAdresse` sprechend benannt (Vorbereitung K2 / EF Core):
    `Titel` → `Adresse`, `Erledigt` → `Status`, `ProjektId` → `SubnetId`.
    Eine bestehende `app.db` wird beim Start automatisch umbenannt,
    die Daten bleiben erhalten.
-   Status als Enum `IpStatus` (Frei, Reserviert, Zugewiesen) statt
    Zahlen 0/1/2. Der Workflow liegt weiterhin im `IPAdresseService`.
-   Unbenutztes NuGet-Paket `EPPlus` entfernt.
-   Die `Update`-Methoden in den DataAccess-Klassen bleiben bewusst
    bestehen: Sie werden in K3 für `IRepository<T>` benötigt.

## Übersicht: Serviceklasse → DataAccess

| Serviceklasse | Service-Methode | Ruft auf in DataAccess |
|---|---|---|
| ApplicationService | InitializeDatabase() | DbInitializer.Initialize() |
| SubnetService | GetAll() | SubnetDataAccess.GetAll() |
| SubnetService | Add(name, beschreibung) | SubnetDataAccess.Add(name, beschreibung) |
| SubnetService | Update(subnet) | SubnetDataAccess.Update(subnet) |
| SubnetService | Delete(subnetId) | SubnetDataAccess.Delete(subnetId) |
| IPAdresseService | GetBySubnetId(subnetId) | IPAdresseDataAccess.GetBySubnetId(subnetId) |
| IPAdresseService | Add(ipText, subnetId) | IPAdresseDataAccess.TryAdd(ipText, subnetId) |
| IPAdresseService | AdvanceStatus(ipAdresse) | IPAdresseDataAccess.UpdateStatus(id, neuerStatus) |
| IPAdresseService | Delete(ipAdresseId) | IPAdresseDataAccess.Delete(ipAdresseId) |

Business-Logik im Service (nicht in DataAccess):

-   `SubnetService.Add` / `Update`: Eingaben trimmen, leere Namen ablehnen.
-   `IPAdresseService.Add`: IP-Adresse mit `IPAddress.TryParse` prüfen,
    Duplikate als `BereitsVorhanden` melden.
-   `IPAdresseService.AdvanceStatus`: Workflow
    Frei → Reserviert → Zugewiesen → Frei.

## Git / GitHub

Die erste K1-Version wurde im Branch `feature/k1-services` erstellt.
Nach dem Review wurde der vollständige K1-Stand inklusive Nachbesserungen
in einem eigenen Branch auf Basis von `master` umgesetzt:

``` bash
git switch -c sabri/feature/k1-schichten
git push -u origin sabri/feature/k1-schichten
```

Commits in Schritten:

-   Review-Verbesserungen (DataAccess, Models, IpStatus, csproj)
-   Services ergänzt und UI auf Service-Schicht umgestellt
-   Dateinamen an Klassennamen angepasst
-   Doku ergänzt (Übersichtstabelle, README, KI-Protokoll)

Anschliessend Merge Request `sabri/feature/k1-schichten` → `master`
mit Review durch ein Gruppenmitglied.

## Ergebnis

Die Anwendung läuft mit der neuen Schichtenarchitektur
(UI → Services → DataAccess → SQLite).\
Die Änderungen sind im Branch **`sabri/feature/k1-schichten`** auf GitHub
gesichert und werden per Merge Request in `master` übernommen.