# MODUL PROG1 – K5: Git und Kollaboration

**Projekt:** IPAM_WPF_App\
**Autoren:** Madalina Luca, Muzamil Abukar Haji Ali, Sabri Mahdi\
**Branch:** `feature/k5-git`\
**Stand:** 27.11.2026 (1 Commit auf `feature/k5-git`, noch nicht gemergt)

## Inhaltsverzeichnis

1.  [Ziel](#ziel)
2.  [Stand der Aufgaben](#stand-der-aufgaben)
3.  [B1 – Git-Verlauf prüfen](#b1--git-verlauf-prüfen)
4.  [B2 – .gitignore und Secrets](#b2--gitignore-und-secrets)
5.  [B3 – Pull-Request-Workflow](#b3--pull-request-workflow)
6.  [B4 – Merge-Konflikt](#b4--merge-konflikt)
7.  [Vertiefung und Challenge](#vertiefung-und-challenge)
8.  [Offene Punkte](#offene-punkte)
9.  [KI-Protokoll K5](#ki-protokoll-k5)
10. [Reflexion](#reflexion)

## Ziel

K5 verändert die App inhaltlich nicht mehr. Ziel ist, dass der Weg von
K0 bis K4 im Git-Verlauf nachvollziehbar ist und dass wir als Gruppe
professionell zusammenarbeiten: Feature-Branches, Pull Requests mit
Review, bewusst gelöste Konflikte und keine Secrets im Repository.

## Stand der Aufgaben

| Aufgabe | Stand 27.11.2026 |
|---|---|
| B1 – Git-Verlauf prüfen und aufräumen | Geprüft (siehe Abschnitt 3), Aufräumen offen |
| B2 – .gitignore und Secrets | **Umgesetzt** (Commit `1e35fee`) |
| B3 – Pull-Request-Workflow mit Review | Offen |
| B4 – Merge-Konflikt provozieren und lösen | Offen |
| Vertiefung – README mit Lernweg | Offen |
| Challenge – Tags pro K-Modul | Offen (bisher keine Tags im Repository) |

------------------------------------------------------------------------

## B1 – Git-Verlauf prüfen

### Branches und Pull Requests K1–K4

| Kapitel | Branch | Pull Request | Schema `feature/kN-thema` |
|---|---|---|---|
| K1 | `Sabri/DB-K1` | #1 | nein |
| K1 | `feature/k1-services` | – (nie gemergt, gelöscht) | ja |
| K1 | `sabri/feature/k1-schichten` | #2 | fast (Präfix `sabri/`) |
| K2 | `feature/k2-efcore` | #3 | ja |
| K3 | `feature/k3-repository-dto` | #4 | ja |
| K4 | `feature/k4-cleancode-tests` | #5 | ja |
| K5 | `feature/k5-git` | – (in Arbeit) | ja |

**Ergebnis:** K2 bis K5 folgen dem Schema genau. Bei K1 weichen die
Namen ab, und die erste K1-Version wurde nie gemergt. Jedes Kapitel
K1–K4 ist aber über mindestens einen Pull Request und Commits mit
Präfix (`K2: …`, `K3: …`, `K4 B1: …`) im Verlauf erkennbar.

### Wer hat gemergt?

| Pull Request | Commits von | Gemergt von |
|---|---|---|
| #1 (K1) | sma128041 | sma128041 |
| #2 (K1) | sma128041 | sma128041 |
| #3 (K2) | sma128041 | sma128041 |
| #4 (K3) | sma128041 | sma128041 |
| #5 (K4) | mlu160999 (3), sma128041 (2) | sma128041 |

**Ehrliche Feststellung:** Bei #1 bis #4 hat dasselbe Konto committet
und gemergt. Die Regel aus K5 («Wer den Code geschrieben hat, merged
ihn nicht selbst») war damit noch nicht eingehalten. Erst bei #5 hat
eine andere Person den Grossteil der Commits gemacht als die, die
gemergt hat. Das üben wir in B3 bewusst.

### Mögliche Aufräumarbeiten (offen)

-   Tags `v1.0-k1` bis `v1.0-k4` auf die Merge-Commits setzen
    (Challenge). Damit wird der Lernweg auch ohne die abweichenden
    K1-Branch-Namen eindeutig.
-   Den Lernweg in der README zusammenfassen (Vertiefung).

------------------------------------------------------------------------

## B2 – .gitignore und Secrets

### Prüfung des gesamten Verlaufs

Wir haben alle 63 Commits im Repository geprüft:

| Prüfung | Ergebnis |
|---|---|
| Datenbankdateien (`*.db`, `*.sqlite`) je committet? | Nein |
| `appsettings.json` oder `.env` je committet? | Nein |
| Passwörter, API-Keys, Tokens im Code? | Nein. Treffer gab es nur in den Kommentaren der GitHub-Vorlage `.github/workflows/dotnet-desktop.yml`, die auf GitHub-Secrets verweisen, aber keine Werte enthalten. |
| `bin/` und `obj/` je committet? | Nein |
| Connection Strings im Verlauf | Nur `Data Source=app.db` (relativer Dateipfad, kein Passwort) |

**Bewertung:** Ein echtes Secret war nie im Repository. Der Connection
String unserer SQLite-Datenbank enthält kein Passwort, nur einen
Dateinamen. Ein Secret ungültig zu machen (Aufgabe B2.3) war deshalb
nicht nötig. Trotzdem haben wir die Konfiguration nach der Vorgabe aus
K5 vom Code getrennt, damit auch ein späterer Wechsel auf eine Datenbank
mit Passwort sicher ist.

### Umsetzung: Commit `1e35fee` «K5 B2: lokale Konfiguration vom Repository trennen.»

| Datei | Änderung | Warum |
|---|---|---|
| `appsettings.Example.json` (**neu**) | Vorlage mit Platzhalter: `"Ipam": "Data Source=<LOKALER_DB_PFAD>"` | Zeigt, welche Einstellung nötig ist, ohne einen echten Wert ins Repository zu legen (Aufgabe B2.4) |
| `.gitignore` | Ignoriert `/appsettings.json`, `/appsettings.*.json` (ausser der Example-Datei), `.env`, `.env.*` sowie alle SQLite-Dateien (`*.db`, `*.db-*`, `*.sqlite*`) | Lokale Konfiguration und die lokale Datenbank gehören nicht ins Repository |
| `DataAccess/AppDbContext.cs` | Der Connection String ist nicht mehr fest im Code (`"Data Source=app.db"`), sondern wird aus `appsettings.json` gelesen (`ConnectionStrings:Ipam`) | Konfiguration und Code getrennt |
| `IPAM_WPF_App.csproj` | `appsettings.json` wird beim Build ins Ausgabeverzeichnis kopiert, aber nie mit veröffentlicht | Die App findet die Datei im `bin`-Ordner. Eine veröffentlichte Version soll keine lokale Konfiguration enthalten. |

**So liest `AppDbContext` den Connection String:**

1.  Pfad zu `appsettings.json` im Programmordner bestimmen.
2.  Fehlt die Datei → Fehlermeldung «Lokale appsettings.json fehlt.
    Vorlage kopieren und ConnectionStrings:Ipam eintragen.»
3.  Fehlt `ConnectionStrings:Ipam` oder ist es kein Text → eigene
    Fehlermeldung.
4.  Steht noch der Platzhalter `<LOKALER_DB_PFAD>` drin → Fehlermeldung
    «Lokalen SQLite-Pfad in appsettings.json eintragen.»
5.  Sonst wird der Wert verwendet.

Die Fehlermeldungen sagen genau, was zu tun ist. Damit ist ein
vergessener Einrichtungsschritt sofort erkennbar.

**Einrichtung nach dem Klonen:**

``` text
1. appsettings.Example.json kopieren und als appsettings.json speichern
2. <LOKALER_DB_PFAD> ersetzen, z. B. "Data Source=app.db"
3. Projekt bauen und starten
```

**Auswirkung auf Tests und Architektur:** Keine. Die Unit Tests
verwenden Fake-Repositories und brauchen keine Datenbank. Die Änderung
betrifft nur `AppDbContext` in der DataAccess-Schicht.

------------------------------------------------------------------------

## B3 – Pull-Request-Workflow

**Offen.** Geplant: eine kleine, echte Anpassung auf eigenem Branch,
Pull Request eröffnen, eine andere Person aus der Gruppe reviewt und
merged.

Passende Kandidaten aus unseren offenen Punkten:

-   Einrichtungsschritt für `appsettings.json` in der README ergänzen
-   Bearbeiten von Subnetzen im UI (Button für `SubnetService.Update`)

------------------------------------------------------------------------

## B4 – Merge-Konflikt

**Offen.** Geplant: denselben UI-Text auf zwei Branches unterschiedlich
ändern, Konflikt mit dem Mergetool lösen und hier festhalten, welche
Version wir gewählt haben und warum.

Ungeplante Merge-Konflikte hatten wir bereits in K3: Beim Merge von
`origin/master` in `feature/k3-repository-dto` entstanden zwei
Konflikte (`IPAdresseDataAccess.cs`, `SubnetDataAccess.cs`: auf einem
Branch gelöscht, auf dem anderen geändert). Wir haben sie in Visual
Studio mit «Delete File» gelöst, weil die Repositories aus K3 diese
Klassen ersetzen. Das ersetzt die bewusste Übung in B4 aber nicht,
weil es kein Konflikt in derselben Codezeile war.

------------------------------------------------------------------------

## Vertiefung und Challenge

| Aufgabe | Stand |
|---|---|
| README mit Lernweg K0–K5 anhand der Branches und Pull Requests | offen |
| Tags `v1.0-k1` bis `v1.0-k4` (+ `v1.0-k5`) | offen |

**Wofür Tags?** Eine Commit-Nachricht beschreibt eine einzelne
Änderung und verschwindet in der Liste aller Commits. Ein Tag markiert
einen bestimmten Stand dauerhaft mit einem Namen (z. B. «Ende K2»).
Man kann diesen Stand jederzeit direkt auschecken oder vergleichen
(`git diff v1.0-k2 v1.0-k3`), ohne Commit-Hashes suchen zu müssen.

------------------------------------------------------------------------

## Offene Punkte

| Punkt | Grund |
|---|---|
| Nach dem Klonen startet die App ohne `appsettings.json` nicht (unbehandelte Exception beim Start) | Die Meldung ist klar, erscheint aber als Absturz. Die README muss den Einrichtungsschritt nennen, oder `App.xaml.cs` fängt den Fehler ab und zeigt eine MessageBox. K5 verlangt, dass `main` fehlerfrei startet. |
| `appsettings.json` wird bei jedem Datenbankzugriff neu gelesen | `ConnectionString` ist eine Property, die bei jedem neuen `AppDbContext` die Datei liest. Funktioniert, ist aber unnötig. Einmal lesen und merken wäre besser. |
| B1 (Tags/README), B3, B4, Vertiefung, Challenge | noch nicht umgesetzt |

------------------------------------------------------------------------

## KI-Protokoll K5

### Eintrag 1 – Umsetzungsanleitung für K5 (ChatGPT)

**Tool:** ChatGPT

**Prompt:**\
«can you create a word documentation first with everything that needs
to be changed in the code according to K5 Folie? explain exactly what
code needs to be changed and in what class.»

**Ergebnis der KI:**\
Eine Anleitung als Word-Dokument mit den nötigen Codeänderungen pro
Klasse (wie im Prompt verlangt). Das Dokument selbst liegt nicht im
Repository.

**Unsere Prüfung und Umsetzung:**\
Umgesetzt haben wir daraus B2 im Commit `1e35fee`: `.gitignore`
ergänzt, `appsettings.Example.json` angelegt, Connection String aus
`AppDbContext` in eine lokale `appsettings.json` ausgelagert und auf
`feature/k5-git` gepusht.
⚠️ *[Bitte bestätigen: App mit lokaler `appsettings.json` gebaut,
gestartet und Tests grün?]*

**Erkenntnis:**\
Wir haben die KI zuerst um eine Anleitung gebeten statt direkt um Code.
So konnten wir vor der Umsetzung sehen, welche Klassen betroffen sind.

### Eintrag 2 – Branch-Name (Claude)

**Tool:** Claude

**Prompt:**\
K5-Aufgabenblatt angehängt: «hier K5. Sag mir den Namen für den Branch.»

**Ergebnis der KI:**\
`feature/k5-git` nach dem Schema `feature/kN-thema` aus Abschnitt 2.3,
mit dem Hinweis, vorher `master` zu pullen, und dass für B3 und B4
weitere Branches nötig sind.

**Unsere Prüfung und Umsetzung:**\
Branch `feature/k5-git` von `master` erstellt.

**Erkenntnis:**\
Das Schema steht im Aufgabenblatt. Die Lehre aus K3 (zuerst `master`
pullen) haben wir dieses Mal umgesetzt: Der Branch startete auf dem
aktuellen Stand nach PR #5.

### Eintrag 3 – Verlauf prüfen und dokumentieren (Claude)

**Tool:** Claude

**Prompt:**\
K5-Aufgabenblatt angehängt: «nun noch K5 mitdokumentieren. Sieh dir die
K5-Commits auf dem K5-Branch an», zusammen mit unserem
ChatGPT-Prompt.

**Ergebnis der KI:**\
Claude hat den Commit `1e35fee` Datei für Datei ausgewertet und den
gesamten Verlauf (63 Commits) auf Datenbankdateien, `appsettings`,
Passwörter, Keys und `bin/obj` durchsucht. Ergebnis: kein Secret im
Verlauf. Zusätzlich hat Claude ausgewertet, wer die Pull Requests
gemergt hat, und zwei Schwachstellen im B2-Commit gefunden (Absturz
ohne `appsettings.json`, wiederholtes Lesen der Datei). Diese
Dokumentation hat Claude geschrieben.

**Unsere Prüfung und Umsetzung:**\
Wir haben angegeben, mit welchem Tool der K5-Prompt gestellt wurde
(ChatGPT). Die gefundenen Punkte stehen unter «Offene Punkte».
⚠️ *[Bitte ergänzen, sobald umgesetzt: wie ihr die zwei Punkte
behoben habt.]*

**Erkenntnis:**\
Die Prüfung auf Secrets muss den **ganzen Verlauf** umfassen, nicht nur
den aktuellen Stand. Ein Secret in einem alten Commit bleibt sichtbar,
auch wenn die Datei heute gelöscht ist.

### Fragen aus dem Aufgabenblatt

**Welche Prompts habt ihr verwendet, um Konflikte zu verstehen oder
PR-Texte zu formulieren?**\
⚠️ *[Noch offen, wird nach B3 und B4 ergänzt.]*

**Wie ist der Pull-Request-Workflow in der Gruppe gelaufen, was hat das
Review gebracht?**\
⚠️ *[Noch offen, wird nach B3 ergänzt.]* Bisheriger Stand: In K1 bis
K4 wurden die Pull Requests meist vom selben Konto gemergt, das auch
committet hat (siehe B1). Ein echtes gegenseitiges Review hat damit
noch nicht stattgefunden.

**Was war die grösste Hürde beim Merge-Konflikt, und wie habt ihr sie
gelöst?**\
⚠️ *[Noch offen, wird nach B4 ergänzt.]*

**Abschlussfrage: Zeigt euer Git-Verlauf von K0 bis K5 euren Lernweg
nachvollziehbar? Was würdet ihr anders machen?**\
Siehe Abschnitt «Reflexion».

------------------------------------------------------------------------

## Reflexion

**Zeigt unser Git-Verlauf den Lernweg?** Grösstenteils ja. Ab K2
sind Branch-Namen, Pull Requests und Commit-Präfixe (`K2:`, `K3:`,
`K4 B1:`) einheitlich, und man sieht, in welchen Schritten wir
umgebaut haben. Bei K1 ist der Verlauf unübersichtlich: zwei
unterschiedlich benannte Branches, ein nie gemergter Branch und
Korrektur-Commits direkt auf `master`. Ausserdem stammen die meisten
Commits von einem Konto, und Review und Merge lagen meist bei derselben
Person. Der Verlauf zeigt also gut *was* wir gemacht haben, aber noch
nicht, dass wir *als Gruppe* zusammengearbeitet haben.

**Was würden wir rückblickend anders machen?**

-   Das Branch-Schema `feature/kN-thema` von Anfang an verwenden.
-   Nie direkt auf `master` committen, auch keine kleinen Korrekturen.
-   Jeden Pull Request von einer anderen Person reviewen und mergen
    lassen.
-   Nach jedem gemergten Kapitel sofort einen Tag setzen.
-   Die `.gitignore` und die Trennung der Konfiguration ganz am Anfang
    einrichten (K0), nicht erst in K5.

⚠️ *[Nach B3 und B4 ergänzen: was das Review und der bewusste Konflikt
gebracht haben.]*
