# Syncfusion Component Onboarding Generator

## Context

Alis.Reactive will onboard 100+ Syncfusion component vertical slices. Hand-writing each
slice (8 files per component) is error-prone, violates DRY, and breaks when SF updates.
This plan creates an **evidence-based code generation pipeline**: a real ASP.NET app
mounts real SF components, introspects their JS API via browser, stores the API in SQLite,
and a generator emits exact vertical slice files from that database.

**Two new projects** in the same solution under `tools/`:
1. `SyncfusionOnboarding` — ASP.NET 10 MVC app (discovery + CRUD editing)
2. `SyncfusionGenerator` — Console app (reads SQLite → generates C# files)

**POC:** FusionDropDownList — full 100% API onboarding.

---

## Architecture

```
┌─────────────────────────────────┐
│   SyncfusionOnboarding (MVC)    │
│                                 │
│  /DropDownList                  │
│  ┌───────────────────────────┐  │
│  │ Real SF component mounted │  │
│  └───────────────────────────┘  │
│  [Discover API] button          │
│       │                         │
│       ▼                         │
│  JS introspection on            │
│  el.ej2_instances[0]            │
│       │                         │
│       ▼                         │
│  POST /api/discover             │
│       │                         │
│       ▼                         │
│  ┌───────────────────────────┐  │
│  │    SQLite (EF Core)       │  │
│  │  Components               │  │
│  │  Properties               │  │
│  │  Methods + MethodArgs     │  │
│  │  Events + EventArgProps   │  │
│  └───────────────────────────┘  │
│       ▲                         │
│  CRUD editing UI (tables,       │
│  checkboxes, type dropdowns)    │
└─────────────────────────────────┘
              │
              │ Same .db file
              ▼
┌─────────────────────────────────┐
│  SyncfusionGenerator (Console)  │
│                                 │
│  Reads SQLite → generates:      │
│  ├── FusionDropDownList.cs      │
│  ├── Extensions.cs              │
│  ├── ReactiveExtensions.cs      │
│  ├── Events.cs                  │
│  ├── Events/OnChanged.cs        │
│  └── Tests/                     │
│                                 │
│  Output: Alis.Reactive.Fusion/  │
│          Components/{Name}/     │
└─────────────────────────────────┘
```

---

## App 1: SyncfusionOnboarding

### Project Setup

- ASP.NET 10 MVC, no Alis.Reactive dependency
- NuGet: `Syncfusion.EJ2.AspNet.Core` v32.2.8, `Microsoft.EntityFrameworkCore.Sqlite`
- Each component gets its own Controller + View
- SF license key configured in `Program.cs`

### Per-Component View (e.g., `/DropDownList/Index.cshtml`)

Mounts a real SF component with minimal config:

```html
<div id="component-mount"></div>

<script>
  // Mount real SF DropDownList with sample data
  var ddl = new ej.dropdowns.DropDownList({
    dataSource: ['Item1', 'Item2', 'Item3'],
    value: 'Item1'
  }, '#component-mount');
</script>
```

Below the component: editable tables for Properties, Methods, Events.

### JS API Discovery Script (`wwwroot/js/api-discoverer.js`)

Runs on button click. Introspects the mounted component:

```javascript
function discover(elementId) {
  const el = document.getElementById(elementId);
  const instance = el.ej2_instances[0];
  const proto = Object.getPrototypeOf(instance);

  // 1. Methods — prototype functions, exclude internal/base
  const methods = Object.getOwnPropertyNames(proto)
    .filter(name =>
      typeof instance[name] === 'function' &&
      !name.startsWith('_') &&
      name !== 'constructor' &&
      !BASE_COMPONENT_METHODS.includes(name));

  // 2. Properties — own enumerable, exclude internal
  const props = Object.keys(instance)
    .filter(name =>
      !name.startsWith('_') &&
      typeof instance[name] !== 'function');

  // 3. Events — EJ2 components expose event list
  //    Try instance.constructor.prototype events or known patterns
  const events = detectEvents(instance);

  // 4. Classify and return
  return { methods, properties: props, events };
}
```

**Event detection heuristics:**
- EJ2 Component base has internal event registration
- Check if property name matches known event patterns (`change`, `open`, `close`, `focus`, `blur`, `select`, `created`, `destroyed`, `beforeOpen`, `actionBegin`, `actionComplete`, etc.)
- Check if property value is `null` or a function (events start as null, become callbacks)
- EJ2 components may expose `this.events` array — check and use if available

### Discovery Endpoint

```
POST /api/{component}/discover
Body: { methods: [...], properties: [...], events: [...] }
```

Controller receives the raw discovered API, saves to SQLite:
- Creates/updates `Component` record with current SF version
- Creates `Property` records for each discovered property
- Creates `Method` records for each discovered method
- Creates `Event` records for each discovered event
- Sets `Onboard = false` by default — user opts in

### Editing UI

Each component page shows 3 editable tables below the mounted component:

**Properties table:**
| Onboard | JS Name | C# Type | Coerce | Readable | Writable | Notes |
|---------|---------|---------|--------|----------|----------|-------|
| ✓ checkbox | text | dropdown | dropdown | checkbox | checkbox | text |

**Methods table:**
| Onboard | JS Name | Args (sub-table) | Notes |
|---------|---------|-------------------|-------|

**Events table:**
| Onboard | JS Event | Friendly Name | Arg Properties (sub-table) | Notes |
|---------|----------|---------------|---------------------------|-------|

Changes save via AJAX to `/api/{component}/save`.

### C# Type Dropdown Options

Standard types used in the framework:
`string`, `string?`, `int`, `int?`, `decimal`, `decimal?`, `bool`, `bool?`,
`DateTime`, `DateTime?`, `object`, `object[]`, `string[]`

### Coerce Type Dropdown Options

From existing `CoercionType`: `string`, `number`, `boolean`, `date`, `raw`, (none)

---

## SQLite Schema (EF Core)

```csharp
public class Component
{
    public int Id { get; set; }
    public string Name { get; set; }            // "DropDownList"
    public string SfNamespace { get; set; }     // "Syncfusion.EJ2.DropDowns"
    public string? ReadExpr { get; set; }       // "value"
    public string SfVersion { get; set; }       // "32.2.8"
    public DateTime DiscoveredAt { get; set; }
    public string? Notes { get; set; }

    public List<ComponentProperty> Properties { get; set; }
    public List<ComponentMethod> Methods { get; set; }
    public List<ComponentEvent> Events { get; set; }
}

public class ComponentProperty
{
    public int Id { get; set; }
    public int ComponentId { get; set; }
    public string JsName { get; set; }          // "value"
    public string CSharpType { get; set; }      // "string?"
    public string? CoerceType { get; set; }     // "number" or null
    public bool IsReadable { get; set; } = true;
    public bool IsWritable { get; set; } = true;
    public bool Onboard { get; set; }
    public string? Notes { get; set; }
}

public class ComponentMethod
{
    public int Id { get; set; }
    public int ComponentId { get; set; }
    public string JsName { get; set; }          // "focusIn"
    public bool Onboard { get; set; }
    public string? Notes { get; set; }

    public List<ComponentMethodArg> Args { get; set; }
}

public class ComponentMethodArg
{
    public int Id { get; set; }
    public int MethodId { get; set; }
    public int Position { get; set; }
    public string Name { get; set; }            // "items"
    public string CSharpType { get; set; }      // "object[]"
    public string? CoerceType { get; set; }
}

public class ComponentEvent
{
    public int Id { get; set; }
    public int ComponentId { get; set; }
    public string JsEventName { get; set; }     // "change"
    public string FriendlyName { get; set; }    // "Changed"
    public bool Onboard { get; set; }
    public string? Notes { get; set; }

    public List<EventArgProperty> ArgProperties { get; set; }
}

public class EventArgProperty
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; }            // "Value"
    public string CSharpType { get; set; }      // "string?"
    public string? JsName { get; set; }         // "value"
}
```

---

## App 2: SyncfusionGenerator (Console)

### Usage

```bash
# Generate one component
dotnet run --project tools/SyncfusionGenerator -- \
  --component DropDownList \
  --output Alis.Reactive.Fusion/Components/FusionDropDownList

# Generate all onboarded components
dotnet run --project tools/SyncfusionGenerator -- --all
```

### What It Generates (per component)

For a component named `DropDownList` with ReadExpr `"value"`:

**1. `FusionDropDownList.cs`** — Sealed phantom type
```csharp
// AUTO-GENERATED from SyncfusionOnboarding DB — do not edit manually.
// SF version: 32.2.8 | Generated: 2026-03-14
public sealed class FusionDropDownList : FusionComponent, IFusionInputComponent, IInputComponent
{
    public string ReadExpr => "value";
}
```

**2. `FusionDropDownListExtensions.cs`** — For each onboarded property/method:
- Per writable property: `Set{PascalName}()` with 4 overloads (static, event, response, component)
- Per readable property: `{PascalName}()` returning `TypedComponentSource<T>`
- Per void method: method returning `CallMutation`
- Per method with args: method with `MethodArg[]`

**3. `FusionDropDownListReactiveExtensions.cs`** — `.Reactive()` extension

**4. `FusionDropDownListEvents.cs`** — Singleton with `TypedEventDescriptor` per event

**5. `Events/FusionDropDownListOn{Event}.cs`** — One file per event with arg class

**6. C# Unit Tests** — Snapshot + schema validation tests per mutation method

### Template Engine

Simple string interpolation using `StringBuilder` — no T4 complexity needed.
The templates follow the exact patterns from the existing hand-written vertical slices
(FusionDropDownList, FusionNumericTextBox as reference implementations).

### Version Tracking

The generator stamps each file with:
```csharp
// AUTO-GENERATED from SyncfusionOnboarding DB — do not edit manually.
// SF version: 32.2.8 | Generated: 2026-03-14
```

When SF version changes:
1. Update NuGet in SyncfusionOnboarding
2. Re-discover (button click per component)
3. SQLite now has new version data
4. Re-generate → diff shows API changes
5. Curate if needed → regenerate

---

## POC Scope: DropDownList

### What Gets Discovered

Based on the real SF DropDownList JS API:

**Properties (writable):** `value`, `text`, `index`, `dataSource`, `enabled`,
`readonly`, `placeholder`, `cssClass`, `width`, `popupHeight`, `popupWidth`, ...

**Properties (readable):** `value`, `text`, `index`, `itemData`, ...

**Methods:** `focusIn()`, `focusOut()`, `showPopup()`, `hidePopup()`,
`getItems()`, `addItem()`, `filter()`, ...

**Events:** `change`, `focus`, `blur`, `open`, `close`, `select`,
`actionBegin`, `actionComplete`, `beforeOpen`, `filtering`, `created`, ...

### What Gets Onboarded (user chooses)

For the POC, onboard the full public API — every property, method, and event
that the real component exposes. The user marks `Onboard = true` for all items.

### Verification

1. Generated files match the exact pattern of existing hand-written FusionDropDownList
2. Generated code compiles (generator project references Alis.Reactive.Fusion)
3. Generated tests pass (snapshot + schema)
4. Diff between generated and hand-written shows ONLY:
   - Additional API surface (more properties/methods/events)
   - AUTO-GENERATED header comment
5. Existing hand-written FusionDropDownList can be REPLACED by generated version

---

## Project Structure

```
Alis.Reactive.sln
├── Alis.Reactive/                           (core — UNCHANGED)
├── Alis.Reactive.Fusion/                    (UNCHANGED — generated files go to staging)
├── Alis.Reactive.Native/                    (UNCHANGED)
├── Alis.Reactive.SandboxApp/                (UNCHANGED)
├── tests/                                   (UNCHANGED)
└── tools/
    ├── SyncfusionOnboarding/                ← NEW: ASP.NET 10 MVC
    │   ├── SyncfusionOnboarding.csproj
    │   ├── Program.cs
    │   ├── Features/                        (vertical slices per component)
    │   │   ├── Home/
    │   │   │   ├── HomeController.cs
    │   │   │   └── Views/Index.cshtml
    │   │   └── DropDownList/
    │   │       ├── DropDownListController.cs
    │   │       ├── DropDownListDiscoveryService.cs
    │   │       └── Views/Index.cshtml
    │   ├── Data/
    │   │   ├── OnboardingDbContext.cs
    │   │   └── Entities/ (Component, Property, Method, Event, etc.)
    │   ├── Discovery/
    │   │   ├── IApiDiscoveryResult.cs
    │   │   └── DiscoveryMapper.cs
    │   ├── Views/Shared/_Layout.cshtml
    │   ├── wwwroot/js/api-discoverer.js
    │   └── onboarding.db                   (SQLite — gitignored)
    │
    └── SyncfusionGenerator/                 ← NEW: Console app
        ├── SyncfusionGenerator.csproj
        ├── Program.cs
        ├── GeneratorPipeline.cs
        ├── Data/
        │   ├── GeneratorDbContext.cs        (read-only)
        │   └── ComponentDefinition.cs       (projection record)
        ├── Templates/                       (vertical slices per file type)
        │   ├── ITemplate.cs
        │   ├── ComponentClassTemplate.cs
        │   ├── ExtensionsTemplate.cs
        │   ├── ReactiveExtensionsTemplate.cs
        │   ├── EventsTemplate.cs
        │   ├── EventArgsTemplate.cs
        │   └── UnitTestTemplate.cs
        ├── Output/
        │   └── FileWriter.cs
        └── output/                          (staging — generated files land here)
```

---

## SOLID + Vertical Slice Architecture (Both Projects)

Both new projects MUST follow SOLID principles and vertical slice organization.

### SyncfusionOnboarding — Vertical Slices per Component

Each SF component is its own vertical slice — controller, view, and discovery
logic are co-located per component, not split across shared layers.

```
SyncfusionOnboarding/
├── Features/
│   ├── Home/
│   │   ├── HomeController.cs
│   │   └── Views/Index.cshtml
│   ├── DropDownList/              ← vertical slice
│   │   ├── DropDownListController.cs
│   │   ├── DropDownListDiscoveryService.cs
│   │   └── Views/Index.cshtml
│   └── NumericTextBox/            ← another slice
│       ├── NumericTextBoxController.cs
│       ├── NumericTextBoxDiscoveryService.cs
│       └── Views/Index.cshtml
├── Data/                          ← shared infrastructure only
│   ├── OnboardingDbContext.cs
│   └── Entities/
│       ├── Component.cs
│       ├── ComponentProperty.cs
│       ├── ComponentMethod.cs
│       ├── ComponentMethodArg.cs
│       ├── ComponentEvent.cs
│       └── EventArgProperty.cs
├── Discovery/                     ← shared discovery infrastructure
│   ├── IApiDiscoveryResult.cs     (interface — what JS returns)
│   └── DiscoveryMapper.cs         (maps JS result → EF entities)
└── wwwroot/js/
    └── api-discoverer.js          ← shared JS introspection
```

**SOLID enforcement:**
- **S** — Each controller handles ONE component. `DiscoveryMapper` has one job.
- **O** — Adding a new component = new folder. No existing code changes.
- **L** — All components share `IApiDiscoveryResult` contract.
- **I** — Controllers depend on `OnboardingDbContext` interface, not concrete.
- **D** — DI for DbContext, discovery services injected via constructor.

### SyncfusionGenerator — Vertical Slices per Template

Each generated file type is its own template slice — not one monolithic generator.

```
SyncfusionGenerator/
├── Program.cs                     ← CLI entry, parses args
├── GeneratorPipeline.cs           ← orchestrator: reads DB, calls templates
├── Data/
│   ├── GeneratorDbContext.cs      ← read-only SQLite access
│   └── ComponentDefinition.cs     ← projection (no EF entities leaked)
├── Templates/                     ← each template is a vertical slice
│   ├── ITemplate.cs               (interface: Generate(ComponentDefinition) → string)
│   ├── ComponentClassTemplate.cs
│   ├── ExtensionsTemplate.cs
│   ├── ReactiveExtensionsTemplate.cs
│   ├── EventsTemplate.cs
│   ├── EventArgsTemplate.cs
│   └── UnitTestTemplate.cs
└── Output/
    └── FileWriter.cs              ← writes files to disk, handles directories
```

**SOLID enforcement:**
- **S** — Each template generates ONE file. Pipeline orchestrates.
- **O** — Adding a new template = new `ITemplate` implementation. Pipeline auto-discovers.
- **L** — All templates implement `ITemplate` — interchangeable.
- **I** — `ITemplate` is narrow: `(ComponentDefinition) → GeneratedFile`.
- **D** — Pipeline depends on `IEnumerable<ITemplate>` via DI, not concrete types.

**Projection pattern:** `GeneratorDbContext` projects EF entities into a
`ComponentDefinition` record. Templates never see EF entities — only the
clean projection. This decouples generation from storage.

---

## Constraints

- SyncfusionOnboarding uses SF directly — zero Alis.Reactive dependency
- Generator outputs to `tools/SyncfusionGenerator/output/` — NOT into Alis.Reactive.Fusion
- Generator output must match the exact vertical slice pattern (sealed class, extensions, etc.)
- SQLite is the single source of truth — generator never reads from code
- Generated files have AUTO-GENERATED header — distinguishes from hand-written
- Existing Alis.Reactive projects are UNTOUCHED — no modifications whatsoever
- Only after tools are 100% stable will generated files be manually copied to Fusion

---

## Tasks

### Task 1: Scaffold SyncfusionOnboarding project
- `dotnet new mvc` under `tools/SyncfusionOnboarding`
- Add to solution: `dotnet sln add`
- Add NuGet: `Syncfusion.EJ2.AspNet.Core` v32.2.8, `Microsoft.EntityFrameworkCore.Sqlite`
- Configure SF license in `Program.cs`
- Create `Data/` with EF entities and `OnboardingDbContext`
- Run initial EF migration to create SQLite schema
- Verify: app starts, serves default page

### Task 2: Build the JS API discoverer
- Create `wwwroot/js/api-discoverer.js`
- Implements `discover(elementId)` — prototype walking, property enumeration, event detection
- Filters out internal/base-class members (`_` prefix, `constructor`, `destroy`, etc.)
- Classifies: properties vs methods vs events
- Returns structured JSON: `{ properties: [...], methods: [...], events: [...] }`
- Verify: manual test in browser console with mounted SF component

### Task 3: DropDownList vertical slice (SyncfusionOnboarding)
- Create `Features/DropDownList/` folder
- `DropDownListController.cs` — serves view, handles discover POST, handles CRUD saves
- `Views/Index.cshtml` — mounts real SF DropDownList, shows editable tables, discover button
- Wire discovery: button click → JS introspect → POST to controller → save to SQLite
- Verify: mount component, click discover, see API in tables, edit fields, save

### Task 4: Home page + navigation
- `Features/Home/` — lists all registered components with links
- Shows component count, SF version, last discovered date
- Navigation to each component's page
- Verify: home page lists DropDownList, link works

### Task 5: Scaffold SyncfusionGenerator project
- `dotnet new console` under `tools/SyncfusionGenerator`
- Add to solution
- Add NuGet: `Microsoft.EntityFrameworkCore.Sqlite`
- Create `Data/GeneratorDbContext.cs` (read-only) and `ComponentDefinition.cs` projection
- Create `ITemplate` interface and `GeneratorPipeline.cs`
- Parse CLI args: `--component`, `--output`, `--db`
- Verify: reads SQLite, loads ComponentDefinition, prints summary

### Task 6: ComponentClass template
- `Templates/ComponentClassTemplate.cs`
- Generates: sealed phantom type with Vendor and ReadExpr
- Matches exact pattern from `FusionDropDownList.cs`
- Verify: generated file matches hand-written structure

### Task 7: Extensions template
- `Templates/ExtensionsTemplate.cs`
- For each onboarded writable property: `Set{Name}()` with 4 overloads (static, event, response, component)
- For each onboarded readable property: `{Name}()` returning `TypedComponentSource<T>`
- For each onboarded void method: `CallMutation` with no args
- For each onboarded method with args: `CallMutation` with `MethodArg[]`
- Matches exact pattern from `FusionDropDownListExtensions.cs`
- Verify: generated file has identical signatures for the subset that overlaps with hand-written

### Task 8: ReactiveExtensions template
- `Templates/ReactiveExtensionsTemplate.cs`
- Generates `.Reactive()` extension on the SF builder type
- Matches exact pattern from `FusionDropDownListReactiveExtensions.cs`
- Verify: generated file matches hand-written structure

### Task 9: Events + EventArgs templates
- `Templates/EventsTemplate.cs` — singleton with `TypedEventDescriptor` per onboarded event
- `Templates/EventArgsTemplate.cs` — one file per event with arg properties
- Matches exact pattern from `FusionDropDownListEvents.cs` and `Events/` folder
- Verify: generated files match hand-written structure

### Task 10: UnitTest template
- `Templates/UnitTestTemplate.cs`
- Generates snapshot + schema validation tests per mutation method
- Matches exact pattern from `WhenMutatingAFusionDropDownList.cs`
- Verify: generated tests compile and pass when placed alongside existing tests

### Task 11: End-to-end POC verification
- Run full pipeline: discover → curate → generate
- Diff generated vs hand-written FusionDropDownList files
- Verify generated code compiles
- Verify generated files cover 100% of the real DropDownList API
- Document any gaps between generated and hand-written

---

## Verification

1. SyncfusionOnboarding app starts, mounts real SF DropDownList
2. "Discover API" captures all properties, methods, events from JS runtime
3. SQLite contains complete DropDownList API with SF version
4. CRUD UI allows editing all fields, toggling onboard flags
5. Generator produces all 6+ files matching the vertical slice pattern
6. Generated code compiles without errors
7. Generated snapshot tests pass
8. Generated Extensions.cs has identical method signatures to hand-written version
9. Full DropDownList API is onboarded — not just the subset currently hand-written
10. Both projects follow SOLID principles — each task verified at task level
