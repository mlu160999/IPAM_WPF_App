# MODUL PROG1 – K2: ORM mit Entity Framework Core

**Projekt:** IPAM_WPF_App\
**Autoren:** Madalina Luca, Muzamil Abukar Haji Ali, Sabri Mahdi\
**Branch:** `feature/k2-efcore` → `master` (Pull Request #3)

## Inhaltsverzeichnis

1.  [Ziel](#ziel)
2.  [Was ist ein ORM?](#was-ist-ein-orm)
3.  [Projektstruktur nach K2](#projektstruktur-nach-k2)
4.  [Welche Klasse macht was?](#welche-klasse-macht-was)
5.  [Änderungen gegenüber K1 und warum](#änderungen-gegenüber-k1-und-warum)
6.  [Migrations](#migrations)
7.  [Ablauf: Klick auf «Status weiter»](#ablauf-klick-auf-status-weiter)
8.  [Übersicht: Serviceklasse → DataAccess](#übersicht-serviceklasse--dataaccess)
9.  [Kontrolle](#kontrolle)
10. [Git](#git)
11. [Stolpersteine](#stolpersteine)
12. [KI-Protokoll K2](#ki-protokoll-k2)
13. [Reflexion](#reflexion)

## Ziel

In K1 hat die DataAccess-Schicht SQL-Befehle als Text geschrieben und
die Ergebnisse Zeile für Zeile in Objekte kopiert. In K2 übernimmt das
**Entity Framework Core (EF Core)**.

Nur das **Innere** der DataAccess-Klassen ändert sich. Die
Methodennamen bleiben gleich, deshalb merken Services und UI nichts vom
Umbau. Genau dafür haben wir in K1 die Schichten getrennt.

## Was ist ein ORM?

ORM steht für **Object-Relational Mapping**. Ein ORM übersetzt
automatisch zwischen C#-Objekten und Datenbanktabellen.

| Vorher (K1, ADO.NET) | Jetzt (K2, EF Core) |
|---|---|
| Wir schreiben SQL als Text: `SELECT ... FROM IPAdresse WHERE ...` | Wir schreiben C# mit LINQ: `.Where(a => a.SubnetId == id)` |
| Wir lesen jede Zeile mit `reader.GetString(1)` und bauen das Objekt selbst | EF Core erzeugt die Objekte automatisch |
| Wir schreiben `CREATE TABLE` selbst | EF Core erzeugt die Tabellen aus den Klassen (**Migration**) |
| Wir öffnen und schliessen Verbindungen selbst | EF Core verwaltet die Verbindung |

## Projektstruktur nach K2

``` text
IPAM_WPF_App/
├── DataAccess/
│   ├── AppDbContext.cs          (neu)
│   ├── DbInitializer.cs         (geändert)
│   ├── IPAdresseDataAccess.cs   (geändert)
│   └── SubnetDataAccess.cs      (geändert)
├── Migrations/                  (neu, von EF Core erzeugt)
│   ├── 20261030103648_InitialDbCreation.cs
│   ├── 20261030103648_InitialDbCreation.Designer.cs
│   └── AppDbContextModelSnapshot.cs
├── Models/
│   ├── IPAdresse.cs             (geändert)
│   ├── IPStatus.cs
│   └── Subnet.cs                (geändert)
├── Services/                    (unverändert)
├── App.xaml / MainWindow.xaml   (unverändert)
├── IPAM_WPF_App.csproj          (geändert)
└── IPAM_WPF_App.slnx            (neu)
```

## Welche Klasse macht was?

| Klasse | Aufgabe in K2 |
|---|---|
| `AppDbContext` (**neu**) | Die «Verbindung» zur Datenbank. Kennt den Connection-String, enthält pro Tabelle ein `DbSet` (`Subnets`, `IPAdressen`) und beschreibt Regeln wie UNIQUE, CHECK und Fremdschlüssel. |
| `DbInitializer` | Ruft beim Start `context.Database.Migrate()` auf. Das erstellt die Datenbank oder bringt sie auf den neusten Stand. |
| `SubnetDataAccess` | Lesen, Speichern, Ändern und Löschen von Subnetzen, jetzt mit LINQ statt SQL |
| `IPAdresseDataAccess` | Dasselbe für IP-Adressen |
| `Subnet` | Hat neu eine Liste `IPAdressen`: alle IP-Adressen dieses Subnets |
| `IPAdresse` | Hat neu ein Feld `Subnet`: das Subnet, zu dem die Adresse gehört |
| Migrations | Von EF Core erzeugter Code, der die Tabellen erstellt |
| Services, UI | Unverändert |

## Änderungen gegenüber K1 und warum

### 1. NuGet-Pakete (`IPAM_WPF_App.csproj`)

| Paket | Wozu |
|---|---|
| `Microsoft.EntityFrameworkCore.Sqlite` | EF Core mit SQLite. Bringt `Microsoft.Data.Sqlite` selbst mit, deshalb wurde das alte Paket entfernt. |
| `Microsoft.EntityFrameworkCore.Design` | Wird für Migrations gebraucht (nur beim Entwickeln) |
| `Microsoft.EntityFrameworkCore.Tools` | Stellt die Befehle `Add-Migration` und `Update-Database` in der Paket-Manager-Konsole bereit |
| `SQLitePCLRaw.lib.e_sqlite3` (3.53.3) | Erzwingt eine aktuelle SQLite-Version. EF Core bringt sonst Version 2.1.11 mit, die eine bekannte Sicherheitslücke hat (Warnung **NU1903**). |

### 2. `AppDbContext` (neu)

``` csharp
public DbSet<Subnet> Subnets { get; set; }
public DbSet<IPAdresse> IPAdressen { get; set; }
```

Ein `DbSet` steht für eine Tabelle. Man kann es mit LINQ abfragen wie
eine Liste.

In `OnModelCreating` stehen die Regeln, die vorher im `CREATE TABLE`
standen (**Fluent API**):

| Regel aus K1 (SQL) | In K2 (Fluent API) | Bedeutung |
|---|---|---|
| Tabellenname `Subnet` | `ToTable("Subnet")` | Sonst würde EF Core die Tabelle `Subnets` nennen |
| `CHECK (Status IN (0, 1, 2))` | `HasCheckConstraint(...)` | Nur gültige Status-Werte |
| `UNIQUE (SubnetId, Adresse)` | `HasIndex(...).IsUnique()` | Dieselbe IP nur einmal pro Subnet |
| `FOREIGN KEY ... ON DELETE CASCADE` | `HasOne(...).WithMany(...).OnDelete(Cascade)` | Beim Löschen eines Subnets werden seine IPs mitgelöscht |

### 3. `DbInitializer`

Vorher etwa 70 Zeilen SQL, jetzt eine Zeile: `context.Database.Migrate()`.

**Warum `Migrate()` beim Start und nicht nur `Update-Database`?**
`Update-Database` legt die `app.db` im Projektordner an. Die App läuft
aber aus `bin\Debug\net10.0-windows\` und sucht die Datei dort. Mit
`Migrate()` entsteht die Datenbank immer am richtigen Ort.

### 4. DataAccess-Klassen

Jede Methode folgt demselben Muster:

``` csharp
using var context = new AppDbContext();   // 1. Context öffnen
var ip = context.IPAdressen.Find(id);     // 2. Daten laden
ip.Status = neuerStatus;                  //    und ändern
context.SaveChanges();                    // 3. Speichern
                                          //    'using' schliesst den Context
```

EF Core merkt sich alle geladenen Objekte (**Change Tracking**). Bei
`SaveChanges()` erkennt es selbst, was sich geändert hat, und erzeugt
das passende `UPDATE`.

| Vorher (SQL) | Jetzt (EF Core) |
|---|---|
| `SELECT ... WHERE SubnetId = ... ORDER BY Adresse` | `.Where(a => a.SubnetId == id).OrderBy(a => a.Adresse).ToList()` |
| `INSERT` + `last_insert_rowid()` | `Add(subnet)` + `SaveChanges()`. Danach ist `subnet.Id` automatisch gesetzt. |
| `UPDATE ... SET ...` | `Find()`, Property ändern, `SaveChanges()` |
| `DELETE FROM ...` | `Find()`, `Remove()`, `SaveChanges()` |
| Fehlercode 2067 (Duplikat) abfangen | Vorher mit `.Any(...)` fragen, ob die Adresse schon existiert |

**Bewusste Entscheidungen:**

-   **Pro Methode ein neuer Context (`using`).** Ein Context soll nur
    kurz leben. Ein dauerhaft offener Context ist eine bekannte
    ORM-Falle.
-   **`Update` lädt zuerst und ändert nur einzelne Felder.** So bleibt
    z. B. das Erstelldatum eines Subnets unverändert.
-   **Duplikat mit `Any()` prüfen.** Den Datenbankfehler abzufangen wäre
    mit EF Core umständlich und an SQLite-Fehlercodes gebunden. Der
    UNIQUE-Index bleibt als Sicherheitsnetz bestehen.

### 5. Navigation Properties in den Models

``` csharp
// Subnet.cs: ein Subnet hat viele IP-Adressen
public ICollection<IPAdresse> IPAdressen { get; set; } = new List<IPAdresse>();

// IPAdresse.cs: eine IP-Adresse gehört zu einem Subnet
public Subnet? Subnet { get; set; }
```

Damit kann man im Code direkt von einem Objekt zum anderen springen,
z. B. `ipAdresse.Subnet.Name`. **Achtung:** Diese Felder sind leer,
solange man sie nicht ausdrücklich mitlädt (`.Include(...)`). Das `?`
bei `Subnet?` erinnert daran.

### 6. Projektmappe `IPAM_WPF_App.slnx` (neu)

Visual Studio braucht eine Projektmappe. Wer das Repo klont, öffnet
einfach die `.slnx`. Wird nur der Ordner geöffnet («Folder View»),
lädt Visual Studio die NuGet-Pakete nicht und der Build schlägt fehl.

## Migrations

Eine Migration ist von EF Core erzeugter C#-Code, der die Datenbank
verändert.

``` text
Add-Migration InitialDbCreation     ← in der Paket-Manager-Konsole
```

| Datei | Inhalt |
|---|---|
| `..._InitialDbCreation.cs` | Erstellt die Tabellen `Subnet` und `IPAdresse` mit Fremdschlüssel, CHECK und Unique-Index |
| `..._InitialDbCreation.Designer.cs` | Metadaten zur Migration |
| `AppDbContextModelSnapshot.cs` | Aktueller Stand des Modells. EF Core vergleicht bei der nächsten Migration damit. |

EF Core merkt sich in der Tabelle `__EFMigrationsHistory`, welche
Migrations schon angewendet wurden.

**Vergleich mit dem alten Schema:** Tabellen, Spalten, Fremdschlüssel
mit Cascade, CHECK und Unique-Index stimmen überein. Einzige Abweichung:
`Beschreibung` ist jetzt `NOT NULL`. Das ist unproblematisch, weil der
Service immer mindestens einen leeren Text speichert.

**Alte `app.db` gelöscht:** Die Datenbank aus K1 wurde mit SQL erstellt
und hat keine `__EFMigrationsHistory`. `Migrate()` hätte die Tabellen
erneut anlegen wollen und wäre mit «table already exists» abgebrochen.

## Ablauf: Klick auf «Status weiter»

1.  **UI** (unverändert): ruft `_ipAdresseService.AdvanceStatus(...)`
    auf.
2.  **Service** (unverändert): berechnet den neuen Status und ruft
    `IPAdresseDataAccess.UpdateStatus(id, neuerStatus)` auf.
3.  **DataAccess** (neu mit EF Core): `Find(id)`, Status setzen,
    `SaveChanges()`.
4.  **EF Core:** erzeugt `UPDATE IPAdresse SET Status = ... WHERE Id = ...`.

## Übersicht: Serviceklasse → DataAccess

| Serviceklasse | Service-Methode | DataAccess-Methode | Umsetzung mit EF Core |
|---|---|---|---|
| ApplicationService | InitializeDatabase() | DbInitializer.Initialize() | `Database.Migrate()` |
| SubnetService | GetAll() | SubnetDataAccess.GetAll() | `OrderBy` + `ToList` |
| SubnetService | Add(...) | SubnetDataAccess.Add(...) | `Add` + `SaveChanges` |
| SubnetService | Update(subnet) | SubnetDataAccess.Update(subnet) | `Find` + Felder setzen + `SaveChanges` |
| SubnetService | Delete(id) | SubnetDataAccess.Delete(id) | `Find` + `Remove` + `SaveChanges` |
| IPAdresseService | GetBySubnetId(id) | IPAdresseDataAccess.GetBySubnetId(id) | `Where` + `OrderBy` + `ToList` |
| IPAdresseService | Add(ipText, subnetId) | IPAdresseDataAccess.TryAdd(...) | `Any` + `Add` + `SaveChanges` |
| IPAdresseService | AdvanceStatus(...) | IPAdresseDataAccess.UpdateStatus(...) | `Find` + Status setzen + `SaveChanges` |
| IPAdresseService | Delete(id) | IPAdresseDataAccess.Delete(id) | `Find` + `Remove` + `SaveChanges` |

## Kontrolle

-   App getestet: Subnet hinzufügen/löschen, IP hinzufügen, Duplikat
    wird gemeldet, Status-Workflow, IP löschen, beim Löschen eines
    Subnets verschwinden die IPs, Daten bleiben nach Neustart erhalten.
-   **Kontrollfrage:** In `DataAccess/` gibt es kein `SqliteConnection`
    und kein `SqliteCommand` mehr.
-   Services und UI wurden nicht verändert.

## Git

Branch `feature/k2-efcore`, gemergt über **Pull Request #3**:

1.  `K2: Ergänzung der EF Core Packages`
2.  `K2: Zentraler Connection String`
3.  `K2: DB Context Anbindung mittels APPDbContext in Models und
    DataAccess für IPAdresse und Subnet`
4.  `K2: DB Migration mit NuGet. APP.DB Wurde gelöscht`
5.  `Projektmappe`
6.  `Dokumentations Struktur angepasst.`
7.  `K2: Readme.md auf neusten Stand gebracht`

## Stolpersteine

| Problem | Ursache | Lösung |
|---|---|---|
| Build-Fehler «EntityFrameworkCore does not exist» | Visual Studio hatte nur den **Ordner** geöffnet (Folder View), die NuGet-Pakete wurden nicht geladen | Projekt über *File → Open → Project/Solution* öffnen |
| Warnung NU1903, `Add-Migration` bricht ab | Beim Umbau wurde das Paket `SQLitePCLRaw.lib.e_sqlite3` entfernt, dadurch kam die alte, unsichere SQLite-Version zurück | Paket in Version 3.53.3 wieder ergänzt |
| Erster Push ohne Migration → App hätte keine Tabellen erstellt | `Add-Migration` war noch nicht ausgeführt | Migration erstellt und committet |
| «table already exists» (vermieden) | Alte `app.db` aus K1 | Datei gelöscht, wird beim Start neu erstellt |

------------------------------------------------------------------------

## KI-Protokoll K2

**Tool:** Claude (Anthropic)

### Eintrag 1 – Umbau auf EF Core

**Prompt / Aufgabe:**\
«Super, nun weiter mit K2.» (Grundlage: Aufgabenblatt K2 und unser
`master` nach K1.)

**Ergebnis der KI:**\
Claude lieferte sieben Dateien: `.csproj` mit EF-Core-Paketen,
`AppDbContext`, `DbInitializer` mit `Migrate()`, beide
DataAccess-Klassen mit LINQ sowie die Models mit Navigation Properties.
Die Methodensignaturen blieben gleich. Dazu kam eine Schritt-für-Schritt-
Anleitung für Branch, Migration und Test.

**Unsere Prüfung und Umsetzung:**\
Wir haben den Branch `feature/k2-efcore` erstellt, die Dateien
eingebaut und in drei Commits gepusht. Bevor wir weitergemacht haben,
haben wir uns den Aufbau erklären lassen (nächster Eintrag).

**Erkenntnis:**\
Weil wir in K1 die Schichten getrennt haben, mussten Services und UI
tatsächlich nicht angefasst werden.

### Eintrag 2 – Verstehen, was geändert wurde

**Prompt / Aufgabe:**\
«Erklär mir wie das ganze funktioniert und was was macht» und später
«Erklär mir alle deine Änderungen in den Files und wieso».

**Ergebnis der KI:**\
Claude erklärte Datei für Datei: was ein `DbSet` ist, was die Fluent
API nachbildet, wie Migrations funktionieren, was Change Tracking ist
und warum pro Methode ein neuer Context verwendet wird.

**Unsere Prüfung und Umsetzung:**\
Mit diesen Erklärungen konnten wir die generierte Migration selbst
prüfen (Aufgabe B2.6) und haben die Abweichung bei `Beschreibung`
(`NOT NULL`) festgestellt und bewertet.

**Erkenntnis:**\
Wir übernehmen KI-Code erst, wenn wir ihn erklären können. Die
Erklärung «Datei für Datei mit Warum» war dafür hilfreicher als der
Code selbst.

### Eintrag 3 – Build-Fehler und Sicherheitswarnung

**Prompt / Aufgabe:**\
Wir haben Claude die vollständige Build-Ausgabe geschickt
(«EntityFrameworkCore does not exist») und später die Meldung, dass
`Add-Migration` wegen der Warnung NU1903 abbricht.

**Ergebnis der KI:**\
Ursache 1: Visual Studio war im Ordner-Modus geöffnet, deshalb wurden
die Pakete nicht geladen. Ursache 2 war ein **Fehler der KI selbst**:
Claude hatte das Paket `SQLitePCLRaw.lib.e_sqlite3` als «überflüssig»
entfernt. Ohne dieses Paket verwendet EF Core eine alte SQLite-Version
mit bekannter Sicherheitslücke. Claude erkannte den eigenen Fehler und
lieferte die Korrektur.

**Unsere Prüfung und Umsetzung:**\
Wir haben das Projekt richtig geöffnet, die Paketzeile ergänzt,
`Add-Migration InitialDbCreation` ausgeführt, die alte `app.db`
gelöscht und alle Funktionen getestet. Beim Branch-Check fiel auf, dass
die Migration im ersten Push fehlte. Wir haben sie nachgeliefert.

**Erkenntnis:**\
Auch die KI macht Fehler, hier eine Sicherheitslücke durch ein
entferntes Paket. Ohne unseren eigenen Build in Visual Studio wäre das
nicht aufgefallen. Build-Warnungen muss man ernst nehmen und nicht
einfach ignorieren.

### Fragen aus dem Aufgabenblatt

**Welche Prompts habt ihr verwendet, um den Umstieg auf EF Core zu
planen?**\
Wir haben Claude mit «Super, nun weiter mit K2» auf Basis des
Aufgabenblatts und unseres `master` beauftragt. Danach haben wir gezielt
Erklärungen verlangt: «Erklär mir, wie das ganze funktioniert und was
was macht» und «Erklär mir alle deine Änderungen in den Files und
wieso». Bei Problemen haben wir die vollständigen Fehlermeldungen aus
Visual Studio geschickt.

**Was hat die KI beim Umbau auf EF Core gut gemacht, wo musstet ihr
korrigieren?**\
Gut: Die Methodensignaturen blieben gleich, Services und UI mussten
nicht angepasst werden. Alle Regeln aus dem alten `CREATE TABLE`
(UNIQUE, CHECK, Cascade) wurden mit der Fluent API nachgebildet. Die
Erklärungen waren verständlich. Korrigieren mussten wir: Claude hatte
das Paket `SQLitePCLRaw.lib.e_sqlite3` entfernt, was zur
Sicherheitswarnung NU1903 führte und `Add-Migration` abbrechen liess.
Ausserdem setzte die Anleitung voraus, dass das Projekt richtig
geöffnet ist. Bei uns war Visual Studio im Ordner-Modus.

**Wo wollte die KI zu schnell Interfaces oder ein Repository-Pattern
einführen, und wie habt ihr sie zurückgeholt?**\
Das ist nicht passiert. Claude hat die Klassen bewusst `DataAccess`
genannt und keine Interfaces eingeführt. Die Navigation Properties
(`Subnet.IPAdressen`, `IPAdresse.Subnet`) wurden zwar schon mit Blick
auf K3 erklärt, sie gehören aber laut Aufgabenblatt zu K2 (1:n-Beziehung
in EF Core).

------------------------------------------------------------------------

## Reflexion

**Was ist am Umgang mit der Datenbank jetzt einfacher als vorher?**

-   **Kein SQL mehr als Text.** Tippfehler in SQL-Strings fielen früher
    erst zur Laufzeit auf. LINQ prüft der Compiler schon beim Bauen.
-   **Kein manuelles Kopieren.** Früher haben wir jede Spalte mit
    `reader.GetString(1)` von Hand ins Objekt übertragen. Jetzt macht
    das EF Core, inklusive der neuen `Id` nach dem Speichern.
-   **Die Datenbank folgt dem Code.** Statt `CREATE TABLE` von Hand zu
    pflegen, ändern wir die Klasse und erzeugen eine Migration.

**Was ist (noch) ungewohnt?**

-   **Vieles passiert unsichtbar.** Welches SQL EF Core erzeugt und
    wann genau es Änderungen erkennt (Change Tracking), sieht man nicht
    direkt.
-   **Der Ablauf mit Migrations.** Paket-Manager-Konsole,
    `Add-Migration`, die alte `app.db` löschen, die Datenbank im
    `bin`-Ordner statt im Projektordner: Das waren viele neue Schritte
    mit eigenen Stolpersteinen.
-   **Navigation Properties sind leer,** solange man sie nicht mit
    `.Include()` lädt. Das muss man wissen, sonst wundert man sich über
    `null`.

**Was wir gelernt haben:**\
Build-Warnungen sind ernst zu nehmen. Die Sicherheitslücke NU1903 kam
durch eine KI-Änderung und wäre ohne unseren eigenen Build nicht
aufgefallen.
