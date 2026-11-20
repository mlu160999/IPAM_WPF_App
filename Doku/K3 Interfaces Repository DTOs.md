# MODUL PROG1 – K3: Interfaces, Repository-Pattern und DTOs

**Projekt:** IPAM_WPF_App\
**Autoren:** Madalina Luca, Muzamil Abukar Haji Ali, Sabri Mahdi\
**Branch:** `feature/k3-repository-dto` → `master` (Pull Request #4)

## Inhaltsverzeichnis

1.  [Ziel](#ziel)
2.  [Drei neue Begriffe](#drei-neue-begriffe)
3.  [Projektstruktur nach K3](#projektstruktur-nach-k3)
4.  [Welche Klasse macht was?](#welche-klasse-macht-was)
5.  [B1 – Interfaces](#b1--interfaces)
6.  [B2 – Repositories und Konstruktor-Injection](#b2--repositories-und-konstruktor-injection)
7.  [B3 – DTOs](#b3--dtos)
8.  [Ablauf: Klick auf «Status weiter»](#ablauf-klick-auf-status-weiter)
9.  [Übersicht: Serviceklasse → Repository](#übersicht-serviceklasse--repository)
10. [B4 – Kontrolle](#b4--kontrolle)
11. [Git](#git)
12. [Stolpersteine](#stolpersteine)
13. [KI-Protokoll K3](#ki-protokoll-k3)
14. [Reflexion](#reflexion)

## Ziel

Bis K2 erstellt jeder Service seine DataAccess-Klasse selbst mit `new`.
Damit ist er fest an EF Core und die echte Datenbank gebunden. In K3
wird diese feste Bindung gelöst:

-   Die Services kennen nur noch ein **Interface**, nicht die konkrete
    Klasse.
-   Die DataAccess-Klassen werden zu **Repositories**.
-   Die UI bekommt bei den IP-Adressen nur noch die Daten, die sie
    wirklich braucht (**DTO**).

Für den Benutzer ändert sich nichts. Die Datenbank ändert sich auch
nicht, es braucht **keine neue Migration**.

## Drei neue Begriffe

| Begriff | Einfach erklärt |
|---|---|
| **Interface** | Ein Vertrag: «Wer mich implementiert, hat diese Methoden.» Es sagt nur **was**, nicht **wie**. |
| **Repository** | Eine Klasse, die nur Daten liest und speichert (CRUD). Keine Regeln, keine Logik. |
| **Dependency Injection (DI)** | Eine Klasse erstellt ihre Hilfsklassen nicht selbst, sondern bekommt sie **von aussen** über den Konstruktor. |
| **DTO** (Data Transfer Object) | Eine schlanke Klasse nur für die Anzeige, mit genau den Feldern, die die UI braucht. |

## Projektstruktur nach K3

``` text
IPAM_WPF_App/
├── DataAccess/
│   ├── AppDbContext.cs
│   ├── DbInitializer.cs
│   ├── IRepository.cs              (neu)
│   ├── IIPAdresseRepository.cs     (neu)
│   ├── SubnetRepository.cs         (neu, ersetzt SubnetDataAccess.cs)
│   └── IPAdresseRepository.cs      (neu, ersetzt IPAdresseDataAccess.cs)
├── Dtos/
│   └── IPAdresseDto.cs             (neu)
├── Services/
│   ├── ApplicationService.cs
│   ├── IPAdresseService.cs         (geändert)
│   └── SubnetService.cs            (geändert)
├── Models/                         (unverändert)
├── Migrations/                     (unverändert)
├── App.xaml / App.xaml.cs          (geändert)
└── MainWindow.xaml.cs              (geändert)
```

## Welche Klasse macht was?

| Klasse | Schicht | Aufgabe |
|---|---|---|
| `App.xaml.cs` | Start | **Composition Root**: Die einzige Stelle, die alle konkreten Klassen kennt. Erstellt Repositories, Services und das Hauptfenster und steckt sie zusammen. |
| `MainWindow.xaml.cs` | UI | Bekommt die Services über den Konstruktor. Arbeitet bei IP-Adressen mit `IPAdresseDto`. |
| `SubnetService` | Service | Regeln für Subnetze. Setzt jetzt auch das Erstelldatum. Kennt nur `IRepository<Subnet>`. |
| `IPAdresseService` | Service | Regeln für IP-Adressen: Validierung, Duplikatprüfung, Status-Workflow. Wandelt Entities in DTOs um. Kennt nur `IIPAdresseRepository`. |
| `IRepository<T>` | DataAccess | Vertrag für die fünf Standard-Operationen einer Entity |
| `IIPAdresseRepository` | DataAccess | Erweitert den Vertrag um zwei Abfragen speziell für IP-Adressen |
| `SubnetRepository` | DataAccess | Erfüllt `IRepository<Subnet>` mit EF Core |
| `IPAdresseRepository` | DataAccess | Erfüllt `IIPAdresseRepository` mit EF Core |
| `IPAdresseDto` | Dtos | Nur `Id`, `Adresse`, `Status` für die Anzeige |

## B1 – Interfaces

### `IRepository<T>`

``` csharp
public interface IRepository<T>
{
    List<T> GetAll();
    T? GetById(int id);
    void Add(T entity);
    void Update(T entity);
    void Delete(int id);
}
```

`T` ist ein Platzhalter. `IRepository<Subnet>` bedeutet: dieselben fünf
Methoden, aber für Subnetze.

### Reichen die fünf Methoden für alle Entitäten? (B1.2)

-   **Subnet: Ja.** Die App braucht nur Standard-CRUD.
-   **IPAdresse: Nein.** Die App zeigt IP-Adressen immer **pro Subnet**
    an und muss **Duplikate** erkennen. Deshalb gibt es ein eigenes
    Interface, das die fünf Methoden erbt und zwei ergänzt:

``` csharp
public interface IIPAdresseRepository : IRepository<IPAdresse>
{
    List<IPAdresse> GetBySubnetId(int subnetId);
    bool ExistsInSubnet(string adresse, int subnetId);
}
```

## B2 – Repositories und Konstruktor-Injection

### DataAccess → Repository

| Vorher (K2) | Nachher (K3) | Implementiert |
|---|---|---|
| `SubnetDataAccess` | `SubnetRepository` | `IRepository<Subnet>` |
| `IPAdresseDataAccess` | `IPAdresseRepository` | `IIPAdresseRepository` |

Der EF-Core-Code ist praktisch gleich geblieben. Geändert hat sich,
**welche Methoden** es gibt. Alle Regeln sind aus den Repositories in
die Services gewandert:

| Vorher | Nachher | Warum |
|---|---|---|
| `SubnetDataAccess.Add(name, beschreibung)` setzt das Datum selbst | `SubnetRepository.Add(subnet)`. Das Datum setzt der `SubnetService`. | Einheitliche Methode aus dem Interface. Das Erstelldatum ist eine Regel und gehört in den Service. |
| `IPAdresseDataAccess.TryAdd(...)` prüft Duplikate | Service ruft `ExistsInSubnet(...)` und dann `Add(...)` auf | «Keine doppelte IP pro Subnet» ist eine Geschäftsregel |
| `IPAdresseDataAccess.UpdateStatus(id, status)` | Service ruft `GetById(id)` auf, ändert den Status und ruft `Update(...)` auf | Der Status-Workflow gehört in den Service. Das Repository macht nur CRUD. |

### Konstruktor-Injection

``` csharp
// Vorher (K2): Service erstellt die Klasse selbst
private readonly SubnetDataAccess _subnetDataAccess = new SubnetDataAccess();

// Nachher (K3): Service bekommt ein Interface von aussen
private readonly IRepository<Subnet> _repository;

public SubnetService(IRepository<Subnet> repository)
{
    _repository = repository;
}
```

**Warum?** Der `SubnetService` weiss nicht mehr, ob dahinter EF Core,
eine andere Datenbank oder eine Testklasse steckt. In K4 nutzen wir
das: Die Unit Tests übergeben ein Fake-Repository ohne Datenbank.

### Composition Root (`App.xaml.cs`)

Irgendwo müssen die konkreten Klassen doch erstellt werden. Diese
Stelle heisst **Composition Root**:

``` csharp
var subnetService = new SubnetService(new SubnetRepository());
var ipAdresseService = new IPAdresseService(new IPAdresseRepository());

var mainWindow = new MainWindow(subnetService, ipAdresseService);
mainWindow.Show();
```

-   `StartupUri="MainWindow.xaml"` wurde aus `App.xaml` entfernt, weil
    das Fenster jetzt hier mit seinen Services erstellt wird.
-   Will man ein Repository austauschen, ändert man nur diese Zeilen.

## B3 – DTOs

### Welche Entitäten brauchen ein DTO? (B3.6)

| Entity | DTO? | Begründung |
|---|---|---|
| `IPAdresse` | **Ja** | Enthält `SubnetId` (Fremdschlüssel) und `Subnet` (Navigation Property). Beides braucht die UI nicht. |
| `Subnet` | **Nein** | Die UI zeigt genau die vier einfachen Felder. Ein DTO wäre nur zusätzlicher Code ohne Nutzen. |

### `IPAdresseDto` und das Mapping

``` csharp
public class IPAdresseDto
{
    public int Id { get; set; }
    public string Adresse { get; set; } = string.Empty;
    public IPStatus Status { get; set; }
}
```

Die Umwandlung (**Mapping**) passiert im `IPAdresseService`, von Hand
mit LINQ `Select` (kein AutoMapper, wie in der Aufgabe verlangt):

``` csharp
return _repository.GetBySubnetId(subnetId)
    .Select(a => new IPAdresseDto { Id = a.Id, Adresse = a.Adresse, Status = a.Status })
    .ToList();
```

### Anpassungen in der UI

-   `MainWindow.xaml.cs` arbeitet bei IP-Adressen mit `IPAdresseDto`.
-   `AdvanceStatus` bekommt nur noch die Id. Der Service lädt die Entity
    selbst.
-   `MainWindow.xaml` musste nicht geändert werden, weil das DTO
    dieselben Property-Namen hat.

## Ablauf: Klick auf «Status weiter»

1.  **UI:** ruft `_ipAdresseService.AdvanceStatus(ipAdresseDto.Id)` auf.
2.  **Service:** `_repository.GetById(id)` (über das Interface),
    berechnet den nächsten Status, dann `_repository.Update(ipAdresse)`.
3.  **Repository:** `context.IPAdressen.Update(...)`, `SaveChanges()`.
4.  **UI:** lädt die Liste neu und erhält wieder DTOs.

## Übersicht: Serviceklasse → Repository

| Serviceklasse | Service-Methode | Aufruf im Repository |
|---|---|---|
| ApplicationService | InitializeDatabase() | DbInitializer.Initialize() |
| SubnetService | GetAll() | IRepository<Subnet>.GetAll() |
| SubnetService | Add(name, beschreibung) | IRepository<Subnet>.Add(subnet) |
| SubnetService | Update(subnet) | GetById(id) + Update(subnet) |
| SubnetService | Delete(subnetId) | IRepository<Subnet>.Delete(id) |
| IPAdresseService | GetBySubnetId(subnetId) | IIPAdresseRepository.GetBySubnetId(...) + Mapping zu DTO |
| IPAdresseService | Add(ipText, subnetId) | ExistsInSubnet(...) + Add(ipAdresse) |
| IPAdresseService | AdvanceStatus(id) | GetById(id) + Update(ipAdresse) |
| IPAdresseService | Delete(id) | IIPAdresseRepository.Delete(id) |

## B4 – Kontrolle

-   App getestet: Sie verhält sich gleich wie vor dem Umbau.
-   **Kontrollfrage:** Keine Serviceklasse verwendet eine konkrete
    Repository-Klasse. `SubnetService` speichert `IRepository<Subnet>`,
    `IPAdresseService` speichert `IIPAdresseRepository`. Konkrete
    Repositories werden nur in `App.xaml.cs` erstellt.
-   `MainWindow.xaml.cs` kennt keine Repositories, nur Services.

## Git

Branch `feature/k3-repository-dto`, gemergt über **Pull Request #4**.
Die Commits gehen von unten nach oben durch die Schichten, damit der
Build nach jedem Commit funktioniert:

1.  `K3: Generisches Interface IRepository<T> mit CRUD-Methoden definiert`
2.  `Rename Irepository.cs to IRepository.cs`
3.  `K3: IIPAdresseRepository um GetBySubnetId und ExistsInSubnet erweitert`
4.  `K3: SubnetRepository implementiert IRepository<Subnet>`
5.  `K3: IPAdresseRepository implementiert IIPAdresseRepository`
6.  `K3: IPAdresseDto ohne Fremdschlüssel und Navigation Property eingeführt`
7.  `K3: Services mit Konstruktor-Injection, Composition Root und DTO im UI`
8.  `K3: Alte DataAccess-Klassen entfernt`
9.  `Merge origin/master (K2) in feature/k3-repository-dto`
10. `K3: Ordner DTOS umbenennen (Zwischenschritt)`
11. `K3: Ordner Dtos an Namespace angepasst`

Commit 7 enthält fünf Dateien (beide Services, `App.xaml`,
`App.xaml.cs`, `MainWindow.xaml.cs`). Die Umstellung auf
Konstruktor-Injection funktioniert nur gemeinsam. Einzeln wären
Zwischenstände entstanden, die nicht bauen oder beim Start abstürzen.

## Stolpersteine

| Problem | Ursache | Lösung |
|---|---|---|
| Build-Fehler «AppDbContext could not be found» | Der K3-Branch wurde von einem **lokalen, veralteten `master`** ohne K2 erstellt | `origin/master` in den Branch gemergt. Die zwei Konflikte (alte DataAccess-Klassen) mit «Delete File» gelöst. **Regel für uns:** vor jedem neuen Branch zuerst `master` pullen. |
| Erster Merge-Versuch ohne Wirkung | Den lokalen statt den GitHub-`master` gemergt | Zuerst `master` gepullt, dann gemergt |
| Ordner `DTOS` passte nicht zum Namespace `Dtos` | Schreibweise | Über den Zwischennamen `DtosTmp` umbenannt (Windows/Git erkennen reine Gross-/Kleinschreibung nicht) |

------------------------------------------------------------------------

## KI-Protokoll K3

**Tool:** Claude (Anthropic)

### Eintrag 1 – Interfaces, Repositories und DTO erstellen

**Prompt / Aufgabe:**\
«Super, ab auf K3.» (Grundlage: Aufgabenblatt K3 und `master` nach K2.)

**Ergebnis der KI:**\
Claude lieferte `IRepository<T>`, `IIPAdresseRepository`, beide
Repositories, `IPAdresseDto`, die Services mit Konstruktor-Injection,
`App.xaml.cs` als Composition Root und das angepasste Code-Behind.
Dazu die Begründungen: warum ein zweites Interface für IP-Adressen,
warum kein DTO für `Subnet`, warum die Regeln aus den Repositories in
die Services wandern. Den Code selbst konnte Claude nicht kompilieren.

**Unsere Prüfung und Umsetzung:**\
Wir haben den Branch erstellt, die Dateien eingebaut und in Schritten
committet. Die Antworten auf B1.2 und B3.6 haben wir mit dem
Aufgabenblatt abgeglichen.

**Erkenntnis:**\
Nicht jede Entity braucht ein DTO. Die Entscheidung muss man pro
Entity begründen können.

### Eintrag 2 – Commit-Reihenfolge

**Prompt / Aufgabe:**\
«Kannst du mir von File zu File einen Commit-Text geben? Und in welcher
Reihenfolge macht man das am besten?» Danach: «Kombinier den Commit für
6, 7, 9, damit wieder ein gültiger Build hochgeladen wird» und «Macht
man wirklich so grosse Commits?»

**Ergebnis der KI:**\
Claude schlug die Reihenfolge «von unten nach oben durch die Schichten»
vor und markierte, nach welchen Commits der Build bricht. Bei unserem
Kombinationsvorschlag wies Claude darauf hin, dass auch `App.xaml` und
`App.xaml.cs` dazugehören. Sonst würde die App zwar bauen, aber beim
Start abstürzen.

**Unsere Prüfung und Umsetzung:**\
Die Idee, die Commits zusammenzufassen, damit jeder Push lauffähig
ist, kam von uns. Wir haben sie mit der Ergänzung um `App.xaml`
umgesetzt.

**Erkenntnis:**\
Ein guter Commit ist eine abgeschlossene, lauffähige Änderung, egal wie
viele Dateien er umfasst.

### Eintrag 3 – Build-Fehler durch veralteten Branch

**Prompt / Aufgabe:**\
Wir haben Claude den Build-Fehler «The type or namespace name
'AppDbContext' could not be found» geschickt und gefragt, was er
bedeutet.

**Ergebnis der KI:**\
Claude prüfte den Branch auf GitHub. Er war von einem `master` ohne K2
abgezweigt worden. Claude simulierte den Merge vorher und konnte so
voraussagen, dass genau zwei Konflikte entstehen und diese durch
Löschen zu lösen sind.

**Unsere Prüfung und Umsetzung:**\
Unser erster Merge hatte keine Wirkung, weil wir den lokalen `master`
gemergt hatten. Das haben wir selbst bemerkt («ah, ich habe es falsch
gemacht»). Danach haben wir den aktuellen `master` gemergt, die zwei
Konflikte über «Delete File» gelöst, gebaut, getestet und gepusht.

**Erkenntnis:**\
Vor jedem neuen Branch zuerst `master` pullen. Und: «Lokal» und
«auf GitHub» sind nicht automatisch derselbe Stand.

### Fragen aus dem Aufgabenblatt

**Welche Prompts habt ihr verwendet, um Interfaces und
Repository-Pattern einzuführen?**\
Wir haben Claude mit «Super, ab auf K3» auf Basis des Aufgabenblatts
und unseres `master` beauftragt. Für die Umsetzung haben wir gefragt:
«Kannst du mir von File zu File einen Commit-Text geben, und in welcher
Reihenfolge macht man das am besten?» Beim Build-Fehler haben wir die
Fehlermeldung geschickt und gefragt, was sie bedeutet.

**Was hat die KI beim Mapping Entity/DTO gut gemacht, wo musstet ihr
korrigieren?**\
Gut: Das Mapping steht an einer Stelle im `IPAdresseService`, ist mit
`Select` kurz und verständlich, und das DTO hat dieselben
Property-Namen wie die Entity. Deshalb musste `MainWindow.xaml` nicht
angepasst werden. Claude hat auch begründet, warum `Subnet` kein DTO
braucht. Am Mapping selbst mussten wir nichts korrigieren. Die Probleme
in K3 lagen bei Git (veralteter Branch) und bei der Schreibweise des
Ordners (`DTOS` statt `Dtos`).

**Wo wollte die KI zu schnell AutoMapper oder ähnliche Bibliotheken
einführen, und wie habt ihr sie zurückgeholt?**\
Das ist nicht passiert. Claude hat das Mapping von Anfang an von Hand
mit LINQ `Select` geschrieben und dabei auf die Vorgabe im
Aufgabenblatt verwiesen (kein AutoMapper).

------------------------------------------------------------------------

## Reflexion

**Bei welcher Entity hat sich ein DTO klar gelohnt, bei welcher nicht?
Warum?**

-   **Gelohnt bei `IPAdresse`:** Die Entity enthält den Fremdschlüssel
    `SubnetId` und die Navigation Property `Subnet`. Beides braucht die
    IP-Liste nicht. Mit dem `IPAdresseDto` bekommt die UI nur `Id`,
    `Adresse` und `Status`. Ändert sich die Entity (z. B. eine neue
    Spalte), muss sich die UI nicht ändern.
-   **Nicht gelohnt bei `Subnet`:** Die Entity enthält genau die vier
    Felder, die die UI anzeigt. Die Navigation Property `IPAdressen`
    wird nie geladen. Ein DTO hätte nur zusätzlichen Mapping-Code ohne
    Nutzen erzeugt.

**Was hat K3 sonst gebracht?**\
Am meisten die Konstruktor-Injection. Für den Benutzer hat sich nichts
verändert, und zuerst wirkte es wie mehr Code für dasselbe Ergebnis.
Der Nutzen wurde erst in K4 sichtbar: Weil die Services nur
Interfaces kennen, konnten wir sie mit Fake-Repositories ohne
Datenbank testen.

**Was wir gelernt haben:**\
Vor jedem neuen Branch zuerst `master` pullen. Der Build-Fehler mit
`AppDbContext` hat uns gezeigt, dass «lokal» und «auf GitHub» nicht
automatisch derselbe Stand sind.
