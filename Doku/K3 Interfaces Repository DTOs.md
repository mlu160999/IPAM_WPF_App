# K3 -- Interfaces, Repository-Pattern und DTOs

## Ziel

Über den bestehenden EF-Core-Code (K2) wurde eine zusätzliche, formale
Ebene eingeführt:

-   **Interfaces** als Kontrakt für den Datenzugriff (`IRepository<T>`)
-   **Repository-Klassen** statt DataAccess-Klassen
-   **Konstruktor-Injection**: Die Services kennen nur noch Interfaces
-   **DTOs** dort, wo die Entity nicht das ist, was die UI braucht

Die Funktionalität der App bleibt gleich.

**UI → Services → IRepository (Interface) → Repository (EF Core) → SQLite**

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
├── Migrations/                     (unverändert, keine neue Migration nötig)
├── Models/                         (unverändert)
│   ├── IPAdresse.cs
│   ├── IPStatus.cs
│   └── Subnet.cs
├── Services/
│   ├── ApplicationService.cs       (unverändert)
│   ├── IPAdresseService.cs         (geändert)
│   └── SubnetService.cs            (geändert)
├── App.xaml / App.xaml.cs          (geändert, Composition Root)
└── MainWindow.xaml / .xaml.cs      (Code-Behind geändert)
```

Die Datenbank ändert sich in K3 nicht. Deshalb ist **keine neue
Migration** nötig.

## B1 -- Interface definieren

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

Ein Interface beschreibt nur, **was** eine Klasse können muss
(Kontrakt), nicht **wie** sie es macht (Implementierung). `T` ist ein
Platzhalter für die Entity, z. B. `IRepository<Subnet>`.

### Reichen die fünf Methoden für alle Entitäten? (B1.2)

-   **Subnet:** Ja. Die App braucht nur Standard-CRUD.
-   **IPAdresse:** Nein. Die App zeigt IP-Adressen immer **pro Subnet**
    an und muss **Duplikate** pro Subnet erkennen. Dafür gibt es ein
    eigenes Interface, das die fünf Standard-Methoden erbt und zwei
    spezifische ergänzt:

``` csharp
public interface IIPAdresseRepository : IRepository<IPAdresse>
{
    List<IPAdresse> GetBySubnetId(int subnetId);
    bool ExistsInSubnet(string adresse, int subnetId);
}
```

So bleibt der Service an ein Interface gebunden (nicht an die konkrete
Klasse) und hat trotzdem die Methoden, die er braucht.

## B2 -- DataAccess zu Repository machen

### Repository-Klassen

| Vorher (K2) | Nachher (K3) | Implementiert |
|---|---|---|
| `SubnetDataAccess` | `SubnetRepository` | `IRepository<Subnet>` |
| `IPAdresseDataAccess` | `IPAdresseRepository` | `IIPAdresseRepository` |

Der EF-Core-Code (`AppDbContext`, LINQ, `SaveChanges`) ist praktisch
gleich geblieben. Geändert hat sich, **welche** Methoden es gibt:

| Vorher (DataAccess) | Nachher (Repository) | Grund |
|---|---|---|
| `SubnetDataAccess.Add(name, beschreibung)` | `SubnetRepository.Add(Subnet entity)` | Einheitliche Signatur aus `IRepository<T>` |
| `IPAdresseDataAccess.TryAdd(adresse, subnetId)` | `ExistsInSubnet(...)` + `Add(IPAdresse entity)` | Die Duplikat-Prüfung ist eine Geschäftsregel und gehört in den Service |
| `IPAdresseDataAccess.UpdateStatus(id, status)` | `GetById(id)` + `Update(entity)` | Der Status-Workflow gehört in den Service; das Repository macht nur CRUD |
| -- | `GetById(int id)` | Neu aus `IRepository<T>` |

**Grundsatz:** Das Repository enthält nur noch reinen Datenzugriff
(CRUD). Alle Regeln (Duplikate, Status-Workflow, Erstelldatum,
Eingabeprüfung) liegen im Service.

### Konstruktor-Injection in den Services

Vorher hat jeder Service seine DataAccess-Klasse selbst erstellt:

``` csharp
private readonly SubnetDataAccess _subnetDataAccess = new SubnetDataAccess();
```

Jetzt bekommt der Service das Repository **von aussen** und speichert
es als **Interface-Typ**:

``` csharp
private readonly IRepository<Subnet> _repository;

public SubnetService(IRepository<Subnet> repository)
{
    _repository = repository;
}
```

Damit kennt der `SubnetService` die Klasse `SubnetRepository` gar nicht
mehr. Für Tests (K4) kann später eine andere Klasse übergeben werden,
die dasselbe Interface implementiert, z. B. ein Repository mit
Testdaten ohne echte Datenbank.

### Composition Root in `App.xaml.cs`

Irgendwo müssen die konkreten Klassen trotzdem erstellt und
zusammengesteckt werden. Diese Stelle heisst **Composition Root** und
ist bei uns `App.xaml.cs`:

``` csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    var applicationService = new ApplicationService();
    applicationService.InitializeDatabase();

    var subnetService = new SubnetService(new SubnetRepository());
    var ipAdresseService = new IPAdresseService(new IPAdresseRepository());

    var mainWindow = new MainWindow(subnetService, ipAdresseService);
    mainWindow.Show();
}
```

-   `StartupUri="MainWindow.xaml"` wurde aus `App.xaml` entfernt, weil
    das Fenster jetzt hier mit seinen Services erstellt wird.
-   `MainWindow` bekommt die Services über den Konstruktor und kennt
    keine Repositories.
-   Nur diese eine Stelle kennt alle konkreten Klassen. Wer z. B. das
    Repository austauschen will, ändert nur hier eine Zeile.

## B3 -- DTOs einführen

### Welche Entitäten brauchen ein DTO? (B3.6)

| Entity | DTO? | Begründung |
|---|---|---|
| `IPAdresse` | **Ja**, `IPAdresseDto` | Hat einen Fremdschlüssel (`SubnetId`) und eine Navigation Property (`Subnet`), die die UI nicht braucht. Die IP-Liste zeigt nur Adresse und Status. |
| `Subnet` | **Nein** | Die UI zeigt genau die vier einfachen Felder (`Id`, `Name`, `Beschreibung`, `Erstellt`). Die Navigation Property `IPAdressen` wird nicht geladen und nicht verwendet. Ein DTO wäre hier nur Mapping-Aufwand ohne Nutzen. |

### `IPAdresseDto`

``` csharp
public class IPAdresseDto
{
    public int Id { get; set; }
    public string Adresse { get; set; } = string.Empty;
    public IPStatus Status { get; set; }
}
```

**Bewusst nicht übernommen:**

-   `SubnetId`: interner Fremdschlüssel, die UI kennt das ausgewählte
    Subnet bereits.
-   `Subnet`: Navigation Property. Sie wäre ohne `Include()` ohnehin
    leer und würde die UI an die Datenbankstruktur koppeln.

### Mapping Entity → DTO (im Service, manuell mit LINQ)

``` csharp
public List<IPAdresseDto> GetBySubnetId(int subnetId)
{
    return _repository.GetBySubnetId(subnetId)
        .Select(a => new IPAdresseDto
        {
            Id = a.Id,
            Adresse = a.Adresse,
            Status = a.Status
        })
        .ToList();
}
```

Kein AutoMapper, sondern manuelles Mapping mit `Select`, wie in der
Aufgabe verlangt.

Die Gegenrichtung (Eingabe → Entity) passiert ebenfalls im Service,
z. B. beim Hinzufügen einer IP:
`new IPAdresse { Adresse = ipText, Status = IPStatus.Frei, SubnetId = subnetId }`.

### Anpassung UI (B3.8)

-   `MainWindow.xaml.cs` arbeitet bei den IP-Adressen mit
    `IPAdresseDto` statt `IPAdresse`.
-   `AdvanceStatus` bekommt nur noch die Id (`AdvanceStatus(int id)`)
    statt der ganzen Entity. Der Service lädt die Entity selbst über
    `GetById`.
-   `MainWindow.xaml` musste nicht angepasst werden, weil das DTO
    dieselben Property-Namen (`Adresse`, `Status`) hat wie die Entity.

## Übersicht: Serviceklasse → Repository

| Serviceklasse | Service-Methode | Aufruf im Repository |
|---|---|---|
| ApplicationService | InitializeDatabase() | DbInitializer.Initialize() |
| SubnetService | GetAll() | IRepository<Subnet>.GetAll() |
| SubnetService | Add(name, beschreibung) | IRepository<Subnet>.Add(subnet) |
| SubnetService | Update(subnet) | IRepository<Subnet>.GetById(id) + Update(subnet) |
| SubnetService | Delete(subnetId) | IRepository<Subnet>.Delete(id) |
| IPAdresseService | GetBySubnetId(subnetId) | IIPAdresseRepository.GetBySubnetId(subnetId) + Mapping zu DTO |
| IPAdresseService | Add(ipText, subnetId) | IIPAdresseRepository.ExistsInSubnet(...) + Add(ipAdresse) |
| IPAdresseService | AdvanceStatus(ipAdresseId) | IIPAdresseRepository.GetById(id) + Update(ipAdresse) |
| IPAdresseService | Delete(ipAdresseId) | IIPAdresseRepository.Delete(id) |

Keine Serviceklasse übernimmt Datenzugriff, und kein Repository enthält
Geschäftslogik.

## B4 -- Kontrolle

-   App getestet: Subnet hinzufügen/löschen, IP hinzufügen, Duplikat
    wird erkannt, Status-Workflow Frei → Reserviert → Zugewiesen →
    Frei, IP löschen, Cascade-Delete, Daten bleiben nach Neustart
    erhalten. Die App verhält sich gleich wie vor dem Umbau.
-   **Kontrollfrage:** Keine Serviceklasse verwendet den konkreten Typ
    einer Repository-Klasse. `SubnetService` speichert
    `IRepository<Subnet>`, `IPAdresseService` speichert
    `IIPAdresseRepository`. Konkrete Repository-Klassen werden nur in
    `App.xaml.cs` (Composition Root) erstellt.
-   `MainWindow.xaml.cs` kennt keine Repositories, nur Services.

## Git / GitHub

Branch: **`feature/k3-repository-dto`**

Commits in Schritten (von unten nach oben durch die Schichten, damit
der Build nach jedem Commit funktioniert):

1.  `K3: Generisches Interface IRepository<T> mit CRUD-Methoden definiert`
2.  `Rename Irepository.cs to IRepository.cs`
3.  `K3: IIPAdresseRepository um GetBySubnetId und ExistsInSubnet erweitert`
4.  `K3: SubnetRepository implementiert IRepository<Subnet>`
5.  `K3: IPAdresseRepository implementiert IIPAdresseRepository`
6.  `K3: IPAdresseDto ohne Fremdschlüssel und Navigation Property eingeführt`
7.  `K3: Services mit Konstruktor-Injection, Composition Root und DTO im UI`
    (Services, App.xaml, App.xaml.cs und MainWindow.xaml.cs zusammen, weil
    die Umstellung auf Konstruktor-Injection nur gemeinsam lauffähig ist)
8.  `K3: Alte DataAccess-Klassen entfernt`
9.  `Merge origin/master (K2) in feature/k3-repository-dto`
10. `K3: Ordner DTOS umbenennen (Zwischenschritt)`
11. `K3: Ordner Dtos an Namespace angepasst`

Gemergt über Pull Request #4 `feature/k3-repository-dto` → `master`.

## Stolpersteine

-   **Branch von veraltetem `master` erstellt:** Der K3-Branch wurde
    von einem lokalen `master` ohne die K2-Änderungen abgezweigt. Dadurch
    fehlten `AppDbContext`, die Migrations und die Doku-Ordner-Struktur.
    Lösung: aktuellen `master` in den Branch mergen und die zwei
    Konflikte (alte DataAccess-Klassen) durch Löschen auflösen.
    Erkenntnis: vor dem Erstellen eines Branches immer zuerst `master`
    pullen.
-   **Gross-/Kleinschreibung von Datei- und Ordnernamen unter Windows:**
    `Irepository.cs` musste in `IRepository.cs` und der Ordner `DTOS`
    in `Dtos` umbenannt werden (passend zum Namespace
    `IPAM_WPF_App.Dtos`). Weil Git unter Windows reine Änderungen der
    Gross-/Kleinschreibung nicht erkennt, lief das über einen
    Zwischennamen (`DtosTmp`) mit einem Commit dazwischen.

## Reflexion: Wo hat sich ein DTO gelohnt, wo nicht?

-   **Gelohnt bei `IPAdresse`:** Die UI bekommt nur noch Id, Adresse und
    Status. Fremdschlüssel und Navigation Property bleiben in der
    DataAccess-Schicht. Ändert sich die Entity (z. B. neue Spalte),
    muss sich die UI nicht ändern.
-   **Nicht gelohnt bei `Subnet`:** Die Entity enthält genau die Felder,
    die die UI anzeigt. Ein DTO hätte nur zusätzlichen Mapping-Code
    erzeugt.

## Ergebnis

Die App funktioniert unverändert. Die Services arbeiten nur noch gegen
Interfaces und sind damit für Unit Tests in K4 vorbereitet. Die UI
erhält bei den IP-Adressen nur die Daten, die sie wirklich braucht.
