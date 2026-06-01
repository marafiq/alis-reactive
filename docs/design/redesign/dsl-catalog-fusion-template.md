# fusion-template

The Fusion Template DSL builds typed, model-bound HTML fragments for Syncfusion
components that render content from a template string — grid column cells,
list-view items, tooltip bodies, dropdown rows, and similar. You author the
markup as a tall fluent chain of typed nodes; each node either emits a fixed
literal, binds to a `TModel` property, or branches on a typed predicate. The
chain terminates in `.Render()`, which produces the Syncfusion template **string**
(an SSR string with `${prop}` bindings and `${if(...)}` blocks), and you hand
that string to the column / list / tooltip template slot.

This is the **SSR-string lane**, not the runtime-mutation lane. The template
nodes append to a string at C# render time; they do not mutate live DOM. That is
why the class verb is `CssClass` (append to an SSR `class` string), **not**
`AddClass` (which mutates a live element), and why the conditional verb is
`WhenTemplate` (emits `${if(...)}`), **not** the runtime Conditions `When` (which
builds a `ConditionGraph`). See naming sheet §3.5 and §4.

Entry point: `FusionTemplate.Create<TModel>()` returns a
`FusionTemplateBuilder<TModel>`. Every builder method returns the builder for a
tall chain. The conditional methods (`WhenTemplate` / `ShowTemplateIf`) take
`then` / `@else` lambdas over a `FusionConditionalBuilder<TModel>` — the nested
body builder. The final `.Render()` (or implicit `ToString()`) yields the string.

> **Finalized names vs current source.** This catalog uses the finalized
> redesign names from `09-dsl-naming-sheet.md` §3.5. The current shipped source
> still spells five of them the old way — `CssClass` is `.Class`, `Attribute` is
> `.Attr`, `WhenTemplate` is `.When`, `ShowTemplateIf` is `.ShowIf`,
> `DispatchButton` is `.EventButton`. Each section names the current spelling so
> a dev reading today's code recognizes it. `Render` is the catalog name for the
> terminal `.Render()` call (it renders the chain to the SF template string).

Domain used throughout: a senior-living roster. Grid/list row models —
`ResidentDirectoryGridItem` (resident name, suite, care level, risk level,
photo, resident id, chart url), `ResidentBillingItem` (resident name, room,
billing status, balance), and `FacilityRosterItem` (facility name, occupancy,
region) — are the per-row `TModel` the Syncfusion component supplies.

---

## Create — open a typed template

### Create<TModel>
Opens a typed template builder bound to the per-row model the Syncfusion component supplies; the chain ends in `.Render()` to produce the template string.

```csharp
var residentNameTemplate = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Text(m => m.ResidentName)
    .Render();
```

---

## Render — terminate the chain into the SF template string

### Render
Renders the whole node chain (wrapped in a single root `<div>`) to the Syncfusion template string; this is the value you assign to a column / list / tooltip template slot. `ToString()` is an alias.

```csharp
var cellTemplate = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .CssClass("grid-resident-cell")
    .Div(d => d
        .CssClass("font-semibold text-slate-900")
        .Span(m => m.ResidentName))
    .Render();
```

```csharp
// ToString() is an alias for Render() — same string, implicit at interpolation.
string listItemTemplate = FusionTemplate.Create<FacilityRosterItem>()
    .Span(m => m.FacilityName)
    .ToString();
```

---

## Text — bound and literal text directly on the root

### Text<TProp> — bind a property as text
Emits the SF `${property}` binding inline (no wrapping element); the runtime substitutes the row value as text.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Text(m => m.ResidentName)
    .Render();
```

### Text<TProp> — nested-path binding
Dotted member paths bind to the camelCase nested path (`m.PrimaryContact.Name` → `${primaryContact.name}`).

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Text(m => m.PrimaryContact.Name)
    .Render();
```

### Text(string) — literal text
Emits a fixed literal string, independent of the row.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Text("Resident")
    .Render();
```

---

## Span — inline span node

`Span` has four overloads: bound / bound+css and literal / literal+css. Unlike
`Div`, `Span` does not take a nested builder lambda — its content is one bound
property or one literal string.

### Span<TProp> — bound span, no class
Emits `<span>${property}</span>`.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Span(m => m.ResidentName)
    .Render();
```

### Span<TProp>(css) — bound span with CSS class
Emits `<span class="...">${property}</span>`.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Span(m => m.CareLevel, "text-xs text-slate-600 font-medium")
    .Render();
```

### Span(string) — literal span, no class
Emits `<span>text</span>`.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Span(" | ")
    .Render();
```

### Span(string, css) — literal span with CSS class
Emits `<span class="...">text</span>`.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Span("Suite ", "font-medium")
    .Render();
```

### Span — composing a label-then-value run
Multiple span calls in a tall chain build a single inline run of static labels and bound values.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Span("Suite ", "font-medium")
    .Span(m => m.Suite)
    .Span(" | ")
    .Span(m => m.CareLevel)
    .Render();
```

---

## Div — block container with nested children

### Div — nested builder lambda
Emits a `<div>` whose children come from a nested `FusionTemplateBuilder<TModel>` lambda; nest `Span`, `Text`, `CssClass`, more `Div`, etc.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Div(d => d
        .CssClass("font-semibold text-slate-900")
        .Span(m => m.ResidentName))
    .Render();
```

### Div — stacked rows (grid cell with two lines)
A realistic two-line grid cell: bold name line over a muted detail line.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .CssClass("grid-resident-template space-y-1")
    .Div(d => d
        .CssClass("font-semibold text-slate-900")
        .Span(m => m.ResidentName))
    .Div(d => d
        .CssClass("text-xs text-slate-600")
        .Span("Suite ", "font-medium")
        .Span(m => m.Suite)
        .Span(" | ")
        .Span(m => m.CareLevel))
    .Render();
```

### Div — deeper nesting
Divs nest arbitrarily; each nested lambda is a fresh `FusionTemplateBuilder<TModel>` over the same row model.

```csharp
var template = FusionTemplate.Create<ResidentBillingItem>()
    .Div(outer => outer
        .CssClass("billing-cell")
        .Div(line => line
            .CssClass("font-semibold")
            .Span(m => m.ResidentName))
        .Div(line => line
            .CssClass("text-xs text-slate-500")
            .Span("Room ", "font-medium")
            .Span(m => m.RoomNumber)))
    .Render();
```

---

## Img — bound image source

`Img` binds the `src` to a property and has three overloads: bare, +css, and
+css+alt.

### Img<TProp> — bound src only
Emits `<img src="${property}" />`.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Img(m => m.PhotoUrl)
    .Render();
```

### Img<TProp>(css) — bound src with CSS class
Emits `<img src="${property}" class="..." />`.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Img(m => m.PhotoUrl, "h-8 w-8 rounded-full object-cover")
    .Render();
```

### Img<TProp>(css, alt) — bound src with CSS class and alt text
Emits `<img src="${property}" class="..." alt="..." />`; the alt is a fixed string.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Img(m => m.PhotoUrl, "h-8 w-8 rounded-full", "Resident photo")
    .Render();
```

### Img + Div — avatar-with-name cell
Combine an avatar image with a bound name in one grid cell.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Div(d => d
        .CssClass("flex items-center gap-2")
        .Img(m => m.PhotoUrl, "h-8 w-8 rounded-full object-cover", "Resident photo")
        .Span(m => m.ResidentName, "font-medium"))
    .Render();
```

---

## Badge — status pill

`Badge` emits `<span class="...">content</span>`; the css argument is optional
and defaults to `"e-badge"`. Two overloads: bound property or literal text.

### Badge<TProp> — bound, default badge class
Emits a badge whose text is the bound property, with class `e-badge`.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Badge(m => m.RiskLevel)
    .Render();
```

### Badge<TProp>(css) — bound, explicit class
Override the badge class to style by domain meaning.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Badge(m => m.RiskLevel, "rounded bg-red-100 px-2 py-0.5 text-xs font-bold text-red-700")
    .Render();
```

### Badge(string) — literal, default badge class
Emits a fixed-text badge with class `e-badge`.

```csharp
var template = FusionTemplate.Create<ResidentBillingItem>()
    .Badge("Overdue")
    .Render();
```

### Badge(string, css) — literal, explicit class
A fixed label with explicit styling.

```csharp
var template = FusionTemplate.Create<ResidentBillingItem>()
    .Badge("Overdue", "rounded bg-amber-100 px-2 py-0.5 text-xs font-semibold text-amber-700")
    .Render();
```

---

## Icon — Syncfusion glyph

`Icon` emits `<span class="e-icons e-{name} ...">`. The builder prefixes
`e-icons e-` to the name you pass (pass `"warning"`, get `e-icons e-warning`).
Two overloads: bare and +css.

### Icon(name) — glyph only
Emits the SF icon span with the default `e-icons e-{name}` class.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Icon("warning")
    .Render();
```

### Icon(name, css) — glyph with extra class
Appends your CSS class after the base `e-icons e-{name}`.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Icon("warning", "text-red-600 mr-1")
    .Render();
```

### Icon + Span — icon-prefixed label
A common cell: a status glyph followed by a bound value.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Div(d => d
        .CssClass("flex items-center")
        .Icon("warning", "text-red-600 mr-1")
        .Span(m => m.RiskLevel))
    .Render();
```

---

## Button — static-action button

`Button` emits `<button class="e-btn ..." onclick="...">text</button>`. The
`onClick` string is injected verbatim into the `onclick` attribute — it is a
trusted developer string, never user input. Two overloads: bare and +css.

### Button(text, onClick) — fixed onclick handler
Emits a button with the given inline `onclick`.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Button("Print", "window.print()")
    .Render();
```

### Button(text, onClick, css) — with CSS class
Appends your CSS class after the base `e-btn`.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Button("Print", "window.print()", "e-small e-flat")
    .Render();
```

---

## ButtonFor — button that calls a JS function with a bound row value

`ButtonFor<TProp>` emits `<button onclick="{onClickFn}(${property})">text</button>`
— it calls the named client function passing the bound row value as the argument.
Signature: `(text, idProperty, onClickFn[, css])`. Two overloads: bare and +css.

### ButtonFor<TProp> — call a function with the bound id
Emits a button whose onclick invokes `openChart(${residentId})`.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .ButtonFor("Open chart", m => m.ResidentId, "openChart")
    .Render();
```

### ButtonFor<TProp>(css) — with CSS class
Same, with explicit styling appended after `e-btn`.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .ButtonFor("Open chart", m => m.ResidentId, "openChart", "e-small e-primary")
    .Render();
```

---

## DispatchButton — button that dispatches a reactive event with the row value

`DispatchButton<TProp>` (current source: `EventButton`) emits a button whose
onclick dispatches a `CustomEvent(eventName, { detail: { id: ${property} } })` on
`document` — the exact event a `t.Event(name)` trigger (current: `CustomEvent`)
listens for. This is how a template cell hands a row id into the reactive plan.
Signature: `(text, eventName, idProperty[, css])`. Two overloads: bare and +css.

### DispatchButton<TProp> — dispatch with the bound row id
Clicking dispatches `grid:review-resident` carrying `{ id: ${residentId} }`.

```csharp
var actionTemplate = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .DispatchButton("Review", "grid:review-resident", m => m.ResidentId)
    .Render();
```

### DispatchButton<TProp>(css) — with CSS class
The shipped grid pattern: a small primary action button in an action column.

```csharp
var actionTemplate = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .CssClass("grid-action-template")
    .DispatchButton("Review", "grid:review-resident", m => m.ResidentId, "e-small e-primary")
    .Render();
```

### DispatchButton — the listening trigger (cross-area)
The dispatched event is consumed by a typed `Event` trigger in the same plan; the row id arrives on the payload.

```csharp
Html.On(plan, t => t.Event<GridTemplateActionPayload>("grid:review-resident", (payload, p) => p
    .Post("/Sandbox/Components/Grid/ReviewResident")
    .Gather(g => g
        .FromEvent(payload, x => x.Id, "id"))
    .Response(r => r
        .OnSuccess<ResidentDirectorySelectionResponse>((json, s) => s
            .Element("template-action-status")
            .SetText(json, x => x.Summary)))));
```

---

## Link — anchor with bound href and bound text

`Link<THref, TText>` emits `<a href="${hrefProperty}">${textProperty}</a>`. Both
the href and the text bind to properties. Two overloads: bare and +css.

### Link<THref, TText> — bound href and text
Emits an anchor whose href and label both come from the row.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Link(m => m.ChartUrl, m => m.ResidentName)
    .Render();
```

### Link<THref, TText>(css) — with CSS class
Same, with styling on the anchor.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Link(m => m.ChartUrl, m => m.ResidentName, "text-primary underline hover:no-underline")
    .Render();
```

---

## Raw — unescaped HTML escape hatch

### Raw
Emits a fixed raw HTML fragment verbatim, for markup the typed nodes cannot express. The string is trusted developer content, never user input.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Span(m => m.ResidentName)
    .Raw("<hr class='my-1 border-slate-200' />")
    .Span(m => m.CareLevel, "text-xs text-slate-500")
    .Render();
```

---

## WhenTemplate — conditional SSR block (then / else)

`WhenTemplate` (current source: `When`) emits a Syncfusion `${if(condition)} ...
${else} ... ${/if}` block. The predicate is an `Expression<Func<TModel, bool>>`
translated to SF condition syntax: `m.RiskLevel == "High"` → `riskLevel === 'High'`,
`m.Balance > 0` → `balance > 0`, `!m.IsActive` → `!isActive`, `&&` → `&&`,
`||` → `||`. The `then` / `@else` lambdas configure a `FusionConditionalBuilder<TModel>`
(Span / Badge / Icon / Div / Img / Button / DispatchButton / Raw). Two overloads:
then-only and then+else.

### WhenTemplate — then-only (equality predicate)
Renders the `then` body only when the row matches; nothing otherwise.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .WhenTemplate(m => m.RiskLevel == "High",
        then: t => t
            .Badge(m => m.RiskLevel, "rounded bg-red-100 px-2 py-0.5 text-xs font-bold text-red-700"))
    .Render();
```

### WhenTemplate — then + else (the shipped risk badge)
The real grid pattern: a red badge for high risk, a neutral badge otherwise. Note the named `then:` / `@else:` arguments (`else` is a C# keyword, so it is escaped).

```csharp
var riskTemplate = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .WhenTemplate(m => m.RiskLevel == "High",
        then: t => t
            .Badge(m => m.RiskLevel, "rounded bg-red-100 px-2 py-0.5 text-xs font-bold text-red-700"),
        @else: e => e
            .Badge(m => m.RiskLevel, "rounded bg-slate-100 px-2 py-0.5 text-xs font-medium text-slate-700"))
    .Render();
```

### WhenTemplate — numeric comparison predicate
`>` translates directly to the SF `>` operator; `then` / `else` style by balance.

```csharp
var template = FusionTemplate.Create<ResidentBillingItem>()
    .WhenTemplate(m => m.Balance > 0,
        then: t => t
            .Badge("Balance due", "rounded bg-red-100 px-2 py-0.5 text-xs font-bold text-red-700"),
        @else: e => e
            .Badge("Paid", "rounded bg-green-100 px-2 py-0.5 text-xs font-semibold text-green-700"))
    .Render();
```

### WhenTemplate — boolean property predicate
A bare boolean member reads as the SF truthiness of that field.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .WhenTemplate(m => m.AssessmentOverdue,
        then: t => t
            .Icon("warning", "text-red-600"))
    .Render();
```

### WhenTemplate — negated boolean predicate
`!member` translates to the SF `!field` negation.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .WhenTemplate(m => !m.IsActive,
        then: t => t
            .Badge("Discharged", "rounded bg-slate-200 px-2 py-0.5 text-xs text-slate-600"))
    .Render();
```

### WhenTemplate — compound predicate (&& / ||)
Logical `&&` / `||` lower to the matching SF operators; group with a nested div in the body.

```csharp
var template = FusionTemplate.Create<ResidentBillingItem>()
    .WhenTemplate(m => m.Balance > 0 && m.BillingStatus != "Paid",
        then: t => t
            .Div(d => d
                .CssClass("flex items-center")
                .Icon("warning", "text-amber-600 mr-1")
                .Span("Action required", "text-xs font-semibold text-amber-700")))
    .Render();
```

### WhenTemplate — else body with a different element kind
The `then` and `@else` bodies are independent; they need not emit the same element kind.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .WhenTemplate(m => m.RiskLevel == "High",
        then: t => t
            .Icon("warning", "text-red-600"),
        @else: e => e
            .Span(m => m.RiskLevel, "text-xs text-slate-500"))
    .Render();
```

---

## ShowTemplateIf — conditional SSR block (then-only alias)

`ShowTemplateIf` (current source: `ShowIf`) is the thin then-only alias of
`WhenTemplate`: render the content only when the predicate holds, no else branch.
Same predicate translation as `WhenTemplate`.

### ShowTemplateIf — show only when true
Emits the body inside a `${if(...)}` block with no `${else}`.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .ShowTemplateIf(m => m.AssessmentOverdue,
        content: t => t
            .Icon("warning", "text-red-600 mr-1"))
    .Render();
```

### ShowTemplateIf — gate an action by row state
Only render the review button for high-risk residents.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .ShowTemplateIf(m => m.RiskLevel == "High",
        content: t => t
            .DispatchButton("Review", "grid:review-resident", m => m.ResidentId, "e-small e-primary"))
    .Render();
```

---

## Id — fixed element id on the root div

### Id
Sets the `id` attribute on the wrapping root `<div>` (the one `Render()` emits). A fixed string, not bound.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Id("resident-cell")
    .Span(m => m.ResidentName)
    .Render();
```

---

## CssClass — append a class to the root div

`CssClass` (current source: `Class`) appends a CSS class to the wrapping root
`<div>`. Call it more than once to accumulate classes (they are space-joined).
This is the SSR-string lane — it is **not** `AddClass` (runtime DOM mutation).

### CssClass — single class on the root
Adds one class to the template's root div.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .CssClass("grid-resident-template space-y-1")
    .Span(m => m.ResidentName)
    .Render();
```

### CssClass — accumulate multiple classes
Each call appends; the root div ends with all of them.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .CssClass("grid-resident-template")
    .CssClass("space-y-1")
    .Span(m => m.ResidentName)
    .Render();
```

---

## Attribute — arbitrary HTML attribute on the root div

`Attribute` (current source: `Attr`) sets an arbitrary `name="value"` attribute on
the wrapping root `<div>`. Re-setting the same name overwrites it.

### Attribute — data attribute on the root
Adds `data-resident="row"` to the template's root div.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Attribute("data-resident", "row")
    .Span(m => m.ResidentName)
    .Render();
```

### Attribute — multiple attributes
Stack several attribute calls in a tall chain.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Attribute("data-kind", "resident")
    .Attribute("role", "gridcell")
    .Span(m => m.ResidentName)
    .Render();
```

---

## Model-bound expression semantics (cross-cutting)

Every `TProp` expression lowers through `FusionTemplateExpression`, shared by all
bound nodes (`Text`, `Span`, `Img`, `Badge`, `Link`, `ButtonFor`,
`DispatchButton`). The rules below apply uniformly.

### Property binding — PascalCase C# → camelCase SF binding
`m => m.ResidentName` emits `${residentName}`, matching the global PascalCase→camelCase JSON serialization.

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Span(m => m.ResidentName)   // → <span>${residentName}</span>
    .Render();
```

### Nested-path binding — dotted members
`m => m.PrimaryContact.Name` emits `${primaryContact.name}` (each segment camelCased, joined by dots).

```csharp
var template = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .Span(m => m.PrimaryContact.Name)   // → <span>${primaryContact.name}</span>
    .Render();
```

### Condition binding — predicate operators
In `WhenTemplate` / `ShowTemplateIf`, the predicate maps C# operators to SF
condition tokens: `==` → `===`, `!=` → `!==`, `>`/`>=`/`<`/`<=` pass through,
`&&` → `&&`, `||` → `||`, `!x` → `!x`. String literals quote with single quotes
(`"High"` → `'High'`), booleans render `true`/`false`, numbers use invariant
culture.

```csharp
var template = FusionTemplate.Create<ResidentBillingItem>()
    .WhenTemplate(m => m.BillingStatus == "Paid",      // → billingStatus === 'Paid'
        then: t => t.Badge("Paid", "text-green-700"))
    .Render();
```

---

## Putting it together — a full grid column template set

A realistic Operations grid defines several column templates from the same row
model, each a tall fluent chain ending in `.Render()`, then assigns them to grid
columns.

```csharp
var residentTemplate = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .CssClass("grid-resident-template space-y-1")
    .Div(d => d
        .CssClass("font-semibold text-slate-900")
        .Span(m => m.ResidentName))
    .Div(d => d
        .CssClass("text-xs text-slate-600")
        .Span("Suite ", "font-medium")
        .Span(m => m.Suite)
        .Span(" | ")
        .Span(m => m.CareLevel))
    .Render();
```

```csharp
var riskTemplate = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .WhenTemplate(m => m.RiskLevel == "High",
        then: t => t
            .Badge(m => m.RiskLevel, "rounded bg-red-100 px-2 py-0.5 text-xs font-bold text-red-700"),
        @else: e => e
            .Badge(m => m.RiskLevel, "rounded bg-slate-100 px-2 py-0.5 text-xs font-medium text-slate-700"))
    .Render();
```

```csharp
var actionTemplate = FusionTemplate.Create<ResidentDirectoryGridItem>()
    .CssClass("grid-action-template")
    .DispatchButton("Review", "grid:review-resident", m => m.ResidentId, "e-small e-primary")
    .Render();
```

---

## Node availability — root vs conditional builder

The root `FusionTemplateBuilder<TModel>` and the nested
`FusionConditionalBuilder<TModel>` (the `then` / `@else` body) expose overlapping
but not identical surfaces. The conditional body is for the SSR `${if}` content,
so it omits root-only structural and link/button-binding members.

| Node | Root builder | Conditional body |
|---|---|---|
| `Text<TProp>` / `Text(string)` | yes | no |
| `Span` (4 overloads) | yes | yes |
| `Div(lambda)` | yes | yes |
| `Img` | yes (3 overloads: bare/+css/+css+alt) | yes (2 overloads: bare/+css) |
| `Badge` | yes | yes |
| `Icon` | yes | yes |
| `Button` | yes | yes |
| `ButtonFor<TProp>` | yes | no |
| `DispatchButton<TProp>` (`EventButton`) | yes | yes |
| `Link<THref,TText>` | yes | no |
| `Raw` | yes | yes |
| `WhenTemplate` / `ShowTemplateIf` | yes | no (no nested branching) |
| `Id` / `CssClass` / `Attribute` | yes (root div only) | no |
| `Render` / `ToString` | yes | (internal `Render` only) |
