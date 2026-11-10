# Alis.Reactive

Alis.Reactive lets developers author dynamic UI in ASP.NET MVC (.NET Framework 4.8 MVC 5 and
.NET 10) with fluent, fully typed C#. The aim is developer and agent productivity: typed
abstractions so the compiler catches mistakes before the browser does, and defaults so developers
type less.

## Playwright is the proof

Behavior is done when a Playwright test proves it in a real browser, and not before.

- A real gesture (click, type, select) and a visible outcome. No `page.evaluate()`, no
  `ej2_instances`, no DOM poking. One exception: gather-pipeline tests may assert `request.PostData`.
- The test fails when the behavior breaks. Ask what defect would still pass it.
- Run through `scripts/playwright.sh` on assets built from the current source. A deny rule in
  `.claude/settings.json` refuses raw `dotnet test` on a Playwright project or the solution; a
  non-browser project runs when it is named first (`dotnet test tests/Alis.Reactive.ArchitectureTests`).
- The test rules are `.claude/rules/tests.md`; they load when you open a file under `tests/`.

Playwright is the only verification; everything else is waste as proof. Typecheck, vitest, PlanTests and
`dotnet build` are feedback while coding, nothing more. Outside `spec/`, whose checks are
`spec/tools/check.py`, build no other gate, coverage map, checklist or proof artifact. A done report
names the Playwright run and the line of output that proves it, and says plainly what was not
exercised. net48 is proven only by the `verify-net48` CI run.

```bash
scripts/playwright.sh --filter "FullyQualifiedName~Components.Fusion.Grid"  # focused proof
scripts/test.sh             # full gate: typecheck, lint, assets, vitest, checks, dotnet build, Playwright
scripts/test.sh --parallel  # Playwright as the CI shards side by side (~12 min)
scripts/test.sh --no-e2e    # everything except the browser leg
```

## Spec-first

The rewrite is spec-first: `spec/BRIEF.md` states its thesis (section 2), its closed principles (section 3) and the
stability rules every test keeps (section 10); `spec/REWRITE.md` holds the repository it builds and its units; the 26
modules and the owner's decisions are `spec/CLAUDE.md`'s.
- A defect is prevented on the highest rung that holds it: the type, an analyzer, a refusal where the view renders,
  then behavior in the browser. A defect a type prevents gets no test.
- A unit runs in this order: its spec lines, TLC green with each mutant failing, code, its Playwright tests failing on
  the planted defect then passing, commit.
- The server decides when async work happens and the test releases it through the harness; a test waits for the
  state it names, never for time, in a world of its own, and asserts literal outcomes.

## Vision: typed access to JavaScript objects

Everything the browser runs (a Syncfusion component, a native input, a plugin, an app service like
Toast or Drawer) is a JavaScript object with public capabilities: properties (read, write), methods
(call, may return a value) and events (callbacks with payloads). The framework gives typed C# access
to those capabilities through one fluent API. Any member that returns a value can be a source
wherever the DSL takes one: reaction values, conditions, gather payloads, route params, headers,
dispatch payloads and plugin arguments.

**Components: the strategic goal.** Today a component must be supported inside the framework (a
slice in `Alis.Reactive.Fusion` or `Alis.Reactive.Native`) before a developer can use it. The goal
inverts that: a developer brings any component through framework abstractions by declaring its
typed properties, methods and events, every framework feature works for it unchanged, and the
framework supplies more defaults so the developer types less. Steer the redesign by this vision and
by the typed SDK that already exists (read the source; it is rich), not by old docs or plans. The
old artifact-gated onboarding pipeline was removed on purpose; do not rebuild it. A component,
shipped or brought, is one declaration
class (members and events by their JavaScript names) and a render helper, built on the core's
public surface, with zero TypeScript runtime changes: a component that seems to need one means the
plan is missing information, and the C# side carries it.

## Architecture

C# DSL → steps, values and conditions that each write the plan grammar where they are used
(`Alis.Reactive/PlanModel`) → plan JSON in the view (`Html.RenderPlan(plan)`) → generated TS contract
(`runtime/types/plan.ts`) → TypeScript runtime executes the plan. There is one C# model; no semantic
graph and no lowering pass sit between the builders and the grammar. The grammar is the sealed nodes in
`Alis.Reactive/Grammar`.

- C# never executes browser behavior. The runtime never invents behavior the plan does not carry:
  what a sentence means when the page does not match its declaration (a member absent, its holder or
  its value missing, a promise where a value was declared) is stated by the C# and carried by the plan.
  A state left to a runtime default is a gap. Make invalid behavior unrepresentable in the plan domain.
  The meanings the C# states:
  - On an element's component or a plugin, a member the declaration names must be there: reading,
    writing or calling an absent one stops its pipeline.
  - A declared method must be found wherever it is called, through an event payload or a returned object too.
  - Data reads as nothing when absent: a member of an event payload, a response body, an array item or a
    returned object, and data past a declared member (a field of the value a property holds, of each item
    of a list it holds).
  - In an array lambda, a member .NET defines on the value's type (Trim, Length, Year, DayOfWeek compared)
    reads nothing on a missing value and stops its pipeline on a value that lacks it. In a validator's
    condition, a missing text's Length or list's Count reads nothing, so no comparison with it holds.
  - A form's check reads each member from the body its request sends: the inputs of the element it sends (the form's
    when it sends none, and for a check with no request) over the members it sends beside them. A member the body
    lacks reads as the endpoint binds it: its value in a new instance of the nearest item the body sends, else of a
    new model.
  - A value read that holds a promise stops its pipeline; a method that returns one is declared with `MethodAsync`.
  - A method called inside a condition (a term of When, And, Or, ElseIf or a group) runs only when its term is reached.
  - A missing value set as an element's text or HTML shows nothing; a missing class name changes no class.
  - A member is named as System.Text.Json names it: its JsonPropertyName, else camel case. Literals, lambda
    reads, argument shapes and markup raise agree; markup refuses a name an attribute cannot carry back.
  - A literal that holds itself, nests past 32 levels or holds what a plan cannot carry (a delegate) is
    refused when the view renders.
  - A date shown as text is its local day, "yyyy-MM-dd", with its time, "yyyy-MM-ddTHH:mm:ss", when not midnight.
  - A DateTime goes to the server as that text, with no offset. A DateTimeOffset, shown or sent, is the browser's
    clock with that clock's offset for its own day, "yyyy-MM-ddTHH:mm:ss-08:00" ("Z" at zero); one that reads
    nothing is sent as "0001-01-01T00:00:00Z".
  - A request member named like a URL placeholder fills the path, and the body too when the request has one.
  - A slot's listener is removed when it unloads, unless the element no longer holds the component: its host destroyed it, listeners and all.
- `runtime/types/plan.ts` is generated by `PlanContractGenerator` and never hand-edited (hook-enforced).
  `npm run typecheck` regenerates it first, so drift fails typecheck.
- A new behavior is a builder method and the step, value or condition that writes it in the
  existing grammar. A new grammar production, the owner's decision since the grammar is closed, is a
  sealed C# node in `Alis.Reactive/Grammar` with `internal` constructor and `kind`, then regenerate and
  typecheck, a runtime switch case with `assertNever`, a sandbox page and a Playwright test.
- The runtime trusts framework-generated plans: no validators, fallbacks, registries or defensive
  throws for shapes the DSL controls. Errors belong at real boundaries: DOM lookup, browser APIs,
  network, component or plugin lookup, and JSON the framework did not produce. A new nullable or
  `?? fallback` needs a written reason why an empty or default value will not do.
- Views carry no JavaScript: no inline `<script>`, no `addEventListener`, no `window.alis`.
  `runtime/root.ts` discovers and boots every plan.

## Domain model and modules

These terms stay identical across C#, generated TS, runtime, tests and docs.

| Module | What it is |
|--------|-----------|
| Triggers and reactions | On load or an event, run a pipeline: set a property, call a method, dispatch, update an element. |
| ValueExpression | The one way to read a value: component or plugin member, a method's result, URL param, event payload, success or error body, literal, object, array. A second resolver is a defect. |
| HTTP pipeline | Get, Post, Put, Delete. Gather is `target <- source`. Responses open success and error scopes. Chained runs after success and may gather from it; Parallel completes after all branches settle. WhileLoading, Into. |
| Conditions | When, ElseIf, Else graphs over the same value sources, in authored order; first match wins. |
| Validation | Rules are written once, in FluentValidation's grammar, on a `ReactiveValidator<T>` registered through DI; the framework implements the grammar and does not use the FluentValidation package. Each check defines what the server decides and, when the browser can decide it the same way, the rule the plan carries, bound by component ID. What only the server can do stays there by what it is: `Must`, `MustAsync`, a message built from the model, a condition with no browser form, `.ServerOnly()`. |
| Partials and slots | SSR plans compose by `PlanId`. Injected partials load and unload by `SlotId`; unload removes slot-owned components, rules and behavior and keeps boot-level objects. |
| Realtime | Server push and SignalR remote triggers. |
| Plugins | The escape hatch for work the DSL cannot express (URL, DOM, array APIs). Plugin names may be strings; everything else is typed. |
| App-level objects | Drawer, Toast, Confirm, Loader, ActionLink: fixed IDs, typed members. |
| IDs | `IdGenerator` derives input IDs from model type and expression (`Namespace_Type__Member_Path`). Other IDs are the developer's explicit choice. The runtime resolves by `getElementById` only; wide DOM queries are allowed only for plan discovery and self-stamped cleanup (architecture test allowlist). |
| Vendors | A vendor is data a component declaration carries (`ComponentVendor`: where the object lives on the element, subscribe and unsubscribe methods, what a listener receives). The plan's `vendors` map carries it; one runtime driver follows it. The runtime names no vendor: the framework's own component code lives in `Alis.Reactive.Assets/components/` and, like an app's, reaches the runtime only through `alis:mount`, `alis:unmount` and `window.__alisPlugins`. |

Async lanes: HTTP, parallel HTTP, remote triggers, user decisions (Confirm) and partial injection.
Everything else stays sync.

## Rules

- Read the DSL source before shared plan or runtime edits; the source is the requirement. Old
  tests, docs and plans are history. Before editing, name the row: DSL call → domain
  term → runtime behavior → Playwright proof.
- Typed APIs only. Analyzer ALIS009 fails the build on the whole public surface of the shipped
  packages and on every component declaration and render helper, shipped or brought: no `object`,
  no `System.Type`, no string that names a member, field, event, method or path. Strings stay for
  text people read, URLs, element ids the developer chose, header names, CSS classes, and JavaScript
  names at the vendor and plugin boundary (`name:`, `ComponentVendor.Define`, `Plugin(name)`). A
  deliberate exception carries `[TypedDslExemption("why")]`.
- Plan model classes are sealed with `internal` constructors and `internal set`. Developers author
  through builders (`Html.On`, `Html.InputField`, `p.Get`, `p.When`).
- The public surface of a shipped package is the text in its `PublicAPI/<tfm>/PublicAPI.Unshipped.txt`
  (`PublicAPI/<tfm>/PublicAPI.Shipped.txt` once released). A public symbol missing from it or a line without a symbol
  fails the build (RS0016, RS0017); a change to the surface is a reviewed diff of that file. A line
  leaves that file only when the commit names the door that replaces it (a rename, or the one name
  that stays when two did the same thing).
- Never lose typed nature or a feature. Nothing is removed, made internal or loosened to a string
  because nothing calls it: coverage is never complete, so a member with no caller is unproven, not
  dead. Prove it with a sandbox page and a Playwright test. Code that new code replaces (main, an older
  runtime) is evidence of intent, not the design: read what it does when the page does not match (each
  throw, guard and default) beside what a developer writes, and give each a verdict in the replacement
  before the old code goes. "Nothing references it" is no verdict.
- No comments: code is the truth. Name the thing what it is, extract a function, guard the
  boundary. The one exception is an XML doc on the public surface of a shipped package (what its
  PublicAPI text lists): the developer's IntelliSense, written rarely and to `.claude/rules/docs.md`.
  A shebang, `#nullable` and the version Dependabot reads beside a pinned action are directives, not
  comments. `WhenCodeIsWritten` fails on any other comment, in every language the repo holds.
- Raise a gap, never invent. When a view, component or test needs a door the DSL or the vendor's
  public API lacks, name the gap with its use case; never invent a member, a syntax or a runtime
  behavior so a test passes. A member's real shape comes from the installed package or the vendor's
  source.
- Never read secrets: no `dotnet user-secrets list`, nothing that prints a secret's value. When a
  secret may be missing, say so without reading the store. Deny rules in `.claude/settings.json`
  refuse both.
- Component slices are self-contained. Duplication between slices beats a shared base class.
- Root cause, not patch. A fix lands in the code that owns the meaning (the C# that states it), not
  where the symptom shows, and covers every place that shares its cause, each proven in the browser. A
  meaning is decided against these rules when a real case or one of the spec's matrices
  (`spec/Reference/`) raises it, never by analogy. After two failed attempts, stop and research. Leave
  touched files cleaner: delete private helpers no feature reaches.
- C# 14, compiled for `net48` and `net10.0` (TagHelpers are net10-only). A net48 language error means
  rework to what PolySharp polyfills; host-type splits go behind `#if NET48`.

## Workflow

- `main` is the trunk and the release line. A release is an annotated `vX.Y.Z[-rc.N]` tag on `main`:
  `scripts/verify-release-tag.sh` checks it and `.github/workflows/nuget-publish.yml` publishes it.
- Branch from `origin/main` in a worktree and land by pull request (`feature/`, `fix/`, `chore/`). A
  SessionStart hook installs the pre-commit guard that refuses commits on `main`. `git stash` is never
  run: `stash@{0}` holds work that stays put, and a deny rule refuses it.

  ```bash
  git worktree add .worktrees/<name> -b feature/<name> origin/main
  ```

- A unit of work moves through five states. Each exit shows in git or a run's log, never in a
  checklist or an artifact:
  1. Restate the goal in your own words; a new direction waits for the owner's yes.
  2. Read every file the unit touches, whole, at HEAD.
  3. Change.
  4. Prove. Behavior: a `scripts/playwright.sh` result line, the test first failing on a planted
     defect. Rules and docs: each rule found in one file, every removed line traced to its home. A
     failure goes back to 3, twice at most; then stop and research.
  5. Commit one unit: what changed, the proof line, what was not exercised. Work an agent finished
     is landed or dropped, with the reason, in the same session.
- A review names the branch and HEAD SHA it read. A finding counts only if its `file:line` exists
  at HEAD.
- `scripts/doctor.sh` (preflight), `scripts/build.sh`, `scripts/run.sh` (sandbox at
  http://localhost:5220), `scripts/pack.sh <version>`. Watchers: `npm run watch:runtime` and the other
  watch scripts in `package.json`.
- Syncfusion EJ2 33.1.47 with npm and NuGet pinned together; upgrade with
  `node scripts/pin-syncfusion.mjs <version>`.

## Local guidance

The code and its XML docs are the reference: read the declaration, the builder and a sandbox
page before writing a view, a component or a test.

Rules for one kind of file live in `.claude/rules/` and load when you open such a file: `tests.md`
(files under `tests/`), `views.md` (`.cshtml`), `csharp.md` (`.cs`) and `docs.md` (docs, Markdown and
XML docs). `CLAUDE.md` files in the runtime, Fusion, sandbox and test folders hold what is true of
that folder. This file wins on conflict. `AGENTS.md` is a symlink to this file.
