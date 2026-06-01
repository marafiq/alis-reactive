# value-arrays

The value + reactive-array DSL: every way to **read a value** and every way to
**transform an array**, the framework can serialize into a plan and execute in the
browser with no server-side rendering.

One concept reads all values: `TypedSource<T>`. It is the single typed authoring
surface for a component member, a plugin member, a URL parameter, an event
payload, a success/error body, a literal, an object, an array, and the scalar
result of an array fold. Anywhere the DSL accepts "a value" — a reaction's right
side, a condition operand, a gather payload, a route param, a header, a dispatch
field, a plugin argument, an array projection — it accepts a `TypedSource<T>`.

`ReactiveArray<T>` is a typed, **deferred** array transform. Its operators capture
authoring intent and compile to plan-JSON `array-op` nodes — nothing runs on the
server. It is deliberately **not** `IEnumerable`/`IQueryable`, so LINQ extension
methods cannot bind (no collision) and lambdas are captured, not invoked. Folds
(`Count`, `Sum`, `Min`, `Max`, `Average`, `Any`, `All`, `FindFirst`) terminate the
chain in a `ReactiveValue<T>`, which is itself a `TypedSource<T>` — so a fold plugs
straight into `SetText`, `When`, a dispatch field, or anywhere a value is read.

The runtime engine is `Alis.Reactive.Assets/runtime/value/array-op-engine.ts`:
eight pure, sync ops (`count·filter·map·sum·any·all·find·orderBy/orderByDescending`,
plus the new numeric folds `min·max·average`). It normalizes array-like browser
values (DOMTokenList, HTMLCollection, NodeList, iterables) to a JS array at the
input boundary, sorts deterministically (numeric when both keys are numbers, else
lexicographic; non-finite keys sort last), and returns `null` on empty for
first/fold ops.

Domain throughout: senior-living — residents, care levels, facilities, billing,
assessments, shift assignments.

> **Names are the finalized green-field names** (`09-dsl-naming-sheet.md`):
> `FindFirst` (was `Find`), `AsArraySource()` (was `AsSource()`), the numeric folds
> `Min`/`Max`/`Average`, the gather literal source `Literal(param, value)` (was
> `Static`), and the `WholeResponseBody` / `WholeElement` value-node kinds (was the
> `responseBody` / `elementValue` sentinels). The per-op verbs
> (`Where/Select/OrderBy/OrderByDescending/Count/Any/All/Sum`) keep their truthful
> names.

---

## TypedSource<T> — the one typed value handle

`TypedSource<T>` is abstract; you never construct it. You obtain one from a typed
factory and the `<T>` flows through every place that reads it, preserving
compile-time safety (a `decimal` source cannot be dropped where a `string` is
expected).

### Read a component member as a source

The `.Value()` (or any value-returning member) of a registered component is a
`TypedSource<T>` — feed it to `SetText`, `When`, a gather `Include`, etc.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("welcome-line")
        .SetText(p
            .Component<FusionTextBox>(m => m.ResidentName)
            .Value()));
```

### Read a URL query parameter as a source

`FromUrl<T>(name)` reads a value from the current URL query string, typed to `T`.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("facility-label")
        .SetText(p.FromUrl<string>("facility")));
```

### Read an event-payload property as a source

Inside a `.Reactive(...)` pipeline, the event args object is a typed payload;
`FromEvent` (the `When(args, x => x.Prop)` / `p.From(args, ...)` family) reads a
property off it.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .When(p.FromUrl<string>("status"))
        .Eq("active")
        .Then(s => s
            .Element("badge")
            .Show()));
```

### Read a success-body property as a source

In a success route, `responseBody.Read(expr)` yields a `TypedSource<TProp>`.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Get("/api/census/summary")
        .Response(r => r
            .OnSuccess<CensusSummary>((json, s) => s
                .Element("occupied-beds")
                .SetText(json.Read(x => x.OccupiedBeds)))));
```

### Read a plugin member as a source

A plugin function return or property is a `TypedSource<T>` at the stringly plugin
boundary.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("slug")
        .SetText(p
            .Plugin<string>("urls", "currentSlug")
            .Value()));
```

---

## Literal — a constant value embedded in the plan

A `Literal` value-node (`kind:"literal"`) carries a JSON-serializable constant and
its inferred `Shape`. You author it implicitly by passing a C# constant where a
source is expected; System.Text.Json serializes it at render time (no reflection).

### Literal string

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("banner")
        .SetText("Welcome to Maple Court"));
```

### Literal int / decimal / double (numeric shape)

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Set(m => m.OpenAssessments, 0));
```

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Set(m => m.MonthlyRate, 4250.00m));
```

### Literal bool (boolean shape)

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Set(m => m.IsAdmitted, true));
```

### Literal DateTime (date shape, ISO-8601 round-trip "O")

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Set(m => m.AdmissionDate, new DateTime(2026, 1, 1)));
```

### Literal as a condition operand

The right side of a comparison is a literal; the left is the typed source.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .When(p.Component<FusionNumericTextBox>(m => m.Age).Value())
        .Gte(65)
        .Then(s => s
            .Element("senior-flag")
            .Show()));
```

### Literal as a gather payload value

In the HTTP gather scope a fixed value is added with `Literal(param, value)`
(the finalized name for the old `Static`), aligning with the value-spine `Literal`.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Post("/api/admissions")
        .Gather(g => g
            .Literal("source", "kiosk")
            .Include(m => m.ResidentName)));
```

---

## Read — a value pulled from a live source at execution time

A `Read` value-node (`kind:"read"`) names a `Source` (component / plugin / URL /
payload / DOM element), a member, an optional nested path, and a `Shape`. A read
may access a **property** or **invoke a method** (the `property` vs `method`
discriminator).

### Property read off a component

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("care-level-echo")
        .SetText(p
            .Component<FusionDropDownList>(m => m.CareLevel)
            .Value()));
```

### Method-invoke read (member returns a value)

Any member that returns a value can be a source. Method args are themselves
value expressions.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("selected-count")
        .SetText(p
            .Component<FusionGrid>("resident-grid")
            .GetSelectedRecords()
            .AsArraySource()
            ... ));
```

### Nested-path payload read

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Get("/api/residents/active")
        .Response(r => r
            .OnSuccess<ResidentEnvelope>((json, s) => s
                .Element("primary-city")
                .SetText(json.Read(x => x.Resident.Address.City)))));
```

### DOM-element member read

`p.FromDom(elementId, member)` reads an array-like member off a DOM element
resolved by `getElementById` (the member name is stringly at the DOM boundary).

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("class-count")
        .SetText(p
            .FromDom("resident-card", "classList")
            .Count()));
```

---

## ObjectValue — a composite value built from named fields

An `ObjectValue` node (`kind:"object"`) assembles a JSON object from named field
expressions; the object `Shape` is inferred from the fields. You author it through
the dispatch / gather object builders, where each field is itself any value source.

### Object payload on a typed dispatch

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .DispatchFrom<AdmissionEvent>("resident-admitted", o => o
            .Set(e => e.ResidentName, p.Component<FusionTextBox>(m => m.ResidentName).Value())
            .Set(e => e.CareLevel, p.Component<FusionDropDownList>(m => m.CareLevel).Value())
            .Set(e => e.AdmittedOn, p.FromUrl<DateTime>("date"))));
```

### Object value mixing literal and live fields

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .DispatchFrom<BillingNote>("note-created", o => o
            .Set(n => n.Category, "monthly-rate")
            .Set(n => n.Amount, p.Component<FusionNumericTextBox>(m => m.MonthlyRate).Value())));
```

---

## ArrayValue — a composite value built from ordered items

An `ArrayValue` node (`kind:"array"`) assembles a JSON array from ordered item
expressions; the element `Shape` is the shared item shape when all items agree,
otherwise `Any`. Authored where the DSL collects ordered values (e.g. a multi-value
gather field or an array dispatch field).

### Literal array of care levels

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .DispatchFrom<FilterEvent>("filter-set", o => o
            .SetArray(f => f.CareLevels, "independent", "assisted", "memory")));
```

### Array of live component reads

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .DispatchFrom<FacilityEvent>("facilities-chosen", o => o
            .SetArray(f => f.FacilityIds,
                p.Component<FusionDropDownList>(m => m.PrimaryFacility).Value(),
                p.Component<FusionDropDownList>(m => m.BackupFacility).Value())));
```

---

## ReactiveArray<T> — entering the array transform

You enter a deferred array transform with `p.From(...)` (over any array-shaped
source) or `p.FromDom(...)` (over a DOM array-like member). The element type flows
through every operator; the lambdas are captured into plan nodes, never invoked.

### From a component array member

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("tag-count")
        .SetText(p
            .From(p.Component<FusionMultiSelect>(m => m.Tags).Value())
            .Count()));
```

### From an event-payload array (.Reactive event args)

`p.From(args, e => e.Data)` opens a transform over an event-payload `T[]`; the
element type `T` flows through the chain.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .From(payload, x => x.Selection.Data)
        .Where(c => c.Value == "memory")
        .Count()
        ... );
```

### FromDom — DOM array-like member (string elements)

`p.FromDom(elementId, member)` yields a `ReactiveArray<string>`; array-like
collections (DOMTokenList / HTMLCollection / NodeList) are normalized at the op
boundary.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("has-alert-class")
        .SetText(p
            .FromDom("resident-card", "classList")
            .Any(c => c == "has-alert")));
```

### FromDom<TElement> — DOM array-like member of a declared element type

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("child-count")
        .SetText(p
            .FromDom<ResidentCard>("roster", "children")
            .Count()));
```

---

## Where — keep matching elements (array → array)

`Where(predicate)` filters by a per-element sync predicate (compiled to a
`ConditionGraph` over the element scope). Chains compose; the element type is
unchanged.

### Filter by a numeric comparison

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("high-acuity-count")
        .SetText(p
            .From(p.Component<FusionGrid>("census").GetCurrentViewRecords().AsArraySource())
            .Where(r => r.AcuityScore >= 8)
            .Count()));
```

### Filter with logical && and ||

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .From(p.Component<FusionMultiSelect>(m => m.Residents).Value())
        .Where(r => r.CareLevel == "memory" && r.Age >= 80)
        .Count()
        ... );
```

### Filter with a string operator (Contains/StartsWith/EndsWith)

String-operator arguments must be constants or captured values, never element
reads.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .From(p.Component<FusionGrid>("directory").GetCurrentViewRecords().AsArraySource())
        .Where(r => r.ResidentName.StartsWith("A"))
        .Count()
        ... );
```

### Filter on a boolean member (truthy)

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .From(p.Component<FusionGrid>("census").GetCurrentViewRecords().AsArraySource())
        .Where(r => r.IsDischargePending)
        .Count()
        ... );
```

---

## Select — project each element (array → array of TResult)

`Select(selector)` projects each element through a per-element value selector,
changing the element type to `TResult` (the result shape flows on).

### Project to a scalar field

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Component<FusionDropDownList>(m => m.FacilityPicker)
        .SetDataSource(p
            .From(p.Component<FusionGrid>("facilities").GetCurrentViewRecords().AsArraySource())
            .Select(f => f.FacilityName)
            .AsArraySource()));
```

### Project then filter (chained)

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .From(p.Component<FusionMultiSelect>(m => m.Residents).Value())
        .Where(r => r.AcuityScore >= 8)
        .Select(r => r.ResidentName)
        .AsArraySource()
        ... );
```

### Project a whitelisted pure element method (e.g. ToUpperCase)

Per-element method calls must be pure and whitelisted
(`toUpperCase`/`toLowerCase`/`trim`/date getters/`getAttribute`/`hasAttribute`).

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .From(p.Component<FusionMultiSelect>(m => m.Codes).Value())
        .Select(c => c.ToUpperCase())
        .AsArraySource()
        ... );
```

---

## OrderBy — order elements ascending (array → array)

`OrderBy(key)` orders by a per-element key projection. The key **must** project to
a comparable scalar (string/number/date/bool/enum) — a non-scalar key throws at
authoring time (it would silently sort every element as `"[object Object]"`).

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Component<FusionGrid>("waitlist")
        .SetDataSource(p
            .From(p.Component<FusionGrid>("waitlist").GetCurrentViewRecords().AsArraySource())
            .OrderBy(r => r.AdmissionDate)
            .AsArraySource()));
```

### OrderBy a string key

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .From(p.Component<FusionGrid>("directory").GetCurrentViewRecords().AsArraySource())
        .OrderBy(r => r.ResidentName)
        .AsArraySource()
        ... );
```

---

## OrderByDescending — order elements descending (array → array)

`OrderByDescending(key)` is the descending mirror; same scalar-key guard.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .From(p.Component<FusionGrid>("census").GetCurrentViewRecords().AsArraySource())
        .OrderByDescending(r => r.AcuityScore)
        .AsArraySource()
        ... );
```

### Filter then order descending (chained)

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .From(p.Component<FusionGrid>("census").GetCurrentViewRecords().AsArraySource())
        .Where(r => r.CareLevel == "memory")
        .OrderByDescending(r => r.MonthlyRate)
        .AsArraySource()
        ... );
```

---

## Count — number of elements (array → ReactiveValue<int>)

A terminal fold. `Count()` is unconditional length; `Count(predicate)` compiles to
`filter -> count`, so the count node never carries a predicate.

### Count all elements

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("total-residents")
        .SetText(p
            .From(p.Component<FusionGrid>("census").GetCurrentViewRecords().AsArraySource())
            .Count()));
```

### Count matching a predicate

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("memory-care-count")
        .SetText(p
            .From(p.Component<FusionGrid>("census").GetCurrentViewRecords().AsArraySource())
            .Count(r => r.CareLevel == "memory")));
```

---

## Any — is the array non-empty / does any match (array → ReactiveValue<bool>)

### Any element exists (no predicate)

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .When(p
            .From(p.Component<FusionMultiSelect>(m => m.Tags).Value())
            .Any())
        .Then(s => s
            .Element("tags-present")
            .Show()));
```

### Any element matches the predicate

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .When(p
            .From(p.Component<FusionGrid>("census").GetCurrentViewRecords().AsArraySource())
            .Any(r => r.IsDischargePending))
        .Then(s => s
            .Element("discharge-banner")
            .Show()));
```

---

## All — do every element match (array → ReactiveValue<bool>)

`All(predicate)` is true when every element matches (vacuously true on empty).

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .When(p
            .From(p.Component<FusionGrid>("assessments").GetCurrentViewRecords().AsArraySource())
            .All(a => a.IsComplete))
        .Then(s => s
            .Element("submit-all")
            .Show()));
```

---

## Sum — add a numeric projection (array → ReactiveValue<number>)

`Sum(selector)` sums a numeric per-element selector; non-finite contributions count
as 0. Three CLR overloads (`int` / `decimal` / `double`) type only the terminal
`ReactiveValue<T>` — all three compile to one `sum` wire node.

### Sum an int selector

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("open-tasks-total")
        .SetText(p
            .From(p.Component<FusionGrid>("care-board").GetCurrentViewRecords().AsArraySource())
            .Sum(r => r.OpenTasks)));
```

### Sum a decimal selector (billing)

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("monthly-revenue")
        .SetText(p
            .From(p.Component<FusionGrid>("census").GetCurrentViewRecords().AsArraySource())
            .Sum(r => r.MonthlyRate)));
```

### Sum a double selector

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("total-hours")
        .SetText(p
            .From(p.Component<FusionGrid>("shifts").GetCurrentViewRecords().AsArraySource())
            .Sum(r => r.ScheduledHours)));
```

### Filter then sum (chained)

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("memory-care-revenue")
        .SetText(p
            .From(p.Component<FusionGrid>("census").GetCurrentViewRecords().AsArraySource())
            .Where(r => r.CareLevel == "memory")
            .Sum(r => r.MonthlyRate)));
```

---

## Min — smallest of a numeric projection (array → ReactiveValue<TNum>)

`Min(selector)` returns the smallest projected value; empty input → `null` (same
null-on-empty contract as `FindFirst`).

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("lowest-acuity")
        .SetText(p
            .From(p.Component<FusionGrid>("census").GetCurrentViewRecords().AsArraySource())
            .Min(r => r.AcuityScore)));
```

### Min over a filtered set (chained)

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("earliest-admission")
        .SetText(p
            .From(p.Component<FusionGrid>("census").GetCurrentViewRecords().AsArraySource())
            .Where(r => r.CareLevel == "assisted")
            .Min(r => r.AdmissionDate)));
```

---

## Max — largest of a numeric projection (array → ReactiveValue<TNum>)

`Max(selector)` mirrors `Min`; empty input → `null`.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("highest-rate")
        .SetText(p
            .From(p.Component<FusionGrid>("census").GetCurrentViewRecords().AsArraySource())
            .Max(r => r.MonthlyRate)));
```

### Max over a filtered set (chained)

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("peak-acuity-memory")
        .SetText(p
            .From(p.Component<FusionGrid>("census").GetCurrentViewRecords().AsArraySource())
            .Where(r => r.CareLevel == "memory")
            .Max(r => r.AcuityScore)));
```

---

## Average — mean of a numeric projection (array → ReactiveValue<double>)

`Average(selector)` returns the mean; the terminal type is always `double`
(the mean is non-integral). Empty input → `null`. Spelled in full to match LINQ
and scream intent.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("average-acuity")
        .SetText(p
            .From(p.Component<FusionGrid>("census").GetCurrentViewRecords().AsArraySource())
            .Average(r => r.AcuityScore)));
```

### Average over a filtered set (chained)

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("avg-memory-rate")
        .SetText(p
            .From(p.Component<FusionGrid>("census").GetCurrentViewRecords().AsArraySource())
            .Where(r => r.CareLevel == "memory")
            .Average(r => r.MonthlyRate)));
```

---

## FindFirst — first matching element, or null (array → ReactiveValue<T>)

`FindFirst(predicate)` returns the first element that matches the predicate, or
`null` when none match. (Renamed from `Find`; the name screams the first-match
semantics. Wire token stays `find`.)

### FindFirst the whole element

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .When(p
            .From(p.Component<FusionGrid>("census").GetCurrentViewRecords().AsArraySource())
            .FindFirst(r => r.IsDischargePending))
        .NotNull()
        .Then(s => s
            .Element("pending-alert")
            .Show()));
```

### FindFirst projecting a field (two-arg overload)

`FindFirst(predicate, selector)` returns a per-element field of the first match.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("first-pending-name")
        .SetText(p
            .From(p.Component<FusionGrid>("census").GetCurrentViewRecords().AsArraySource())
            .FindFirst(r => r.IsDischargePending, r => r.ResidentName)));
```

---

## AsArraySource — expose a transformed array as TypedSource<T[]>

`AsArraySource()` (renamed from `AsSource()`) ends a `ReactiveArray<T>` chain and
exposes the transformed array as a `TypedSource<T[]>`, so it binds to any
`SetDataSource(TypedSource<T[]>)` overload — no HTTP round-trip. The result type
screams the array shape so the conversion target is unambiguous.

### Bind a filtered+sorted array straight to a component

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Component<FusionGrid>("memory-census")
        .SetDataSource(p
            .From(p.Component<FusionGrid>("census").GetCurrentViewRecords().AsArraySource())
            .Where(r => r.CareLevel == "memory")
            .OrderByDescending(r => r.AcuityScore)
            .AsArraySource()));
```

### Bind a projected array to a dropdown

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Component<FusionDropDownList>(m => m.FacilityPicker)
        .SetDataSource(p
            .From(p.Component<FusionMultiSelect>(m => m.Facilities).Value())
            .Select(f => f.FacilityName)
            .AsArraySource()));
```

---

## From — open an array transform over any array source

`p.From(TypedSource<T[]>)` is the array-source entry verb. The source can be a
component member, a plugin return, a previous `AsArraySource()`, or any
array-shaped `TypedSource<T[]>` — the same `From` voice as the HTTP pipeline.

### From a plugin array return

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("recent-count")
        .SetText(p
            .From(p.Plugin<string[]>("history", "recentResidents").Value())
            .Count()));
```

### From an event payload array (.Reactive args)

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("selected-memory")
        .SetText(p
            .From(payload, e => e.Selection.Data)
            .Count(c => c.Value == "memory")));
```

---

## FromDom — open an array transform over a DOM array-like member

`p.FromDom(elementId, member)` opens a transform over a DOM element's array-like
member (resolved by `getElementById`); DOMTokenList / HTMLCollection / NodeList are
normalized at the op boundary. The string overload yields `ReactiveArray<string>`;
the generic overload declares the element type.

### FromDom over classList (string elements)

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .When(p
            .FromDom("resident-card", "classList")
            .Any(c => c == "high-risk"))
        .Then(s => s
            .Element("risk-badge")
            .Show()));
```

### FromDom<TElement> over child elements

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("row-count")
        .SetText(p
            .FromDom<RosterRow>("roster", "children")
            .Count()));
```

---

## ReactiveValue<T> — the scalar terminal of a fold

Every fold (`Count`, `Sum`, `Min`, `Max`, `Average`, `Any`, `All`, `FindFirst`)
returns a `ReactiveValue<T>`, which **is** a `TypedSource<T>`. So a fold needs no
new overloads — it plugs into `SetText`, `When`, and dispatch fields directly.
(Gather intake is typed to component/plugin sources, not the base source, so a
fold feeds `SetText`/`When`, not `Include`.)

### Fold feeding SetText

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("avg-rate")
        .SetText(p
            .From(p.Component<FusionGrid>("census").GetCurrentViewRecords().AsArraySource())
            .Average(r => r.MonthlyRate)));
```

### Fold feeding a When condition

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .When(p
            .From(p.Component<FusionGrid>("assessments").GetCurrentViewRecords().AsArraySource())
            .Count(a => !a.IsComplete))
        .Gt(0)
        .Then(s => s
            .Element("incomplete-warning")
            .Show()));
```

### Fold feeding a dispatch field

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .DispatchFrom<CensusEvent>("census-counted", o => o
            .Set(e => e.TotalResidents, p
                .From(p.Component<FusionGrid>("census").GetCurrentViewRecords().AsArraySource())
                .Count())));
```

---

## WholeResponseBody — read the entire success body, not a member

`WholeResponseBody` (`kind:"whole-response-body"`, was the `responseBody` sentinel)
is the value-node for the **whole** success body — it carries no member. The HTTP
`Into(elementId)` reaction emits this kind, putting the full response body into an
element.

### Into — inject the whole response body into an element

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Get("/api/census/snapshot")
        .Response(r => r
            .OnSuccess(s => s
                .Into("census-panel"))));
```

### Whole body as a When operand

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Get("/api/flags/active")
        .Response(r => r
            .OnSuccess<string>((json, s) => s
                .When(json.Read(x => x))
                .NotEmpty()
                .Then(b => b
                    .Element("flag-row")
                    .Show()))));
```

---

## WholeElement — read the array element itself (identity, x => x)

`WholeElement` (`kind:"whole-element"`, was the `elementValue` sentinel) is the
value-node for the **current array element itself** — the identity projection
`x => x` over a primitive-element array. A distinct kind from `WholeResponseBody`
(different source); both carry no member.

### Sum a primitive-element array (identity projection)

For an `int[]`/`decimal[]`, the per-element selector is the element itself.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("score-total")
        .SetText(p
            .From(p.Plugin<int[]>("assessments", "scores").Value())
            .Sum(x => x)));
```

### Filter a string array on the element itself

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("memory-tag-count")
        .SetText(p
            .From(p.Component<FusionMultiSelect>(m => m.Tags).Value())
            .Where(x => x == "memory")
            .Count()));
```

### OrderBy a primitive-element array by identity

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Component<FusionDropDownList>(m => m.Codes)
        .SetDataSource(p
            .From(p.Plugin<string[]>("codes", "all").Value())
            .OrderBy(x => x)
            .AsArraySource()));
```

---

## Notes on the closed op set and determinism

- **Closed transform set.** The array ops are a fixed switch
  (`count·filter·map·sum·min·max·average·any·all·find·orderBy·orderByDescending`),
  named explicitly as `ArrayOp` — not an open `IEnumerable` surface. LINQ
  extension methods cannot bind to `ReactiveArray<T>`.
- **Sync per-element lane.** Every per-element predicate is the **sync** condition
  subset (`compare`/`all`/`any`/`not`) — never `Confirm` — so element evaluation
  stays on the immediate lane.
- **Scalar sort keys only.** `OrderBy`/`OrderByDescending` keys must project to a
  comparable scalar; a non-scalar key throws at plan render time rather than
  mis-sorting in the browser.
- **Empty / null contract.** `FindFirst`, `Min`, `Max`, and `Average` return
  `null` on empty input; `Sum` returns 0; `Count` returns 0; `Any()` returns
  false; `All()` returns true (vacuous).
- **Boundary normalization.** The engine normalizes array-like and iterable
  browser values to a JS array at the input boundary (the same category as
  `getElementById` returning null) — not a plan validator or fallback. A
  non-iterable object (e.g. `DOMStringMap`) fails fast, keeping it in the plugin
  escape hatch's domain.
- **Deterministic ordering.** Sort keys compare numerically when both are finite
  numbers, else lexicographically; non-finite keys (NaN/Infinity from a missing
  field) sort last, deterministically — `NaN` is never fed to `Array.sort`.
