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

## Git / GitHub


Die eigene K1-Service-Version wurde anschließend in einem separaten
Branch gesichert:

``` bash
git switch -c feature/k1-services
git push -u origin feature/k1-services
```


## Ergebnis

Die Anwendung läuft wieder erfolgreich mit der neuen
Schichtenarchitektur.\
Die Änderungen sind im Branch **`feature/k1-services`** auf GitHub
gesichert.
