# AST Proof — Two DSL Grammar Patterns (Grounded in Source Signatures)

The proof is the SIGNATURE. Source is the complete authority. A sandbox example
is illustration only, cited WHERE ONE EXISTS.

Two patterns under proof:

1. **Multi-statement conditions** — `Then`/`ElseIf`/`Else` accept an
   `Action<PipelineBuilder<TModel>>` so the callback may emit many statements:
   `then => { then.A(); then.B(); then.C(); }`.
2. **Multi-event component** — `.Reactive(...)` returns the component builder so
   different events chain: `b.Reactive(e => e.Changed, ...).Reactive(e => e.Blur, ...)`.

Row format: `| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |`
- Callback = callback param type if any, else `-`.
- ReturnsSelf = `yes` when the member returns its own receiver type (chainable/repeatable).
- A Callback handing back a `PipelineBuilder` is a NESTING (recursion) point.
- ReturnsSelf=yes means the member can be CHAINED/REPEATED.

Paths are relative to repo root `Alis.Reactive/`.

---

## Pattern 1 — Multi-Statement Conditions (Then / ElseIf / Else)

### PROOF (signature is the authority)

The branch callback type is `Action<PipelineBuilder<TModel>>` on every entry
point. A lambda bound to `Action<PipelineBuilder<TModel>>` is a statement body,
so it permits any number of statements against the supplied builder
(`then => { then.A(); then.B(); then.C(); }`). The same `PipelineBuilder<TModel>`
is the full pipeline DSL receiver, making each branch a NESTING (recursion) point
back into the whole pipeline grammar.

Exact callback-type evidence:

- `GuardBuilder<TModel>.Then(Action<PipelineBuilder<TModel>> pipeline)` — the
  public entry to a branch — `Alis.Reactive/Builders/Conditions/GuardBuilder.cs:126`.
- `BranchBuilder<TModel>.Else(Action<PipelineBuilder<TModel>> pipeline)` —
  `Alis.Reactive/Builders/Conditions/BranchBuilder.cs:61`.
- The `Then` core that both pipeline-rooted and branch-rooted continuations
  implement is declared as
  `Then(ConditionGraph condition, Action<PipelineBuilder<TModel>> pipeline)` —
  `Alis.Reactive/Builders/Conditions/ConditionContinuation.cs:73-75` — and each
  override builds a fresh `PipelineBuilder<TModel>` and invokes the callback
  (`ConditionContinuation.cs:95-96`, `ConditionContinuation.cs:121-122`).
- `ElseIf` returns a `ConditionSourceBuilder<TModel,TProp>` whose operators (e.g.
  `.Eq`, `.Gte`) return a `GuardBuilder<TModel>`, whose `.Then` again takes
  `Action<PipelineBuilder<TModel>>` — so each `ElseIf` tier ends in the same
  multi-statement callback.

### Sandbox illustration (multi-statement bodies + ElseIf ladder + Else)

`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Patterns/PlaygroundSyntax/ReactiveConditions.cshtml:40`
— `.Then(then => { ...7 statements... })`, then `.ElseIf(args, x => x.Value).Eq("inactive").Then(then => { ... })` (`:50-51`), closed by `.Else(else_ => { ... })` (`:60`). The first `Then` body runs seven statements against the `then` pipeline builder:

```csharp
p.When(args, x => x.Value).Eq("active")
    .Then(then =>
    {
        then.Component<FusionNumericTextBox>(m => m.Amount).SetValue(100);
        then.Component<NativeDropDown>(m => m.Address!.City).SetValue("seattle");
        then.Element("status-result").SetText("Active: Amount=100, City=Seattle");
        then.Element("status-result").AddClass("text-emerald-700");
        then.Element("status-result").RemoveClass("text-amber-600");
        then.Element("status-result").RemoveClass("text-slate-500");
        then.Element("address-section").Show();
    })
    .ElseIf(args, x => x.Value).Eq("inactive")
    .Then(then => { /* 6 statements */ })
    .Else(else_ => { /* 5 statements */ });
```

A second illustration with a numeric `Gte` tier ladder lives in the same file at
`ReactiveConditions.cshtml:80,87-88,95`.

### Table — condition-cluster builders

#### GuardBuilder<TModel>
Source: `Alis.Reactive/Builders/Conditions/GuardBuilder.cs`

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| GuardBuilder<TModel> | And<TPayload,TProp>(TPayload payload, Expression<Func<TPayload,TProp>> path) | ConditionSourceBuilder<TModel,TProp> | - | no | GuardBuilder.cs:43 |
| GuardBuilder<TModel> | And<TPayload,TProp>(ResponseBody<TPayload> responseBody, Expression<Func<TPayload,TProp>> path) | ConditionSourceBuilder<TModel,TProp> | - | no | GuardBuilder.cs:52 |
| GuardBuilder<TModel> | Or<TPayload,TProp>(TPayload payload, Expression<Func<TPayload,TProp>> path) | ConditionSourceBuilder<TModel,TProp> | - | no | GuardBuilder.cs:62 |
| GuardBuilder<TModel> | Or<TPayload,TProp>(ResponseBody<TPayload> responseBody, Expression<Func<TPayload,TProp>> path) | ConditionSourceBuilder<TModel,TProp> | - | no | GuardBuilder.cs:71 |
| GuardBuilder<TModel> | And<TProp>(TypedSource<TProp> source) | ConditionSourceBuilder<TModel,TProp> | - | no | GuardBuilder.cs:81 |
| GuardBuilder<TModel> | Or<TProp>(TypedSource<TProp> source) | ConditionSourceBuilder<TModel,TProp> | - | no | GuardBuilder.cs:88 |
| GuardBuilder<TModel> | And(Func<ConditionStart<TModel>,GuardBuilder<TModel>> inner) | GuardBuilder<TModel> | Func<ConditionStart<TModel>,GuardBuilder<TModel>> | yes | GuardBuilder.cs:95 |
| GuardBuilder<TModel> | Or(Func<ConditionStart<TModel>,GuardBuilder<TModel>> inner) | GuardBuilder<TModel> | Func<ConditionStart<TModel>,GuardBuilder<TModel>> | yes | GuardBuilder.cs:106 |
| GuardBuilder<TModel> | Not() | GuardBuilder<TModel> | - | yes | GuardBuilder.cs:118 |
| GuardBuilder<TModel> | **Then(Action<PipelineBuilder<TModel>> pipeline)** | **BranchBuilder<TModel>** | **Action<PipelineBuilder<TModel>>** (NESTING) | no | GuardBuilder.cs:126 |

#### BranchBuilder<TModel>
Source: `Alis.Reactive/Builders/Conditions/BranchBuilder.cs`

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| BranchBuilder<TModel> | ElseIf<TPayload,TProp>(TPayload payload, Expression<Func<TPayload,TProp>> path) | ConditionSourceBuilder<TModel,TProp> | - | no | BranchBuilder.cs:29 |
| BranchBuilder<TModel> | ElseIf<TPayload,TProp>(ResponseBody<TPayload> responseBody, Expression<Func<TPayload,TProp>> path) | ConditionSourceBuilder<TModel,TProp> | - | no | BranchBuilder.cs:40 |
| BranchBuilder<TModel> | ElseIf<TProp>(TypedSource<TProp> source) | ConditionSourceBuilder<TModel,TProp> | - | no | BranchBuilder.cs:52 |
| BranchBuilder<TModel> | **Else(Action<PipelineBuilder<TModel>> pipeline)** | void | **Action<PipelineBuilder<TModel>>** (NESTING) | no | BranchBuilder.cs:61 |

Note: after `ElseIf(...)` returns a `ConditionSourceBuilder`, an operator (e.g.
`.Eq`/`.Gte`) returns a `GuardBuilder<TModel>` whose `.Then(...)` (GuardBuilder.cs:126)
returns the `BranchBuilder<TModel>` again — that is how the `ElseIf ... Then ...`
ladder repeats. The branch-rooted continuation returns the SAME branch instance
(`ConditionContinuation.cs:124`), so the ladder is chainable/repeatable.

#### ConditionStart<TModel>
Source: `Alis.Reactive/Builders/Conditions/ConditionStart.cs`

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| ConditionStart<TModel> | When<TPayload,TProp>(TPayload payload, Expression<Func<TPayload,TProp>> path) | ConditionSourceBuilder<TModel,TProp> | - | no | ConditionStart.cs:16 |
| ConditionStart<TModel> | When<TPayload,TProp>(ResponseBody<TPayload> responseBody, Expression<Func<TPayload,TProp>> path) | ConditionSourceBuilder<TModel,TProp> | - | no | ConditionStart.cs:25 |
| ConditionStart<TModel> | When<TProp>(TypedSource<TProp> source) | ConditionSourceBuilder<TModel,TProp> | - | no | ConditionStart.cs:35 |
| ConditionStart<TModel> | Confirm(string message) | GuardBuilder<TModel> | - | no | ConditionStart.cs:42 |

#### ConditionSourceBuilder<TModel,TProp>
Source: `Alis.Reactive/Builders/Conditions/ConditionSourceBuilder.cs`
Every operator returns `GuardBuilder<TModel>` (which then exposes `.Then(...)`).

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| ConditionSourceBuilder<TModel,TProp> | Eq(TProp operand) | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:49 |
| ConditionSourceBuilder<TModel,TProp> | NotEq(TProp operand) | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:51 |
| ConditionSourceBuilder<TModel,TProp> | Gt(TProp operand) | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:53 |
| ConditionSourceBuilder<TModel,TProp> | Gte(TProp operand) | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:55 |
| ConditionSourceBuilder<TModel,TProp> | Lt(TProp operand) | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:57 |
| ConditionSourceBuilder<TModel,TProp> | Lte(TProp operand) | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:59 |
| ConditionSourceBuilder<TModel,TProp> | Truthy() | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:63 |
| ConditionSourceBuilder<TModel,TProp> | Falsy() | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:65 |
| ConditionSourceBuilder<TModel,TProp> | IsNull() | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:67 |
| ConditionSourceBuilder<TModel,TProp> | NotNull() | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:69 |
| ConditionSourceBuilder<TModel,TProp> | IsEmpty() | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:71 |
| ConditionSourceBuilder<TModel,TProp> | NotEmpty() | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:73 |
| ConditionSourceBuilder<TModel,TProp> | In(params TProp[] values) | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:77 |
| ConditionSourceBuilder<TModel,TProp> | NotIn(params TProp[] values) | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:79 |
| ConditionSourceBuilder<TModel,TProp> | Between(TProp low, TProp high) | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:83 |
| ConditionSourceBuilder<TModel,TProp> | Contains(string substring) | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:88 |
| ConditionSourceBuilder<TModel,TProp> | StartsWith(string prefix) | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:91 |
| ConditionSourceBuilder<TModel,TProp> | EndsWith(string suffix) | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:94 |
| ConditionSourceBuilder<TModel,TProp> | Matches(string pattern) | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:97 |
| ConditionSourceBuilder<TModel,TProp> | MinLength(int length) | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:100 |
| ConditionSourceBuilder<TModel,TProp> | ArrayContains(object item) | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:105 |
| ConditionSourceBuilder<TModel,TProp> | Eq(TypedSource<TProp> right) | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:112 |
| ConditionSourceBuilder<TModel,TProp> | NotEq(TypedSource<TProp> right) | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:114 |
| ConditionSourceBuilder<TModel,TProp> | Gt(TypedSource<TProp> right) | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:116 |
| ConditionSourceBuilder<TModel,TProp> | Gte(TypedSource<TProp> right) | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:118 |
| ConditionSourceBuilder<TModel,TProp> | Lt(TypedSource<TProp> right) | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:120 |
| ConditionSourceBuilder<TModel,TProp> | Lte(TypedSource<TProp> right) | GuardBuilder<TModel> | - | no | ConditionSourceBuilder.cs:122 |

---

## Pattern 2 — Multi-Event Component (`.Reactive(...)` chaining)

### PROOF (return type is the authority)

Each Fusion `.Reactive(...)` extension is declared
`public static <SfBuilder> Reactive<TModel,TArgs>(this <SfBuilder> builder, ...)`
and `return builder;`. Because the RETURN TYPE equals the RECEIVER builder type
(`ReturnsSelf = yes`), the result of one `.Reactive(...)` is the same builder, so
a second `.Reactive(...)` for a different event is type-valid:

`b.Reactive(plan, e => e.Changed, ...).Reactive(plan, e => e.Blur, ...)`.

This return-type fact is the proof on its own.

Representative signatures (both return the receiver builder type):

- `FusionDatePickerReactiveExtensions`:
  `public static DatePickerBuilder Reactive<TModel,TArgs>(this DatePickerBuilder builder, ReactivePlan<TModel> plan, Func<FusionDatePickerEvents,TypedEvent<TArgs>> eventSelector, Action<TArgs,PipelineBuilder<TModel>> pipeline)` — returns `DatePickerBuilder` —
  `Alis.Reactive.Fusion/Components/FusionDatePicker/FusionDatePickerReactiveExtensions.cs:37-52` (`return builder;` at `:51`).
- `FusionTextBoxReactiveExtensions`:
  `public static TextBoxBuilder Reactive<TModel,TArgs>(this TextBoxBuilder builder, ReactivePlan<TModel> plan, Func<FusionTextBoxEvents,TypedEvent<TArgs>> eventSelector, Action<TArgs,PipelineBuilder<TModel>> pipeline)` — returns `TextBoxBuilder` —
  `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxReactiveExtensions.cs:25-40` (`return builder;` at `:39`).

The event selector `Func<FusionTextBoxEvents, TypedEvent<TArgs>>` ranges over a
component-specific events object that exposes several DISTINCT events, so distinct
selectors target distinct events. `FusionTextBoxEvents` exposes four:
`Input`, `Changed`, `Focus`, `Blur` —
`Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxEvents.cs` (Input, Changed, Focus, Blur properties).

The pipeline callback is `Action<TArgs, PipelineBuilder<TModel>>` — also a NESTING
point: each event handler body is a full pipeline (`(args, p) => { p.Element(...)...; }`).

#### Cluster fact (all 49 Fusion `*ReactiveExtensions.cs` files)

47 of 49 `Reactive` extensions return their receiver SF builder type
(`ReturnsSelf = yes`, chainable for multiple events). The exception is
`FusionSmartTextArea`, whose TWO `Reactive` overloads return `void`
(NOT chainable) —
`Alis.Reactive.Fusion/Components/FusionSmartTextArea/FusionSmartTextAreaReactiveExtensions.cs:11`
and `:22`. So the pattern holds for every input/display builder except
FusionSmartTextArea, which is explicitly NOT chainable by its `void` return.

### Sandbox illustration (4 different events chained on ONE builder)

`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/TextBox/Index.cshtml:29-52`
chains four `.Reactive(...)` calls — `Input` (`:29`), `Changed` (`:34`),
`Focus` (`:43`), `Blur` (`:48`) — on a single `FusionTextBox(b => b...)` builder:

```csharp
.FusionTextBox(b => b
    .Placeholder("Resident name")
    .ShowClearButton(true)
    .Reactive(plan, evt => evt.Input,   (args, p) => { /* commands */ })
    .Reactive(plan, evt => evt.Changed, (args, p) => { /* commands */ })
    .Reactive(plan, evt => evt.Focus,   (args, p) => { /* commands */ })
    .Reactive(plan, evt => evt.Blur,    (args, p) => { /* commands */ }));
```

This is a real, in-repo illustration of multiple `.Reactive` for multiple
distinct events. (The proof does not depend on it — the `return builder;`
return type already establishes type validity.)

### Table — `.Reactive(...)` extension members (representative + cluster)

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| TextBoxBuilder (this) | Reactive<TModel,TArgs>(ReactivePlan<TModel> plan, Func<FusionTextBoxEvents,TypedEvent<TArgs>> eventSelector, Action<TArgs,PipelineBuilder<TModel>> pipeline) | TextBoxBuilder | Action<TArgs,PipelineBuilder<TModel>> (NESTING) | yes | FusionTextBox/FusionTextBoxReactiveExtensions.cs:25 |
| DatePickerBuilder (this) | Reactive<TModel,TArgs>(ReactivePlan<TModel> plan, Func<FusionDatePickerEvents,TypedEvent<TArgs>> eventSelector, Action<TArgs,PipelineBuilder<TModel>> pipeline) | DatePickerBuilder | Action<TArgs,PipelineBuilder<TModel>> (NESTING) | yes | FusionDatePicker/FusionDatePickerReactiveExtensions.cs:37 |
| SmartTextAreaBuilder (this) | Reactive<TModel,TArgs>(...) | void | Action<TArgs,PipelineBuilder<TModel>> (NESTING) | no | FusionSmartTextArea/FusionSmartTextAreaReactiveExtensions.cs:11 |
| SmartTextAreaBuilder (this) | Reactive<TModel,TProp,TArgs>(...) | void | Action<TArgs,PipelineBuilder<TModel>> (NESTING) | no | FusionSmartTextArea/FusionSmartTextAreaReactiveExtensions.cs:22 |

Cluster: the remaining 45 receiver builders each follow the TextBox/DatePicker
shape — `Reactive<TModel,TArgs>(this <Builder>, ReactivePlan<TModel>, Func<<Events>,TypedEvent<TArgs>>, Action<TArgs,PipelineBuilder<TModel>>) : <Builder>`,
`ReturnsSelf = yes`. Distinct builder return types confirmed (one `Reactive`
each): `AutoCompleteBuilder`, `CheckBoxBuilder`, `ColorPickerBuilder`,
`ComboBoxBuilder`, `DateRangePickerBuilder`, `DateTimePickerBuilder`,
`DropDownListBuilder`, `DropDownTreeBuilder`, `InPlaceEditorBuilder`,
`MaskedTextBoxBuilder`, `MultiColumnComboBoxBuilder`, `MultiSelectBuilder`,
`NumericTextBoxBuilder`, `OtpInputBuilder`, `RatingBuilder`,
`RichTextEditorBuilder`, `SliderBuilder`, `SwitchBuilder`, `TextAreaBuilder`,
`TimePickerBuilder`, `UploaderBuilder` (plus display/app builders in the same
folder set). Evidence: `grep "public static <T> Reactive"` over
`Alis.Reactive.Fusion/Components/*/*ReactiveExtensions.cs` yields 47 non-void
builder returns and 2 `void` (FusionSmartTextArea).

---

## Verdict

- **Pattern 1 — Multi-statement conditions: PROVEN.** `Then`/`Else` take
  `Action<PipelineBuilder<TModel>>` (`GuardBuilder.cs:126`, `BranchBuilder.cs:61`,
  core at `ConditionContinuation.cs:73-75`), and `ElseIf` tiers funnel back into
  the same `.Then(Action<PipelineBuilder<TModel>>)`. Multi-statement bodies are
  illustrated at `ReactiveConditions.cshtml:40` (7 statements in one `Then`).
- **Pattern 2 — Multi-event component: PROVEN.** Each `.Reactive(...)` returns its
  receiver SF builder type (`FusionTextBoxReactiveExtensions.cs:25`,
  `FusionDatePickerReactiveExtensions.cs:37` — `return builder;`), making
  chained `.Reactive(e => e.Input, ...).Reactive(e => e.Changed, ...)` type-valid;
  `FusionTextBoxEvents` exposes 4 distinct events. Illustrated by 4 chained,
  distinct-event `.Reactive` calls at
  `Components/Fusion/TextBox/Index.cshtml:29-52`. The single exception
  (FusionSmartTextArea returns `void`, not chainable) is recorded explicitly.
