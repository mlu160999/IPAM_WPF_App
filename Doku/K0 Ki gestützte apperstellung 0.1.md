# MODUL PROG1 – K0: KI-gestützte App-Erstellung

**Projekt:** IPAM_WPF_App (IP Address Management)\
**Autoren:** Madalina Luca, Muzamil Abukar Haji Ali, Sabri Mahdi\
**Version:** 0.2

## Inhaltsverzeichnis

1.  [Ziel](#ziel)
2.  [Was die App kann](#was-die-app-kann)
3.  [Technische Rahmenbedingungen](#technische-rahmenbedingungen)
4.  [Aufbau der App in K0](#aufbau-der-app-in-k0)
5.  [Datenmodell](#datenmodell)
6.  [Ablauf: Was passiert bei einem Klick?](#ablauf-was-passiert-bei-einem-klick)
7.  [Ausführung und Test](#ausführung-und-test)
8.  [Schwächen von K0 (Ausgangslage für K1)](#schwächen-von-k0-ausgangslage-für-k1)
9.  [KI-Protokoll K0](#ki-protokoll-k0)
10. [Reflexion](#reflexion)

## Ziel

In K0 wurde mithilfe einer KI (ChatGPT) eine erste lauffähige
WPF-Desktop-App erstellt. Ziel war nicht schöner Code, sondern eine
funktionierende Ausgangsbasis, die in den Kapiteln K1 bis K4 Schritt für
Schritt sauberer aufgebaut wird.

## Was die App kann

Die App verwaltet **Subnetze** und die **IP-Adressen** darin:

-   Subnetze anlegen und löschen
-   Ein Subnet auswählen und nur dessen IP-Adressen anzeigen
-   IP-Adressen hinzufügen und löschen
-   Den Status einer IP-Adresse weiterschalten:
    **Frei → Reserviert → Zugewiesen → Frei**

## Technische Rahmenbedingungen

| Vorgabe | Bedeutung |
|---|---|
| WPF mit **Code-Behind** | Die Logik steht direkt in `MainWindow.xaml.cs`, **kein MVVM** |
| **SQLite** mit `Microsoft.Data.Sqlite` | Lokale Datenbankdatei `app.db`, **kein Entity Framework** |
| Direkte SQL-Abfragen | `SqliteConnection`, `SqliteCommand`, `SqliteDataReader` |
| **.NET 10** | Zielframework `net10.0-windows` |

## Aufbau der App in K0

``` text
IPAM_WPF_App/
├── Models/
│   ├── Subnet.cs
│   └── IPAdresse.cs
├── DataAccess/
│   ├── DbInitializer.cs
│   ├── SubnetRepository.cs
│   └── IPAdresseRepository.cs
├── App.xaml / App.xaml.cs
├── MainWindow.xaml / MainWindow.xaml.cs
└── IPAM_WPF_App.csproj
```

### Welche Klasse macht was?

| Klasse | Aufgabe |
|---|---|
| `App.xaml` / `App.xaml.cs` | Startpunkt der Anwendung. Öffnet das Hauptfenster. |
| `MainWindow.xaml` | Die Oberfläche: zwei Tabellen (Subnetze, IP-Adressen), Eingabefelder und Buttons. |
| `MainWindow.xaml.cs` | Code-Behind. Reagiert auf Klicks, prüft Eingaben, zeigt Meldungen an und ruft die Repositories auf. |
| `DbInitializer` | Erstellt beim Start die Tabellen `Subnet` und `IPAdresse` mit `CREATE TABLE IF NOT EXISTS`. |
| `SubnetRepository` | Liest, speichert und löscht Subnetze mit SQL. |
| `IPAdresseRepository` | Liest, speichert und löscht IP-Adressen mit SQL und ändert den Status. |
| `Subnet` | Datenklasse für ein Subnetz. |
| `IPAdresse` | Datenklasse für eine IP-Adresse. Enthält eine Hilfs-Property, die den Status als Text («Frei», …) liefert. |

## Datenmodell

Zwischen den beiden Tabellen besteht eine **1:n-Beziehung**: Ein Subnet
hat viele IP-Adressen, jede IP-Adresse gehört zu genau einem Subnet.

``` text
Subnet                     IPAdresse
├── Id          ◄──────┐   ├── Id
├── Name               └── ├── ProjektId   (Fremdschlüssel auf Subnet.Id)
├── Beschreibung           ├── Titel       (enthält die IP, z. B. 192.168.10.15)
└── Erstellt               └── Erledigt    (Status als Zahl 0 / 1 / 2)
```

| Wert in `Erledigt` | Status |
|---|---|
| 0 | Frei |
| 1 | Reserviert |
| 2 | Zugewiesen |

**Wichtig:** Die Spaltennamen `Titel`, `Erledigt` und `ProjektId`
stammen aus der Aufgaben-Vorlage (dort ging es um Projekte und
Aufgaben). Für eine IP-Verwaltung sind sie irreführend. Sie wurden in
K1 umbenannt in `Adresse`, `Status` und `SubnetId`.

Zusätzlich gilt:

-   `UNIQUE (ProjektId, Titel)`: Dieselbe IP darf pro Subnet nur einmal
    vorkommen.
-   `ON DELETE CASCADE`: Wird ein Subnet gelöscht, werden seine
    IP-Adressen mitgelöscht.
-   `CHECK (Erledigt IN (0, 1, 2))`: Nur gültige Status-Werte.

## Ablauf: Was passiert bei einem Klick?

Beispiel «IP-Adresse hinzufügen»:

1.  `MainWindow.xaml.cs` liest den Text aus dem Eingabefeld.
2.  Das Code-Behind ruft `IPAdresseRepository` auf.
3.  Das Repository führt ein `INSERT INTO IPAdresse ...` aus.
4.  Das Code-Behind lädt die Liste neu und zeigt sie an.

## Ausführung und Test

**Start:** `IPAM_WPF_App.csproj` in Visual Studio öffnen und mit **F5**
starten. Beim ersten Start entsteht automatisch die Datei `app.db`.

**Problem beim ersten Start:** Das Projekt liess sich nicht ausführen,
weil die installierte .NET-Version das Zielframework **.NET 10** nicht
unterstützte. Nach dem Update von Visual Studio bzw. des .NET SDK lief
das Projekt.

**Getestet:**

-   Subnetze erfassen und löschen
-   IP-Adressen erfassen und löschen
-   Status einer IP-Adresse weiterschalten
-   Daten bleiben nach einem Neustart erhalten

Alle Grundfunktionen funktionierten wie vorgesehen.

## Schwächen von K0 (Ausgangslage für K1)

| Schwäche | Folge |
|---|---|
| Das Code-Behind entscheidet selbst (Validierung, Workflow) und spricht direkt mit den Repositories | Logik, Oberfläche und Datenbank sind vermischt. Änderungen an einer Stelle wirken überall. |
| SQL-Fehler (z. B. doppelte IP) werden im Code-Behind abgefangen | Die Oberfläche muss Datenbank-Details kennen. |
| Spaltennamen aus der Vorlage (`Titel`, `Erledigt`, `ProjektId`) | Schwer verständlich |
| Status als Zahlen 0/1/2 | «Magic Numbers», man muss die Bedeutung kennen |

Diese Punkte werden in K1 bis K4 schrittweise behoben.

------------------------------------------------------------------------

## KI-Protokoll K0

**Tool:** ChatGPT

### Eintrag 1 – App generieren

**Prompt / Aufgabe:**\
Wir haben ChatGPT eine detaillierte Vorgabe gegeben: WPF-App in C# mit
.NET 10 für ein IP Address Management System, Code-Behind ohne MVVM,
SQLite mit `Microsoft.Data.Sqlite` ohne Entity Framework, nur direkte
SQL-Abfragen. Dazu das Datenmodell (Subnet 1:n IP-Adresse), der
Status-Workflow Frei → Reserviert → Zugewiesen, die fünf
Mindestfunktionen, die gewünschte Struktur (Repository-Klassen pro
Entität, Code-Behind nur für UI) und die Liste der zu erstellenden
Dateien.

**Ergebnis der KI:**\
ChatGPT erstellte ein vollständiges Projekt: `MainWindow.xaml` und
`.xaml.cs`, `DbInitializer.cs` mit Tabellen und Fremdschlüssel,
`SubnetRepository.cs`, `IPAdresseRepository.cs`, `App.xaml`/`.cs` und
die `.csproj`. Zusätzlich schlug es als spätere Verbesserung eine
CIDR-Prüfung vor (damit z. B. `10.0.1.200` nicht einem Subnet
`192.168.5.0/24` zugeordnet werden kann).

**Unsere Prüfung und Umsetzung:**\
Wir haben das Projekt in Visual Studio geöffnet, gestartet und alle
Grundfunktionen von Hand getestet. Die Struktur haben wir mit unserer
Vorgabe verglichen.

**Erkenntnis:**\
Je genauer der Prompt (Technik, Datenmodell, Funktionen, Struktur),
desto besser passt das Ergebnis. Die Spaltennamen aus der Vorlage hat
die KI unverändert übernommen, obwohl sie fachlich nicht passen. Das
mussten wir später selbst korrigieren.

### Eintrag 2 – Startproblem mit .NET 10

**Prompt / Aufgabe:**\
Wir haben ChatGPT die Fehlermeldung beim Start gezeigt und nach der
Ursache gefragt.

**Ergebnis der KI:**\
ChatGPT grenzte die Ursache ein: Die installierte .NET-Version
unterstützt das Zielframework .NET 10 nicht. Empfohlen wurde ein Update
der Entwicklungsumgebung.

**Unsere Prüfung und Umsetzung:**\
Wir haben Visual Studio bzw. das .NET SDK aktualisiert. Danach liess
sich das Projekt laden, kompilieren und ausführen.

**Erkenntnis:**\
Zielframework, installiertes .NET SDK und Visual-Studio-Version müssen
zusammenpassen. Die KI half vor allem bei der Fehlersuche.

------------------------------------------------------------------------

## Reflexion

**Was hat gut funktioniert?**\
Mit einem genauen Prompt hatten wir in kurzer Zeit eine lauffähige App
mit Datenbank, Oberfläche und allen Mindestfunktionen. Ohne KI hätten
wir dafür deutlich länger gebraucht, vor allem für das XAML-Layout und
die SQL-Abfragen.

**Was hat weniger gut funktioniert?**\
Die KI hat genau umgesetzt, was wir verlangt haben, aber nicht
mitgedacht. Die Spaltennamen `Titel`, `Erledigt` und `ProjektId` aus
der Vorlage hat sie unverändert übernommen, obwohl sie für IP-Adressen
keinen Sinn ergeben. Auch die Trennung von Oberfläche, Regeln und
Datenbank war nur teilweise vorhanden. Das Code-Behind hat noch selbst
Datenbankfehler abgefangen.

**Was wir gelernt haben:**

-   Eine funktionierende App ist noch keine gut aufgebaute App. Genau
    das bearbeiten wir in K1 bis K4.
-   Den generierten Code muss man lesen und verstehen. Nur so fallen
    Dinge wie unpassende Namen oder vermischte Zuständigkeiten auf.
-   Probleme mit der Entwicklungsumgebung (hier .NET 10) kann die KI
    gut eingrenzen. Lösen mussten wir sie aber selbst auf unserem
    Rechner.

**Was wir nächstes Mal anders machen würden:**\
Im Prompt auch die Namen festlegen (z. B. `Adresse`, `Status`,
`SubnetId` statt der Vorlage-Namen) und verlangen, dass das
Code-Behind keine Datenbankfehler kennt.
