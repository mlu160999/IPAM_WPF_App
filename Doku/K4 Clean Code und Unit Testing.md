# MODUL PROG1 – K4: Clean Code und Unit Testing

**Projekt:** IPAM_WPF_App\
**Autoren:** Madalina Luca, Muzamil Abukar Haji Ali, Sabri Mahdi\
**Branch:** `feature/k4-cleancode-tests`

## Inhaltsverzeichnis

1.  [Ziel](#ziel)
2.  [Projektstruktur nach K4](#projektstruktur-nach-k4)
3.  [Neue Klassen und ihre Aufgabe](#neue-klassen-und-ihre-aufgabe)
4.  [B1 – Code-Review](#b1--code-review)
5.  [B2 – Single Responsibility](#b2--single-responsibility)
6.  [B3 – Unit Tests mit xUnit](#b3--unit-tests-mit-xunit)
7.  [B4 – Kontrolle](#b4--kontrolle)
8.  [Vertiefung – Fake-Repositories](#vertiefung--fake-repositories)
9.  [Challenge – Testabdeckung](#challenge--testabdeckung)
10. [Git](#git)
11. [Bekannte offene Punkte](#bekannte-offene-punkte)
12. [KI-Protokoll K4](#ki-protokoll-k4)
13. [Reflexion](#reflexion)

## Ziel

Die Architektur aus K1 bis K3 bleibt gleich. In K4 geht es um die
**Qualität**:

-   Den bestehenden Code nach Clean-Code-Regeln verbessern (Namen,
    Magic Strings, eine Aufgabe pro Klasse/Methode)
-   Das Verhalten mit **automatischen Tests** absichern, damit wir nach
    jeder Änderung in Sekunden sehen, ob noch alles funktioniert

Die Datenbank ändert sich nicht, es braucht **keine neue Migration**.

## Projektstruktur nach K4

``` text
IPAM_WPF_App/
├── DataAccess/
│   └── AppDbContext.cs             (geändert: CHECK aus Enum)
├── Services/
│   ├── IPAdresseAddResult.cs       (neu: aus IPAdresseService.cs ausgelagert)
│   ├── IPAdresseService.cs         (geändert)
│   ├── IPStatusWorkflow.cs         (neu: Status-Regel)
│   └── SubnetService.cs            (geändert)
├── MainWindow.xaml.cs              (geändert)
├── IPAM_WPF_App.csproj             (geändert: Testordner ausgeschlossen)
├── IPAM_WPF_App.slnx               (geändert: Testprojekt eingetragen)
└── IPAM_WPF_App.Tests/             (neu: zweites Projekt)
    ├── IPAM_WPF_App.Tests.csproj
    ├── Fakes/
    │   ├── FakeIPAdresseRepository.cs
    │   └── FakeSubnetRepository.cs
    ├── IPAdresseServiceTests.cs
    ├── IPStatusWorkflowTests.cs
    └── SubnetServiceTests.cs
```

Nicht aufgeführte Dateien sind unverändert.

## Neue Klassen und ihre Aufgabe

| Klasse | Projekt | Aufgabe |
|---|---|---|
| `IPStatusWorkflow` | App | Enthält **nur** die Regel, welcher Status auf welchen folgt: Frei → Reserviert → Zugewiesen → Frei. Ein unbekannter Status löst einen Fehler aus. |
| `IPAdresseAddResult` | App | Das Enum `Erfolgreich` / `Ungueltig` / `BereitsVorhanden`, jetzt in einer eigenen Datei |
| `FakeIPAdresseRepository` | Tests | Tut so, als wäre es das echte Repository, speichert aber nur in einer Liste im Speicher. Keine Datenbank nötig. |
| `FakeSubnetRepository` | Tests | Dasselbe für Subnetze |
| `IPAdresseServiceTests` | Tests | Prüft Validierung, Duplikatprüfung, Status-Workflow, Laden und Löschen von IP-Adressen |
| `SubnetServiceTests` | Tests | Prüft Erstellen und Ändern von Subnetzen |
| `IPStatusWorkflowTests` | Tests | Prüft jeden einzelnen Status-Übergang |

## B1 – Code-Review

### Naming

| Fundstelle | Problem | Änderung |
|---|---|---|
| `MainWindow.xaml.cs`: `wurdeHinzugefuegt` | bool-Variablen sollen laut Naming Convention mit *ist*/*hat* beginnen | → `istHinzugefuegt` |
| `IPAdresseService.cs` enthielt zusätzlich das Enum `IPAdresseAddResult` | Ein Typ pro Datei, Dateiname = Typname | Enum in eigene Datei `IPAdresseAddResult.cs` |
| `if (x == null) return;` ohne Klammern (2×) | Konvention: immer geschweifte Klammern | Klammern ergänzt |
| Öffentliche Service-Methoden ohne Beschreibung | Konvention: XML-Kommentare (`///`) | Ergänzt in `SubnetService` und `IPAdresseService`. Sie erscheinen in Visual Studio als Tooltip. |

Bei `IRepository.cs`, `IIPAdresseRepository.cs` und `DbInitializer.cs`
hätten sich nur Kommentare geändert. Diese haben wir **bewusst nicht
übernommen**, um den Commit auf echte Änderungen zu beschränken.

**Bewusst offen: Deutsch und Englisch gemischt.** Die Naming Convention
verlangt eine einzige Sprache. Eine komplette Umbenennung hätte
Datenbank, Migrations und UI betroffen. Wir haben deshalb eine feste
Regel festgelegt:

-   **Fachbegriffe auf Deutsch:** `IPAdresse`, `Beschreibung`,
    `IPStatus.Frei`, Texte im UI
-   **Technische Begriffe auf Englisch:** `GetAll`, `Add`,
    `AdvanceStatus`, `Repository`, `Service`, `Dto`

### Magic Strings / Magic Numbers

Ein «Magic String» ist ein Text oder eine Zahl direkt im Code, deren
Bedeutung man nicht sieht oder die an mehreren Stellen wiederholt wird.

| # | Wo | Vorher | Nachher | Warum |
|---|---|---|---|---|
| 1 | `SubnetService` | `"yyyy-MM-dd HH:mm:ss"` | Konstante `ErstelltFormat` | Der Name erklärt die Bedeutung. Der Test prüft gegen dieselbe Konstante. |
| 2 | `MainWindow.xaml.cs` | `"Bitte zuerst ein Subnet auswählen."` (6×) | `SubnetAuswaehlenText` | Text an einer Stelle ändern statt an sechs |
| 3 | `MainWindow.xaml.cs` | `"Bitte zuerst eine IP-Adresse auswählen."` (2×) | `IPAdresseAuswaehlenText` | wie oben |
| 4 | `MainWindow.xaml.cs` | `"Keine Auswahl"` (6×) | `KeineAuswahlTitel` | wie oben |
| 5 | `AppDbContext` | `"Status" IN (0, 1, 2)` | Werte werden aus dem Enum `IPStatus` gelesen | Kommt ein Status dazu, passt sich die Regel automatisch an. Das SQL bleibt identisch, deshalb keine Migration. |

## B2 – Single Responsibility

**Single Responsibility Principle (SRP):** Eine Klasse oder Methode soll
genau **eine** Aufgabe haben und nur aus **einem** Grund geändert werden
müssen.

### 1. Status-Regel ausgelagert: `IPStatusWorkflow`

**Vorher:** `IPAdresseService.AdvanceStatus` hat drei Dinge gemacht:
IP-Adresse laden, mit einem `switch` den nächsten Status bestimmen,
speichern.

**Nachher:**

``` csharp
// IPAdresseService: laden, Regel aufrufen, speichern
ipAdresse.Status = IPStatusWorkflow.GetNextStatus(ipAdresse.Status);
_repository.Update(ipAdresse);
```

**Warum:** Die Reihenfolge der Status ist eine eigene fachliche Regel.
Kommt z. B. ein Status «Gesperrt» dazu, ändert man nur
`IPStatusWorkflow`. Die Regel lässt sich ausserdem ohne Repository
direkt testen. Ein unbekannter Status wird nicht mehr stillschweigend
auf «Frei» gesetzt, sondern löst einen Fehler aus.

### 2. Auswahlprüfung im `MainWindow` zusammengefasst

**Vorher:** Vier Button-Handler enthielten fast identischen Code: Ist
ein Subnet bzw. eine IP-Adresse ausgewählt? Wenn nicht, MessageBox
anzeigen. Rund 90 Zeilen Wiederholung (Verstoss gegen DRY: «Don't
Repeat Yourself»).

**Nachher:** Fünf kleine Hilfsmethoden mit je einer Aufgabe:

| Methode | Aufgabe |
|---|---|
| `TryGetSelectedSubnet(out subnet)` | Ausgewähltes Subnet holen, sonst Hinweis zeigen und `false` zurückgeben |
| `TryGetSelectedIPAdresse(out ipAdresse)` | Dasselbe für die IP-Adresse |
| `ShowInfo(text)` | Info-Meldung anzeigen |
| `ShowWarning(text, titel)` | Warnung anzeigen |
| `ResetIpAdressenAnzeige()` | IP-Liste leeren und Hinweistext setzen |

Ein Button-Handler sieht jetzt so aus:

``` csharp
if (!TryGetSelectedSubnet(out var subnet)
    || !TryGetSelectedIPAdresse(out var ipAdresse))
{
    return;
}

_ipAdresseService.Delete(ipAdresse.Id);
LoadIpAdressen(subnet.Id);
```

**Warum:** Ein Klick-Handler beschreibt nur noch, *was* beim Klick
passiert. Prüfung und Meldungen sind an einer Stelle und dort leicht zu
ändern.

## B3 – Unit Tests mit xUnit

### Aufbau des Testprojekts

-   Zweites Projekt `IPAM_WPF_App.Tests` mit **xUnit**, eingetragen in
    `IPAM_WPF_App.slnx`
-   Verweist auf das Hauptprojekt (`ProjectReference`), damit es die
    Services testen kann
-   `TargetFramework` `net10.0-windows` wie das Hauptprojekt, sonst lässt
    sich ein WPF-Projekt nicht referenzieren
-   **Wichtig:** Im Hauptprojekt wird der Testordner mit
    `Compile Remove="IPAM_WPF_App.Tests\**"` ausgeschlossen. Weil das
    Testprojekt im Ordner des Hauptprojekts liegt, würde das
    Hauptprojekt sonst die Testdateien mitkompilieren und mit
    «xunit not found» scheitern.

### Das AAA-Muster

Jeder Test hat drei Teile:

``` csharp
[Fact]
public void Add_UngueltigeAdresse_GibtUngueltigZurueckUndSpeichertNichts()
{
    // Arrange – Ausgangslage vorbereiten
    var repository = new FakeIPAdresseRepository();
    var service = new IPAdresseService(repository);

    // Act – die zu testende Methode aufrufen
    var resultat = service.Add("keine-ip", 1);

    // Assert – Ergebnis prüfen
    Assert.Equal(IPAdresseAddResult.Ungueltig, resultat);
    Assert.Empty(repository.GespeicherteIPAdressen);
}
```

**Namensschema:** `Methode_Szenario_ErwartetesResultat`. So sieht man
schon am Namen, was ein Test prüft.

`[Fact]` ist ein einzelner Test. `[Theory]` mit `[InlineData]` führt
denselben Test mit mehreren Datensätzen aus.

### Übersicht der Tests (24)

**`IPAdresseServiceTests` (14)**

| Test | Art |
|---|---|
| `Add_GueltigeNeueAdresse_GibtErfolgreichZurueckUndSpeichertMitStatusFrei` | Positiv |
| `Add_GueltigeIPv6Adresse_GibtErfolgreichZurueck` | Positiv |
| `Add_UngueltigeAdresse_GibtUngueltigZurueckUndSpeichertNichts` | **Negativ** |
| `Add_LeereEingabe_GibtUngueltigZurueck` | **Negativ** |
| `Add_AdresseExistiertImGleichenSubnet_GibtBereitsVorhandenZurueck` | **Negativ** |
| `Add_DuplikatMitLeerzeichen_GibtBereitsVorhandenZurueckUndSpeichertNichts` | **Negativ** (nachträglich ergänzt, siehe unten) |
| `Add_GleicheAdresseInAnderemSubnet_GibtErfolgreichZurueck` | Positiv (Grenzfall) |
| `Add_AdresseMitLeerzeichen_SpeichertGetrimmteAdresse` | Positiv |
| `AdvanceStatus_ExistierendeAdresse_SchaltetStatusWeiter` (3 Fälle) | Positiv |
| `AdvanceStatus_NichtExistierendeId_AendertNichts` | **Negativ** |
| `GetBySubnetId_MehrereSubnets_LiefertNurAdressenDesSubnets` | Positiv |
| `Delete_ExistierendeId_EntferntAdresse` | Positiv |

**`SubnetServiceTests` (6)**

| Test | Art |
|---|---|
| `Add_GueltigerName_GibtTrueZurueckUndSpeichertSubnet` | Positiv |
| `Add_LeererName_GibtFalseZurueckUndSpeichertNichts` | **Negativ** |
| `Add_NameMitLeerzeichen_SpeichertGetrimmtenNamen` | Positiv |
| `Add_GueltigerName_SetztErstelltImErwartetenFormat` | Positiv |
| `Update_LeererName_WirftArgumentException` | **Negativ** |
| `Update_NeuerName_AendertNameUndLaesstErstelltUnveraendert` | Positiv |

**`IPStatusWorkflowTests` (4)**

| Test | Art |
|---|---|
| `GetNextStatus_Frei_GibtReserviertZurueck` | Positiv |
| `GetNextStatus_Reserviert_GibtZugewiesenZurueck` | Positiv |
| `GetNextStatus_Zugewiesen_GibtFreiZurueck` | Positiv |
| `GetNextStatus_UnbekannterStatus_WirftArgumentOutOfRangeException` | **Negativ** |

### Zusätzlicher Negativtest

``` csharp
[Fact]
public void Add_DuplikatMitLeerzeichen_GibtBereitsVorhandenZurueckUndSpeichertNichts()
{
    // Arrange: Die Adresse existiert bereits im Subnet
    var repository = new FakeIPAdresseRepository(
        new IPAdresse { Adresse = "10.10.10.25", SubnetId = SubnetId });
    var service = new IPAdresseService(repository);

    // Act: Dieselbe Adresse, aber mit Leerzeichen davor und danach
    var resultat = service.Add("  10.10.10.25  ", SubnetId);

    // Assert: Als Duplikat erkannt, nichts zusätzlich gespeichert
    Assert.Equal(IPAdresseAddResult.BereitsVorhanden, resultat);
    Assert.Single(repository.GespeicherteIPAdressen);
}
```

**Was prüft der Test?** Ein Benutzer kopiert eine IP oft mit
Leerzeichen ins Eingabefeld. Würde der Service erst nach der
Duplikatprüfung trimmen, würde `"  10.10.10.25  "` nicht als Duplikat
erkannt. Je nach Ablauf würde dann die Datenbank mit einem Fehler
abbrechen oder eine zweite, fast gleiche Adresse gespeichert.

**Warum ist das ein echter Fehlerfall?** Der Test prüft nicht den
Normalfall, sondern eine Eingabe, die **abgelehnt** werden muss. Er
sichert die Reihenfolge im `IPAdresseService.Add` ab: zuerst trimmen,
dann prüfen. Ändert jemand später diese Reihenfolge, wird der Test rot.

## B4 – Kontrolle

-   **Alle Tests laufen grün im Test-Explorer:** Ja. Die ersten 23 Tests
    waren beim ersten Testlauf alle grün. Nach dem zusätzlichen
    Negativtest laufen alle 24 Tests grün.
-   **Kontrollfrage: Testet mindestens ein Test einen echten
    Fehlerfall?** Ja, 8 Negativtests: ungültige oder leere IP, Duplikat
    im gleichen Subnet (auch mit Leerzeichen), nicht existierende Id,
    leerer Subnet-Name beim Erstellen und Ändern, unbekannter Status.
-   Die App wurde zusätzlich von Hand getestet und verhält sich gleich
    wie vor K4.

## Vertiefung – Fake-Repositories

`FakeIPAdresseRepository` und `FakeSubnetRepository` implementieren
dieselben Interfaces wie die echten Repositories aus K3. Sie speichern
aber nur in einer Liste:

``` csharp
var repository = new FakeIPAdresseRepository();   // statt new IPAdresseRepository()
var service = new IPAdresseService(repository);
```

Über `GespeicherteIPAdressen` bzw. `GespeicherteSubnets` prüft der Test,
was der Service gespeichert hat.

**Warum Fakes?** Tests mit der echten Datenbank wären langsam, würden
die echte `app.db` verändern und hingen von vorhandenen Daten ab.

**Wäre das ohne das Interface aus K3 möglich gewesen?** Nein. Vor K3
hat jeder Service seine DataAccess-Klasse selbst mit `new` erstellt.
Erst weil der Service in K3 ein Interface über den Konstruktor bekommt,
kann der Test eine eigene Implementierung übergeben.

## Challenge – Testabdeckung

-   Die Methode mit den meisten Fallunterscheidungen ist der
    Status-Workflow. Jeder Übergang hat einen eigenen Test in
    `IPStatusWorkflowTests` und zusätzlich einen Fall im `[Theory]`-Test
    über den Service. Dazu kommt ein Test für den unbekannten Status.
-   **Anzahl Tests: 24** (21 × `[Fact]` + 1 × `[Theory]` mit 3 Fällen)
-   Code Coverage in Visual Studio: *Test → Analyze Code Coverage for
    All Tests*.
    -   Gut abgedeckt: `IPAdresseService` und `IPStatusWorkflow`
    -   Teilweise: `SubnetService` (`Add` und `Update` getestet, `GetAll`
        und `Delete` reichen nur an das Repository weiter)
    -   Nicht getestet: Repositories (brauchen eine echte Datenbank) und
        `MainWindow` (Oberfläche)

## Git

Branch `feature/k4-cleancode-tests`, ein Commit pro Aufgabe:

1.  `K4 - B1 Code-Review nach Clean-Code-Checkliste`
2.  `K4 B2: Zuständigkeiten getrennt (SRP)`
3.  `K4 B3: xUnit-Testprojekt mit 23 Tests`
4.  `K4 B4: Negativtest für Duplikat mit Leerzeichen, Doku ergänzt`

Anschliessend Merge Request nach `master` mit Review durch ein
Gruppenmitglied.

## Bekannte offene Punkte

-   **Keine Subnetz-Prüfung (CIDR):** Eine IP wird nur auf gültiges
    Format geprüft, nicht darauf, ob sie ins Subnet passt.
    `10.0.1.200` kann z. B. einem Subnet `192.168.5.0/24` zugeordnet
    werden. Diese Idee kam schon in K0 auf.
-   **Kurzformen von IPv4 werden akzeptiert:** `IPAddress.TryParse` aus
    .NET akzeptiert z. B. `10.1` und macht daraus `10.0.0.1`. Für eine
    IP-Verwaltung ist das unerwünscht. Eine strengere Prüfung (genau
    vier Zahlenblöcke) wäre eine sinnvolle Erweiterung mit eigenem
    Negativtest.
-   `Subnet.Erstellt` ist ein `string` statt `DateTime`. Eine Änderung
    bräuchte eine Migration.
-   `SubnetService.Update` wird vom UI noch nicht verwendet. Die Methode
    bleibt, weil sie getestet ist und das Bearbeiten von Subnetzen
    geplant ist.

------------------------------------------------------------------------

## KI-Protokoll K4

**Tool:** Claude (Anthropic)

### Eintrag 1 – Code-Review und Tests erstellen

**Prompt / Aufgabe:**\
Wir haben Claude die Aufgabenblätter K4 und «Zusatz Naming Convention»
gegeben: «Weiter mit K4. Kannst du diese Dokumente lesen und damit
weiterfahren?»

**Ergebnis der KI:**\
Claude prüfte unseren `master` gegen die Clean-Code-Checkliste und die
Naming Convention. Es fand die Magic Strings, die Naming-Verstösse und
die zwei SRP-Verstösse und lieferte die überarbeiteten Dateien, das
Testprojekt mit Fake-Repositories, 23 Tests und eine Doku-Vorlage.
Die Paketversionen (xUnit 2.9.3, Test-SDK 18.5.1) hat Claude online
geprüft. Kompilieren und testen konnte Claude nicht.

**Unsere Prüfung und Umsetzung:**\
Wir haben die Änderungen nicht als Ganzes übernommen, sondern
nachgefragt, was sich genau ändert (nächster Eintrag), und sie dann
Aufgabe für Aufgabe eingebaut und committet.

**Erkenntnis:**\
Die KI findet Code-Smells schnell, wenn man ihr die konkreten Regeln
(Checkliste, Naming Convention) mitgibt.

### Eintrag 2 – Nur echte Änderungen übernehmen

**Prompt / Aufgabe:**\
«Was ändert sich genau von K3 bis K4? Ich brauche genau nur diese
Änderungen», «Gib mir die Änderungen nach B1, B2, B3 und B4» und «Sag
mir bei B1, welche Dateien sich wie geändert haben. Wenn es nur
Kommentare sind, brauche ich das nicht.»

**Ergebnis der KI:**\
Claude listete die 18 betroffenen Dateien, teilte sie auf B1 bis B4 auf
(mit einer Zwischenversion für B1, damit jeder Schritt einzeln baut)
und zeigte bei B1 nur die echten Codeänderungen als Vorher/Nachher.

**Unsere Prüfung und Umsetzung:**\
Wir haben entschieden, die drei Dateien mit reinen
Kommentaränderungen (`IRepository.cs`, `IIPAdresseRepository.cs`,
`DbInitializer.cs`) nicht zu übernehmen. B1 bis B3 haben wir als je
einen Commit gepusht.

**Erkenntnis:**\
Je kleiner und klarer abgegrenzt die Änderungen sind, desto besser
können wir sie prüfen und erklären.

### Eintrag 3 – Commits begründen

**Prompt / Aufgabe:**\
«Bei B2 bitte via Commit begründen, weshalb welche Zuständigkeit
getrennt wurde», danach «kürzer bitte, wir committen alles zusammen».
Dasselbe für B3.

**Ergebnis der KI:**\
Claude schlug zuerst zwei ausführliche Commits vor. Nach unserem
Feedback lieferte es eine kurze Nachricht mit je einer Zeile Begründung
pro Änderung.

**Unsere Prüfung und Umsetzung:**\
Wir haben die kurze Variante verwendet, weil sie im Git-Verlauf
übersichtlicher ist.

**Erkenntnis:**\
Eine Commit-Nachricht soll das *Warum* enthalten, aber kurz bleiben.

### Eintrag 4 – Zusätzlicher Negativtest

**Prompt / Aufgabe:**\
«Schreibe einen Negativtest und ergänze diesen in Doku K4.»

**Ergebnis der KI:**\
Claude schrieb den Test
`Add_DuplikatMitLeerzeichen_GibtBereitsVorhandenZurueckUndSpeichertNichts`.
Er prüft, ob eine bereits vorhandene Adresse auch dann als Duplikat
erkannt wird, wenn sie mit Leerzeichen eingegeben wird. Vorher prüfte
Claude im Code des Services, dass zuerst getrimmt und erst dann auf
Duplikate geprüft wird, damit der Test grün sein muss. Zusätzlich wies
Claude auf eine Lücke hin: `IPAddress.TryParse` akzeptiert Kurzformen
wie `10.1` (wird zu `10.0.0.1`), und kein Test deckt das ab.

**Unsere Prüfung und Umsetzung:**\
Wir haben den Test in `IPAdresseServiceTests.cs` eingefügt und im
Test-Explorer ausgeführt. Alle 24 Tests sind grün.

**Erkenntnis:**\
Gute Negativtests entstehen aus realistischen Fehleingaben von
Benutzern (hier: kopierte IP mit Leerzeichen), nicht nur aus
offensichtlich falschen Werten wie `"keine-ip"`.

### Fragen aus dem Aufgabenblatt

**Welche Prompts habt ihr verwendet, um Code-Smells zu finden und Tests
zu generieren?**\
Wir haben Claude die beiden Aufgabenblätter (Clean Code / Unit Testing
und Naming Convention) als Grundlage gegeben und den aktuellen Code im
Repository prüfen lassen. Danach haben wir gezielt nachgefragt: welche
Dateien sich wie ändern, getrennt nach B1 bis B4, und nur echte
Codeänderungen ohne reine Kommentare.

**Wo hat die KI beim Testen gut geholfen, wo musstet ihr korrigieren
oder ergänzen?**\
Gut geholfen hat die KI beim Aufbau des Testprojekts (inklusive des
`Compile Remove`-Problems, das wir sonst erst beim Build bemerkt
hätten), bei den Fake-Repositories und bei der Abdeckung der
Negativfälle.\
Korrigieren mussten wir keinen Test: Alle 23 Tests liefen beim ersten
Ausführen grün. Ergänzt haben wir einen weiteren Negativtest (Duplikat
mit Leerzeichen), weil die bestehenden Negativtests nur offensichtlich
falsche Eingaben wie `"keine-ip"` oder einen leeren Text geprüft haben.

**Hat die KI fachlich falsche Testfälle vorgeschlagen? Woran habt ihr
das erkannt?**\
Falsch im Sinne von «testet etwas Falsches» haben wir keinen Test
gefunden. Zwei Punkte sind aber fachlich zu hinterfragen:

1.  `Add_GueltigeIPv6Adresse_GibtErfolgreichZurueck` zeigt eine Lücke:
    Geprüft wird nur das Format der IP, nicht, ob sie ins Subnet passt.
    Der Test bestätigt also ein Verhalten, das fachlich nicht ganz
    richtig ist (siehe «Bekannte offene Punkte»).
2.  `GetNextStatus_UnbekannterStatus_WirftArgumentOutOfRangeException`
    prüft ein **neues** Verhalten: Vorher wurde ein unbekannter Status
    auf «Frei» gesetzt, jetzt gibt es einen Fehler. Da die Datenbank
    (CHECK-Constraint) ohnehin nur 0, 1 und 2 zulässt, halten wir das
    für richtig.

Beide Punkte haben wir geprüft und teilen die Einschätzung: Punkt 1
ist eine bekannte fachliche Lücke (siehe «Bekannte offene Punkte»),
Punkt 2 ist eine gewollte Verbesserung.

**Reflexionsfrage: Was hat sich für euch am meisten verändert – der
Code selbst oder euer Vertrauen in seine Korrektheit?**\
Siehe Abschnitt «Reflexion» unten.

------------------------------------------------------------------------

## Reflexion

**Was hat sich mehr verändert: der Code oder unser Vertrauen in ihn?**\
Eindeutig das Vertrauen. Am Code selbst hat sich in K4 wenig geändert:
ein paar Konstanten, zwei ausgelagerte Zuständigkeiten, bessere Namen.
Die App kann genau dasselbe wie vorher. Neu ist, dass 24 Tests in
wenigen Sekunden zeigen, ob Validierung, Duplikatprüfung und
Status-Workflow noch funktionieren. Bisher haben wir nach jeder
Änderung die App gestartet und von Hand durchgeklickt, und dabei nie
alle Fälle geprüft (z. B. eine nicht existierende Id oder eine
Eingabe mit Leerzeichen).

**Alle Tests waren sofort grün – und genau deshalb haben wir noch einen
Negativtest ergänzt.**\
Dass alle 23 Tests beim ersten Lauf grün waren, hat uns zuerst
gefreut. Dann haben wir uns gefragt, was das eigentlich beweist: Ein
grüner Test zeigt nur, dass der Code das tut, was der Test erwartet.
Wenn alle Tests auf Anhieb bestehen, kann das auch heissen, dass sie zu
einfache Fälle prüfen. Unsere bisherigen Negativtests nahmen nur
offensichtlich falsche Eingaben (`"keine-ip"`, leerer Text). Deshalb
haben wir einen Fall ergänzt, der in der Praxis wirklich vorkommt: eine
bereits vorhandene IP, die mit Leerzeichen eingefügt wird. Auch dieser
Test ist grün. Jetzt wissen wir aber, dass die Duplikatprüfung auch in
diesem Fall funktioniert, und wenn jemand später die Reihenfolge im
Service ändert, merken wir es sofort.

**Was wir aus K4 mitnehmen:**

-   **K3 war die Voraussetzung für K4.** Ohne die Interfaces aus K3
    hätten wir die Services nur mit der echten Datenbank testen können.
    Erst beim Schreiben der Fake-Repositories haben wir verstanden,
    wozu Dependency Injection gut ist.
-   **Tests zeigen auch, was fehlt.** Die Tests bestätigen das heutige
    Verhalten, auch dort, wo es fachlich nicht ganz richtig ist
    (keine Subnetz-Prüfung, Kurzformen wie `10.1`). Ein grüner Test
    heisst nur, dass der Code tut, was der Test erwartet.
-   **Clean Code ist eine Entscheidung, keine Checkliste.** Bei der
    Sprache (Deutsch/Englisch) haben wir bewusst eine Regel festgelegt,
    statt alles umzubenennen. Bei den reinen Kommentaränderungen haben
    wir entschieden, sie wegzulassen.

**Was wir nächstes Mal anders machen würden:**\
Tests früher schreiben, nicht erst am Schluss. Viele Fehler aus K1 bis
K3 (z. B. die vertauschte Schreibweise von Klassennamen oder fehlende
Dateien) hätten wir schneller gefunden, wenn nach jedem Schritt
automatisch Tests gelaufen wären.
