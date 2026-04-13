# Eliminate Null-as-Domain-Vocabulary — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove null-as-domain-vocabulary from the plan model end-to-end: C# stores domain defaults, JSON carries defaults, TS types are required, TS runtime has zero undefined guards.

**Architecture:** Vertical slices per plan model type. Each slice fixes C# constructor + property, updates schema definition, updates TS type, updates TS runtime consumers, updates VerifyJson snapshots. Global WhenWritingNull removed at the end after all types are fixed.

**Tech Stack:** C# .NET 10, System.Text.Json, JSON Schema, TypeScript 5.8, NUnit + Verify, Playwright

**Baseline:** 128 unit + 78 Fusion + 109 FluentValidator + 19 Native + 825 Playwright

---

## Review Loop Protocol

Two continuous review loops govern this work. No shortcuts. No skipping.

### Loop 1: Plan Review (before any implementation)

```
Write/update plan → Codex xhigh reviews plan → BLOCK findings?
  YES → fix plan → Codex xhigh re-reviews → repeat until SIGN-OFF
  NO  → SIGN-OFF → implementation may begin
```

Every plan change triggers a new review cycle. The plan is not approved until Codex xhigh
gives an unconditional SIGN-OFF with zero BLOCK findings.

### Loop 2: Post-Implementation Review (after EVERY group)

```
Implement group → run tests → Codex xhigh reviews actual diff → BLOCK findings?
  YES → fix code → run tests → Codex xhigh re-reviews → repeat until SIGN-OFF
  NO  → SIGN-OFF → next group may begin
```

Every group goes through this loop. No group starts until the previous group has a clean
SIGN-OFF. "Every change requires a review cycle" — no batching, no skipping.

### What Codex xhigh reviews at each group

| Check | How |
|-------|-----|
| No null stored for "not specified" | `rg "IsNone \? null" --type cs` = 0 in touched files |
| Domain defaults flow to JSON | Pick one test, trace through data flow diagram |
| Tests pass | `dotnet test` output |
| Schema aligned | AssertSchemaValid passes |
| TS types match schema | `npm run typecheck` clean |
| No dirt left behind | `rg "= null" PlanModel/` — every match justified |

---

## Master Index

| Group | Tasks | Layer | Post-Group Review |
|-------|-------|-------|-------------------|
| **G1: Foundation** | T1-T3 | C# model + Schema + TS types | Codex xhigh SIGN-OFF required |
| **G2: ValueProducer slice** | T4-T7 | C# + Schema + TS | Codex xhigh SIGN-OFF required |
| **G3: Condition slice** | T8 | C# + Schema + TS | Codex xhigh SIGN-OFF required |
| **G4: JsType slice** | T9 | C# + Schema + TS | Codex xhigh SIGN-OFF required |
| **G5: Reaction slice** | T10 | C# + Schema + TS | Codex xhigh SIGN-OFF required |
| **G6: Request slice** | T11 | C# + Schema + TS | Codex xhigh SIGN-OFF required |
| **G7: Component slice** | T12 | C# + Schema + TS | Codex xhigh SIGN-OFF required |
| **G8: Builder fixes** | T13 | C# builders | Codex xhigh SIGN-OFF required |
| **G9: TS runtime** | T14-T18 | TS runtime | Codex xhigh SIGN-OFF required |
| **G10: Serialization cleanup** | T19 | C# serialization | Codex xhigh SIGN-OFF required |
| **G11: Full verification** | T20-T22 | All layers | Codex xhigh FINAL SIGN-OFF |

---

## Data Flow Verification Diagram

Run this trace after EVERY group to verify no regression:

```
BEFORE (null pattern):
  C# builder           → constructor           → property        → serializer        → JSON             → TS type          → TS runtime
  shape: null default   → IsNone ? null : shape → Shape = null   → WhenWritingNull   → field omitted    → shape?: Shape    → if (x.shape)
  args: null default    → Count > 0 ? x : null  → Args = null    → WhenWritingNull   → field omitted    → args?: VP[]      → if (x.args?.length)
  path: null default    → IsNone ? null : path   → Path = null    → WhenWritingNull   → field omitted    → path?: Path      → if (x.path)

AFTER (None pattern):
  C# builder           → constructor           → property          → serializer      → JSON                    → TS type        → TS runtime
  shape: null param     → shape ?? Shape.None   → Shape = Shape.None → always written → "shape":{"kind":"none"} → shape: Shape   → shape.kind !== "none"
  args: null param      → args ?? Empty         → Args = []         → always written → "args":[]               → args: VP[]     → args.map(...)
  path: null param      → path ?? Path.None     → Path = Path.None  → always written → "path":[]               → path: Path     → path.length > 0
```

**Regression check:** After each group, pick one test case and trace through both diagrams. The BEFORE path must no longer exist in code. The AFTER path must work end-to-end.

---

## Genuinely Nullable Properties — Exhaustive Audit Table

These properties stay nullable with per-property `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]`.
Every property NOT in this table gets a domain default. No exceptions.

| File:Line | Property | Type | Why nullable |
|-----------|----------|------|-------------|
| `Plan.cs:7` | `PartId` | `string?` | Only set for partial plans |
| `Component.cs:13` | `BindingPath` | `string?` | Unbound components have no binding |
| `Component.cs:20` | `ValueMember` | `string?` | Display components have no value to read |
| `Component.cs:23` | `Container` | `ContainerScope?` | Not all components are in a form |
| `Request.cs:15` | `Container` | `string?` | Not all requests target a form |
| `Request.cs:17` | `Input` | `RequestInput?` | GET requests have no body |
| `Request.cs:27` | `Next` | `Request?` | Not all requests chain |
| `ResponseHandler.cs` | `Status` | `int?` | Null = match any status |
| `Reaction.cs:74` | `OnSettled` | `Reaction?` | Not all parallels need cleanup |
| `Reaction.cs:173` | `PayloadType` | `string?` | Untyped events |
| `StartsWhen.cs:28` | `PayloadType` | `string?` | Untyped document events |
| `StartsWhen.cs:54` | `Event` | `string?` | Server push without event filter |
| `StartsWhen.cs:55` | `PayloadType` | `string?` | Server push untyped |
| `StartsWhen.cs:70` | `PayloadType` | `string?` | SignalR untyped |
| `BranchCase` | `When` | `Condition?` | Null = else/default case (NOT "not specified") |
| `PathSegment` | `Name` | `string?` | Segment is indexed, not named |
| `PathSegment` | `Index` | `int?` | Segment is named, not indexed |
| `GatherInput` | `Statics` | `ValueProducer?` | Not all gathers have statics |
| `GatherInput` | `IncludeAll` | `bool?` | Uses WhenWritingDefault, not WhenWritingNull |
| `Source.cs:39` | `PayloadSource.Type` | `string?` | Untyped payload events (construction-nullable) |
| `JsType.cs:119` | `JsEvent.PayloadType` | `string?` | Untyped component events |
| `Shape.cs:106` | `Additional` | `bool?` | Only set for open-object shapes (`Shape.OpenObject()`) |

Every property in this table MUST have `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]`
(or `WhenWritingDefault` for `bool?`) BEFORE Task 19 removes global WhenWritingNull.

### Pre-Task-19 Safety Check

Two greps — catches both explicit `?` annotations AND construction-nullable patterns:

```bash
# Check 1: Explicit nullable annotations
rg "public.*\?\s+\w+\s*\{" Alis.Reactive/PlanModel/ --type cs

# Check 2: Construction-nullable (fields set to null in constructors or left uninitialized)
rg "= null;|= null!;" Alis.Reactive/PlanModel/ --type cs

# Check 3: Properties without initializers that could default to null
rg "public\s+\w+\s+\w+\s*\{\s*get;" Alis.Reactive/PlanModel/ --type cs

# Cross-reference EVERY match against the audit table above.
# If a match is NOT in the table and NOT getting a domain default in the plan, it's a bug.
```

---

## G1: Foundation — New Sentinels

### Task 1: Create ValueProducer.None

**Files:**
- Modify: `Alis.Reactive/PlanModel/ValueProducer.cs`

- [ ] **Step 1: Add NoneProducer subclass and None static field**

```csharp
// In ValueProducer.cs — add after the Null() factory method (line 38)

internal static readonly ValueProducer None = new NoneProducer();

// Add as new sealed class after ArrayProducer (line 148)

/// <summary>Sentinel for "no value specified." Not constructed in application code.</summary>
public sealed class NoneProducer : ValueProducer
{
    /// <summary>Gets the kind. Always <c>"none"</c>.</summary>
    public string Kind => "none";

    internal NoneProducer() { }
}
```

- [ ] **Step 2: Add IsNone property to ValueProducer base**

```csharp
// In ValueProducer abstract class, add:
internal bool IsNone => this is NoneProducer;
```

- [ ] **Step 3: Build to verify compilation**

Run: `dotnet build Alis.Reactive.slnx -nologo 2>&1 | tail -3`
Expected: 0 Error(s)

- [ ] **Step 4: Commit**

```bash
git add Alis.Reactive/PlanModel/ValueProducer.cs
git commit -m "feat: add ValueProducer.None sentinel for 'not specified'"
```

### Task 2: Create Condition.None

**Files:**
- Modify: `Alis.Reactive/PlanModel/Condition.cs`

- [ ] **Step 1: Add NoneCondition subclass and None static field**

```csharp
// In Condition abstract class, after Confirm() factory (line 29):
internal static readonly Condition None = new NoneCondition();

// Add as new sealed class after ConfirmCondition (line 112):

/// <summary>Sentinel for "no guard specified." Evaluates to true (no restriction). Not constructed in application code.</summary>
public sealed class NoneCondition : Condition
{
    /// <summary>Gets the kind. Always <c>"none"</c>.</summary>
    public string Kind => "none";

    internal NoneCondition() { }
}
```

- [ ] **Step 2: Add IsNone property to Condition base**

```csharp
internal bool IsNone => this is NoneCondition;
```

- [ ] **Step 3: Build to verify compilation**

Run: `dotnet build Alis.Reactive.slnx -nologo 2>&1 | tail -3`
Expected: 0 Error(s)

- [ ] **Step 4: Commit**

```bash
git add Alis.Reactive/PlanModel/Condition.cs
git commit -m "feat: add Condition.None sentinel for 'no guard specified'"
```

### Task 3: Register new sentinels in WriteOnlyPolymorphicConverter

**Files:**
- Modify: `Alis.Reactive/Schemas/reactive-plan.schema.json`
- Modify: `Alis.Reactive.SandboxApp/Scripts/types/plan.ts`

The `WriteOnlyPolymorphicConverter<T>` dispatches by concrete type — NoneProducer and NoneCondition
will serialize automatically as `{"kind":"none"}`. No converter changes needed.

- [ ] **Step 1: Add NoneProducer to schema ValueProducer oneOf**

In `reactive-plan.schema.json`, add to the ValueProducer `oneOf` array:

```json
{
  "$ref": "#/$defs/NoneProducer"
}
```

And add the `$defs/NoneProducer` definition:

```json
"NoneProducer": {
  "type": "object",
  "properties": {
    "kind": { "const": "none" }
  },
  "required": ["kind"],
  "additionalProperties": false
}
```

- [ ] **Step 2: Add NoneCondition to schema Condition oneOf**

Same pattern — add `$ref` to oneOf and `$defs/NoneCondition`:

```json
"NoneCondition": {
  "type": "object",
  "properties": {
    "kind": { "const": "none" }
  },
  "required": ["kind"],
  "additionalProperties": false
}
```

- [ ] **Step 3: Add "none" to Shape kind enum**

In the Shape definition, add `"none"` to the `kind` enum values.

- [ ] **Step 4: Add NoneProducer and NoneCondition to TS types**

In `Scripts/types/plan.ts`:

```typescript
// Add to ValueProducer discriminated union:
export interface NoneProducer { kind: "none" }

// Update ValueProducer type:
export type ValueProducer = LiteralProducer | ReadProducer | ObjectProducer | ArrayProducer | NoneProducer;

// Add to Condition discriminated union:
export interface NoneCondition { kind: "none" }

// Update Condition type:
export type Condition = CompareCondition | AllCondition | AnyCondition | NotCondition | ConfirmCondition | NoneCondition;
```

- [ ] **Step 5: Build + typecheck**

Run: `dotnet build Alis.Reactive.slnx -nologo 2>&1 | tail -3`
Run: `npm run typecheck`
Expected: both clean

- [ ] **Step 6: Commit**

```bash
git add Alis.Reactive/Schemas/reactive-plan.schema.json
git add Alis.Reactive.SandboxApp/Scripts/types/plan.ts
git commit -m "feat: add NoneProducer, NoneCondition, Shape 'none' to schema + TS types"
```

---

## Review: G1 Post-Implementation

- [ ] **Run all unit tests**: `dotnet test tests/Alis.Reactive.UnitTests -nologo` — must pass (128)
- [ ] **Dispatch Codex xhigh** to review G1 diff: `git diff HEAD~3..HEAD`
- [ ] **Codex xhigh SIGN-OFF** required before starting G2
- [ ] If BLOCK: fix → re-test → re-review → repeat until SIGN-OFF

---

## G2: ValueProducer Slice (4 subtypes)

### Task 4: Fix LiteralProducer — Shape.None stored, not null

**Files:**
- Modify: `Alis.Reactive/PlanModel/ValueProducer.cs:77-81` (LiteralProducer constructor)

- [ ] **Step 1: Change constructor to store domain default**

```csharp
// BEFORE (line 77-81):
internal LiteralProducer(object value, Shape shape)
{
    Value = value;
    Shape = shape == Shape.None ? null : shape;
}

// AFTER:
internal LiteralProducer(object value, Shape shape)
{
    Value = value;
    Shape = shape ?? Shape.None;
}
```

Note: `Value` stays `object?` — null is a valid JSON literal.

- [ ] **Step 2: Run unit tests — expect snapshot failures**

Run: `dotnet test tests/Alis.Reactive.UnitTests -nologo`
Expected: Some tests FAIL because VerifyJson snapshots now include `"shape": {"kind": "none"}`
where they previously omitted the field.

- [ ] **Step 3: Update VerifyJson snapshots**

For each failing snapshot, accept the new output. The diff should show ONLY the addition
of `"shape": {"kind": "none"}` — no other changes.

- [ ] **Step 4: Update schema — LiteralProducer.shape becomes required**

In `reactive-plan.schema.json`, in the LiteralProducer definition:
- Move `"shape"` into the `"required"` array: `["kind", "value", "shape"]`

- [ ] **Step 5: Run all unit tests — expect pass**

Run: `dotnet test tests/Alis.Reactive.UnitTests -nologo`
Expected: 128 passed (same count, updated snapshots)

- [ ] **Step 6: Trace verification**

Manually verify one test's JSON output:
- Find a test that creates `ValueProducer.Literal("hello")`
- Confirm JSON contains `"shape": {"kind": "string"}` (was already present)
- Find a test that creates `ValueProducer.Null()`
- Confirm JSON contains `"shape": {"kind": "none"}` (NEW — was omitted before)

- [ ] **Step 7: Commit**

```bash
git add Alis.Reactive/PlanModel/ValueProducer.cs
git add tests/ # updated snapshots
git add Alis.Reactive/Schemas/reactive-plan.schema.json
git commit -m "fix: LiteralProducer stores Shape.None instead of null"
```

### Task 5: Fix ReadProducer — Shape, Path, Args all get domain defaults

**Files:**
- Modify: `Alis.Reactive/PlanModel/ValueProducer.cs:106-113` (ReadProducer constructor)

- [ ] **Step 1: Change constructor**

```csharp
// BEFORE (lines 106-113):
internal ReadProducer(Source from, string member, Path path, Shape shape, List<ValueProducer> args = null)
{
    From = from ?? throw new ArgumentNullException(nameof(from));
    Member = member ?? throw new ArgumentNullException(nameof(member));
    Path = path == null || path.IsNone ? null : path;
    Shape = shape == null || shape.IsNone ? null : shape;
    Args = args != null && args.Count > 0 ? args : null;
}

// AFTER:
internal ReadProducer(Source from, string member, Path path = null, Shape shape = null, List<ValueProducer> args = null)
{
    From = from ?? throw new ArgumentNullException(nameof(from));
    Member = member ?? throw new ArgumentNullException(nameof(member));
    Path = path ?? Path.None;
    Shape = shape ?? Shape.None;
    Args = args is { Count: > 0 } ? args : Array.Empty<ValueProducer>();
}
```

- [ ] **Step 2: Run unit tests — expect snapshot failures**

Run: `dotnet test tests/Alis.Reactive.UnitTests -nologo`
Expected: Failures where snapshots now include `"path": [], "shape": {"kind": "none"}, "args": []`

- [ ] **Step 3: Update VerifyJson snapshots**

Accept new output. Diff should show ONLY additions of path/shape/args defaults.

- [ ] **Step 4: Update schema — ReadProducer path/shape/args become required**

In `reactive-plan.schema.json`, ReadProducer definition:
- `"required": ["kind", "from", "member", "path", "shape", "args"]`

- [ ] **Step 5: Run all unit tests — expect pass**

Run: `dotnet test tests/Alis.Reactive.UnitTests -nologo`
Expected: 128 passed

- [ ] **Step 6: Trace verification**

Pick a ReadProducer test:
- Confirm `"path": []` present (was omitted)
- Confirm `"shape": {"kind": "none"}` or actual shape present
- Confirm `"args": []` present (was omitted)

- [ ] **Step 7: Commit**

### Task 6: Fix ObjectProducer — Shape domain default

**Files:**
- Modify: `Alis.Reactive/PlanModel/ValueProducer.cs:126-130` (ObjectProducer constructor)

- [ ] **Step 1: Change constructor**

```csharp
// BEFORE:
internal ObjectProducer(Dictionary<string, ValueProducer> fields, Shape shape)
{
    Fields = fields ?? throw new ArgumentNullException(nameof(fields));
    Shape = shape == null || shape.IsNone ? null : shape;
}

// AFTER:
internal ObjectProducer(Dictionary<string, ValueProducer> fields, Shape shape = null)
{
    Fields = fields ?? throw new ArgumentNullException(nameof(fields));
    Shape = shape ?? Shape.None;
}
```

- [ ] **Step 2: Run unit tests — expect snapshot failures**

Run: `dotnet test tests/Alis.Reactive.UnitTests -nologo`
Expected: Failures where snapshots now include `"shape": {"kind": "none"}`

- [ ] **Step 3: Update VerifyJson snapshots**

Accept new output. Diff shows ONLY addition of `"shape": {"kind": "none"}`.

- [ ] **Step 4: Update schema — ObjectProducer.shape becomes required**

In `reactive-plan.schema.json`, ObjectProducer definition:
- `"required": ["kind", "fields", "shape"]`

- [ ] **Step 5: Run all unit tests — expect pass**

Run: `dotnet test tests/Alis.Reactive.UnitTests -nologo`
Expected: 128 passed

- [ ] **Step 6: Commit**

```bash
git add Alis.Reactive/PlanModel/ValueProducer.cs tests/ Alis.Reactive/Schemas/reactive-plan.schema.json
git commit -m "fix: ObjectProducer stores Shape.None instead of null"
```

### Task 7: Fix ArrayProducer — Shape domain default

**Files:**
- Modify: `Alis.Reactive/PlanModel/ValueProducer.cs:143-148` (ArrayProducer constructor)

- [ ] **Step 1: Change constructor**

```csharp
// BEFORE (lines 143-148):
internal ArrayProducer(List<ValueProducer> items, Shape shape)
{
    Items = items ?? throw new ArgumentNullException(nameof(items));
    Shape = shape == null || shape.IsNone ? null : shape;
}

// AFTER:
internal ArrayProducer(List<ValueProducer> items, Shape shape = null)
{
    Items = items ?? throw new ArgumentNullException(nameof(items));
    Shape = shape ?? Shape.None;
}
```

- [ ] **Step 2: Run unit tests — expect snapshot failures**

Run: `dotnet test tests/Alis.Reactive.UnitTests -nologo`

- [ ] **Step 3: Update VerifyJson snapshots**

- [ ] **Step 4: Update schema — ArrayProducer.shape becomes required**

In `reactive-plan.schema.json`, ArrayProducer definition:
- `"required": ["kind", "items", "shape"]`

- [ ] **Step 5: Run all unit tests — expect pass**

Run: `dotnet test tests/Alis.Reactive.UnitTests -nologo`
Expected: 128 passed

- [ ] **Step 6: Commit**

```bash
git add Alis.Reactive/PlanModel/ValueProducer.cs tests/ Alis.Reactive/Schemas/reactive-plan.schema.json
git commit -m "fix: ArrayProducer stores Shape.None instead of null"
```

**Gate: Run all unit tests + Fusion tests**

Run: `dotnet test tests/Alis.Reactive.UnitTests -nologo && dotnet test tests/Alis.Reactive.Fusion.UnitTests -nologo`
Expected: 128 + 78 passed

---

## G3: Condition Slice

### Task 8: Fix CompareCondition — Right, Shape, ItemShape get domain defaults

**Files:**
- Modify: `Alis.Reactive/PlanModel/Condition.cs:48-55`

- [ ] **Step 1: Change constructor + properties**

```csharp
// BEFORE:
public ValueProducer? Right { get; }
public Shape Shape { get; }
public Shape ItemShape { get; }

internal CompareCondition(ValueProducer left, string op, ValueProducer right, Shape shape, Shape itemShape)
{
    Left = left ?? throw new ArgumentNullException(nameof(left));
    Op = op ?? throw new ArgumentNullException(nameof(op));
    Right = right;
    Shape = shape == null || shape.IsNone ? null : shape;
    ItemShape = itemShape == null || itemShape.IsNone ? null : itemShape;
}

// AFTER:
public ValueProducer Right { get; }
public Shape Shape { get; }
public Shape ItemShape { get; }

internal CompareCondition(ValueProducer left, string op, ValueProducer right = null, Shape shape = null, Shape itemShape = null)
{
    Left = left ?? throw new ArgumentNullException(nameof(left));
    Op = op ?? throw new ArgumentNullException(nameof(op));
    Right = right ?? ValueProducer.None;
    Shape = shape ?? Shape.None;
    ItemShape = itemShape ?? Shape.None;
}
```

- [ ] **Step 2: Update factory method**

```csharp
// Condition.Compare — default params already nullable, no change needed
// BUT: callers that pass explicit null need checking
```

- [ ] **Step 3: Update schema — CompareCondition right/shape/itemShape become required**

- [ ] **Step 4: Run tests, update snapshots, verify, commit**

---

## G4: JsType Slice

### Task 9a: Fix JsType dictionaries — Properties, Methods, Events become empty dicts

**Files:**
- Modify: `Alis.Reactive/PlanModel/JsType.cs:8-10` (backing fields)

- [ ] **Step 1: Initialize dictionaries to empty, make non-nullable**

```csharp
// BEFORE (JsType.cs:8-10):
private Dictionary<string, JsProperty> _properties;
private Dictionary<string, JsMethod> _methods;
private Dictionary<string, JsEvent> _events;

// AFTER:
private Dictionary<string, JsProperty> _properties = new();
private Dictionary<string, JsMethod> _methods = new();
private Dictionary<string, JsEvent> _events = new();
```

Update public properties to return non-nullable:
```csharp
public IReadOnlyDictionary<string, JsProperty> Properties => _properties;
public IReadOnlyDictionary<string, JsMethod> Methods => _methods;
public IReadOnlyDictionary<string, JsEvent> Events => _events;
```

- [ ] **Step 2: Update schema — JsType properties/methods/events become required**
- [ ] **Step 3: Update TS types — properties/methods/events become required in plan.ts**

```typescript
// BEFORE:
properties?: Record<string, Property>;
methods?: Record<string, Method>;
events?: Record<string, Event>;

// AFTER:
properties: Record<string, Property>;
methods: Record<string, Method>;
events: Record<string, Event>;
```

- [ ] **Step 4: Run tests, update snapshots, commit**

### Task 9b: Fix JsMethod — Args and Returns get domain defaults

**Files:**
- Modify: `Alis.Reactive/PlanModel/JsType.cs:105-112`

- [ ] **Step 1: Change constructor**

```csharp
// BEFORE:
public List<Shape>? Args { get; }
public Shape? Returns { get; }

internal JsMethod(List<Shape> args, Shape returns)
{
    Args = args != null && args.Count > 0 ? args : null;
    Returns = returns == null || returns.IsNone ? null : returns;
}

// AFTER:
public IReadOnlyList<Shape> Args { get; }
public Shape Returns { get; }

internal JsMethod(List<Shape> args = null, Shape returns = null)
{
    Args = args is { Count: > 0 } ? args : Array.Empty<Shape>();
    Returns = returns ?? Shape.None;
}
```

- [ ] **Step 2: Update schema — JsMethod args/returns become required**
- [ ] **Step 3: Run tests, update snapshots, verify, commit**

---

## G5: Reaction Slice

### Task 10: Fix Reaction subtypes — Args, When, Data get domain defaults

**Files:**
- Modify: `Alis.Reactive/PlanModel/Reaction.cs`

- [ ] **Step 1: Fix CallReaction.Args**

```csharp
// BEFORE (line 148):
Args = args != null && args.Count > 0 ? args : null;

// AFTER:
Args = args is { Count: > 0 } ? args : Array.Empty<ValueProducer>();
```

Property: `public IReadOnlyList<ValueProducer> Args { get; }` (non-nullable)

- [ ] **Step 2: Fix DispatchReaction.Data — use ValueProducer.None**

```csharp
// BEFORE (Reaction.cs:171):
public ValueProducer? Data { get; }

// AFTER:
public ValueProducer Data { get; }

// Constructor:
Data = data ?? ValueProducer.None;
```

- [ ] **Step 3: Fix genuinely nullable properties — add per-property attributes**

These stay nullable (genuine domain absence). Add `[JsonIgnore(WhenWritingNull)]` to each:

```csharp
// Reaction.cs — DispatchReaction:
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public string? PayloadType { get; }

// Reaction.cs — ParallelReaction:
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public Reaction? OnSettled { get; }
```

Note: `BranchCase.When` stays nullable with NO change — null means "else/default case."
This is genuine domain absence (the else branch has no condition), NOT "not specified."

- [ ] **Step 4: Update schema**

- CallReaction: `"required": ["kind", "on", "method", "args"]` (args now required)
- DispatchReaction: `"data"` becomes required (NoneProducer when no data)
- BranchCase.when stays optional (else-case)

- [ ] **Step 5: Run unit tests, update snapshots**

Run: `dotnet test tests/Alis.Reactive.UnitTests -nologo`
Accept new snapshots. Verify diffs show only expected changes.

- [ ] **Step 6: Commit**

```bash
git add Alis.Reactive/PlanModel/Reaction.cs tests/ Alis.Reactive/Schemas/reactive-plan.schema.json
git commit -m "fix: Reaction subtypes use domain defaults — Args=[], Data=VP.None"
```

---

## G6: Request Slice

### Task 11: Fix Request — all collection properties get empty defaults

**Files:**
- Modify: `Alis.Reactive/PlanModel/Request.cs`

- [ ] **Step 1: Fix list properties**

```csharp
// BEFORE:
public List<Reaction>? Before { get; internal set; }
public List<ResponseHandler>? Success { get; internal set; }
public List<ResponseHandler>? Error { get; internal set; }
public List<Reaction>? Complete { get; internal set; }
public Dictionary<string, ValueProducer>? Headers { get; internal set; }
public Dictionary<string, ValueProducer>? RouteParams { get; internal set; }

// AFTER:
public IReadOnlyList<Reaction> Before { get; internal set; } = Array.Empty<Reaction>();
public IReadOnlyList<ResponseHandler> Success { get; internal set; } = Array.Empty<ResponseHandler>();
public IReadOnlyList<ResponseHandler> Error { get; internal set; } = Array.Empty<ResponseHandler>();
public IReadOnlyList<Reaction> Complete { get; internal set; } = Array.Empty<Reaction>();
public IReadOnlyDictionary<string, ValueProducer> Headers { get; internal set; } = new Dictionary<string, ValueProducer>();
public IReadOnlyDictionary<string, ValueProducer> RouteParams { get; internal set; } = new Dictionary<string, ValueProducer>();
```

- [ ] **Step 2: Genuinely nullable properties stay nullable with per-property attribute**

```csharp
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public string? Container { get; internal set; }

[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public RequestInput? Input { get; internal set; }

[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public Request? Next { get; internal set; }
```

- [ ] **Step 3: Fix builders that assign these properties**

Search for every `request.Before =`, `request.Headers =`, etc. in builders
and ensure they assign lists/dicts, never null.

- [ ] **Step 4: Update schema — Before/Success/Error/Complete/Headers/RouteParams required**
- [ ] **Step 5: Run tests, update snapshots, verify, commit**

---

## G7: Component Slice

### Task 12: Fix Component — ValidationRules, Constraint, OtherValue, When, Shape

**Files:**
- Modify: `Alis.Reactive/PlanModel/Component.cs`

- [ ] **Step 1: Fix collection + sentinel properties**

```csharp
// BEFORE:
public List<ComponentValidation>? ValidationRules { get; internal set; }
public ValueProducer? Constraint { get; internal set; }
public ValueProducer? OtherValue { get; internal set; }
public Condition? When { get; internal set; }
public Shape? Shape { get; internal set; }

// AFTER:
public IReadOnlyList<ComponentValidation> ValidationRules { get; internal set; } = Array.Empty<ComponentValidation>();
public ValueProducer Constraint { get; internal set; } = ValueProducer.None;
public ValueProducer OtherValue { get; internal set; } = ValueProducer.None;
public Condition When { get; internal set; } = Condition.None;
public Shape Shape { get; internal set; } = Shape.None;
```

- [ ] **Step 2: Genuinely nullable properties stay nullable with per-property attribute**

```csharp
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
internal string? BindingPath { get; set; }

[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
internal string? ValueMember { get; set; }

[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public ContainerScope? Container { get; internal set; }
```

- [ ] **Step 3: Remove existing [JsonIgnore(WhenWritingNull)] from properties that now have domain defaults**

The 8 existing per-property attributes on Component.cs — remove the ones for properties
that now carry domain defaults (ValidationRules, Constraint, OtherValue, When, Shape).
Keep the ones for genuinely nullable properties (BindingPath, ValueMember, Container).

- [ ] **Step 4: Fix ReactivePlan.cs builder code that assigns Component properties**

Search for every `comp.ValidationRules =`, `comp.Constraint =`, `comp.When =`, etc.
Ensure they never assign null.

- [ ] **Step 5: Update schema, run tests, update snapshots, verify, commit**

---

## Review: G2-G7 Post-Implementation (each group individually)

Each group (G2, G3, G4, G5, G6, G7) follows the same review loop:

- [ ] **Run unit tests**: `dotnet test tests/Alis.Reactive.UnitTests -nologo` + relevant project tests
- [ ] **Dispatch Codex xhigh** to review that group's diff
- [ ] **Trace data flow diagram** — pick one type from the group, trace end-to-end
- [ ] **Codex xhigh SIGN-OFF** required before starting next group
- [ ] If BLOCK: fix → re-test → re-review → repeat until SIGN-OFF

---

## G8: Builder Fixes

### Task 13: Fix builder code that passes null where domain defaults are expected

**Files:**
- Modify: `Alis.Reactive/Builders/PluginCallBuilder.cs:78`
- Modify: `Alis.Reactive/Builders/PipelineBuilder.cs:272`
- Modify: `Alis.Reactive/Builders/Requests/HttpRequestBuilder.cs` (multiple)
- Modify: `Alis.Reactive/Builders/Requests/ResponseBuilder.cs`
- Modify: `Alis.Reactive/ReactivePlan.cs` (validation wiring)

- [ ] **Step 1: Fix PluginCallBuilder — empty args instead of null**

```csharp
// BEFORE (line 78):
_args.Count > 0 ? _args : null

// AFTER:
_args.Count > 0 ? _args : Array.Empty<ValueProducer>()

// OR simply: _args (constructor now handles empty)
```

- [ ] **Step 2: Fix PipelineBuilder — empty steps instead of null**

```csharp
// BEFORE (line 272):
var pendingCommands = Steps.Count > 0 ? Steps : null;

// AFTER:
var pendingCommands = Steps.Count > 0 ? Steps : Array.Empty<Reaction>();
```

- [ ] **Step 3: Fix HttpRequestBuilder — ensure lists assigned, not null**

Trace every assignment to Request.Before, .Success, .Error, .Complete, .Headers, .RouteParams.
Ensure none assign null.

- [ ] **Step 4: Fix ReactivePlan.cs validation wiring**

```csharp
// BEFORE (line 251):
if (extracted.Shape != null && !extracted.Shape.IsNone)

// AFTER:
if (!extracted.Shape.IsNone)
```

And fix line 300 where `right = null` for unary ops:
```csharp
// BEFORE:
right = null;

// AFTER:
// right is already ValueProducer.None from CompareCondition default
```

- [ ] **Step 5: Audit FluentValidationRuleExtractor for null assignments**

```bash
rg "\.Shape = null|\.Args = null|\.When = null|\.Constraint = null" Alis.Reactive.FluentValidator/ --type cs
```

Fix any assignments that set plan model properties to null when they should set domain defaults.
Key file: `FluentValidationAdapter.cs:484` — `Shape = shape ?? Shape.None` (already correct).

- [ ] **Step 6: Audit Fusion extension files for null assignments**

```bash
rg "\.Shape = null|\.Args = null|\.When = null|\.Constraint = null|\.Data = null" Alis.Reactive.Fusion/ --type cs
```

Fix any assignments that set plan model properties to null.

- [ ] **Step 7: Run ALL unit tests (all projects)**

```bash
dotnet test tests/Alis.Reactive.UnitTests -nologo
dotnet test tests/Alis.Reactive.Fusion.UnitTests -nologo
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests -nologo
dotnet test tests/Alis.Reactive.Native.UnitTests -nologo
```

Expected: ALL pass.

- [ ] **Step 8: Commit**

---

## Review: G8 Post-Implementation

- [ ] Run all unit tests (all projects)
- [ ] Dispatch Codex xhigh to review G8 diff
- [ ] Codex xhigh SIGN-OFF required before G9

---

## G9: TS Runtime — Remove undefined/null guards

### Task 14: Fix evaluate.ts — shape always present

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Scripts/core/evaluate.ts`

- [ ] **Step 1: Fix shape fallback chain**

```typescript
// BEFORE (line 62):
return raw == null ? raw : applyShape(raw, producer.shape ?? prop.shape);

// AFTER:
return raw == null ? raw : applyShape(raw, producer.shape.kind !== "none" ? producer.shape : prop.shape);

// Same pattern for lines 69, 83, 92, 95, 98
```

Note: `raw == null` checks are NOT plan model guards — they check the actual runtime VALUE.
These stay.

- [ ] **Step 2: Run typecheck**

Run: `npm run typecheck`
Expected: clean

- [ ] **Step 3: Commit**

### Task 15: Fix execute.ts — args always an array

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Scripts/execution/execute.ts`

- [ ] **Step 1: Fix args access**

```typescript
// BEFORE (line 228):
const args = reaction.args?.map(a => evaluateValue(a, plan, ctx)) ?? [];

// AFTER:
const args = reaction.args.map(a => evaluateValue(a, plan, ctx));
```

```typescript
// BEFORE (line 216):
const prop = jsType.properties?.[reaction.property];

// AFTER:
const prop = jsType.properties[reaction.property];
```

```typescript
// BEFORE (line 245):
const method = jsType.methods?.[reaction.method];

// AFTER:
const method = jsType.methods[reaction.method];
```

- [ ] **Step 2: Run typecheck + commit**

### Task 16: Fix conditions.ts — right is always a ValueProducer

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Scripts/conditions/conditions.ts`

- [ ] **Step 1: Fix unary operator check**

```typescript
// BEFORE (line 81):
if (unary !== undefined) return unary;

// AFTER — check if right is NoneProducer:
if (condition.right.kind === "none") {
    // unary operator — evaluate without right operand
}
```

Trace the full evaluateCondition function and update all `right` checks.

- [ ] **Step 2: Run typecheck + commit**

### Task 17: Fix orchestrator.ts — validationRules always an array

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Scripts/validation/orchestrator.ts`

- [ ] **Step 1: Fix optional chaining on validationRules**

```typescript
// BEFORE (line 46):
if ((containerScope.validationRules?.length ?? 0) > 0) {

// AFTER:
if (containerScope.validationRules.length > 0) {
```

```typescript
// BEFORE (line 134):
if (!containerComp?.container?.validationRules) return;

// AFTER:
if (!containerComp?.container) return;
// validationRules is always an array now — check container existence only
```

Similar fixes for lines 186, 347.

- [ ] **Step 2: Run typecheck + commit**

### Task 18a: Fix rule-engine.ts — CRITICAL: shape guard change

**Files:**
- Modify: `Scripts/validation/rule-engine.ts:30,46`

- [ ] **Step 1: Fix shape guard (CRITICAL — wrong behavior if missed)**

```typescript
// BEFORE (line 30):
if (!rule.shape) return true;

// AFTER:
if (rule.shape.kind === "none") return true;
```

Why critical: With NoneShape (`{kind: "none"}`), `!rule.shape` is FALSE (truthy object).
Without this fix, validation falls through to `compareValues` with unconverted values,
breaking min/max/range rules. This is the highest-risk change in the TS runtime.

- [ ] **Step 2: Fix constraint optional chaining**

```typescript
// BEFORE (line 42):
if (rule.constraint?.kind === "literal") return rule.constraint.value;

// AFTER:
if (rule.constraint.kind === "literal") return (rule.constraint as LiteralProducer).value;
```

- [ ] **Step 3: Commit**

### Task 18b: Fix shape-convert.ts and wire-format.ts — remove optional parameter

**Files:**
- Modify: `Scripts/core/shape-convert.ts:23-24,28`
- Modify: `Scripts/core/wire-format.ts:7`

- [ ] **Step 1: Update applyShape signature**

```typescript
// BEFORE (shape-convert.ts:23):
export function applyShape(value: unknown, shape?: Shape): unknown {
  if (!shape) return value;

// AFTER:
export function applyShape(value: unknown, shape: Shape): unknown {
  if (shape.kind === "none") return value;
```

- [ ] **Step 2: Update formatForWire signature**

```typescript
// BEFORE (wire-format.ts:7):
export function formatForWire(value: unknown, shape?: Shape): unknown {
  if (!shape) return value;

// AFTER:
export function formatForWire(value: unknown, shape: Shape): unknown {
  if (shape.kind === "none") return value;
```

- [ ] **Step 3: Run typecheck — fix any callers that pass undefined**

Run: `npm run typecheck`
Fix any errors where callers pass `undefined` to these functions.

- [ ] **Step 4: Commit**

### Task 18c: Fix remaining TS files — gather, trigger, merge-plan

**Files:**
- Modify: `Scripts/execution/gather.ts:115,193,223`
- Modify: `Scripts/execution/trigger.ts:61-62`
- Modify: `Scripts/lifecycle/merge-plan.ts:10,186`

- [ ] **Step 1: Fix gather.ts**

```typescript
// BEFORE (line 115):
const itemShape = shape?.kind === "array" ? shape.item : undefined;
// AFTER:
const itemShape = shape.kind === "array" ? shape.item : undefined;

// BEFORE (line 223):
const prop = jsType?.properties?.[comp.valueMember];
// AFTER:
const prop = jsType.properties[comp.valueMember];
```

- [ ] **Step 2: Fix trigger.ts**

```typescript
// BEFORE (line 61-62):
const eventDef = jsType.events?.[trigger.event];
const channel = eventDef?.channel ?? trigger.event;
// AFTER:
const eventDef = jsType.events[trigger.event];
const channel = eventDef?.channel ?? trigger.event;  // eventDef may not exist in dict — keep ?.
```

- [ ] **Step 3: Fix merge-plan.ts**

```typescript
// BEFORE (line 10):
if (!existing?.container?.validationRules || !incoming.container?.validationRules) return;
// AFTER — container is genuinely nullable (not all components have containers):
if (!existing?.container || !incoming.container) return;
// validationRules is always an array now, no need to check it
```

- [ ] **Step 4: Leave DOM/vendor checks unchanged**

These are NOT plan model guards — do not touch:
- `el.ej2_instances?.[0]` (vendor DOM check)
- `target?.closest` (DOM navigation)
- `window.alis?.confirm` (global check)

- [ ] **Step 2: Run typecheck**

Run: `npm run typecheck`
Expected: clean

- [ ] **Step 3: Build JS bundle**

Run: `npm run build`
Expected: clean

- [ ] **Step 4: Commit**

---

## G10: Serialization Cleanup

### Task 19: Remove global WhenWritingNull, finalize per-property attributes

**Files:**
- Modify: `Alis.Reactive/ReactivePlan.cs:309-320` (ReactivePlanSerializer)
- Modify: `Alis.Reactive.Native/Components/NativeActionLink/NativeActionLinkSerializer.cs:15`

- [ ] **Step 1: Remove DefaultIgnoreCondition from Compact options**

```csharp
// BEFORE:
private static readonly JsonSerializerOptions Compact = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

// AFTER:
private static readonly JsonSerializerOptions Compact = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};
```

- [ ] **Step 2: Same for Formatted options**

- [ ] **Step 3: Same for NativeActionLinkSerializer**

- [ ] **Step 4: Verify all genuinely nullable properties have per-property [JsonIgnore(WhenWritingNull)]**

Grep for every `?` property on plan model types. Each genuinely nullable property must have
the per-property attribute. If any is missing, the property will now serialize as `null` in JSON
(visible regression).

- [ ] **Step 5: Run ALL unit tests**

```bash
dotnet test tests/Alis.Reactive.UnitTests -nologo
dotnet test tests/Alis.Reactive.Fusion.UnitTests -nologo
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests -nologo
dotnet test tests/Alis.Reactive.Native.UnitTests -nologo
```

Expected: ALL pass. If any fail, a genuinely nullable property is missing its per-property attribute.

- [ ] **Step 6: Commit**

---

## Review: G8-G10 Post-Implementation (each group individually)

Same review loop for G8, G9, G10:

- [ ] **Run tests**: unit tests for G8/G10, `npm run typecheck` + `npm run build` for G9
- [ ] **Dispatch Codex xhigh** to review that group's diff
- [ ] **Codex xhigh SIGN-OFF** required before next group
- [ ] If BLOCK: fix → re-test → re-review → repeat until SIGN-OFF

After G10 (serialization cleanup), run the full null-elimination verification:

```bash
rg "IsNone \? null" --type cs                            # Expected: 0
rg "Count > 0 \? .+ : null" Alis.Reactive/PlanModel/ --type cs  # Expected: 0
rg "DefaultIgnoreCondition.*WhenWritingNull" --type cs    # Expected: 0
```

---

## G11: Full Verification

### Task 20: Run full C# test suite

```bash
dotnet test tests/Alis.Reactive.UnitTests -nologo
dotnet test tests/Alis.Reactive.Fusion.UnitTests -nologo
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests -nologo
dotnet test tests/Alis.Reactive.Native.UnitTests -nologo
```

Expected: >= baseline (128 + 78 + 109 + 19)

### Task 21: Run full Playwright suite

- [ ] **Step 1: Kill lingering processes**

```bash
lsof -ti:5220 | xargs kill -9 2>/dev/null
pkill -f "dotnet.*SandboxApp" 2>/dev/null
```

- [ ] **Step 2: Build JS + CSS**

```bash
npm run build:all
dotnet build Alis.Reactive.slnx -nologo
```

- [ ] **Step 3: Run Playwright**

```bash
dotnet test tests/Alis.Reactive.PlaywrightTests/Alis.Reactive.PlaywrightTests.csproj -nologo --logger "console;verbosity=detailed"
```

Expected: >= 825 passed. Behavior unchanged — same user-visible output.

### Task 22: Verify NRT warnings resolved

```bash
dotnet build Alis.Reactive/Alis.Reactive.csproj -nologo 2>&1 | grep -c "CS86"
```

Expected: significantly fewer than 120. Goal is zero NRT warnings on plan model files.

---

## Review: G11 Final Regression Review

- [ ] **Dispatch Codex xhigh** for final regression review on the complete branch diff

Input: `git diff gettingclose...fix/null-design-smell`

Review criteria:
1. Zero `null` stored for "not specified" in plan model properties
2. All domain defaults flow through to JSON
3. All TS types match schema (required fields aligned)
4. All TS runtime guards removed for plan model properties
5. Genuinely nullable properties have per-property [JsonIgnore(WhenWritingNull)]
6. No regressions — test counts >= baseline
7. No dirt left behind — no pattern that teaches the next session the wrong way

- [ ] **Data flow diagram — final verification**

Trace THREE flows end-to-end through the AFTER diagram:
1. A ReadProducer with no shape specified
2. A CompareCondition with unary operator (no right operand)
3. A Request with no headers and no before handlers

Each must show: C# domain default → JSON with default → TS required field → TS runtime direct access.

- [ ] **Codex xhigh FINAL SIGN-OFF** — unconditional, zero BLOCK findings
- [ ] If BLOCK: fix → full test suite → re-review → repeat until SIGN-OFF

---

## Outcome Verification Checklist

| Criterion | Evidence | Status |
|-----------|----------|--------|
| Zero `IsNone ? null` in plan model | `rg` returns 0 | |
| Zero `Count > 0 ? : null` in plan model | `rg` returns 0 | |
| Zero global WhenWritingNull | `rg` returns 0 | |
| NRT warnings on plan model files = 0 | `dotnet build` grep CS86 | |
| All unit tests pass (>= 334) | dotnet test output | |
| All Playwright tests pass (>= 825) | Playwright output | |
| TypeScript typecheck clean | `npm run typecheck` | |
| JS bundle builds | `npm run build` | |
| Schema valid | AssertSchemaValid tests pass | |
| TS types match schema | Manual verification | |
| Codex xhigh SIGN-OFF | R4 review | |
