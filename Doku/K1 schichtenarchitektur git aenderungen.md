# MODUL PROG1 – K1: Schichtenarchitektur

**Projekt:** IPAM_WPF_App\
**Autoren:** Madalina Luca, Muzamil Abukar Haji Ali, Sabri Mahdi\
**Branch:** `sabri/feature/k1-schichten` → `master` (Pull Request #2)

## Inhaltsverzeichnis

1.  [Ziel](#ziel)
2.  [Die Idee: drei Schichten](#die-idee-drei-schichten)
3.  [Projektstruktur nach K1](#projektstruktur-nach-k1)
4.  [Welche Klasse macht was?](#welche-klasse-macht-was)
5.  [Änderungen gegenüber K0 und warum](#änderungen-gegenüber-k0-und-warum)
6.  [Ablauf: Klick auf «IP hinzufügen»](#ablauf-klick-auf-ip-hinzufügen)
7.  [Übersicht: Serviceklasse → DataAccess](#übersicht-serviceklasse--dataaccess)
8.  [Kontrolle](#kontrolle)
9.  [Git](#git)
10. [Stolpersteine](#stolpersteine)
11. [KI-Protokoll K1](#ki-protokoll-k1)
12. [Reflexion](#reflexion)

## Ziel

Die App aus K0 funktioniert, aber Oberfläche, Regeln und Datenbank sind
vermischt. In K1 wird der Code in **drei Schichten** aufgeteilt. Für den
Benutzer ändert sich nichts: Die App kann genau dasselbe wie vorher.

## Die Idee: drei Schichten

``` text
UI (MainWindow.xaml.cs)        "Was hat der Benutzer geklickt? Was zeige ich an?"
        ↓
Services                       "Ist das erlaubt? Was ist die Regel?"
        ↓
DataAccess                     "Wie lese/speichere ich das in der Datenbank?"
        ↓
SQLite (app.db)
```

**Regeln:**

-   Jede Schicht ruft nur die **direkt darunterliegende** auf.
-   Keine Schicht wird übersprungen: Die UI spricht nie direkt mit
    DataAccess.
-   Die Abhängigkeit zeigt nur nach unten: DataAccess weiss nichts von
    der UI.

**Warum?** Wenn sich etwas ändert, muss man nur an einer Stelle
eingreifen. Beispiel: In K2 wird die Datenbank-Technik ausgetauscht, und
dabei muss nur DataAccess angepasst werden. UI und Services bleiben
gleich.

## Projektstruktur nach K1

``` text
IPAM_WPF_App/
├── Models/
│   ├── IPAdresse.cs
│   ├── IPStatus.cs              (neu)
│   └── Subnet.cs
├── Services/                    (neu)
│   ├── ApplicationService.cs
│   ├── IPAdresseService.cs
│   └── SubnetService.cs
├── DataAccess/
│   ├── DbInitializer.cs
│   ├── IPAdresseDataAccess.cs   (ersetzt IPAdresseRepository.cs)
│   └── SubnetDataAccess.cs      (ersetzt SubnetRepository.cs)
├── App.xaml / App.xaml.cs
└── MainWindow.xaml / MainWindow.xaml.cs
```

## Welche Klasse macht was?

### UI-Schicht

| Klasse | Aufgabe |
|---|---|
| `MainWindow.xaml` | Oberfläche: Tabellen, Eingabefelder, Buttons |
| `MainWindow.xaml.cs` | Nimmt Klicks entgegen, ruft **nur Services** auf und zeigt Ergebnisse oder Meldungen an. Enthält **kein SQL** und **keine Regeln** mehr. |

### Service-Schicht (Business-Logik)

| Klasse | Aufgabe |
|---|---|
| `ApplicationService` | Startet beim Programmstart die Datenbank-Initialisierung. So muss die UI `DbInitializer` nicht kennen. |
| `SubnetService` | Regeln für Subnetze: Eingaben trimmen, leere Namen ablehnen. Gibt `true`/`false` zurück, ob das Speichern geklappt hat. |
| `IPAdresseService` | Regeln für IP-Adressen: Prüft mit `IPAddress.TryParse`, ob die Eingabe eine gültige IPv4/IPv6-Adresse ist. Meldet Duplikate. Kennt den Status-Workflow Frei → Reserviert → Zugewiesen → Frei. |
| `IPAdresseAddResult` (Enum in `IPAdresseService.cs`) | Ergebnis beim Hinzufügen: `Erfolgreich`, `Ungueltig` oder `BereitsVorhanden`. Damit muss die UI keine Datenbankfehler mehr kennen. |

### DataAccess-Schicht

| Klasse | Aufgabe |
|---|---|
| `DbInitializer` | Erstellt die Tabellen. Hält den **einzigen** Connection-String und öffnet Verbindungen über `CreateOpenConnection()`. Benennt beim Start alte Spalten automatisch um (siehe unten). |
| `SubnetDataAccess` | SQL für Subnetze: `GetAll`, `Add`, `Update`, `Delete` |
| `IPAdresseDataAccess` | SQL für IP-Adressen: `GetBySubnetId`, `TryAdd` (gibt `false` bei Duplikat zurück), `Update`, `UpdateStatus`, `Delete` |

### Models

| Klasse | Aufgabe |
|---|---|
| `Subnet` | Datenklasse: `Id`, `Name`, `Beschreibung`, `Erstellt` |
| `IPAdresse` | Datenklasse: `Id`, `Adresse`, `Status`, `SubnetId` |
| `IPStatus` | Enum mit `Frei = 0`, `Reserviert = 1`, `Zugewiesen = 2` |

## Änderungen gegenüber K0 und warum

| Änderung | Warum |
|---|---|
| Neuer Ordner `Services/` mit drei Serviceklassen | Die Regeln (Validierung, Workflow) stehen jetzt an einer Stelle statt im Code-Behind. |
| `SubnetRepository` / `IPAdresseRepository` → `SubnetDataAccess` / `IPAdresseDataAccess` | Laut Aufgabe heissen die Klassen in K1/K2 `DataAccess`. Echte Repositories (mit Interface) kommen erst in K3. |
| `MainWindow.xaml.cs` ruft nur noch Services auf | UI und Datenbank sind getrennt. Kontrollfrage K1: kein «Sqlite» mehr im Code-Behind. |
| Doppelte IP: Die UI bekommt `IPAdresseAddResult.BereitsVorhanden` statt einer `SqliteException` | Die UI muss keine Datenbank-Fehlercodes kennen. In K0 gab es dabei einen Fehler: Doppelte IPs wurden stillschweigend nicht gespeichert, ohne Meldung. |
| Connection-String nur noch in `DbInitializer` | Vorher stand er dreimal im Code. Bei einer Änderung hätte man ihn an allen Stellen anpassen müssen. |
| Spalten umbenannt: `Titel` → `Adresse`, `Erledigt` → `Status`, `ProjektId` → `SubnetId` | Sprechende Namen. Zudem erkennt EF Core in K2 nur `SubnetId` automatisch als Fremdschlüssel. Eine bestehende `app.db` wird beim Start automatisch umbenannt (`ALTER TABLE ... RENAME COLUMN`), die Daten bleiben erhalten. |
| Enum `IPStatus` statt Zahlen 0/1/2 | Der Code ist lesbar (`IPStatus.Reserviert` statt `1`). |
| Unbenutztes NuGet-Paket `EPPlus` entfernt | Wurde nirgends verwendet. |
| Einheitliche Schreibweise `IP` (z. B. `IPStatus`, `IPAdresseAddResult`) | .NET-Konvention: Abkürzungen mit zwei Buchstaben werden komplett gross geschrieben (wie `IPAddress` in .NET selbst). |
| Dateiname = Klassenname (z. B. `IPAdresseService.cs`) | Man findet jede Klasse sofort. |

## Ablauf: Klick auf «IP hinzufügen»

1.  **UI:** `AddIpAdresseButton_Click` liest den Text und das
    ausgewählte Subnet und ruft `_ipAdresseService.Add(text, subnetId)`
    auf.
2.  **Service:** `IPAdresseService.Add` trimmt den Text und prüft mit
    `IPAddress.TryParse`. Ist die Eingabe ungültig, gibt er
    `Ungueltig` zurück. Sonst ruft er `IPAdresseDataAccess.TryAdd` auf.
3.  **DataAccess:** `TryAdd` führt das `INSERT` aus. Verletzt die
    Adresse die UNIQUE-Regel, fängt die Methode den Fehler ab und gibt
    `false` zurück.
4.  **Service:** Aus `false` wird `BereitsVorhanden`, aus `true` wird
    `Erfolgreich`.
5.  **UI:** Zeigt je nach Ergebnis eine Meldung oder lädt die Liste neu.

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

## Kontrolle

-   App gebaut und getestet: Subnet hinzufügen/löschen, IP hinzufügen,
    Duplikat wird gemeldet, Status weiterschalten, IP löschen.
-   **Kontrollfrage:** Ausserhalb von `DataAccess/` kommt im Code kein
    «Sqlite» mehr vor.

## Git

-   Die erste K1-Version entstand im Branch `feature/k1-services`, wurde
    aber nie gemergt. Der Branch wurde später gelöscht.
-   Der vollständige K1-Stand inklusive Review-Verbesserungen wurde im
    Branch `sabri/feature/k1-schichten` umgesetzt und über
    **Pull Request #2** in `master` gemergt.
-   Danach wurden auf `master` die Schreibweisen vereinheitlicht
    (`IPStatus`, `IPAdresseAddResult`, Dateinamen).

## Stolpersteine

| Problem | Lösung |
|---|---|
| `master` war noch auf K0-Stand, der K1-Branch wurde nie gemergt | Neuer Branch von `master`, K1-Stand dort vollständig eingebaut |
| Nach dem Kopieren fehlten `SubnetService`, `ApplicationService` und die neue `MainWindow.xaml.cs` → Build-Fehler | Fehlende Dateien ergänzt |
| Dateinamen waren plötzlich klein geschrieben (`Ipstatus.cs`) | Umbenannt. Weil Windows Gross-/Kleinschreibung ignoriert, erkennt Git eine reine Änderung der Schreibweise nicht. Lösung: über einen Zwischennamen umbenennen (mit Commit dazwischen) oder direkt auf GitHub. |
| Klassenname und Verwendung unterschiedlich geschrieben (`IpAdresseDataAccess` vs. `IPAdresseDataAccess`) → Build-Fehler | Vereinheitlicht. C# unterscheidet Gross-/Kleinschreibung. |

------------------------------------------------------------------------

## KI-Protokoll K1

**Tool:** Claude (Anthropic)

### Eintrag 1 – Review unseres bestehenden Stands

**Prompt / Aufgabe:**\
Wir haben Claude die drei Aufgabenblätter K1 bis K3 und unser
GitHub-Repository gegeben und gebeten, ab K1 zu prüfen, ob wir die
Aufgaben richtig umgesetzt haben. Ausdrücklich ohne Änderungen am Repo,
weil wir die Anpassungen selbst machen wollten.

**Ergebnis der KI:**\
Claude fand, dass unser `master` noch im Kern K0 war und der K1-Branch
`feature/k1-services` nie gemergt wurde. Im K1-Branch selbst war die
Schichtentrennung korrekt. Als Verbesserungspunkte nannte Claude:
Dateiname ≠ Klassenname, Connection-String dreifach kopiert,
irreführende Spaltennamen aus der Vorlage, Magic Numbers beim Status,
ungenutztes Paket `EPPlus`, fehlende Übersichtstabelle und fehlendes
KI-Protokoll. Zusätzlich einen Fehler auf `master`: Doppelte IPs wurden
ohne Meldung verworfen.

**Unsere Prüfung und Umsetzung:**\
Wir haben die Punkte mit dem Aufgabenblatt abgeglichen und entschieden,
alle umzusetzen, auch die Umbenennung der Spalten vor K2.

**Erkenntnis:**\
Ein Review von aussen findet Dinge, die man selbst übersieht, z. B. dass
ein Branch nie gemergt wurde. Wir müssen nach jedem Kapitel prüfen, ob
der Stand wirklich in `master` angekommen ist.

### Eintrag 2 – Verbesserungen umsetzen und einbauen

**Prompt / Aufgabe:**\
«Kannst du die Verbesserungspunkte umsetzen und mir die angepassten
Files in diesem Chat zusenden?»

**Ergebnis der KI:**\
Claude lieferte die angepassten Dateien: zentraler Connection-String,
Enum `IPStatus`, umbenannte Spalten inklusive automatischer Umbenennung
einer bestehenden `app.db`, bereinigte `.csproj`, Übersichtstabelle und
das wiederhergestellte KI-Protokoll. Die Spaltenumbenennung hat Claude
vorher mit SQLite getestet (UNIQUE, CHECK und CASCADE bleiben erhalten).
Kompilieren konnte Claude den Code nicht.

**Unsere Prüfung und Umsetzung:**\
Wir haben einen neuen Branch erstellt und die Dateien eingebaut. Dabei
fiel auf, dass die gelieferten Dateien auf dem K1-Branch basierten,
während wir auf `master` arbeiteten: `SubnetService`,
`ApplicationService` und die neue `MainWindow.xaml.cs` fehlten noch.
Diese haben wir ergänzt. Danach haben wir in Visual Studio gebaut, die
App durchgetestet und über Pull Request #2 gemergt.

**Erkenntnis:**\
KI-Code muss man immer gegen den eigenen, aktuellen Stand prüfen. Die
Dateien passten zu einem anderen Branch als dem, auf dem wir gearbeitet
haben.

### Eintrag 3 – Dateinamen und Schreibweise

**Prompt / Aufgabe:**\
Nachdem wir die Dateien lokal umbenannt hatten, waren sie auf GitHub
immer noch klein geschrieben. Wir fragten, warum, und später: «Ist
`IpStatus` extra so geschrieben? Wieso nicht gleich wie `IPAdresse`?»

**Ergebnis der KI:**\
Claude erklärte, dass Git unter Windows reine Änderungen der
Gross-/Kleinschreibung nicht erkennt, und zeigte den Umweg über einen
Zwischennamen. Zur Schreibweise erklärte Claude die .NET-Konvention
(Abkürzungen mit zwei Buchstaben komplett gross, wie `IPAddress`) und
bestätigte, dass `IpStatus` eine Inkonsistenz aus den eigenen
Vorschlägen war. Ausserdem lieferte es die Liste aller betroffenen
Stellen.

**Unsere Prüfung und Umsetzung:**\
Die Inkonsistenz haben wir selbst entdeckt. Wir haben die Umbenennungen
direkt auf GitHub gemacht und `IpStatus` → `IPStatus` sowie
`IpAdresseAddResult` → `IPAdresseAddResult` vereinheitlicht. Zwei
dabei entstandene Build-Fehler (Klassenname und Verwendung
unterschiedlich geschrieben) haben wir nach dem Hinweis korrigiert.

**Erkenntnis:**\
Einheitliche Namen sind keine Kosmetik: In C# führt eine andere
Schreibweise direkt zum Build-Fehler. Auch die KI ist bei Namen nicht
immer konsistent, deshalb lohnt sich ein kritischer Blick.

### Fragen aus dem Aufgabenblatt

**Welche Prompts habt ihr verwendet, um den Umbau zu planen?**\
Für den ersten Umbau in drei Schichten (Branch `feature/k1-services`,
18. September) haben wir Claude das Aufgabenblatt K1 gegeben und
gebeten, uns beim Umbau unserer App in die Schichtenarchitektur zu
helfen (sinngemäss, der genaue Wortlaut ist nicht mehr vorhanden).\
Für die Überprüfung und Verbesserung haben wir Claude die
Aufgabenblätter und unser Repository gegeben: «Bitte überprüfe von K1
an, ob wir das richtig umgesetzt haben» und danach «Kannst du die
Verbesserungspunkte umsetzen und mir die angepassten Files zusenden?»

**Was hat die KI beim Aufteilen der Schichten gut gemacht, wo musstet
ihr korrigieren?**\
Gut: Claude hat die Schichtentrennung gegen die Regeln aus dem
Aufgabenblatt geprüft (keine übersprungene Schicht, kein «Sqlite» im
Code-Behind) und dabei auch gefunden, dass unser K1-Branch nie gemergt
wurde. Korrigieren mussten wir: Die gelieferten Dateien passten zum
K1-Branch, nicht zu unserem `master`. Es fehlten zwei Services und die
neue `MainWindow.xaml.cs`. Zudem war die Schreibweise uneinheitlich
(`IpStatus` neben `IPAdresse`), was wir selbst bemerkt haben.

**Wo wollte die KI zu schnell zu Interfaces, Repository oder EF Core
springen, und wie habt ihr sie zurückgeholt?**\
Bei Claude ist das nicht passiert. Der Vorgriff steckte schon in
unserem K0-Code: Die Klassen hiessen bereits `SubnetRepository` und
`IPAdresseRepository`, obwohl echte Repositories (mit Interface) erst
in K3 kommen. Claude wies darauf hin, dass sie in K1/K2 laut Aufgabe
`DataAccess` heissen sollen. Die Spaltenumbenennung hat Claude zwar mit
Blick auf K2 empfohlen, aber mit reinem SQL umgesetzt, ohne EF Core
vorwegzunehmen.

------------------------------------------------------------------------

## Reflexion

**Was ist an unserer App jetzt einfacher als vorher?**

-   **Jede Aufgabe hat ihren festen Platz.** Wer eine Regel ändern will
    (z. B. den Status-Workflow), sucht im Service. Wer an der Datenbank
    arbeitet, sucht in DataAccess. Vorher war alles im Code-Behind
    vermischt.
-   **Die Oberfläche ist schlanker.** `MainWindow.xaml.cs` kennt kein
    SQL und keine Datenbankfehler mehr. Es reagiert nur auf Ergebnisse
    wie `BereitsVorhanden`.
-   **Lesbarer Code.** `IPStatus.Reserviert` statt `1`, `Adresse` statt
    `Titel`.

**Was ist (noch) komplizierter geworden?**

-   **Mehr Dateien und Klassen.** Ein einziger Klick läuft jetzt durch
    drei Klassen (UI → Service → DataAccess). Für eine so kleine App
    wirkt das zuerst umständlich.
-   **Mehr Stellen, die zusammenpassen müssen.** Das haben wir bei den
    Build-Fehlern gemerkt: Eine falsche Schreibweise in einer Datei
    bricht den ganzen Build.
-   **Git wurde anspruchsvoller.** Mehrere Branches, Umbenennungen
    unter Windows und ein nie gemergter Branch haben uns viel Zeit
    gekostet.

**Was wir gelernt haben:**\
Der Nutzen der Schichten zeigt sich nicht sofort, sondern beim
nächsten Umbau. In K2 mussten wir tatsächlich nur DataAccess ändern.
Ausserdem prüfen wir jetzt nach jedem Kapitel, ob der Stand wirklich
in `master` angekommen ist.
