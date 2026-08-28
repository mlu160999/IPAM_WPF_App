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
| Titel | TEXT | IP-Adresse bzw. Bezeichnung |
| Erledigt | INTEGER | Status |
| ProjektId | INTEGER | Fremdschlüssel zum Subnetz |

## Projektstruktur

```text
IPAM_WPF_App/
├── App.xaml
├── App.xaml.cs
├── DbInitializer.cs
├── IPAdresseRepository.cs
├── IPAM_WPF_App.csproj
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── SubnetRepository.cs
├── KI_Gestuetzte_Apperstellung_V0.1.md
└── README.md
```

### Wichtige Dateien

**MainWindow.xaml**  
Enthält die grafische Benutzeroberfläche der Anwendung.

**MainWindow.xaml.cs**  
Enthält die Code-Behind-Logik für Benutzeraktionen und die Kommunikation mit den Repositories.

**DbInitializer.cs**  
Initialisiert die SQLite-Datenbank und erstellt die benötigten Tabellen.

**SubnetRepository.cs**  
Enthält die direkten SQL-Abfragen für die Verwaltung der Subnetze.

**IPAdresseRepository.cs**  
Enthält die direkten SQL-Abfragen für die Verwaltung der IP-Adressen.

**App.xaml / App.xaml.cs**  
Enthalten die grundlegende WPF-Anwendungskonfiguration.

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

Die Anwendung verwendet eine einfache **WPF-Code-Behind-Architektur**.

Die Verantwortlichkeiten sind trotzdem auf mehrere Dateien verteilt:

```text
Benutzeroberfläche
      ↓
MainWindow.xaml.cs
      ↓
Repositories
      ↓
Microsoft.Data.Sqlite
      ↓
SQLite-Datenbank
```

Damit bleibt der Datenbankzugriff von einem grossen Teil der UI-Logik getrennt, ohne MVVM oder ein ORM einzusetzen.

## Aktueller Stand

Die Grundfunktionen der IPAM-Anwendung sind implementiert. Subnetze und IP-Adressen können erfasst und verwaltet werden. Auch Statusänderungen und die Speicherung in der SQLite-Datenbank funktionieren.

Das Projekt kann für weitere Aufgaben und Anforderungen schrittweise erweitert werden.

## Mögliche Erweiterungen

Spätere Erweiterungen können beispielsweise sein:

- Excel-Import von IP-Adressen
- Erweiterte Suche und Filter
- Validierung von IP-Adressen
- Erkennung von doppelten IP-Adressen
- Zusätzliche Statuswerte
- Verbesserte Benutzeroberfläche
- Exportfunktionen

## Dokumentation

Zusätzliche Informationen zur KI-gestützten Erstellung und Bearbeitung des Projekts befinden sich in:

`KI_Gestuetzte_Apperstellung_V0.1.md`

## Autor

Schulprojekt im Rahmen der Ausbildung an der GIBB Bern.
