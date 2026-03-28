---
name: solid-ts-research
description: SOLID principles research from Uncle Bob, Fowler, TanStack — applied to TS module design for Alis.Reactive runtime audit
type: reference
---

# SOLID TypeScript Research — Credible Sources Only

## Sources Used
- **Robert C. Martin (Uncle Bob)** — blog.cleancoder.com, Clean Architecture (2017), Functional Design (2023)
- **Martin Fowler** — martinfowler.com, Refactoring 2nd Ed (2018), code smells catalog
- **TanStack** — Query, Router, Table source code patterns
- **TypeScript Handbook** — syntax only, NOT design authority
- **NO Google style guide** — user explicitly excluded

## Uncle Bob — SOLID for Modules (Not Just Classes)

### SRP — "One Actor" NOT "One Thing"
- "A module should be responsible to one, and only one, actor."
- "Gather together the things that change for the same reasons. Separate those things that change for different reasons."
- Ask "WHO would request changes to this module?" not "does it do one thing?"
- From *Functional Design* (2023): SOLID applies to Clojure (functional, no classes) — confirms module-level applicability

### OCP — Open for Extension, Closed for Modification
- "A Module should be open for extension but closed for modification."
- Anti-pattern: switch statements that must change when new types added
- Fix: dispatch tables/maps, plugin architecture
- Protection hierarchy: highest-level policy = most protected from change

### LSP — Subtypes Must Be Substitutable
- "Not about inheritance — it is about sub-typing. All duck-types are subtypes of an implied interface."
- Violation symptom: `if (vendor === "specific") { ... }` checks downstream of abstraction
- Our architecture already correct: resolveRoot returns substitutable roots

### ISP — No Client Should Depend on Methods It Doesn't Use
- Keep module exports narrow
- If module exports 10 functions but caller uses 1, changes to other 9 cause unnecessary coupling
- In ESM: larger bundle invalidation from fat exports

### DIP — Depend Toward Abstraction
- "Source code dependencies can only point inwards" (Dependency Rule)
- Pure utilities (walk.ts, resolver.ts) = most stable, everything depends inward on them
- Vendor-aware (component.ts) = volatile detail, should be depended ON not depend outward

## Fowler — Code Smells as Diagnostic Tools

### Most Relevant Smells for Our Runtime Audit

1. **Divergent Change** — module changes for multiple unrelated reasons → Split Phase, Extract Module
2. **Shotgun Surgery** — one logical change touches many files → Move Function, consolidate
3. **Feature Envy** — function uses more of another module's data than its own → Move Function
4. **Insider Trading** — modules knowing each other's internals, circular imports → Hide Delegate
5. **Repeated Switches** — same switch on kind/type in multiple places → dispatch map
6. **Data Clumps** — same params travel together (vendor, readExpr, componentId) → Introduce Parameter Object
7. **Long Function** — phases separated by comments → Extract Function per phase
8. **Global Data** — module-level mutable state written by multiple functions → Encapsulate Variable

### Diagnostic Questions
- "How many files to add a new command kind?" — if >2, Shotgun Surgery
- "Does this module change for more than one reason?" — Divergent Change
- "Does this function use more imports than local symbols?" — Feature Envy
- "Do these 3+ params always travel together?" — Data Clumps
- "Does this switch appear in more than one file?" — Repeated Switches

## TanStack — Real-World SOLID TS Patterns

### Single Dispatch Is Fine
- TanStack Query uses `switch (action.type)` in ONE reducer — same as our `executeCommand()`
- Goal: "one switch in one place", NOT "zero switches"

### Feature Registration Array (TableFeature[])
- Each feature implements lifecycle hooks (getDefaultOptions, createTable, createColumn)
- Adding feature = push to array, ZERO changes to createTable()
- Maps to our component vertical slices

### Behavior Injection (QueryBehavior)
- Infinite queries extend base via behavior object, not class modification
- `context.fetchFn = customFetchFunction` — override without touching base

### Module Boundary Patterns
- Single barrel export (index.ts)
- `#` private for true private, `/** @internal */` for inter-module-only
- Keep exports narrow and intentional

### Context Passing
- Cumulative context via intersection: `Assign<Parent, Own>`
- Lazy property evaluation via Object.defineProperty (track consumption)
- Immutable context creation: `{ ...parent, ...own } satisfies ContextType`

## Kent Beck — "Tidy First?" (Coupling/Decoupling Chapters 29-33)

### Constantine's Equivalence (The Golden Chain)
```
cost(software) ~= cost(change) ~= cost(big changes) ~= coupling
```
Therefore: **cost(software) ~= coupling**

### Coupling Is Change-Relative
- `coupled(E1, E2, Δ) ≡ ΔE1 → ΔE2` — coupled WITH RESPECT TO a specific change
- If two elements are coupled with respect to a change that never happens, it doesn't matter
- "Analyzing coupling requires knowing what changes have happened and/or are likely to happen"
- Practical test: "see which pairs of files tend to show up together in commits"

### Two Dangerous Properties of Coupling
1. **1-N** — one element coupled to many others (mitigable with tooling/automated refactoring)
2. **Cascading** — change ripples trigger more changes (the real cost driver, power law distribution)

### Coupling vs Decoupling Trade-Off
- "The more you reduce coupling for one class of changes, the greater the coupling becomes for other classes of changes"
- Don't try to squeeze out every last bit of coupling — the decoupling cost eventually exceeds the benefit
- Trade-off curve: coupling cost rises with more coupling, decoupling cost rises with more decoupling
- "You're faced with a choice today: pay the cost of coupling or pay the cost of decoupling"

### Cohesion
- "Coupled elements should be subelements of the same containing element"
- Two fixes for incohesive modules: (1) extract coupled group INTO submodule, (2) move uncoupled elements OUT
- "Make no sudden moves... Move one element at a time"
- Helper function extraction = "extract a cohesive subelement"

### The Four Tidying Forces
1. **Cost** — Will tidying make costs smaller, later, or less likely?
2. **Revenue** — Will tidying make revenue larger, sooner, or more likely?
3. **Coupling** — Will tidying reduce the number of elements that must change?
4. **Cohesion** — Will tidying concentrate changes into a smaller scope?

### Key Wisdom
- "Tidy to enable the next behavior change. Save the tidying binge for later."
- "Some coupling is just inevitable"
- Software is NOT a thing that is "made" — its value emerges from ongoing change

## Async vs Sync — Four Recognized Patterns

| Pattern | Used by | Duplication | Perf overhead | Complexity |
|---------|---------|-------------|---------------|------------|
| **Unified async** (always await) | Koa, TanStack Router | Zero | 1 microtick/await (negligible for UI) | Low |
| **Sync core, async pushed out** | Redux, Zustand | Zero | Zero | Requires architectural separation |
| **PromiseOrValue** (isPromise branching) | GraphQL-JS | Zero | Zero for sync | High — .then() chains in loops |
| **Dual path** (our current approach) | Cloudflare streams | ~40 lines | Zero for sync | Low — both readable |

### Key Findings
- V8 v7.2+: awaiting non-Promise costs 1 microtick (was 3), sub-microsecond
- For UI framework (handful of DOM mutations), unified async overhead is unmeasurable
- GraphQL-JS PromiseOrValue is most applicable but sequential branch evaluation is hard to express with .then()
- Bob Nystrom "What Color is Your Function?" — JS is stuck with coloring, no coroutines
- TanStack Query: always async fetch, sync observation layer — separated concerns, not duplicated code
- Zustand: sync-only core, async is user-land — store doesn't care about async

### Our Decision Space
- Current dual path: defensible, zero overhead, but ~40 lines duplicated
- Unified async: eliminates duplication, unmeasurable overhead at our scale
- Decision should be driven by: which changes happen more often?

## Date/Time Coercion — Native APIs Only

### The Off-by-One Day Bug (CRITICAL)
- `new Date("2025-03-19")` → parsed as UTC midnight → shows as March 18 in western timezones
- Fix: split and construct `new Date(y, m-1, d)` for local, or append `T00:00:00`

### What Input Elements Return
| Input type | `.value` format | `.valueAsDate` |
|------------|----------------|----------------|
| date | `"YYYY-MM-DD"` string | Date (UTC) |
| time | `"HH:mm"` string | Date (1970-01-01) |
| datetime-local | `"YYYY-MM-DDTHH:mm"` string | null (!!) |

### Syncfusion vs Native Mismatch
- FusionDatePicker `.value` → `Date` object (local time)
- Native `<input type="date">` `.value` → `"YYYY-MM-DD"` string
- Same coercion function receives different types depending on vendor

### Comparison Strategy
- Equality: `getTime()` — Date objects are reference types, `===` compares references
- Ordering: `getTime()` — explicit is better than implicit valueOf()
- Time-only: minutes-since-midnight (`hours * 60 + minutes`) or `HH:mm` string (lexicographic works)
- Date-only: `YYYY-MM-DD` string comparison (lexicographic works, zero-padded)

### Temporal API Status (March 2026)
- TC39 Stage 4, Chrome 144+, Firefox 139+, Edge 144+
- **Safari: NOT shipped** — hard blocker, polyfill is 100KB
- When Safari ships → PlainDate/PlainTime eliminate all timezone ambiguity

## Switch vs Map vs Dictionary — Verdict

| Question | Answer |
|----------|--------|
| Switch vs Record map for 4 command kinds? | **Switch wins** — free type narrowing, exhaustiveness via assertNever, V8 sequential comparison at 4 cases is optimal |
| Add `assertNever` defaults? | **Yes, immediately** — zero cost, compile-time enforcement, strict improvement |
| Map vs Object for dispatch? | Object is 3.6x faster lookup, but irrelevant — use switch |
| When would you switch to a map? | 10+ kinds AND handlers registered from different modules. Not our architecture |
| Immutability for ExecContext? | Current code already safe. Cheap win: `Readonly<ExecContext>` on signatures |

### Where Each Shines
- **Switch**: small stable unions (4-8 cases), free TS narrowing, exhaustiveness via `never`
- **Record<Kind, Handler>**: plugin architectures, 10+ extensible kinds, runtime registration
- **Map**: dynamic keys, non-string keys, frequent insertion/deletion, large datasets (8M+ keys)
- **TypeScript compiler itself uses giant switches** for SyntaxKind dispatch — authoritative validation

## Chaining vs Functions — Verdict
- **C# DSL uses chaining** — correct, it configures value objects (Fowler endorses for configuration)
- **TS runtime uses function dispatch** — correct, CQS applies, functions are tree-shakeable
- Fowler coined "fluent interface" but warns it's only for configuration, NOT execution
- Class methods can't be individually tree-shaken; standalone functions can

## Nested Ifs — Patterns
- **Guard clauses** (Fowler): early returns flatten nested ifs into a flat list
- **Decompose Conditional** (Fowler): extract complex boolean expressions into named functions
- Our conditions.ts is already well-structured — recursive evaluation is natural for tree data structures
- switch(op) block is flat, each case a single expression return — ideal pattern

## Right Amount of Parameters
- Uncle Bob (Clean Code): "One or two arguments is ideal, three should be avoided"
- Fowler "FlagArgument": avoid boolean params — separate methods communicate intention
- Fowler "Introduce Parameter Object": apply when same param group travels across multiple call sites
- Our functions: 2 params (ideal), 4 params (borderline but acceptable at single call sites)
- **Our functions have zero boolean flags** — this is a strength

## Type Explosion — Not a Problem for Us
- ~40 named types for plan executor with triggers, reactions, commands, guards, HTTP, validation = lean
- TanStack Query: 60+ types for just data fetching. TypeScript compiler: hundreds
- Our types map 1:1 to JSON plan primitives — correct design for schema-validated contract
- TS team warns about quadratic union comparison beyond ~12 members; our largest union is 5
- Optional fields on existing interfaces handle variants correctly (no need for per-variant subtypes)
- "Prefer duplication over the wrong abstraction" (Sandi Metz / Kent C. Dodds AHA)

## User's Design Principles (Non-Negotiable)

1. **Runtime is dumb executor of plan** — runtime NEVER invents behavior, plan carries ALL info
2. **ID-aware architecture** — we always use element IDs, never wide selectors (querySelector("input"))
3. **No fallbacks** — fallbacks create an illusion of safety; force explicit behavior instead
4. **No wide selectors** — they are hacks that avoid thinking deeply about correct behavior
5. **Fail-fast, not fail-safe** — throw on missing component, unknown vendor, missing readExpr
6. **No caching** — premature for now, will add in future when needed

## Key Validation for Our Architecture
- Our `executeCommand()` switch is architecturally identical to TanStack Query's `#dispatch()`
- Our vertical slices match TanStack Table's `TableFeature` pattern
- Our `resolveRoot()` is DIP in action — volatile vendor detail behind stable abstraction
- Our `ExecContext` flowing through pipeline matches TanStack Query's context pattern
- Our discriminated unions match TanStack's structural discrimination
- Kent Beck's coupling analysis validates our approach: concentrate vendor coupling in ONE module (component.ts)
