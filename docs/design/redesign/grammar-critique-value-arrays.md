# Grammar critique — Value spine + ReactiveArray

PL-architect hardening of the **Value/Arrays** cluster grammar: `ReactiveArray<TElement>`
(the deferred array ops), `ReactiveValue<TValue>` (the scalar terminal), `AsSource()` (the
exit-to-source), the `From*` pipeline entry points, and the `TypedSource<TProp>` spine that
glues them to the rest of the DSL.

**Authority.** Every cited shape is a real public builder edge from
`ast-grammar-value-arrays-validation.md` (Receiver → Member(params) → Return, with `file:line`).
Names are reconciled against `09-dsl-naming-sheet.md` (decided) and seams against
`08-determinism-formalization.md` (§6.1, §6.3). This file is **value-spine + array-ops only**;
the validation half of the cluster (`ClientValidation*`) is out of scope here.

**The bar.** Easy to write, reads TALL (one fluent call per line, top-to-bottom). Judged on
orthogonality, composability, TALL-reading, least-surprise, discoverability, consistency,
easy-to-write. **No capability may be dropped** — every adjustment is a `BEFORE → AFTER` that
preserves every feature (zero feature loss = zero tech debt).

---

## What is ALREADY good (do NOT churn)

These read well and are PL-correct as cut. Churning them would be regression.

1. **The terminal/continuation split is honest in the type.** Chain ops
   (`Where`/`Select`/`OrderBy`/`OrderByDescending`) return `ReactiveArray<T>`; folds
   (`Count`/`Any`/`All`/`Sum`/`Find`) return `ReactiveValue<T>`
   (`ReactiveArray.cs:28,34,43,47` vs `:70,78,86,90,102`). **Least-surprise:** the return type
   *is* the documentation — you cannot accidentally chain `.Where(...)` after `.Count()` because
   `ReactiveValue` has no array ops. The grammar makes "fold ends the chain" unrepresentable. Keep.

2. **`Select<TResult>` re-types the element through the chain.** `Select<TResult>(...)` returns
   `ReactiveArray<TResult>` (`ReactiveArray.cs:34`), so a projection flows its new element type
   forward and every downstream op is typed against the projected shape.
   **Composability:** `cod(Select) = ReactiveArray<TResult> = dom(Where over TResult)` — the
   seam composes by construction. Keep.

3. **`ReactiveValue<TValue>` carries no members of its own; its whole role is being a
   `TypedSource<TValue>`** (`ReactiveValue.cs:13`, table note lines 65-70). This is the *right*
   amount of surface: a fold result is just a value, so it plugs into `SetText`, `When`, dispatch
   payloads with **zero new overloads**. **Orthogonality + ISP:** no second value abstraction is
   minted for "array results". Keep — this is the spine working as designed.

4. **`Sum` is one wire op with three CLR-typed terminals** (`ReactiveArray.cs:90,94,98` →
   `ReactiveValue<int/decimal/double>`). The overloads differ only in the numeric return type;
   they are not three spellings of different intents. The naming sheet confirms KEEP
   (`09:292`). The CLR type narrows the terminal so `SetText(sum)` stays typed. Keep.

5. **`From*` entry verbs share the HTTP pipeline's `From` voice and scream the boundary with a
   suffix.** `From` (typed source / args+selector) and `FromDom` (DOM read)
   (`PipelineBuilder.Arrays.cs:15,23,37,41`); the `Dom` suffix names the external boundary, the
   bare `From` reads identically to the HTTP `FromEvent`/`FromUrl` grammar. **Consistency:**
   same concept ("read an array from a source to begin a transform") = same `From*` shape.
   Naming sheet KEEP (`09:295`). Keep.

6. **`TypedSource<T>` exposes no public DSL edges and is the single typed authoring surface**
   (`TypedSource.cs:9`, all members `internal`; naming sheet `09:273,430` "sacred"). Array ops
   exit *into* it; they do not fork a parallel "array source" abstraction. **DIP/orthogonality:**
   everything depends inward on one value spine. Keep.

These six are the load-bearing good bones. The adjustments below sharpen the **few** edges that
read cold, collide, or fail to compose — without touching the above.

---

## Proposed adjustments (BEFORE → AFTER)

### A1. `AsSource()` → `AsArraySource()` — name the exit so the conversion target is unambiguous

**Current shape.** `ReactiveArray<TElement>.AsSource() : TypedSource<TElement[]>`
(`ast-grammar-value-arrays-validation.md:51`, source `ReactiveArray.cs:121`).

**Property hurt — DISCOVERABILITY + least-surprise.** `AsSource` reads cold as a generic
cast. "Source" is the most overloaded noun in this DSL — data source, event source, HTTP
source, plugin source — so `AsSource()` hides that the result is specifically a *typed array*
source (`TypedSource<T[]>`). A dev scanning for "how do I hand the whole transformed array to a
gather/dispatch" does not find their intent screamed in the name. This is the blind-fail the
naming sheet already flagged (`09:294`).

**BEFORE**
```csharp
p.From(m => m.Residents)
 .Where(r => r.CareLevel == CareLevel.Memory)
 .AsSource()              // ← "Source" of what? reads as a bare cast
```

**AFTER**
```csharp
p.From(m => m.Residents)
 .Where(r => r.CareLevel == CareLevel.Memory)
 .AsArraySource()         // ← names the TypedSource<T[]> result; array shape screamed
```

Decided in `09:294,451` (RENAME). The whole-array exit is the **only** array op whose return
shape (`T[]`) is invisible in the verb; naming it removes the ambiguity. **No capability change**
— same `TypedSource<TElement[]>` return, same wire output. Pure rename for discoverability.

---

### A2. `Find` → `FindFirst` — the verb must scream first-match-or-null

**Current shape.**
`Find(Expression<Func<TElement,bool>>) : ReactiveValue<TElement>` and
`Find<TField>(predicate, selector) : ReactiveValue<TField>`
(`ast-grammar-value-arrays-validation.md:49-50`, source `ReactiveArray.cs:102,107`).

**Property hurt — least-surprise + CONSISTENCY.** `Find` lies twice. (1) It mimics
`List<T>.Find` — but `ReactiveArray` is deliberately **not** `IEnumerable`/`IQueryable`
(`09:131`), so borrowing a `List<T>`-only verb implies an API surface that does not exist.
(2) It is silently first-match-or-null; nothing in `Find` says "first" or "or null". A reader
expects `Find` might throw, or return all matches. **Discoverability** also suffers: the empty
contract (`empty → null`, `09:120-121`) is invisible.

**BEFORE**
```csharp
p.From(m => m.Residents)
 .Find(r => r.RoomNumber == target)        // first? all? throws on miss? unclear
```

**AFTER**
```csharp
p.From(m => m.Residents)
 .FindFirst(r => r.RoomNumber == target)   // first match, null if none — screamed
```

Decided in `09:138,293,451` (GRAMMAR-FIX). **Wire token stays `find`** — pure surface rename,
both overloads (predicate; predicate+selector) preserved. **No capability change.**

---

### A3. Complete the numeric folds — add `Min` / `Max` / `Average`

**Current shape.** The array fold family is `Count` / `Any` / `All` / `Sum` / `Find`
(`ast-grammar-value-arrays-validation.md:41-50`). `Sum` exists; **`Min`, `Max`, `Average` do
not** (`09:118`, table lines 46-48 show only `Sum`).

**Property hurt — ORTHOGONALITY (incomplete family) + EASY-TO-WRITE.** A numeric-fold family
that has `Sum` but not `Min`/`Max`/`Average` is asymmetric: a dev who wants the largest balance
or the mean care-cost has no fold and must drop out of the grammar (manual loop in C# before
render, or a plugin). One clear intent ("aggregate this numeric projection") has a hole. A
complete family is discoverable by symmetry — if `Sum` exists, the dev *expects* `Min`/`Max`/
`Average` to exist and finds them without reading source.

**BEFORE** — only `Sum`; min/max/mean fall out of the DSL.
```csharp
p.From(m => m.Residents)
 .Sum(r => r.MonthlyBill)          // ✓
// .Min / .Max / .Average           // ✗ — not expressible
```

**AFTER**
```csharp
p.From(m => m.Residents)
 .Min(r => r.MonthlyBill)          // → ReactiveValue<TNum>, wire op "min", empty → null
 .Max(r => r.MonthlyBill)          // → ReactiveValue<TNum>, wire op "max", empty → null
 .Average(r => r.MonthlyBill)      // → ReactiveValue<double>, wire op "average"
```

Decided as **NEW** edges in `09:118,297-299,463`. `Average` spelled in full (matches LINQ,
screaming intent over `Avg`); terminal `double` (mean is non-integral). Empty-input contract
`empty → null` (same as `FindFirst`, `09:120-121`). NaN/Infinity ordering reuses the solved
`compareKeys`. These are **new capabilities the cluster brief required** (`FindFirst/.../Min/
Max/Average` per the area name) — adding them is zero-feature-loss completion, not churn.

---

### A4. Close the `AsArraySource ⨾ Include` seam — widen gather `Include` to `TypedSource`

**Current shape.** `AsSource()` yields the **abstract** `TypedSource<TElement[]>`
(`ast-grammar-value-arrays-validation.md:51`). But gather `Include`'s intake is typed to the
**concrete** `TypedComponentSource` / `TypedPluginSource` only (`08:1033-1040`, source
`GatherBuilder.cs:206,266`; `01:410-419`).

**Property hurt — COMPOSABILITY (the seam is the bug).** This is a textbook
`cod(f) ≠ dom(g)`: `cod(AsArraySource) = TypedSource ⊄ dom(Include) = TypedComponentSource ⊎
TypedPluginSource ⊊ TypedSource` (`08:1038-1040`). The morphism `AsArraySource ⨾ Include`
**does not compose** — a transformed/filtered array cannot be gathered into a request payload,
even though the whole point of `AsArraySource` is "hand this array somewhere a value is
accepted." The "one `ValueExpression` reads all values" spine has a real hole at exactly this
boundary, and a `ReactiveValue` fold result hits the same wall.

**BEFORE** — does not compile / forces an awkward workaround.
```csharp
var memoryResidents = p.From(m => m.Residents)
                        .Where(r => r.CareLevel == CareLevel.Memory)
                        .AsArraySource();          // TypedSource<Resident[]>

p.Post("/api/intake").Gather(g =>
    g.Include(memoryResidents));                   // ✗ dom(Include) too narrow — seam break
```

**AFTER** — widen `Include`'s intake to the abstract `TypedSource<TProp>`.
```csharp
p.Post("/api/intake").Gather(g =>
    g.Include(memoryResidents));                   // ✓ cod(AsArraySource) = dom(Include)
```

Decided in `08:1031-1048` (§6.3). Every concrete source already lowers to
`ValueExpression.Read`/`Invoke`, so the **reader needs no change**; only the parameter type
widens. This re-cuts the seam so `cod(AsArraySource) = dom(Include)`, making the value spine
genuinely one-write-path for **all** readable values including array-op and fold results.
**No capability removed** — strictly *adds* the array/fold-into-gather path that was a hole.

---

### A5. `FromDom(string, string)` → builder-callback so the DOM read reads TALL and typed

**Current shape.**
`FromDom(string elementId, string member) : ReactiveArray<string>` and
`FromDom<TElement>(string elementId, string member) : ReactiveArray<TElement>`
(`ast-grammar-value-arrays-validation.md:62-63`, source `PipelineBuilder.Arrays.cs:37,41`).

**Property hurt — TALL-reading + EASY-TO-WRITE (stringly args).** This is a **wide,
two-positional-string** call: `FromDom("resident-grid", "selectedRecords")`. Two bare strings
sit side-by-side with no labels — a reader cannot tell at the call site which is the element id
and which is the member, and there is no compile-time guard on either. Every other `From*` in
the cluster either takes a typed source (`From`, `:15`) or a typed args+selector
(`From<TArgs,TElement>`, `:23`). `FromDom` is the lone stringly outlier, and a wide multi-arg
string call is exactly the anti-TALL shape the bar warns against.

> Note: the **`FromDom` name stays** (decided KEEP, `09:295` — the `Dom` suffix screams the
> boundary). This adjustment is about the **argument shape**, not the verb.

**BEFORE** — wide, two unlabeled strings, untyped result element.
```csharp
p.FromDom("resident-grid", "selectedRecords")    // which string is which? no element typing
 .Where(...)
```

**AFTER** — keep the bare string overload (boundary often *is* stringly), but add a TALL
builder overload that labels the two reads and carries the element type top-to-bottom.
```csharp
p.FromDom<Resident>(d => d
     .Element("resident-grid")     // labeled: the DOM element id
     .Member("selectedRecords"))   // labeled: the member to read as the array
 .Where(r => r.CareLevel == CareLevel.Memory)
```

**Property improved — TALL-reading + discoverability.** The element id and member become
named, one-per-line reads instead of two anonymous adjacent strings; the `<Resident>` element
type threads through. **Capability preserved exactly** — the existing
`FromDom(string,string)` / `FromDom<TElement>(string,string)` overloads **stay** for the
terse case (the boundary is genuinely stringly, `09:295`); the callback form is an *additive*
overload, not a replacement. Same `ReactiveArray<TElement>` result, same wire output. This is
the only adjustment here not pre-decided in the naming sheet — flag for naming-sheet
ratification before implementation.

---

### A6. Disambiguate `Count()` vs `Count(predicate)` reading — confirm KEEP, document the pair

**Current shape.** `Count() : ReactiveValue<int>` and
`Count(Expression<Func<TElement,bool>>) : ReactiveValue<int>`
(`ast-grammar-value-arrays-validation.md:41-42`, source `ReactiveArray.cs:70,74`). Same for
`Any()` / `Any(predicate)` (`:43-44`).

**Property assessed — ORTHOGONALITY (overload-collision check).** I checked whether these are a
redundant-spelling / overload-collision smell. **They are not.** `Count()` is "size of the
current (already-filtered) chain"; `Count(predicate)` is "size matching an inline predicate
without a separate `.Where`". These are two distinct intents over the same fold, and the
predicate overload is a genuine ergonomic shortcut (saves a `.Where(p).Count()` two-liner).
`Any()` ("non-empty") vs `Any(predicate)` ("any match") is the same honest pair. The naming
sheet keeps the per-op verbs (`09:133,290`).

**Verdict: KEEP as-is — no change.** Recorded here only so the orthogonality audit is complete
and a future reviewer does not mistake the predicate overloads for redundant spellings and
"simplify" them away (that would *remove* the inline-predicate capability). This is a
**no-op-by-design** entry, not an adjustment, and is excluded from the adjustment count below.

---

## Summary table

| # | Edge (file:line) | Property | BEFORE | AFTER | Status |
|---|------------------|----------|--------|-------|--------|
| A1 | `AsSource()` `ReactiveArray.cs:121` | Discoverability / least-surprise | `AsSource()` | `AsArraySource()` | Decided `09:294` |
| A2 | `Find(...)` `ReactiveArray.cs:102,107` | Least-surprise / consistency | `Find` | `FindFirst` (wire `find`) | Decided `09:293` |
| A3 | fold family `ReactiveArray.cs:90-98` | Orthogonality / easy-to-write | only `Sum` | `+ Min / Max / Average` | Decided `09:297-299` |
| A4 | `Include` intake `GatherBuilder.cs:206,266` | Composability (seam `cod≠dom`) | `Include(concrete source)` | `Include(TypedSource<T>)` | Decided `08:§6.3` |
| A5 | `FromDom(string,string)` `PipelineBuilder.Arrays.cs:37,41` | TALL-reading / no-stringly | two bare strings | + builder-callback overload (strings kept) | **NEW — needs ratification** |

**Count of proposed adjustments: 5** (A1–A5). A6 is a verified KEEP (no change) and is not
counted.

**Capability ledger (zero feature loss):** A1/A2 are pure renames; A3 strictly *adds* three
folds the cluster brief requires; A4 strictly *widens* an intake (closes a hole); A5 strictly
*adds* an overload (both stringly overloads retained). No feature is removed by any adjustment.
