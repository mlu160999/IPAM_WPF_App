# IPAM_WPF_App

Eine einfache **IP Address Management (IPAM)** Desktop-Applikation, entwickelt mit **C#**, **WPF** und **.NET 10**.

Die Anwendung dient zur Verwaltung von Subnetzen und IP-Adressen. Die Daten werden lokal in einer **SQLite-Datenbank** gespeichert.

## Projektziel

Ziel des Projekts ist es, eine funktionierende WPF-Anwendung zu erstellen, mit der IP-Adressen strukturiert verwaltet werden können.

Das Projekt wurde im Rahmen einer Schulaufgabe erstellt und wird schrittweise erweitert und verbessert.

## Technologien

- C#
- WPF (Windows Presentation Foundation)
- .NET 10
- SQLite
- Microsoft.Data.Sqlite
- Direkte SQL-Abfragen
- Code-Behind
- Git / GitHub

> Das Projekt verwendet bewusst **kein MVVM**, **kein Entity Framework** und **kein ORM**.

## Funktionen

Die Anwendung ermöglicht unter anderem:

- Subnetze anzeigen und verwalten
- Neue Subnetze erfassen
- IP-Adressen anzeigen und verwalten
- Neue IP-Adressen erfassen
- IP-Adressen einem Subnetz zuordnen
- Status einer IP-Adresse ändern
- Daten dauerhaft in SQLite speichern
- Vorhandene Einträge bearbeiten
- Einträge löschen
- IP-Adressen nach Subnetz anzeigen bzw. filtern

## Datenmodell

Zwischen **Subnetz** und **IP-Adresse** besteht eine **1:N-Beziehung**.

Ein Subnetz kann mehrere IP-Adressen enthalten. Eine IP-Adresse gehört jeweils zu einem Subnetz.

### Subnet

| Feld | Typ | Beschreibung |
|---|---|---|
| Id | INTEGER | Primärschlüssel |
| Name | TEXT | Name des Subnetzes |
| Beschreibung | TEXT | Beschreibung |
| Erstellt | TEXT | Erstellungsdatum |

### IP-Adresse

| Feld | Typ | Beschreibung |
|---|---|---|
| Id | INTEGER | Primärschlüssel |
| Adresse | TEXT | IP-Adresse (IPv4 oder IPv6) |
| Status | INTEGER | 0 = Frei, 1 = Reserviert, 2 = Zugewiesen (Enum `IpStatus`) |
| SubnetId | INTEGER | Fremdschlüssel zum Subnetz |

## Projektstruktur

```text
IPAM_WPF_App/
├── DataAccess/
│   ├── DbInitializer.cs
│   ├── IPAdresseDataAccess.cs
│   └── SubnetDataAccess.cs
├── Models/
│   ├── IPAdresse.cs
│   ├── IpStatus.cs
│   └── Subnet.cs
├── Services/
│   ├── ApplicationService.cs
│   ├── IPAdresseService.cs
│   └── SubnetService.cs
├── App.xaml / App.xaml.cs
├── MainWindow.xaml / MainWindow.xaml.cs
├── IPAM_WPF_App.csproj
├── K1_Schichtenarchitektur_Git_Aenderungen.md
├── KI_Gestuetzte_Apperstellung_V0.1.md
└── README.md
```

### Wichtige Dateien

**MainWindow.xaml / MainWindow.xaml.cs**  
Grafische Benutzeroberfläche und Code-Behind. Das Code-Behind nimmt nur Benutzeraktionen entgegen und ruft die Services auf – kein SQL, kein direkter Datenbankzugriff.

**Services/**  
Business-Logik: Eingabeprüfung, IP-Validierung, Status-Workflow. `SubnetService` und `IPAdresseService` rufen die DataAccess-Klassen auf, `ApplicationService` initialisiert die Datenbank.

**DataAccess/**  
Direkte SQL-Abfragen mit `Microsoft.Data.Sqlite`. `DbInitializer` erstellt die Tabellen und stellt den zentralen Connection-String bereit.

**Models/**  
Reine Datenklassen `Subnet` und `IPAdresse` sowie das Enum `IpStatus`.

**App.xaml / App.xaml.cs**  
Grundlegende WPF-Anwendungskonfiguration.

## Datenbankzugriff

Der Zugriff auf SQLite erfolgt direkt mit `Microsoft.Data.Sqlite`.

Verwendet werden unter anderem:

```csharp
SqliteConnection
SqliteCommand
SqliteDataReader
```

Dadurch werden die SQL-Abfragen direkt im C#-Code ausgeführt.

## Anwendung starten

### Voraussetzungen

- Windows
- Visual Studio 2026 oder eine Entwicklungsumgebung mit Unterstützung für .NET 10
- .NET 10 SDK

### Start

1. Repository klonen oder als ZIP herunterladen.
2. `IPAM_WPF_App.csproj` in Visual Studio öffnen.
3. NuGet-Pakete wiederherstellen.
4. Projekt kompilieren.
5. Anwendung mit **Start** oder `F5` ausführen.

Beim ersten Start wird die benötigte SQLite-Datenbank automatisch initialisiert.

## Bedienung

Nach dem Start können Subnetze und IP-Adressen über die Benutzeroberfläche verwaltet werden.

Der grundlegende Ablauf ist:

1. Subnetz erstellen oder auswählen.
2. IP-Adresse erfassen.
3. IP-Adresse dem gewünschten Subnetz zuordnen.
4. Status der IP-Adresse bei Bedarf ändern.
5. Änderungen werden in der SQLite-Datenbank gespeichert.

## Architektur

Seit K1 verwendet die Anwendung eine **Schichtenarchitektur**. Jede Schicht kennt nur die direkt darunterliegende:

```text
Benutzeroberfläche (MainWindow.xaml.cs)
      ↓
Services (Business-Logik)
      ↓
DataAccess (SQL mit Microsoft.Data.Sqlite)
      ↓
SQLite-Datenbank (app.db)
```

Details zum Umbau stehen in `K1_Schichtenarchitektur_Git_Aenderungen.md`.

## Aktueller Stand

Die Grundfunktionen der IPAM-Anwendung sind implementiert. Subnetze und IP-Adressen können erfasst und verwaltet werden. Auch Statusänderungen und die Speicherung in der SQLite-Datenbank funktionieren.

Das Projekt kann für weitere Aufgaben und Anforderungen schrittweise erweitert werden.

## Mögliche Erweiterungen

Spätere Erweiterungen können beispielsweise sein:

- Excel-Import von IP-Adressen
- Erweiterte Suche und Filter
- Zusätzliche Statuswerte
- Verbesserte Benutzeroberfläche
- Exportfunktionen

## Dokumentation

Zusätzliche Informationen zur KI-gestützten Erstellung und Bearbeitung des Projekts befinden sich in:

`KI_Gestuetzte_Apperstellung_V0.1.md`

## Autor

Schulprojekt im Rahmen der Ausbildung an der GIBB Bern.