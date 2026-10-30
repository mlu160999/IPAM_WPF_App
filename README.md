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
- Entity Framework Core 10 (ORM, SQLite-Provider)
- LINQ
- Code-Behind
- Git / GitHub

> Das Projekt verwendet bewusst **kein MVVM**. Seit K2 erfolgt der Datenzugriff über **Entity Framework Core** statt über direkte SQL-Abfragen.

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
| Status | INTEGER | 0 = Frei, 1 = Reserviert, 2 = Zugewiesen (Enum `IPStatus`) |
| SubnetId | INTEGER | Fremdschlüssel zum Subnetz |

## Projektstruktur

```text
IPAM_WPF_App/
├── DataAccess/
│   ├── AppDbContext.cs
│   ├── DbInitializer.cs
│   ├── IPAdresseDataAccess.cs
│   └── SubnetDataAccess.cs
├── Doku/
│   ├── K0 Ki gestützte apperstellung 0.1.md
│   ├── K1 schichtenarchitektur git aenderungen.md
│   └── K2 ORM Entity Framework Core.md
├── Migrations/
│   └── (von EF Core generierte Migrations)
├── Models/
│   ├── IPAdresse.cs
│   ├── IPStatus.cs
│   └── Subnet.cs
├── Services/
│   ├── ApplicationService.cs
│   ├── IPAdresseService.cs
│   └── SubnetService.cs
├── App.xaml / App.xaml.cs
├── MainWindow.xaml / MainWindow.xaml.cs
├── IPAM_WPF_App.csproj
├── IPAM_WPF_App.slnx
└── README.md
```

### Wichtige Dateien

**MainWindow.xaml / MainWindow.xaml.cs**  
Grafische Benutzeroberfläche und Code-Behind. Das Code-Behind nimmt nur Benutzeraktionen entgegen und ruft die Services auf – kein SQL, kein direkter Datenbankzugriff.

**Services/**  
Business-Logik: Eingabeprüfung, IP-Validierung, Status-Workflow. `SubnetService` und `IPAdresseService` rufen die DataAccess-Klassen auf, `ApplicationService` initialisiert die Datenbank.

**DataAccess/**  
Datenzugriff mit Entity Framework Core. `AppDbContext` enthält die `DbSet`s, den Connection-String und die Konfiguration der Tabellen. `DbInitializer` wendet beim Start die Migrations an. `SubnetDataAccess` und `IPAdresseDataAccess` arbeiten mit LINQ.

**Migrations/**  
Von EF Core generierter Code, der die Datenbank aus den Entity-Klassen erstellt (`InitialDbCreation`).

**Models/**  
Entity-Klassen `Subnet` und `IPAdresse` (mit Navigation Properties für die 1:n-Beziehung) sowie das Enum `IPStatus`.

**App.xaml / App.xaml.cs**  
Grundlegende WPF-Anwendungskonfiguration.

## Datenbankzugriff

Der Zugriff auf SQLite erfolgt über **Entity Framework Core** (ORM). Statt SQL-Strings werden C#-Objekte und LINQ verwendet, EF Core übersetzt diese in SQL:

```csharp
using var context = new AppDbContext();
return context.IPAdressen
    .Where(a => a.SubnetId == subnetId)
    .OrderBy(a => a.Adresse)
    .ToList();
```

Die Datenbank wird über **Migrations** aus den Entity-Klassen erzeugt. Beim Start ruft die App `context.Database.Migrate()` auf und erstellt bzw. aktualisiert die Datei `app.db` automatisch.

Neue Migration nach einer Änderung an den Entity-Klassen (Paket-Manager-Konsole):

```text
Add-Migration <SprechenderName>
```

## Anwendung starten

### Voraussetzungen

- Windows
- Visual Studio 2026 oder eine Entwicklungsumgebung mit Unterstützung für .NET 10
- .NET 10 SDK

### Start

1. Repository klonen oder als ZIP herunterladen.
2. `IPAM_WPF_App.slnx` in Visual Studio öffnen (nicht nur den Ordner, sonst werden die NuGet-Pakete nicht geladen).
3. NuGet-Pakete wiederherstellen.
4. Projekt kompilieren.
5. Anwendung mit **Start** oder `F5` ausführen.

Beim ersten Start wird die SQLite-Datenbank automatisch aus den Migrations erstellt.

> Eine `app.db` aus der Zeit vor K2 (mit SQL erstellt) muss vorher aus `bin\Debug\net10.0-windows\` gelöscht werden.

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
DataAccess (LINQ)
      ↓
AppDbContext (Entity Framework Core)
      ↓
SQLite-Datenbank (app.db)
```

Details zu den Umbauten stehen im Ordner `Doku/` (K1: Schichtenarchitektur, K2: Entity Framework Core).

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

Die Dokumentation zu den einzelnen Kapiteln befindet sich im Ordner `Doku/`:

- `K0 Ki gestützte apperstellung 0.1.md`: KI-gestützte Erstellung der App (K0) und KI-Protokoll
- `K1 schichtenarchitektur git aenderungen.md`: Umbau in die Schichtenarchitektur
- `K2 ORM Entity Framework Core.md`: Umstellung auf Entity Framework Core

## Autor

Schulprojekt im Rahmen der Ausbildung an der GIBB Bern.