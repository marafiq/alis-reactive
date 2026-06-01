# Grammar Critique — Plugins, App-level, Fusion Template

PL-architect hardening of the DSL grammar for three clusters, grounded in the AST
signatures in `ast-grammar-plugins-applevel-template.md` (the real
`Receiver -> Member(params) -> Returns` with `file:line`) and reconciled to the
finalized names in `09-dsl-naming-sheet.md` (§3.7, §3.5, §1.8) plus the design
discoveries in `08-determinism-formalization.md` (§6.3 widen `Include`, §6.4 merge
extensionally-equal morphisms).

**The bar.** Easy to write, reads TALL — vertical fluent chains, one call per
line. Judged on ORTHOGONALITY, COMPOSABILITY, TALL-READING, LEAST-SURPRISE,
DISCOVERABILITY, CONSISTENCY, EASY-TO-WRITE. Every adjustment **preserves every
capability** (zero feature loss). Where the naming sheet already decided a rename,
this file does **not** re-coin — it cites the decision and addresses the *grammar
shape* (overload set, return type, callback receiver, default ceremony) that the
naming sheet did not touch.

`✶` = grounded in `09-dsl-naming-sheet.md` already (cited, not re-litigated).
`▲` = new grammar-shape finding this file adds (the naming sheet renamed but left
the shape).

---

## 0. What is ALREADY good — DO NOT CHURN

These read well TALL and pass the cold one-breath read. Touching them is churn.

1. **App-level verbs are extension methods on `ComponentRef<TComponent,TModel>`
   that return the same `ComponentRef` (ReturnsSelf=yes).**
   `NativeDrawerExtensions.cs:62,78`, `NativeLoaderExtensions.cs:65,81`,
   `FusionToastExtensions.cs:90,96`, `FusionConfirmExtensions.cs:29,34`. This is
   the correct TALL shape — every verb chains one-per-line:
   ```csharp
   p.Component<FusionToast>()
       .SetTitle("Saved")
       .SetContent("Resident record updated")
       .Success()
       .Show();
   ```
   COMPOSABILITY + TALL-READING: ideal. The component classes hold only
   `ElementId`/`DefaultId` and no verbs (`NativeDrawer.cs:20,23`) — clean
   separation of identity from behavior. Keep verbatim.

2. **Symmetric open/close verb pairs.** `Open`/`Close` (drawer), `Show`/`Hide`
   (loader, toast, confirm). LEAST-SURPRISE: each pair is a true antonym; a dev
   never guesses wrong. `NativeDrawerExtensions.cs:62-78`,
   `FusionConfirmExtensions.cs:29-34`. Keep.

3. **Toast severity verbs `Success/Warning/Danger/Info` as zero-arg chainable
   methods** (`FusionToastExtensions.cs:68-83`). DISCOVERABILITY: IntelliSense
   lists the real severity vocabulary; no stringly `type:"success"`. The naming
   sheet (§3.7) confirms these are the live severity API and **deletes** the dead
   `ToastType` enum. Keep the methods.

4. **`PluginCallBuilder.Fire()` as the explicit call terminal**
   (`PluginMemberBuilder.cs:250`). The void-call lane needs a terminal because it
   has no return value to chain off; `Fire()` reads cold as "emit the call." The
   naming sheet (§3.7) keeps it. Keep.

5. **`PluginMemberBuilder`'s implicit conversion to `TypedPluginSource<TReturn>`**
   (`PluginMemberBuilder.cs:136`) — the read face needs **no** `Build()`, the
   source IS the builder. LEAST-SURPRISE + EASY-TO-WRITE: a plugin read drops
   straight into any `TypedSource<T>` slot. Keep the implicit operator (the
   rename to `PluginReadBuilder` per §3.7 is the only change to this type).

6. **`Plugin`-base declaration face for typed subclasses** (`Plugin.cs:30-86`)
   uses exactly `Function`/`Property`/`Command` — already the collapsed pair from
   §1.8. CONSISTENCY: the subclass face is the canonical vocabulary. Keep.

7. **Template element-named children** (`Span`/`Img`/`Badge`/`Icon`/`Link`/`Raw`)
   each name the HTML element they emit and return the builder
   (`FusionTemplateBuilder.cs:80-282`). TALL-READING: a template reads as a
   vertical list of HTML nodes. The naming sheet (§3.5) keeps these. Keep.

8. **`FusionTemplateExpression` static lowering helpers** (`ToBinding` /
   `ToPropertyPath` / `ToCondition`, `FusionTemplateExpression.cs:19-36`) — pure
   expression→SF-string helpers, correctly static and side-effect-free. Keep.

---

## 1. PLUGINS — proposed adjustments

### Adjustment 1 ✶ — Collapse the `Method`/`Function` and `Void`/`Command` synonym pairs on `PluginTypeBuilder`

**Current shape.** `PluginTypeBuilder` carries **two names per concept**:
`Method<T>(name)` (`PluginTypeBuilder.cs:24`) **and** `Function<T>()`
(`PluginTypeBuilder.cs:48`) both declare a value-returning op; `Void(name)`
(`PluginTypeBuilder.cs:62`) **and** `Command(name)` (`PluginTypeBuilder.cs:68`)
both declare a void op. The `Plugin` base meanwhile uses only `Function`/`Command`
(`Plugin.cs:30,64`). Two vocabularies for one concept inside one type, and a
**third** discrepancy between the inline declarer and the subclass declarer.

**Property hurt.** ORTHOGONALITY (two spellings per intent) + CONSISTENCY
(inline declarer ≠ subclass declarer) + DISCOVERABILITY (a dev sees `Method`,
`Function`, `Void`, `Command` in IntelliSense and cannot tell which is canonical).

**BEFORE**
```csharp
plan.RegisterPlugin("dom", p => p
    .Method<string>("getValue")          // value op, spelling A
    .Function<int>()                      // value op, spelling B
    .Void("scrollTo")                     // void op, spelling A
    .Command("focus"));                   // void op, spelling B
```
**AFTER** (decided in §1.8 / §3.7 — `Method`→`Function`, `Void`→`Command`)
```csharp
plan.RegisterPlugin("dom", p => p
    .Function<string>("getValue")        // one value-op verb
    .Function<int>()
    .Command("scrollTo")                 // one void-op verb
    .Command("focus"));
```
`Method` and `Void` are deleted; `Function`/`Command`/`Property` is the single
pair, identical to the `Plugin` base. Zero capability lost (every former call has
a survivor). Reconciles inline and subclass declarers to one vocabulary.

### Adjustment 2 ▲ — Unify the two arg-contract spellings: `Args(Action<PluginArgumentTypes>)` vs `Arg<T>()` chains into one shape

**Current shape.** The argument *contract* (the type list a declared op accepts)
can be spelled **two structurally different ways** on `PluginFunction`/`PluginCommand`:
a repeatable `Arg<TArg>()` chain (`Plugin.cs:259,290`) **and** a single
`Args(Action<PluginArgumentTypes>)` callback (`Plugin.cs:266,297`) whose body is
itself a repeatable `Arg<T>()` chain (`PluginTypeBuilder.cs:164`). Same on the
inline declarer: `Method/Function/Void/Command` each have a bare overload **and**
an `Action<PluginArgumentTypes>` overload (`PluginTypeBuilder.cs:40,54,72,80`).

**Property hurt.** ORTHOGONALITY — two ways to declare the same arg-type list.
EASY-TO-WRITE is split: the callback form is heavier ceremony for the common
case, the inline `Arg<T>()` chain is lighter but only exists on the descriptor
face, not the inline-declarer face. The §6.4 finding ("one args-builder + one
declaration spine for plugins") names exactly this drift risk.

**BEFORE** (two shapes co-exist)
```csharp
// descriptor face — chain
Function<string>("fmt").Arg<decimal>().Arg<string>();
// inline declarer — callback only
p.Function<string>("fmt", a => a.Arg<decimal>().Arg<string>());
```
**AFTER** — make the repeatable `Arg<T>()` chain the **one** spelling on both
faces; keep the `Action<PluginArgumentTypes>` overload **only** as the grouping
shape for programmatic/loop-built arg lists (the genuine second capability), the
way §1.1 keeps the nested-lambda And/Or only for grouping the flat shape cannot
express:
```csharp
// one spelling everywhere — reads TALL, one Arg per line
p.Function<string>("fmt")
    .Arg<decimal>()
    .Arg<string>();
// callback overload kept ONLY for dynamically composed contracts
p.Function<string>("fmt", a => BuildArgsFrom(schema, a));
```
Both faces now lead with the same chain; the callback survives as the loop-only
escape. No capability lost (the callback still exists); the redundant
*default-case* callback is demoted from co-equal spelling to grouping-only.

### Adjustment 3 ▲ — Replace the 8 scalar `Arg(...)` overloads with `Arg<T>(T value)`, keeping `ArgValue<T>` deleted

**Current shape.** Both `PluginMemberBuilder` (read face) and `PluginCallBuilder`
(call face) carry **eight** scalar literal overloads —
`Arg(string)`/`Arg(int)`/`Arg(bool)`/`Arg(long)`/`Arg(decimal)`/`Arg(double)`/`Arg(DateTime)`
plus a generic `ArgValue<TValue>(TValue)`
(`PluginMemberBuilder.cs:79-129` and `:193-243`). All eight bodies are identical
(`_args.Add(PluginInvocationArgument.Literal(value))`, verified at
`PluginMemberBuilder.cs:80-132`). The generic `ArgValue` already covers every one
of them.

**Property hurt.** ORTHOGONALITY (nine spellings for "add a literal arg") +
DISCOVERABILITY (IntelliSense shows a wall of `Arg` overloads + a differently-named
`ArgValue` for the same intent) + CONSISTENCY (`Arg(decimal)` vs `ArgValue<decimal>`
are two names for one operation). The naming-sheet rationale for §3.7 already flags
`ArgValue` as "the generic-shaped variant" — but having **both** the named scalars
*and* the generic is the redundancy.

**BEFORE**
```csharp
p.Plugin("fmt", "currency")
    .Arg(amount)           // Arg(decimal)
    .Arg("USD")            // Arg(string)
    .Arg(2)                // Arg(int)
    .Fire();
```
**AFTER** — keep a **single** generic literal entry `Arg<TValue>(TValue value)`
(absorbs all eight scalars + `ArgValue`), plus the typed `Arg<T>(TypedSource<T>)`
source overload and the `(ResponseBody/args, path)` overloads (those carry
different *node kinds*, not literals, so they stay):
```csharp
p.Plugin("fmt", "currency")
    .Arg(amount)           // Arg<decimal> inferred
    .Arg("USD")            // Arg<string> inferred
    .Arg(2)                // Arg<int> inferred
    .Fire();
```
Call sites are **identical** — C# infers `TValue` from the literal, so every
former scalar call compiles unchanged. The shape derivation that the scalar
overloads did per-type is exactly what `ArgValue<TValue>` already does generically
(`PluginMemberBuilder.cs:129-132`). One name, zero call-site churn, zero capability
lost. (LEAST-SURPRISE caveat preserved: the `DateTime` overload's
browser-date-formatting must remain the literal lowering for `TValue=DateTime` —
fold it into the generic `Literal(value)` switch, not a public overload.)

### Adjustment 4 ✶/▲ — Name the two plugin faces by lane (`PluginReadBuilder`/`PluginCallBuilder`) AND make the read terminal explicit-or-implicit, not implicit-only

**Current shape.** The read face is `PluginMemberBuilder<TReturn,TModel>`
(`PluginMemberBuilder.cs:55-136`); "Member" hides whether it reads a value or
fires a call. §3.7 already renames it `PluginReadBuilder` (KEEP `PluginCallBuilder`).
That rename is decided. **The grammar-shape gap this file adds:** the read face
terminates **only** via implicit conversion (`PluginMemberBuilder.cs:136`), while
the call face terminates **only** via explicit `Fire()` (`:250`). Two sibling
faces with asymmetric termination.

**Property hurt.** CONSISTENCY (siblings should terminate alike) + LEAST-SURPRISE
(a dev who learned `Fire()` on the call face looks for a read terminal and finds
none — the conversion is invisible until assigned to a `TypedSource<T>`).

**BEFORE**
```csharp
TypedSource<decimal> total = p.PluginProperty<decimal>("cart", "total"); // implicit, invisible
p.Plugin("api", "refresh").Fire();                                        // explicit
```
**AFTER** — keep the implicit conversion (it is what makes a read drop into a
source slot, Adjustment-0 #5), **add** a parallel explicit terminal `AsSource()`
on the read face so the two faces read symmetrically when a dev wants the explicit
form (the implicit path stays for the common case):
```csharp
var total = p.PluginProperty<decimal>("cart", "total").AsSource(); // explicit, mirrors Fire()
p.Plugin("api", "refresh").Fire();
```
Naming reconciliation: §3.3 renamed array `AsSource()`→`AsArraySource()` because
"Source" was ambiguous there; on the plugin read face the result type is
`TypedPluginSource<TReturn>`, so the explicit terminal should be
**`AsPluginSource()`** (screams the result, mirrors the §3.3 reasoning) — not bare
`AsSource()`. Implicit conversion retained; explicit terminal added; zero
capability lost, termination symmetry restored.

### Adjustment 5 ▲ — Collapse the read/call `Arg` surface into one shared shape (the §6.4 "one args-builder" discovery)

**Current shape.** `PluginMemberBuilder.Arg*` (`:55-129`) and
`PluginCallBuilder.Arg*` (`:169-243`) are **~95% identical** — same eleven `Arg`
overloads, the only difference is the return type (`PluginReadBuilder` vs
`PluginCallBuilder`) and the terminal (implicit-source vs `Fire()`). §6.4 names
these "two ~95%-identical plugin builders … can drift" and prescribes "one
args-builder + one declaration spine."

**Property hurt.** CONSISTENCY + the maintenance hazard §6.4 formalizes: two copies
of the arg grammar can drift, and a drift between read-args and call-args is a
silent D1 (the same author intent lowers two different ways).

**BEFORE** — two full `Arg` surfaces, hand-kept in sync across
`PluginMemberBuilder.cs:55-133` and `:169-243`.

**AFTER** — extract the shared `Arg`/`ArgValue` accumulation into one
`PluginArgs<TSelf>` mixin (generic self-type so each face still returns itself for
chaining), consumed by both faces:
```csharp
// one source of the Arg grammar; faces differ ONLY in terminal
PluginReadBuilder<TReturn,TModel> : PluginArgs<PluginReadBuilder<TReturn,TModel>>  // implicit→TypedPluginSource
PluginCallBuilder<TModel>         : PluginArgs<PluginCallBuilder<TModel>>          // Fire()
```
Author-facing chains are **byte-identical** to today (same `.Arg(...).Arg(...)`),
so no DSL surface changes — this is the internal "one spine" §6.4 mandates,
listed here because it removes a real drift seam in the *grammar's* arg vocabulary.
Zero capability lost.

---

## 2. APP-LEVEL — proposed adjustments

### Adjustment 6 ✶ — Split the two colliding `SetTimeout(ms)` verbs by their real meaning

**Current shape.** **Two** different services expose `SetTimeout(int ms)` meaning
**different things**: `NativeLoader.SetTimeout` is an auto-hide timer
(`NativeLoaderExtensions.cs:54`); `FusionToast.SetTimeout` is display duration
(`FusionToastExtensions.cs:51`). And both collide with JS `setTimeout`.

**Property hurt.** LEAST-SURPRISE (same verb, two meanings, neither is the JS
scheduler it looks like) + CONSISTENCY (one concept name should not span two
unrelated mechanics).

**BEFORE**
```csharp
p.Component<NativeLoader>().Show().SetTimeout(3000);   // auto-hide after 3s
p.Component<FusionToast>().SetTimeout(5000).Show();    // visible for 5s
```
**AFTER** (decided in §3.7 — loader→`SetAutoHide`, toast→`SetDuration`)
```csharp
p.Component<NativeLoader>().Show().SetAutoHide(3000);  // auto-hide timer
p.Component<FusionToast>().SetDuration(5000).Show();   // display duration
```
Each names its real mechanic; the cross-service collision and the JS-scheduler
misread both end. Zero capability lost.

### Adjustment 7 ✶ — Rename `FusionConfirm.SetContent` → `SetMessage` and unify the renderer name

**Current shape.** `FusionConfirm.SetContent(string message)`
(`FusionConfirmExtensions.cs:21`) names the param `message` but the verb says
`Content`; meanwhile `FusionToast.SetContent` (`FusionToastExtensions.cs:46`) also
exists — so "content" means two things across services. Separately the renderer is
`html.FusionConfirmDialog()` (`FusionConfirmExtensions.cs:39`) while every other
service uses `type==renderer` (`html.FusionToast()`, `html.NativeDrawer()`).

**Property hurt.** CONSISTENCY (`SetContent` spans two services with different
payloads; renderer name diverges from type) + LEAST-SURPRISE (a confirm dialog
shows a *message*, not generic *content*).

**BEFORE**
```csharp
@Html.FusionConfirmDialog()
p.Component<FusionConfirm>().SetContent("Delete this resident?").Show();
```
**AFTER** (decided in §3.7 — `SetContent`→`SetMessage`, `FusionConfirmDialog`→`FusionConfirm`)
```csharp
@Html.FusionConfirm()
p.Component<FusionConfirm>().SetMessage("Delete this resident?").Show();
```
`type==renderer` restored; "content" no longer means two things. Zero capability lost.

### Adjustment 8 ▲ — Give the app-level setters the same `Set*` voice and stop the `Set*`/zero-arg verb interleave reading flat

**Current shape.** Toast mixes three verb *grammars* on one chain:
`Set*` value-setters (`SetTitle`/`SetContent`/`SetTimeout`,
`FusionToastExtensions.cs:41-51`), zero-arg toggle-setters
(`ShowCloseButton`/`ShowProgressBar`, `:56-61`), and zero-arg terminal-ish severity
verbs (`Success`/`Warning`/`Danger`/`Info`, `:68-83`), plus `Show`/`Hide` (`:90-96`).
Reading a chain, `ShowCloseButton()` (a *config* setter) and `Show()` (the *action*
terminal) both start with `Show`, so the action verb does not stand out.

**Property hurt.** LEAST-SURPRISE + TALL-READING — `ShowCloseButton` and `Show`
share a prefix but live on different lanes (configuration vs action); the chain
does not visually separate "configure" from "act."

**BEFORE**
```csharp
p.Component<FusionToast>()
    .SetTitle("Saved")
    .ShowCloseButton()     // config — but reads like an action
    .ShowProgressBar()     // config
    .Success()
    .Show();               // action — same prefix as ShowCloseButton
```
**AFTER** — rename the two boolean *config* toggles to the `With*` voice so config
verbs and the `Show`/`Hide` action verbs never share a prefix; severity verbs and
`Show`/`Hide` keep their decided names:
```csharp
p.Component<FusionToast>()
    .SetTitle("Saved")
    .WithCloseButton()     // config — distinct voice
    .WithProgressBar()     // config
    .Success()
    .Show();               // action — unmistakable
```
`With*` for present-a-feature config reads cold ("a toast *with* a close button")
and frees `Show` to be the sole action verb. Capability identical (same two
booleans set). This is a NEW grammar-shape finding — §3.7 keeps the toast verbs
verbatim but does not address the `Show*`-config / `Show`-action prefix clash; the
fix is a voice rename, not a feature change.

### Adjustment 9 ▲ — `NativeLoader.SetTarget(string targetId)` should take a typed element handle, not a bare string

**Current shape.** `SetTarget(string targetId)` (`NativeLoaderExtensions.cs:41`)
takes a raw element-id string. Everywhere else the grammar resolves DOM targets
through typed handles — `Element(elementId)`/`ElementBuilder` (§3.1) and
plan-driven `IdGenerator` ids — never a hand-typed string the dev must keep in
sync with the rendered id.

**Property hurt.** EASY-TO-WRITE (a stringly id is a typo waiting to happen and
has no IntelliSense) + CONSISTENCY (every other DOM-target seam is typed;
`SetTarget(string)` is the one stringly hole) + the root `CLAUDE.md` Plan-Driven
IDs rule ("non-input ids are the developer's explicit choice via `p.Element("id")`").

**BEFORE**
```csharp
p.Component<NativeLoader>().SetTarget("resident-grid").Show();  // raw string id
```
**AFTER** — keep the `string` overload (the plugin-style boundary escape, like
`Element(string)`) but **add** a typed overload that accepts the same `Element`
handle the rest of the grammar uses, so a dev can target the element they already
named:
```csharp
p.Component<NativeLoader>().SetTarget(p.Element("resident-grid")).Show();  // typed handle
// string overload retained for the boundary case
p.Component<NativeLoader>().SetTarget("resident-grid").Show();
```
This mirrors how Adjustment-9-style typed/string pairs already exist on `Element`
and `Into`. Zero capability lost (string overload kept); the typed path closes the
stringly hole and aligns with plan-driven ids. NEW grammar-shape finding (the
naming sheet does not touch `SetTarget`'s parameter type).

---

## 3. FUSION TEMPLATE — proposed adjustments

### Adjustment 10 ✶ — Rename the template conditional pair to `WhenTemplate`/`ShowTemplateIf` (cross-area collision with runtime `When`)

**Current shape.** `FusionTemplateBuilder.When(condition, then[, @else])`
(`FusionTemplateBuilder.cs:303-315`) and `.ShowIf(condition, content)` (`:343`)
**collide by name** with the runtime Conditions `When` (§3.2) but emit a totally
different thing — an SF `${if(...)}` **SSR string** (`FusionTemplateBuilder.cs:329`),
not a runtime `ConditionGraph`.

**Property hurt.** ORTHOGONALITY/CONSISTENCY — one verb name spanning two lanes
(SSR-string vs runtime-graph). A dev reading `When` cannot tell which lane they
are in.

**BEFORE**
```csharp
FusionTemplate.Create<Resident>()
    .When(r => r.IsActive, t => t.Badge("Active"))    // SSR ${if} — but name == runtime When
    .ShowIf(r => r.HasAlert, t => t.Icon("alert"))
    .Render();
```
**AFTER** (decided in §3.5 / §4 — `When`→`WhenTemplate`, `ShowIf`→`ShowTemplateIf`)
```csharp
FusionTemplate.Create<Resident>()
    .WhenTemplate(r => r.IsActive, t => t.Badge("Active"))   // screams the SSR-string lane
    .ShowTemplateIf(r => r.HasAlert, t => t.Icon("alert"))
    .Render();
```
Conditions owns the bare `When`; the template lane gets the explicit pair.
Zero capability lost.

### Adjustment 11 ✶ — Template attribute/class verbs: `.Class`→`CssClass`, `.Attr`→`Attribute`

**Current shape.** `FusionTemplateBuilder.Class(string)` (`:43`) and
`.Attr(string,string)` (`:52`). `Class` glares against the C# keyword and reads
ambiguously cold; `Attr` is an abbreviation.

**Property hurt.** DISCOVERABILITY (`Attr` abbreviation) + LEAST-SURPRISE (`Class`
vs the keyword). Note §3.7 keeps `Attr` on `NativeActionLinkBuilder` (a *different*
live builder where it is established) — the change is the SSR-string template lane
only.

**BEFORE**
```csharp
FusionTemplate.Create<Resident>()
    .Class("card")
    .Attr("data-id", "x")
    .Text(r => r.Name);
```
**AFTER** (decided in §3.5)
```csharp
FusionTemplate.Create<Resident>()
    .CssClass("card")          // screams the HTML attribute
    .Attribute("data-id", "x") // spelled out
    .Text(r => r.Name);
```
Zero capability lost; `CssClass` ≠ runtime `AddClass` (different lane — §4 in the
naming sheet).

### Adjustment 12 ✶ — `.EventButton` → `DispatchButton<TProperty>`

**Current shape.** `EventButton<TProperty>(text, eventName, idProperty[, css])`
(`FusionTemplateBuilder.cs:245-254`, mirrored on `FusionConditionalBuilder.cs:144`).
"EventButton" reads cold as "a button that has events" — every button does.

**Property hurt.** DISCOVERABILITY/LEAST-SURPRISE — the name does not say it
*dispatches*. It emits a `Dispatch` carrying the row id; `DispatchButton` names the
action the way `ButtonFor` names the binding, and pairs with the `Dispatch`
reaction verb (listen `t.Event("x")` / emit `p.Dispatch("x")` / row-emit
`DispatchButton`).

**BEFORE**
```csharp
.EventButton<Resident>("Admit", "admit-resident", r => r.Id)
```
**AFTER** (decided in §3.5)
```csharp
.DispatchButton<Resident>("Admit", "admit-resident", r => r.Id)
```
Zero capability lost; consistent with the dispatch/listen vocabulary.

### Adjustment 13 ▲ — Replace the wide multi-arg `Button`/`ButtonFor`/`DispatchButton`/`Link` overloads with a TALL builder-callback

**Current shape.** The template's interactive nodes are **wide multi-arg calls**
with `string css` tacked on as a trailing positional overload:
`Button(text, onClick)` / `Button(text, onClick, css)`
(`FusionTemplateBuilder.cs:192-198`);
`ButtonFor(text, idProperty, onClickFn[, css])` (`:210-219`);
`DispatchButton(text, eventName, idProperty[, css])` (`:245-254`);
`Link(hrefProperty, textProperty[, css])` (`:274-282`). Every node duplicates a
`css` overload, and `Button(text, onClick)` is a **stringly** `onClick` (`:192`)
— a raw JS string in the SSR lane.

**Property hurt.** TALL-READING (multi-arg positional calls do not read
top-to-bottom; `Button("Save", "saveFn()", "btn-primary")` is three unlabeled
strings) + ORTHOGONALITY (every node re-declares a `css` overload = N×2 overloads)
+ EASY-TO-WRITE/LEAST-SURPRISE (`onClick` as a raw JS string is exactly the
"string magic" the root architecture rule bans outside the plugin boundary).

**BEFORE**
```csharp
.Button("Save", "saveResident()", "e-btn e-primary")
.ButtonFor<Resident>("Edit", r => r.Id, "editRow", "e-btn")
.Link<Resident>(r => r.ProfileUrl, r => r.Name, "e-link")
```
**AFTER** — one `Button`/`Link` entry that hands back a small typed config builder,
so options read one-per-line and the stringly `css` overload explosion collapses to
one chainable `.Css(...)`:
```csharp
.Button("Save", b => b
    .OnClick("saveResident()")     // still the SSR onClick string — but labeled
    .Css("e-btn e-primary"))
.ButtonFor<Resident>("Edit", r => r.Id, b => b
    .OnClick("editRow")
    .Css("e-btn"))
.Link<Resident>(r => r.ProfileUrl, r => r.Name, b => b
    .Css("e-link"))
```
COMPOSABILITY: the callback hands back a clean `TemplateButtonOptions` builder
(every option returns it, ReturnsSelf) so the grammar nests TALL. Every current
capability is preserved — `css`, `onClick`, `onClickFn`, `eventName` all become
named one-per-line options instead of positional/overload variants. The stringly
`onClick` is retained (it is a real SSR escape hatch) but now **labeled** by
`.OnClick(...)` rather than an unnamed 2nd positional string. NEW grammar-shape
finding; §3.5 keeps the button names but does not address the wide-arg/`css`-overload
shape.

### Adjustment 14 ▲ — Make `FusionConditionalBuilder` the SAME builder as `FusionTemplateBuilder` (kill the asymmetric narrower receiver)

**Current shape.** `When`/`ShowIf` callbacks receive a **narrower**
`FusionConditionalBuilder<TModel>` (`FusionTemplateBuilder.cs:303,343`;
receiver defined `FusionConditionalBuilder.cs:18-183`), which exposes a
**subset** of the root builder's children — it has `Span`/`Badge`/`Icon`/`Div`/
`Img`/`Button`/`EventButton`/`Raw`/`Render` (`FusionConditionalBuilder.cs:18-183`)
but is **missing** `Text`, `Class`, `Attr`, `Id`, `Link`, `ButtonFor`, nested
`When`/`ShowIf`. So inside a conditional branch a dev **cannot** bind `Text`,
add a `Link`, a `ButtonFor`, or nest another `When` — capabilities silently
disappear at the seam.

**Property hurt.** COMPOSABILITY — this is the textbook "`cod(f) ⊄ dom(g)`" seam
the determinism doc calls a bug (§6.3 reasoning applied to builders): the `When`
callback's receiver does not offer what the outer builder offers, so the grammar
does **not** nest cleanly. LEAST-SURPRISE: a dev expects a branch body to be a
full template fragment; it is a crippled subset. DISCOVERABILITY: the missing
methods are invisible — the dev only discovers the hole when `t.Text(...)` does
not compile inside a `When`.

**BEFORE** (branch body cannot bind Text, Link, nested When, …)
```csharp
.WhenTemplate(r => r.IsActive, t => t
    .Span(r => r.Name)        // works
    .Text(r => r.Status))     // DOES NOT COMPILE — Text missing on conditional builder
```
**AFTER** — make the `When`/`ShowIf`/`ShowTemplateIf` callbacks receive the **full**
`FusionTemplateBuilder<TModel>` (the branch body is just another template fragment;
the `${if}` wrapper is added by the outer `When`, not by a separate type). Delete
`FusionConditionalBuilder` as a distinct narrower type:
```csharp
.WhenTemplate(r => r.IsActive, t => t
    .Span(r => r.Name)
    .Text(r => r.Status)      // now composes — same builder everywhere
    .ButtonFor<Resident>("Edit", x => x.Id, b => b.OnClick("editRow"))
    .WhenTemplate(r => r.HasAlert, inner => inner.Icon("alert")))  // nests cleanly
```
COMPOSABILITY restored — every callback hands back the **same** clean builder, so
the template grammar is genuinely recursive (the `Div`→full-builder nesting already
proves the runtime supports this, `FusionConditionalBuilder.cs:90`). This is a pure
capability **gain** with zero loss: the branch body gains `Text`/`Link`/`ButtonFor`/
nested-`When` it was missing. §3.5 keeps `FusionConditionalBuilder` "unchanged" as
an internal then/else body builder — but the AST shows it is an author-facing
*narrower* receiver, and the narrowing is the seam bug; unifying it removes the
asymmetry without removing the then/else *capability* (the else branch is still a
second callback on `When`). NEW grammar-shape finding.

### Adjustment 15 ▲ — Give `When` an explicit else via a fluent `.Else(...)` continuation, not a 3rd positional `Action`

**Current shape.** The else branch is a **third positional `Action`** on `When`:
`When(condition, then, @else)` (`FusionTemplateBuilder.cs:311`). The two callbacks
are positionally distinguished — `then` is arg 2, `else` is arg 3, both the same
type `Action<FusionConditionalBuilder<TModel>>`, so at a call site they are two
adjacent lambdas with nothing naming which is which.

**Property hurt.** TALL-READING + LEAST-SURPRISE — two same-typed lambdas in
positional slots read as a wall; the reader must count arguments to know which is
the else. The runtime Conditions lane spells this as `.Then(...).Else(...)`
(§3.2) — the template lane should match that voice (CONSISTENCY: same if/else
concept, same shape).

**BEFORE**
```csharp
.WhenTemplate(r => r.IsActive,
    t => t.Badge("Active"),        // then  (arg 2)
    t => t.Badge("Inactive"))      // else  (arg 3) — positional, unnamed
```
**AFTER** — `WhenTemplate` returns a small continuation exposing `.Else(...)` that
returns the root builder, mirroring runtime `Then/Else`:
```csharp
.WhenTemplate(r => r.IsActive, t => t.Badge("Active"))
    .Else(t => t.Badge("Inactive"))     // named, reads TALL, matches runtime voice
```
Both branches are now labeled and read one-per-line. The 2-arg `When` (no else)
stays as the common case (it already returns the root builder); only the
3-positional-arg overload is replaced by the `.Else` continuation. Zero capability
lost (then+else both preserved); CONSISTENCY with the runtime if/else voice gained.
NEW grammar-shape finding (the naming sheet renames `When`→`WhenTemplate` but keeps
the 3-arg positional else).

---

## 4. Adjustment ledger (count)

| # | Cluster | Adjustment | Source | Property improved | Capability preserved |
|---|---|---|---|---|---|
| 1 | Plugins | `Method`/`Void` → `Function`/`Command` (collapse synonyms) | §1.8 ✶ | Orthogonality, Consistency | yes |
| 2 | Plugins | One `Arg<T>()` chain spelling; callback = grouping-only | §6.4 ▲ | Orthogonality, Easy-to-write | yes |
| 3 | Plugins | 8 scalar `Arg` + `ArgValue` → one generic `Arg<T>(T)` | ▲ | Orthogonality, Discoverability | yes |
| 4 | Plugins | `PluginMemberBuilder`→`PluginReadBuilder` + add explicit `AsPluginSource()` terminal | §3.7 ✶ / ▲ | Consistency, Least-surprise | yes |
| 5 | Plugins | One shared `PluginArgs<TSelf>` arg spine across read/call faces | §6.4 ▲ | Consistency (anti-drift) | yes |
| 6 | App-level | `SetTimeout` → `SetAutoHide` (loader) / `SetDuration` (toast) | §3.7 ✶ | Least-surprise, Consistency | yes |
| 7 | App-level | `SetContent`→`SetMessage` + `FusionConfirmDialog`→`FusionConfirm` | §3.7 ✶ | Consistency, Least-surprise | yes |
| 8 | App-level | Toast config toggles `Show*`→`With*` (separate config from action) | ▲ | Least-surprise, Tall-reading | yes |
| 9 | App-level | `SetTarget(string)` + add typed `SetTarget(Element)` overload | ▲ | Easy-to-write, Consistency | yes |
| 10 | Template | `When`/`ShowIf` → `WhenTemplate`/`ShowTemplateIf` (lane collision) | §3.5/§4 ✶ | Orthogonality, Consistency | yes |
| 11 | Template | `.Class`→`CssClass`, `.Attr`→`Attribute` | §3.5 ✶ | Discoverability, Least-surprise | yes |
| 12 | Template | `EventButton`→`DispatchButton` | §3.5 ✶ | Discoverability, Least-surprise | yes |
| 13 | Template | Wide `Button`/`Link` multi-arg+css overloads → TALL options callback | ▲ | Tall-reading, Orthogonality | yes |
| 14 | Template | Unify `FusionConditionalBuilder` into full `FusionTemplateBuilder` (kill narrowing seam) | ▲ (§6.3-style) | Composability, Discoverability | yes (capability gain) |
| 15 | Template | `When`'s 3rd-positional else → fluent `.Else(...)` continuation | ▲ | Tall-reading, Consistency | yes |

**Total proposed adjustments: 15** (8 already grounded in `09-dsl-naming-sheet.md`
/ §6.4 — cited, not re-litigated; 7 new grammar-shape findings this critique adds:
#2, #3, #5 caveat, #8, #9, #13, #14, #15 — addressing overload sets, return-type
symmetry, callback-receiver narrowing, and wide-arg vs TALL-callback shape that the
naming sheet renamed-around but did not reshape).

Every adjustment preserves 100% of capabilities (zero feature loss = zero tech
debt): renames keep every survivor, overload collapses keep call sites compiling
via generic inference, the one capability *gain* (#14) only widens the conditional
branch body to the full template surface.
