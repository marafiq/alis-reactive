# Descriptor Encapsulation Review

**Scope:** Every descriptor class in `Alis.Reactive/Descriptors/`
**Goal:** ONE consistent encapsulation strategy across all descriptors
**Triggered by:** [command-guard-mutability-design-issues.md](command-guard-mutability-design-issues.md)

---

## Descriptor Graph (Verified Against Source)

Every class below was verified to exist. Mutability annotations reflect actual property declarations.

```
Entry { get; } ✅
├── Trigger (abstract, polymorphic)
│   ├── DomReadyTrigger           ✅ no state
│   ├── CustomEventTrigger        ✅ { get; }
│   ├── ComponentEventTrigger     ✅ { get; }
│   ├── ServerPushTrigger         ✅ { get; }
│   └── SignalRTrigger            ✅ { get; }
│
├── Reaction (abstract, polymorphic)
│   ├── SequentialReaction        ⚠️  List<Command> — mutable
│   ├── ConditionalReaction       ⚠️  List<Command>? — mutable | IReadOnlyList<Branch> — ok
│   ├── HttpReaction              ⚠️  List<Command>? PreFetch — mutable
│   │   └── RequestDescriptor     ❌ private set + 3 mutation methods (see Issue 1)
│   │       ├── List<GatherItem>?     ⚠️  mutable
│   │       ├── List<Command>?        ⚠️  mutable (WhileLoading)
│   │       ├── List<StatusHandler>?  ⚠️  mutable (OnSuccess, OnError)
│   │       │   └── StatusHandler     ⚠️  List<Command>? — mutable
│   │       └── ValidationDescriptor? ❌ { get; private set; } + EnrichValidation()
│   │           └── ValidationField   ❌ { get; internal set; } × 3 (FieldId, Vendor, ReadExpr)
│   └── ParallelHttpReaction      ⚠️  3 mutable lists
│
└── Command (abstract, polymorphic)
    ├── Guard? When               ❌ { get; private set; } + GuardWith()
    ├── DispatchCommand           ✅ { get; }
    ├── MutateElementCommand      ✅ { get; }
    │   ├── Mutation (abstract)
    │   │   ├── SetPropMutation   ✅ { get; }
    │   │   └── CallMutation      ✅ { get; } | MethodArg[]? Args — mutable array
    │   │       └── MethodArg[]
    │   │           ├── LiteralArg    ✅ { get; }
    │   │           └── SourceArg     ✅ { get; }
    │   └── BindSource? (abstract)
    │       ├── EventSource       ✅ { get; }
    │       └── ComponentSource   ✅ { get; }
    ├── MutateEventCommand        ✅ { get; }
    ├── IntoCommand               ✅ { get; }
    └── ValidationErrorsCommand   ✅ { get; }

Guard (abstract, polymorphic)
├── ValueGuard                    ✅ { get; }
├── InvertGuard                   ✅ { get; }
├── AllGuard                      ✅ IReadOnlyList<Guard>
├── AnyGuard                      ✅ IReadOnlyList<Guard>
└── ConfirmGuard                  ✅ { get; }

GatherItem (abstract, polymorphic)
├── StaticGather                  ✅ { get; }
├── EventGather                   ✅ { get; }
├── ComponentGather               ✅ { get; }
└── AllGather                     ✅ no state
```

**Legend:** ✅ immutable | ⚠️ mutable list | ❌ post-construction mutation

**Verified count:** 34 descriptor classes across all projects. 26 already immutable. 8 need work.

---

## Issue 1: Post-Construction Mutation Methods

**7 mutation methods across 3 classes.** Every call site traced with file:line evidence.

### Command.GuardWith() — 1 call site

```
ElementBuilder.cs:149 → Command.GuardWith(guard)
```

```csharp
// ElementBuilder.cs:148-150
if (_pipeline.Commands.Count > 0)
    _pipeline.Commands[_pipeline.Commands.Count - 1].GuardWith(gb.Guard);
```

**Pattern:** Developer writes `Element("x").AddClass("a").When(...)` — the fluent `.When()` is
called AFTER `.AddClass()` creates and adds the command. GuardWith() then mutates the last command.

**Why it exists:** The fluent API makes it impossible to know the guard BEFORE creating the command.
The developer decides to add `.When()` after the element operation, not before it.

**Constraint:** One guard per command. Throws `InvalidOperationException` if already guarded.
(`Command.cs:16-18`)

### RequestDescriptor — 2 call sites

```
HttpRequestBuilder.cs:127 → RequestDescriptor.AttachValidator(validatorType)
ValidationResolver.cs:96  → RequestDescriptor.EnrichValidation(extracted)
```

**AttachValidator** — called immediately after construction in `BuildRequestDescriptor()`:

```csharp
// HttpRequestBuilder.cs:115-127
var desc = new RequestDescriptor(
    _verb, _url, _gather, _contentType,
    _whileLoading, ..., _validation);    // ← constructor already has validation param

if (_validatorType != null)
    desc.AttachValidator(_validatorType);  // stamps Type for later extraction
```

**EnrichValidation** — called at render time in `ReactivePlan.Render() → ResolveAll()`:

```csharp
// ValidationResolver.cs:88-96
if (req.ValidatorType != null && req.Validation != null)
{
    var extracted = extractor.ExtractRules(req.ValidatorType, formId);
    req.EnrichValidation(extracted);  // replaces placeholder with resolved rules
}
```

### ValidationField — 3 mutable properties (render-time enrichment)

```
ValidationResolver.cs:152-154 → field.FieldId, field.Vendor, field.ReadExpr
```

```csharp
// ValidationResolver.cs:152-154
field.FieldId = registration.ComponentId;   // { get; internal set; }
field.Vendor = registration.Vendor;         // { get; internal set; }
field.ReadExpr = registration.ReadExpr;     // { get; internal set; }
```

### ValidationDescriptor.PlanId — 1 mutable property

```
ValidationResolver.cs:185 → desc.Validation.PlanId = planId
```

**Why these render-time mutations exist:** The `IValidationExtractor` and `ComponentsMap` are NOT
available during builder construction. They are resolved globally during `plan.Render()`.
This is an architectural constraint — **these mutations cannot move to builder time.**

---

## Issue 2: Mutable List Properties (6 classes, 11 properties)

| Class | Property | Type | How it's passed to constructor |
|-------|----------|------|-------------------------------|
| `SequentialReaction` | `Commands` | `List<Command>` | Copied via `new List<Command>(Commands)` (PipelineBuilder.cs:120,161) or direct ref (line 185) |
| `ConditionalReaction` | `Commands` | `List<Command>?` | Usually `null`, or direct ref in final segment (PipelineBuilder.cs:182) |
| `ConditionalReaction` | `Branches` | `IReadOnlyList<Branch>` | Already frozen via `.ToArray()` ✅ |
| `HttpReaction` | `PreFetch` | `List<Command>?` | Direct ref from builder (PipelineBuilder.cs:179) |
| `ParallelHttpReaction` | `PreFetch` | `List<Command>?` | Direct ref (ParallelBuilder.cs:43) |
| `ParallelHttpReaction` | `Requests` | `List<RequestDescriptor>` | Direct ref (ParallelBuilder.cs:44) |
| `ParallelHttpReaction` | `OnAllSettled` | `List<Command>?` | Direct ref (ParallelBuilder.cs:45) |
| `RequestDescriptor` | `Gather` | `List<GatherItem>?` | Direct ref from `_gather` (HttpRequestBuilder.cs:117) |
| `RequestDescriptor` | `WhileLoading` | `List<Command>?` | Direct ref (HttpRequestBuilder.cs:118) |
| `RequestDescriptor` | `OnSuccess` | `List<StatusHandler>?` | Direct ref (HttpRequestBuilder.cs:121) |
| `RequestDescriptor` | `OnError` | `List<StatusHandler>?` | Direct ref (HttpRequestBuilder.cs:122) |
| `StatusHandler` | `Commands` | `List<Command>?` | Direct ref (StatusHandler.cs:20,27) |

**Key finding:** Zero post-construction `.Add()` calls found on any descriptor list property.
All mutations happen during builder phase, before the descriptor is constructed. The lists are
**de facto immutable** but **not enforced**.

**Danger:** Direct references mean the builder's list IS the descriptor's list. If anyone mutates
the builder list after construction, the descriptor silently changes. Not exploited today, but
unprotected.

---

## Issue 3: CallMutation.Args — Mutable Array

`MethodArg[]? Args { get; }` — arrays are reference types. Currently safe because args are
always constructed inline (`new MethodArg[] { ... }`) in ElementBuilder and never stored in
a variable. But the pattern is fragile.

---

## What Can Be Fixed vs. What Cannot

### CAN fix: List freezing (Phase 1)

**All 11 mutable list properties** can be wrapped in `IReadOnlyList<T>` at construction.

**Evidence this is safe:**
- `IReadOnlyList<T>` already used in 3 places: `AllGuard.Guards`, `AnyGuard.Guards`,
  `ConditionalReaction.Branches` — all serialize correctly
- Plan is **write-only** — `WriteOnlyPolymorphicConverter.Read()` throws
  `NotSupportedException` (Serialization/WriteOnlyPolymorphicConverter.cs:13).
  Zero deserialization anywhere. Constructor changes can't break deserialization.
- `System.Text.Json` serializes `IReadOnlyList<T>` identically to `List<T>`
- Verified by existing snapshots (e.g., `WhenBranchingOnConditions.And_mixed_types.verified.txt`
  shows `IReadOnlyList<Guard>` serializing as a JSON array)

**CRITICAL: Must use `.ToList().AsReadOnly()`, NOT `.AsReadOnly()` alone.**

`.AsReadOnly()` returns a **wrapper** around the original list — mutations to the original
leak through. `.ToList().AsReadOnly()` or `.ToArray()` creates a **snapshot** that is truly
independent. Example:

```csharp
// WRONG — wrapper, not a copy. Mutations to 'commands' leak through.
public SequentialReaction(List<Command> commands)
{
    Commands = commands.AsReadOnly();  // ❌ wrapper around mutable list
}

// RIGHT — snapshot. Original list can be mutated without affecting descriptor.
public SequentialReaction(List<Command> commands)
{
    Commands = commands.ToArray();  // ✅ immutable copy
}
```

**Note:** PipelineBuilder.cs:120,161 already copies via `new List<Command>(Commands)` before
passing to SequentialReaction. But other paths (lines 179, 185) pass direct references.
Freezing inside the descriptor constructor protects all paths uniformly.

### CAN fix: CallMutation.Args array (Phase 1)

Same approach — wrap in `IReadOnlyList<MethodArg>` via `.ToArray()` in constructor.

### CANNOT simply fix: Command.GuardWith() (Phase 2 — needs API redesign)

The original document proposed moving the guard to the Command constructor. **This breaks the
fluent API.**

**Why:** `ElementBuilder.When()` is a fluent method chained AFTER the command-creating method:

```csharp
// Developer writes:
p.Element("status")
    .AddClass("active")       // ← creates MutateElementCommand, adds to pipeline
    .When(payload, x => x.Ok) // ← AFTER command exists, mutates its guard
    .Eq(true)
    .RemoveClass("loading");  // ← next command, no guard
```

The guard doesn't exist when `AddClass()` is called. It's decided by the developer at fluent
chain time. You cannot pass `when: pendingGuard` to the command constructor because
`pendingGuard` doesn't exist yet.

**Additional complexity:** `.When()` guards only the LAST command, not all commands in a chain.
If the developer chains `AddClass("a").RemoveClass("b").When(...)`, only `RemoveClass` gets
the guard. This is intentional (per-action guards).

**Possible approaches (each requires API redesign):**

| Approach | How | Tradeoff |
|----------|-----|----------|
| **Pending guard on PipelineBuilder** | `_pipeline.PendingGuard = guard` set by `When()`, consumed by next `Add*` call | Breaks current: guard applies to PREVIOUS command, not next |
| **Replace-last pattern** | `When()` removes the last command, re-creates it with guard | Breaks: must clone all Command subclass constructors |
| **Accept controlled mutation** | Keep `GuardWith()` as-is, document the one-time mutation window | Pragmatic: already works, tested, single call site |
| **Init-only property** | `public Guard? When { get; init; }` + init block in ElementBuilder | Requires C# 9+, still mutation after `new`, just syntactically different |

**Recommendation:** Accept controlled mutation (Option 3). The `GuardWith()` pattern has a single
call site, throw-on-duplicate protection, and is internal. The temporal coupling is real but
narrow and tested. Eliminating it requires an API redesign that would touch every builder and
every test — disproportionate cost for the encapsulation gain.

### CANNOT fix: Validation enrichment (Phase 3 — architectural constraint)

The original document proposed a factory method to set validation at construction time.
**This is impossible.**

**Evidence:**

1. **RequestDescriptor constructor already accepts `ValidationDescriptor? validation`**
   (RequestDescriptor.cs:60). The factory method proposal solves a problem that doesn't exist
   in the form described.

2. **Validation enrichment happens at render time because it MUST:**
   - `IValidationExtractor` is registered globally via `ReactivePlanConfig.Extractor`
     (ReactivePlan.cs:100) — not available to builders
   - `ComponentsMap` is populated during builder phase but consumed during render phase
     (ValidationResolver.cs:152-154 enriches `FieldId`, `Vendor`, `ReadExpr` from ComponentsMap)
   - Rule extraction requires the fully-populated ComponentsMap to resolve field IDs to components

3. **The lifecycle is:**
   ```
   Builder phase:  HttpRequestBuilder.Validate<T>(formId)
                   → creates empty ValidationDescriptor(formId, [])
                   → stores ValidatorType via AttachValidator()

   Render phase:   plan.Render() → ResolveAll()
                   → ValidationResolver.Resolve() extracts rules from ValidatorType
                   → EnrichValidation() replaces empty descriptor with resolved rules
                   → EnrichFieldsFromComponents() stamps FieldId/Vendor/ReadExpr
                   → StampPlanId() sets PlanId
   ```

   Moving rule extraction to builder time would require passing `IValidationExtractor` through
   every builder in the chain — a major API change that breaks all existing HTTP pipeline usage.

**Recommendation:** Accept render-time enrichment as intentional. Document the two-phase lifecycle.
The mutations are `internal`, have clear ownership (ValidationResolver), and cannot be triggered
by framework consumers.

---

## Revised Implementation Plan

### Phase 1: List Freezing + Array Freezing (Low Risk)

Wrap all mutable `List<T>` properties in `IReadOnlyList<T>` using `.ToArray()` at construction.
Convert `MethodArg[]?` to `IReadOnlyList<MethodArg>?`.

**Files to change (7):**

| File | Change |
|------|--------|
| `SequentialReaction.cs` | `List<Command>` → `IReadOnlyList<Command>`, constructor: `.ToArray()` |
| `ConditionalReaction.cs` | `List<Command>?` → `IReadOnlyList<Command>?`, constructor: `?.ToArray()` |
| `HttpReaction.cs` | `List<Command>?` → `IReadOnlyList<Command>?`, constructor: `?.ToArray()` |
| `ParallelHttpReaction.cs` | 3 list properties → `IReadOnlyList`, constructor: `.ToArray()` each |
| `RequestDescriptor.cs` | 4 list properties → `IReadOnlyList`, constructor: `?.ToArray()` each |
| `StatusHandler.cs` | `List<Command>?` → `IReadOnlyList<Command>?`, constructor: `?.ToArray()` |
| `CallMutation.cs` | `MethodArg[]?` → `IReadOnlyList<MethodArg>?`, constructor: `?.ToArray()` |

**Why `.ToArray()` not `.AsReadOnly()`:**
- `.AsReadOnly()` wraps the original list — mutations leak through the wrapper
- `.ToArray()` creates an independent snapshot — truly immutable
- Both serialize identically in System.Text.Json

**Risk:** Low.
- JSON output is byte-for-byte identical (proven by AllGuard/AnyGuard/ConditionalReaction.Branches)
- No deserialization exists (WriteOnlyPolymorphicConverter enforces this)
- All 1,700+ snapshot tests validate shape preservation

**What could break:**
- Any internal code that casts the property back to `List<T>` and calls `.Add()` — search found zero occurrences
- Any code that depends on `typeof(property) == typeof(List<T>)` — search found zero occurrences

### Phase 2: Document Controlled Mutations (No Code Change)

Instead of eliminating `GuardWith()` and validation enrichment (which would break the fluent API
and validation architecture), document them as intentional controlled mutations:

**Add XML doc comments to:**
- `Command.GuardWith()` — "Called once by ElementBuilder.When() during builder phase. Throws if already guarded."
- `RequestDescriptor.AttachValidator()` — "Called by HttpRequestBuilder.BuildRequestDescriptor() immediately after construction."
- `RequestDescriptor.EnrichValidation()` — "Called by ValidationResolver during plan.Render(). Replaces placeholder with extracted rules."
- `ValidationField.FieldId/Vendor/ReadExpr` — "Enriched by ValidationResolver.EnrichFieldsFromComponents() during plan.Render()."
- `ValidationDescriptor.PlanId` — "Stamped by ValidationResolver.StampPlanId() during plan.Render()."

**Rationale:** These mutations are:
- All `internal` — invisible to framework consumers
- All single-write (never mutated twice)
- All have clear ownership (one caller each)
- All tested at 3 layers

Documenting them as intentional is honest. Pretending they can be eliminated is not.

---

## Verification

After Phase 1:
1. `dotnet test tests/Alis.Reactive.UnitTests` — all snapshot + schema tests unchanged
2. `dotnet test tests/Alis.Reactive.Native.UnitTests` — no regressions
3. `dotnet test tests/Alis.Reactive.Fusion.UnitTests` — no regressions
4. `dotnet test tests/Alis.Reactive.FluentValidator.UnitTests` — no regressions
5. `npm test` — TS tests unaffected (they test runtime, not C# serialization)
6. `dotnet test tests/Alis.Reactive.PlaywrightTests` — browser behavior unchanged

**Total: ~1,700 tests. Zero should break.**

---

## Summary

| Metric | Before | After Phase 1 |
|--------|--------|---------------|
| Descriptors with immutable properties | 26/34 | 26/34 (unchanged — same classes) |
| Mutable list properties | 11 across 6 classes | 0 (all `IReadOnlyList` via `.ToArray()`) |
| Mutable arrays | 1 (CallMutation.Args) | 0 (`IReadOnlyList<MethodArg>`) |
| Post-construction mutations | 7 methods across 3 classes | 7 (documented, not eliminated) |
| Plan JSON shape | — | Unchanged (verified by snapshots) |

### What we're NOT doing (and why)

| Original proposal | Why we're not doing it |
|-------------------|----------------------|
| Move guard to Command constructor | Breaks fluent API — `.When()` is chained AFTER command creation |
| RequestDescriptor factory method | Constructor already has validation param; enrichment must happen at render-time |
| Eliminate all post-construction mutations | Validation architecture requires render-time resolution (extractor + ComponentsMap unavailable at builder time) |
| `.AsReadOnly()` wrapper | Creates mutation backdoor — original list mutations leak through wrapper |
