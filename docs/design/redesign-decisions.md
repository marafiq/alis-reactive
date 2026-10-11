# Connected redesign

Branch `feature/component-redesign`, read at `0702327`. What is built is recorded in
[`bring-your-own-component.md`](bring-your-own-component.md). This file records the owner's
decisions and the design proposed next. Nothing here is built yet.

## Owner's decisions

- **Greenfield.** The only constraints are that everything is fully typed and that every feature
  the framework has today is preserved. Beyond that, the design should be much better and much
  smaller.
- **Modular, with the relationship inverted.** The core has a few concepts, and every module is a
  use of them rather than a grammar of its own. No source generator.
- **The developer should not learn a new API.** A Syncfusion developer writes Syncfusion's API.
- **Validation is written once and works on server and browser.** That stays. Its grammar should
  follow an established one.
- **Arrays are central.** Most components are array-driven: grids, lists, dropdowns, charts,
  schedules, kanban boards and trees.
- **The framework controls identity.** The developer gets a public method they must use.
- **Mounting is minor.** Syncfusion constructs itself; the framework only locates
  `ej2_instances[0]`.

## The problem: one concept, six grammars

Every module reads values from JavaScript objects, tests them, and calls members. Each one invents
its own way to say so:

| Module | Its own grammar | Lines today |
|---|---|---|
| Components | `Property<T>("js")`, `ComponentMethod<T1..T3>`, `ComponentFunction<..>`, `Argument<T>`, `Field`, `.Passing`, the verbs `Set`/`Call`/`Get` | |
| Conditions | 27 operator methods, a left fold with no precedence (`When(a).Or(b).And(c)` is `(a \|\| b) && c`) | C# 1,312, TS 555 |
| Validation | `ClientRule`, 18 `WhenField*` methods, rule names that differ from the operators, and a second runtime evaluator that disagrees with conditions | C# 3,343, TS 1,203 |
| Arrays | a 251-line LINQ-subset compiler, 15 `ReactiveArray` methods, 9 array ops and their own engine | |
| Plugins | string builders, 9+9 duplicated `Arg` overloads, 442 lines of plugin-only contracts | arrays, plugins and gather together: 2,722 |
| Gather | `Include`, `FromEvent`, `RouteParam`, `Plugin` | |

Eight `TypedSource` subclasses carry values between all of these modules.

## The core: four parts

### 1. Declarations: a JavaScript object is a C# interface

The interface mirrors the object's own API. C# names are camel-cased by convention, and
`[Js("...")]` is needed only when a name differs. The same form serves components (Fusion, Native
or brought), plugins, app objects, payloads that have behavior, and the DOM element.

```csharp
[Fusion]
public interface FusionGrid
{
    object[] DataSource { get; set; }
    TRow[] GetSelectedRecords<TRow>();
    void SelectRow(int index);
    void SetCellValue<TRow, TValue>(object key, Expression<Func<TRow, TValue>> field, TValue value);
    JsEvent<RowSelectedArgs<TRow>> RowSelected<TRow>();
}
```

A C# signature is the typed declaration, with any arity, overloads and generics. The developer
writes what Syncfusion's documentation says:

```csharp
grid.Call(g => g.SelectRow(2));
grid.Get(g => g.GetSelectedRecords<Resident>());
rate.Set(r => r.Value, 5m);
dialog.Call(d => d.Show());
```

There is no shortcut layer because the declaration is the familiar API.

**Replaces:**
- the member factories and member types
- `Argument<T>`, `Field`, `.Passing` and the arity caps
- the plugin contracts

### 2. Values

`Value<T>` is the one source type. It replaces the eight `TypedSource` subclasses. A value can come
from:
- a member of a declared object
- a model member (meaning the input bound to it)
- an event payload
- a response
- the URL
- a literal

### 3. Lambdas, one compiler

Every expression is a C# lambda. One compiler, grown from today's `ElementExpressionCompiler`,
turns it into a plan expression:

- **Member reads and calls on declarations** become `read` and `call`.
- **`== != < <= > >= && || !`** become compare, `all`, `any` and `not`, with C#'s own precedence.
- **Members of strings and arrays** map to JavaScript built-ins through one table. For example:
  `StartsWith`→`startsWith`, `Length`→`length`, `Contains`→`includes`, `Where`→`filter`,
  `Select`→`map`, `Any`→`some`, `All`→`every`, `First`→`find`, `OrderBy`→a sort, `Sum`→`reduce`,
  `Skip`/`Take`→`slice`.
- **A nested lambda** (`rows.Where(r => r.Open)`) becomes a `lambda` value, which the runtime runs
  as a JavaScript function.
- **An object initializer** (`new SaveAdmission { Age = m.Age }`) becomes an object value.
- **Anything else** fails at render. An analyzer reports it at compile time, as ALIS009 already
  does for typed slices.

### 4. One evaluator

The runtime has one function, `evaluate(expression, scope)`, for every expression kind: literal,
read, call, lambda, object, array, compare, `all`, `any` and `not`.
- Conditions are boolean expressions.
- Array work runs on JavaScript's own array methods.
- `Confirm` is an async call on the confirm object.
- Sync work stays sync.

## Each module as a use of the core

**Components.** A component is a declaration plus a render helper.
- `field.Identity` writes the framework's `id` and `name`.
- `field.Render<T>` and `plan.Render<T>` throw at render, naming the expected value, if the markup
  does not carry them.

**Conditions.** `p.When` takes a lambda over the model, a payload, a response or an object:

```csharp
p.When(m => m.Age > 65 && m.CareLevel == CareLevel.Memory)   // the inputs bound to these members
p.When(args, a => a.Value > 100m)
p.When(grid, g => g.GetSelectedRecords<Resident>().Length > 0)
```

`Then`, `ElseIf`, `Else` and `Confirm` stay.

**Deleted:**
- the operator builder
- the guard and continuation types
- the string cases in `compare-engine.ts`

**Validation.** Validation is plain FluentValidation, the grammar .NET developers already know:

```csharp
public sealed class AdmissionValidator : ReactiveValidator<Admission>
{
    public AdmissionValidator()
    {
        RuleFor(x => x.Age).GreaterThan(0).WithMessage("Enter the resident's age.");
        RuleFor(x => x.DischargeDate).GreaterThan(x => x.AdmitDate);
        When(x => x.CareLevel == CareLevel.Memory, () =>
            RuleFor(x => x.Physician).NotEmpty());
        RuleForEach(x => x.Contacts).SetValidator(new ContactValidator());
    }
}
```

- **Which rules reach the browser.** The framework reads FluentValidation's public rule descriptors
  for the rules a browser can run: not-empty, not-null, length, comparison against a literal or a
  peer, between, regex, email and credit card. This is the metadata FluentValidation's own
  client-side adapters were built on.
- **`When` takes an expression.** The server compiles it. The browser receives it through the same
  compiler `p.When` uses, so one lambda works in a view and in a validator.
- **Server-only rules.** `Must`, async rules and delegate conditions stay server-only with no extra
  code.
- **In the plan.** A browser rule is `{field, test, message, when}`, run by the one evaluator.
- **Deleted:**
  - `ClientRule` and its builders
  - the 18 `WhenField*` methods
  - the field-condition builders
  - the operand and activation types
  - three interfaces
  - `rule-engine.ts` and `rule-operands.ts`
- **The double statements go.** The about 100 restated `RuleFor` + `ClientRule` pairs become one
  statement each.
- **Rule change.** This replaces the repo's "no rule extraction" rule. Descriptors are
  FluentValidation's public API, not reflection over private state.
- **Learned from:** FluentValidation; the jQuery unobtrusive adapters (server rules projected to
  browser metadata); Zod and Valibot (one schema for both sides, with cross-field refinements).

**Arrays: the data plane.** Arrays connect components, HTTP, conditions and validation:
- a response list becomes a component's data source;
- selected records become a request body;
- rows are filtered, sorted and projected in between;
- `Any`, `All` and `Count` drive conditions;
- `RuleForEach` validates each row.

Any array value takes LINQ inside a lambda:

```csharp
grid.Set(g => g.DataSource, res.Read(x => x.Rows.Where(r => r.Status == "open").OrderBy(r => r.Name)));
p.Post("/discharge", grid, g => g.GetSelectedRecords<Resident>().Select(r => r.Id));
p.When(m => m.Contacts.Any(c => c.IsPrimary))
```

The mapping table gives the breadth; there is no array engine to maintain. **Deleted:**
`ReactiveArray`, the array-op node and `array-op-engine.ts`.

**Plugins.** A plugin is a declaration found by name instead of on an element. It uses the same
verbs, and its name is the only string:

```csharp
[Plugin("addressLookup")]
public interface AddressLookup
{
    Task<Address> Find(string postcode);
}
```

Arrays are now in the core, so plugins are for real escape hatches: browser APIs and outside
libraries.

**Deleted:**
- the `Plugin` base class
- `PluginContract.cs`
- `PluginTypeBuilder`
- `PluginMemberBuilder`
- `GatherBuilder.Plugin`

**HTTP and gather.** `Get`, `Post`, `Put`, `Delete`, `Response`, `Chained`, `Parallel`,
`WhileLoading` and `Into` stay. The body is a typed lambda:
- a typed request: `p.Post<SaveAdmission>(url, m => new SaveAdmission { Age = m.Age, Contacts = m.Contacts })`
- the whole form, posted by binding names: `p.Post(url, m => m)`

This replaces `Include`, `FromEvent` and `RouteParam`.

**Unchanged in grammar:**
- triggers: `.Reactive(c => c.Change, ...)`
- dispatch
- partials and slots
- realtime
- app objects, declared as interfaces with fixed IDs
- the DOM element: its operations already lower to `set` and `call`

## Plan document

```json
{
  "planId": "…", "scope": {},
  "vendors":      { "fusion": {} },
  "declarations": { "FusionGrid": { "vendor": "fusion", "members": {} } },
  "objects": {
    "Ns_Admission__Age": { "kind": "input", "declaration": "FusionNumericTextBox", "name": "Age", "valueMember": "value" },
    "roster":            { "kind": "component", "declaration": "FusionGrid" },
    "addressLookup":     { "kind": "plugin", "declaration": "AddressLookup" },
    "admission-form":    { "kind": "form", "rules": [ { "field": "…", "test": {}, "message": "…", "when": {} } ] }
  },
  "behaviors": [ { "trigger": {}, "reaction": {} } ]
}
```

- **One entry per declaration.** Today's types repeat per element (`fusion.component.<id>`,
  `BrowserObjectId.cs:13`).
- **Objects carry only their own fields.** There are no `binding:{kind:"none"}` placeholders
  (`plan.ts:86-120,148`).
- **One expression union.** Every value, condition and rule test is an expression.

## Rejected

- **A source generator:** the owner rejected it.
- **Recording proxies** (`grid.SelectRow(2)` runs and records itself): a runtime value cannot flow
  through a `T`-typed parameter without a hidden side channel, and proxies on net48 need
  `Reflection.Emit`.
- **Operators on values** (`rate > 100m`): this needs `==` overloaded on a class, ordering cannot
  be restricted by `T`, and arrays would still need lambdas, so the grammar would stay split.
- **Freezing arrays and pushing the rest to plugins:** it contradicts array-driven components.

## Risks and the order of work

1. **Spike first.**
   - Read FluentValidation descriptors on both pinned versions (11.12 for net48, 12.1 for net10).
   - Compile one view condition, one validator `When`, one array chain and one request body.
   - Prove each one in Playwright.
2. **Identity method.**
3. **Core.** `Value<T>`, the compiler, the evaluator with `lambda` and `object` values. Conditions
   and arrays move to lambdas.
4. **Validation on descriptors.** Both runtime evaluators become one.
5. **Declarations as interfaces.** Components, plugins and app objects.
6. **Gather as request lambdas.**
7. **Plan document names.**

Each step lands green in CI before the next one starts: `gate / test`, the five Playwright shards
and `verify-net48`. Each step reports lines before and after for the modules it touches.

Two risks:
- **Typing of lambdas.** A lambda can contain a construct the browser cannot run. Without the
  analyzer that becomes a render-time error instead of a compile-time one, so the analyzer ships
  with step 3.
- **Model lambdas in views.** They bind to inputs at plan render, as validation does today. A member
  with no input on the page fails at render, naming the member.

## Evidence

The evidence comes from three read-only module maps at `0702327`. These were rechecked at source:
- the left fold (`ConditionContinuation.cs:43-57`)
- `ClientRule` registering `RuleFor` (`ReactiveValidator.cs:37`)
- the restated rules: a grep counted 100 properties in 20 validators
- the unguarded `new RegExp` (`compare-engine.ts:332`)
- the per-element type keys (`BrowserObjectId.cs:13`)
- reaction kinds: there is no element-specific reaction
- FluentValidation pins: 11.12.0 and 12.1.1
- the line counts above, from `wc -l` over the module directories
