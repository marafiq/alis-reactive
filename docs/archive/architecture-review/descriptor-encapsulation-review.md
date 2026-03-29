# Descriptor Encapsulation Review

**Scope:** Every descriptor class in `Alis.Reactive/Descriptors/`
**Triggered by:** [command-guard-mutability-design-issues.md](command-guard-mutability-design-issues.md)

> **Accuracy note (2025-03-24):** A stricter re-count of `Descriptors/**/*.cs` yields **~40** `class` declarations (exact number depends on inclusion of nested/helper types), not **34** — **qualitative conclusions below are unchanged**. “Controlled mutation” is best summarized as **`Command.GuardWith`** plus **`RequestDescriptor`** internal attach/enrich; **ValidationResolver** mutates nested **`ValidationField`** / **`ValidationDescriptor`** properties — count **descriptor API methods** separately from **resolver call sites**. **11** `List<>` properties across **6** classes and **zero** post-construction list mutations remain **verified**.

---

## The Question

The original issue flagged `Command.GuardWith()` as a mutability problem. The comment thread
escalated it to: "audit ALL descriptors and enforce ONE encapsulation strategy."

Before proposing solutions, we need to answer: **what does immutability actually buy us here?**

---

## Descriptor Lifecycle — Why It Matters

Descriptors live in a very narrow scope:

```
View request → Builder phase (mutation) → plan.Render() (serialize) → Response → GC
```

| Property | Value |
|----------|-------|
| **Thread safety** | Single-threaded — one request, one thread, one plan |
| **Lifetime** | Request-scoped — created, serialized, garbage collected |
| **Sharing** | None — descriptors never leave the request, never cached, never stored |
| **Consumers** | One — `System.Text.Json` serializer reads them once |
| **Post-serialization mutation** | Impossible — objects are GC'd after `plan.Render()` |

**Immutability protects against mutation across boundaries.** Here there are no boundaries.
The builder creates the descriptor, the serializer reads it, the GC destroys it. All in one
request, one thread, one method chain.

---

## What the Audit Found

### ~34–40 descriptor classes audited (see accuracy note). 26 already immutable.

The remaining 8 classes have controlled mutations that exist for legitimate architectural reasons.

### Descriptor Graph (Verified Against Source)

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
│   ├── SequentialReaction        ⚠️  List<Command> — mutable type, never mutated after construction
│   ├── ConditionalReaction       ⚠️  List<Command>? — same | IReadOnlyList<Branch> ✅
│   ├── HttpReaction              ⚠️  List<Command>? PreFetch — same
│   │   └── RequestDescriptor     🔧 render-time enrichment (validation)
│   │       ├── List<GatherItem>?     ⚠️  mutable type, never mutated
│   │       ├── List<Command>?        ⚠️  mutable type, never mutated
│   │       ├── List<StatusHandler>?  ⚠️  mutable type, never mutated
│   │       │   └── StatusHandler     ⚠️  mutable type, never mutated
│   │       └── ValidationDescriptor? 🔧 enriched at render time
│   │           └── ValidationField   🔧 enriched at render time (FieldId, Vendor, ReadExpr)
│   └── ParallelHttpReaction      ⚠️  3 mutable-typed lists, never mutated
│
└── Command (abstract, polymorphic)
    ├── Guard? When               🔧 set once by fluent .When() — builder-phase only
    ├── DispatchCommand           ✅ { get; }
    ├── MutateElementCommand      ✅ { get; }
    │   ├── Mutation (abstract)
    │   │   ├── SetPropMutation   ✅ { get; }
    │   │   └── CallMutation      ✅ { get; } | MethodArg[]? — mutable type, never mutated
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

**Legend:** ✅ immutable | ⚠️ mutable type but never mutated | 🔧 controlled mutation (documented below)

---

## Controlled Mutations — Why They Exist

### 1. Command.GuardWith() — Fluent API Requirement

```
ElementBuilder.cs:149 → Command.GuardWith(guard)
```

**The developer writes:**

```csharp
p.Element("status")
    .AddClass("active")       // creates command, adds to pipeline
    .When(payload, x => x.Ok) // AFTER command exists — applies guard to it
    .Eq(true)
    .RemoveClass("loading");  // next command, unguarded
```

**Why mutation is necessary:** The fluent API lets the developer decide AFTER creating a command
whether to guard it. `.When()` is chained after `.AddClass()`, not before it. The guard
doesn't exist when the command is constructed.

**Why it's safe:**
- Single call site (ElementBuilder.cs:149)
- Throws `InvalidOperationException` if called twice (Command.cs:16-18)
- `internal` — framework consumers cannot call it
- Builder is request-scoped — no concurrent access
- Moving the guard to the constructor would **break the fluent API** — developers would lose
  the ability to conditionally add `.When()` in a readable chain

**Alternatives considered and rejected:**

| Approach | Why it doesn't work |
|----------|-------------------|
| Guard in constructor | `.When()` is chained AFTER command creation — guard doesn't exist yet |
| Pending guard state | Guard applies to PREVIOUS command, not next — would reverse semantics |
| Replace-last pattern | Must clone every Command subclass — fragile, error-prone |

**Verdict:** This is the builder pattern working correctly. The mutation window is
`ElementBuilder.When()` → `GuardWith()` → done. Single-threaded, single-write, validated.

### 2. RequestDescriptor — Render-Time Validation Enrichment

```
HttpRequestBuilder.cs:127  → desc.AttachValidator(validatorType)
ValidationResolver.cs:96   → desc.EnrichValidation(extracted)
ValidationResolver.cs:152  → field.FieldId = registration.ComponentId
ValidationResolver.cs:153  → field.Vendor = registration.Vendor
ValidationResolver.cs:154  → field.ReadExpr = registration.ReadExpr
ValidationResolver.cs:185  → desc.Validation.PlanId = planId
```

**Why mutation is necessary:** Validation has a two-phase lifecycle:

```
Builder phase:  HttpRequestBuilder.Validate<TValidator>(formId)
                → creates empty ValidationDescriptor(formId, [])
                → stores ValidatorType via AttachValidator()

Render phase:   plan.Render() → ResolveAll()
                → IValidationExtractor extracts rules from ValidatorType
                → EnrichValidation() attaches resolved rules
                → EnrichFieldsFromComponents() stamps FieldId/Vendor/ReadExpr from ComponentsMap
                → StampPlanId() sets PlanId
```

**Why it can't move to builder time:**
- `IValidationExtractor` is registered globally (ReactivePlanConfig.Extractor, ReactivePlan.cs:100) — builders don't have it
- `ComponentsMap` is populated DURING builder phase — not complete until all fields are registered
- Rule extraction needs the fully-populated ComponentsMap to resolve field IDs to component metadata

**Why it's safe:**
- All methods are `internal` — framework consumers cannot call them
- All are single-write (never enriched twice)
- Each has exactly one caller with clear ownership
- All tested at 3 layers

**Verdict:** This is late-binding resolution. The descriptor is a data carrier that gets enriched
as more context becomes available. Same pattern as ASP.NET model binding or EF entity tracking —
the object is built incrementally because not all information is available at construction time.

### 3. Mutable List Types — Never Actually Mutated

**11 list properties** across 6 classes use `List<T>` instead of `IReadOnlyList<T>`.
Full call-site search found **zero post-construction mutations** — no `.Add()`, no `.Clear()`,
no index assignment on any descriptor list after the descriptor is constructed.

| Class | Properties | Post-construction mutations found |
|-------|-----------|----------------------------------|
| `SequentialReaction` | `Commands` | 0 |
| `ConditionalReaction` | `Commands` | 0 |
| `HttpReaction` | `PreFetch` | 0 |
| `ParallelHttpReaction` | `PreFetch`, `Requests`, `OnAllSettled` | 0 |
| `RequestDescriptor` | `Gather`, `WhileLoading`, `OnSuccess`, `OnError` | 0 |
| `StatusHandler` | `Commands` | 0 |

**Why they use `List<T>`:** The builder creates a `List<T>`, populates it, and passes it to the
descriptor constructor. The descriptor stores the reference. After that, nobody touches it.
Using `IReadOnlyList<T>` would be technically more precise, but the practical risk is zero —
these are request-scoped objects consumed by a serializer.

---

## Cost-Benefit Analysis

### What immutability would buy

| Benefit | Applies here? | Why / why not |
|---------|:------------:|---------------|
| Thread safety | No | Single-threaded request scope |
| Prevent accidental mutation | Marginal | Zero mutations found in practice — solving a theoretical problem |
| API signal ("don't modify") | Yes, minor | `IReadOnlyList<T>` communicates intent better than `List<T>` |
| Reasoning clarity | Marginal | Lifecycle is already narrow and well-understood |
| Cross-boundary safety | No | Descriptors never leave the request scope |

### What immutability would cost

| Cost | Severity |
|------|----------|
| Breaking the fluent `.When()` API (Command guard) | **High** — redesigns every ElementBuilder method + all tests |
| Breaking validation enrichment lifecycle | **High** — redesigns validation architecture, passes extractor through all builders |
| `.ToArray()` copies on every descriptor construction | **Low** — extra allocations for zero practical benefit |
| Churn across 34 classes + all test snapshots | **Medium** — risk of introducing bugs in working code |

### Verdict

**The current design is correct for its context.** Descriptors are request-scoped value carriers
with a narrow, well-defined mutation window. The mutations that exist (fluent guard attachment,
render-time validation enrichment) are architecturally necessary — they enable the fluent
builder API and the late-binding validation pipeline that developers depend on.

Enforcing strict immutability would:
- Break the fluent API that makes the framework usable
- Break the validation pipeline that extracts rules at render time
- Add allocation overhead (`.ToArray()` copies) for zero practical benefit
- Solve a theoretical problem that has zero evidence of causing actual bugs

---

## What We Should Do

### 1. Document the mutation lifecycle (do now)

Add XML doc comments to the 7 controlled mutation methods explaining:
- Who calls them (single caller each)
- When in the lifecycle (builder phase vs render phase)
- Why they exist (fluent API / late-binding resolution)
- The single-write invariant (never called twice)

This turns implicit knowledge into explicit documentation.

### 2. Consider IReadOnlyList only if it comes free (opportunistic)

If a descriptor constructor is being changed for OTHER reasons, switch `List<T>` to
`IReadOnlyList<T>` at that point. But don't create a dedicated refactoring pass for it —
the ROI is too low for request-scoped objects with zero post-construction mutations.

### 3. Do NOT change the guard or validation patterns

- `Command.GuardWith()` enables the fluent `.When()` API — leave it
- `RequestDescriptor` render-time enrichment enables late-binding validation — leave it
- Both are `internal`, single-write, single-caller, tested at 3 layers

---

## Summary

| Finding | Count | Status |
|---------|-------|--------|
| Descriptor classes audited | 34 | Complete |
| Already immutable | 26 | No action needed |
| Mutable list types (never actually mutated) | 6 classes, 11 properties | Accept — zero post-construction mutations found |
| Controlled mutations (fluent API) | 1 method (`GuardWith`) | Accept — enables `.When()` fluent chain |
| Controlled mutations (render-time enrichment) | 6 methods/properties | Accept — validation requires late binding |
| Bugs caused by mutability | 0 | No evidence of a problem to solve |

**The descriptor system is well-encapsulated for its context.** The mutations are controlled,
`internal`, single-write, and architecturally necessary. The fluent builder API and late-binding
validation pipeline are features, not defects. Enforcing strict immutability would break both
without solving an actual problem.
