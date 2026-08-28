# MODUL PROG1

## K0 -- KI-GESTÜTZTE APPERSTELLUNG

**Dokument:** KI_Gestützte_Apperstellung_V0.1.docx\
**Authors:** Madalina Luca, Muzamil Abukar Haji Ali, Sabri Mahdi\
**Version:** 0.1

## Inhaltsverzeichnis

-   [Ziel](#ziel)
-   [Aufgabe B: KI-gestützte Entwicklung eines IP Address Management
    Systems
    (IPAM)](#aufgabe-b-ki-gestützte-entwicklung-eines-ip-address-management-systems-ipam)
    -   [Tool](#tool)
    -   [Prompt / Aufgabe](#prompt--aufgabe)
    -   [Ergebnis der KI](#ergebnis-der-ki)
    -   [Ausführung und Test](#ausführung-und-test)
    -   [Erkenntnis](#erkenntnis)

## Ziel

Im Rahmen dieser Aufgabe wird mithilfe von ChatGPT eine einfache
WPF-Desktop-Applikation für ein **IP Address Management System (IPAM)**
entwickelt.

Die Anwendung basiert auf **C# und .NET 10** und verwendet **SQLite**
zur lokalen Datenspeicherung. Der Fokus liegt auf der Verwaltung von
Subnetzen und IP-Adressen sowie auf grundlegenden Funktionen wie
Erfassen, Bearbeiten, Löschen, Suchen und Importieren von Daten.

Zusätzlich wird die Interaktion mit der KI sowie die schrittweise
Anpassung und Verbesserung der generierten Anwendung dokumentiert.

## Aufgabe B: KI-gestützte Entwicklung eines IP Address Management Systems (IPAM)

### Tool

**ChatGPT**

### Prompt / Aufgabe

Erstelle eine WPF Desktop-Applikation in C# mit .NET 10 mit folgenden
Anforderungen:

**App-Idee:** IP Address Management System

#### Technische Rahmenbedingungen

-   WPF mit Code-Behind (**KEIN MVVM, kein INotifyPropertyChanged, kein
    ViewModel!**)
-   SQLite mit `Microsoft.Data.Sqlite` (**KEIN Entity Framework, KEIN
    ORM!**)
-   Nur direkte SQL-Abfragen:
    -   `SqliteConnection`
    -   `SqliteCommand`
    -   `SqliteDataReader`
-   Ziel-Framework: **.NET 10**

#### Datenmodell (1:N-Beziehung)

**Entität 1 -- Subnet**

-   `Id INTEGER PK`
-   `Name TEXT`
-   `Beschreibung TEXT`
-   `Erstellt TEXT`

**Entität 2 -- IP Addresse**

-   `Id INTEGER PK`
-   `Titel TEXT`
-   `Erledigt INTEGER`
-   `ProjektId INTEGER FK`

#### Prozess / Workflow

Ändern einer IP-Adresse von:

`Frei → Reserviert → Zugewiesen`

#### Mindestfunktionen

1.  `DbInitializer.cs` mit `CREATE TABLE IF NOT EXISTS` für beide
    Tabellen inklusive Foreign Key.
2.  Entität 1 anzeigen (`ListBox` oder `DataGrid`).
3.  Entität 1 hinzufügen (Textfeld + Button + `INSERT`) und löschen
    (`DELETE`).
4.  Entität 2 anzeigen, gefiltert nach dem gewählten Eintrag in Entität
    1.
5.  Entität 2 hinzufügen und den definierten Prozess/Workflow ausführen.

#### Struktur

-   Alle DB-Zugriffe in Repository-Klassen, z. B. `ProjektRepository.cs`
    und `AufgabeRepository.cs`.
-   Im Code-Behind nur UI-Logik und Aufrufe der Repository-Methoden.
-   Verbindungszeichenfolge:

``` text
Data Source=app.db
```

#### Zu erstellende Dateien

1.  `MainWindow.xaml` -- vollständiges XAML-Layout
2.  `MainWindow.xaml.cs` -- Code-Behind
3.  `DbInitializer.cs`
4.  Je eine Repository-Klasse pro Entität mit CRUD-Methoden

### Ergebnis der KI

ChatGPT erstellte eine komplette WPF-App als Projekt.

Enthalten waren:

-   `MainWindow.xaml` -- vollständige Oberfläche
-   `MainWindow.xaml.cs` -- UI-Logik und Repository-Aufrufe
-   `DbInitializer.cs` -- SQLite-Tabellen mit
    `CREATE TABLE IF NOT EXISTS` und Foreign Key
-   `SubnetRepository.cs` -- CRUD für Subnetze
-   `IPAdresseRepository.cs` -- CRUD und Status-Workflow für IP-Adressen
-   `App.xaml`
-   `App.xaml.cs`
-   `IPAM_WPF_App.csproj` -- .NET 10, WPF und `Microsoft.Data.Sqlite`

#### Status-Workflow

    Wert Status
  ------ ------------
       0 Frei
       1 Reserviert
       2 Zugewiesen

Beim Button **„Status weiter"** läuft der Workflow:

`Frei → Reserviert → Zugewiesen → Frei`

#### Datenbankstruktur

``` text
Subnet
├── Id
├── Name
├── Beschreibung
└── Erstellt

IPAdresse
├── Id
├── Titel       ← enthält z. B. 192.168.10.15
├── Erledigt    ← 0 / 1 / 2 für den Status
└── ProjektId   ← Foreign Key auf Subnet.Id
```

#### Start in Visual Studio

ZIP-Datei entpacken und folgende Projektdatei öffnen:

``` text
IPAM_WPF_App.csproj
```

Visual Studio sollte danach automatisch das NuGet-Paket
`Microsoft.Data.Sqlite` wiederherstellen.

Anschliessend kann die Anwendung mit **F5** oder dem grünen Start-Button
gestartet werden.

Beim ersten Start wird automatisch die SQLite-Datenbank erstellt:

``` text
app.db
```

Die Anwendung kann:

-   Subnetze anlegen und löschen
-   IP-Adressen nach ausgewähltem Subnet filtern
-   IP-Adressen hinzufügen und löschen
-   Status von IP-Adressen ändern

Die Projektstruktur wurde für `net10.0-windows` erstellt.

Als mögliche spätere Verbesserung wurde eine
**CIDR-/Subnetz-Validierung** vorgeschlagen, damit beispielsweise eine
IP-Adresse wie `10.0.1.200` nicht versehentlich einem
`192.168.5.0/24`-Subnetz zugeordnet werden kann.

### Ausführung und Test

Beim ersten Start konnte das Projekt nicht ausgeführt werden, da die
lokal installierte .NET-Version das verwendete **.NET-10-Zielframework**
nicht unterstützte.

Mithilfe von ChatGPT wurde die Ursache eingegrenzt und die
Aktualisierung der lokalen Entwicklungsumgebung empfohlen.

Nach der Aktualisierung von **Visual Studio** beziehungsweise des
benötigten **.NET SDK** konnte das Projekt erfolgreich geladen,
kompiliert und ausgeführt werden.

Anschliessend wurde die Anwendung getestet:

-   Subnetze konnten erfasst und verwaltet werden.
-   IP-Adressen konnten erfasst und verwaltet werden.
-   Der Status einer IP-Adresse konnte geändert werden.
-   Die eingegebenen Daten wurden korrekt angezeigt und gespeichert.
-   Die getesteten Grundfunktionen funktionierten wie vorgesehen.

> **Screenshot:** Auf Seite 7 des Originaldokuments ist die laufende
> Anwendung zu sehen. Sie zeigt mehrere Subnetze sowie IP-Adressen mit
> den Statuswerten „Frei", „Reserviert" und „Zugewiesen".

### Erkenntnis

Bei .NET-Projekten müssen das verwendete **Zielframework**, das
installierte **.NET SDK** und die **Visual-Studio-Version** miteinander
kompatibel sein.

Die KI unterstützte dabei vor allem bei der Fehlersuche und bei der
Auswahl der notwendigen Aktualisierung.
