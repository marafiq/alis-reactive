# Alis.Reactive Framework

## Architecture

A developer expresses reactive intent with the C# DSL. The plan domain
captures that intent. `Html.RenderPlan(plan)` serializes that intent as plan
JSON inside the view. In the browser, the TypeScript runtime has one job: execute
that plan. The generated contract (`runtime/types/plan.ts`) mirrors the C#
domain, so plan-shape drift fails typecheck before it reaches the browser.

C# never executes browser behavior. TypeScript never invents behavior the
plan does not carry. Invalid behavior is made unrepresentable in the C# plan
domain. The runtime guards external boundaries only: DOM lookup, browser
APIs, network, and JSON the framework did not produce.

Five layers, four boundaries. Every rules file, dispatch table, and memory
document uses this numbering.

```
Layer 1  C# DSL & Plan Domain (frozen public authoring surface + rich domain)
         Quality: typed authoring, compile-time component/member APIs, value objects,
                  invariants, no string magic except the plugin boundary
         Harness: domain behavior tests; plans proven end-to-end by Playwright slices using the DSL
         ↓
         BOUNDARY: plan shape change → regenerate runtime/types/plan.ts (PlanContractGenerator)
         ↓
Layer 2  Generated TS Plan Contract (runtime/types/plan.ts)
         Quality: generated discriminated unions — never hand-edited (hook-enforced)
         Harness: npm run typecheck (generation runs first; drift is a typecheck failure)
         ↓
         BOUNDARY: contract change → failing vitest drives the runtime handler
         ↓
Layer 3  TS Runtime Executor
         Quality: direct executor, sync/async lanes, no fallback behavior, vendor isolation
         Harness: vitest + boot(), architecture enforcement tests
         ↓
         BOUNDARY: eyes first in a real browser, then Playwright — browser is truth
         ↓
Layer 4  Browser Verification
         Quality: real interactions, visible outcomes, no page.evaluate()
         Harness: manual smoke → Playwright BDD slices
         ↓
         BOUNDARY: working sandbox example before writing docs
         ↓
Layer 5  Documentation & Skills
         Quality: dev-facing language, question-driven, glossary aligned with code
         Harness: sandbox-verified examples, Rider diagnostics
```

## Must Do

- Read the DSL source before shared plan and runtime edits. The source is the
  requirement.
- Close one matrix row per commit. Report progress only from committed,
  verified work.
- Use typed component APIs and object-member contracts. Strings belong only
  at the plugin boundary.
- Keep the runtime a direct executor of generated plans.
- Keep sync reactions sync.
- Generate the TS plan contract from the C# plan domain. Regenerate and
  typecheck on every plan-shape change.
- Delete wrappers, helpers, and syntax-pinning tests that map to no DSL graph
  node.
- Prove page-visible behavior with Playwright through real interactions.

## Never Do

- Infer behavior from old tests, docs, memories, or comments.
- Claim progress from uncommitted local edits.
- Add stringly component APIs outside the plugin boundary.
- Add plan-shape validators, fallback paths, registries, claims, or rejects
  for framework-generated plans.
- Make the runtime async by default.
- Hand-edit `runtime/types/plan.ts`.
- Revive JSON schema as a contract: no schema drift gates, no
  `AssertSchemaValid`, no schema-first process.
- Use `page.evaluate()` to simulate user behavior.
- Preserve confusing code because it already exists.

## Key Concepts

These terms stay consistent across C#, generated TS, runtime code, tests, and
docs.

**Browser object model.** Every object the runtime touches is a JavaScript
object with members: properties, methods, and events with callbacks.
Properties are read and written. Methods take arguments and may return
values. Events fire callbacks. Components, plugins, and app-level objects all
share this shape. Any member that returns a value can serve as a source
wherever the DSL accepts one: reaction values, conditions, gather payloads,
route params, headers, dispatch payloads, and plugin arguments. Component
slices expose members through typed APIs.

**ValueExpression.** One concept reads every value: component member, plugin
member, URL parameter, event payload, success body, error body, request
snapshot, literal, object, and array. A module that needs a value uses this
shared path. A second resolver is a defect.

**Rich domain model.** The fewest concepts that name DSL behavior. Wrappers,
registries, fallback paths, claims, and validators for generated plans do not
qualify. Delete or inline a type that only carries parameters, hides a
branch, needs explanation before its value shows, or maps to no DSL graph
node.

**The runtime.** The TypeScript code in `Alis.Reactive.Assets/runtime/` that
executes plans in the browser. esbuild bundles it into
`Alis.Reactive.Assets/dist/scripts/alis-reactive.dev.js` (IIFE). It ships
inside the AlisReactive NuGet. The shipped `AlisReactive.targets` copies it
into the consumer's `wwwroot/scripts/` with the package version in the
filename.

**The plan contract.** C# `Render()` serializes the plan to JSON inside a
`<script type="application/json" data-reactive-plan>` element. The runtime
discovers these elements, parses the JSON, merges partials by `planId`, and
boots each composed plan. `PlanNodeDiscriminator<T>` delegates polymorphic
serialization to the concrete type; each plan model class carries its own
`kind` property (e.g., `public string Kind => "set"`), matched by TypeScript
discriminated unions. Type generation runs at the front of `npm run
typecheck`. When plan shape changes: update the C# domain, regenerate
`runtime/types/plan.ts`, run `npm run typecheck`, and prove runtime behavior.

**Plan-driven IDs.** `IdGenerator` produces every input element ID at C#
render time from the model type and property expression. Format:
`{Namespace_TypeName}__{MemberPath}`.

```
Model:      Alis.Reactive.SandboxApp.Models.OrderModel
Expression: m => m.Address.City
ID:         Alis_Reactive_SandboxApp_Models_OrderModel__Address_City
```

Every vendor produces the same ID for the same expression, so IDs are
deterministic and collision-free by construction. Non-input IDs (buttons,
elements, containers) are the developer's choice, made explicit through
`p.Element("my-id")` or `Html.NativeButton("btn-id", ...)`. A collision there
is a developer error. No fallback IDs. No generated suffixes. The plan
carries every ID the runtime needs. The runtime resolves each one with
`getElementById`. A wide DOM query (`querySelectorAll`, tag or class scans,
traversal) means the plan is missing information; fix the C# plan model to
carry it. Two wide-query categories are justified, both external boundaries:

- **Plan discovery** — finding `[data-reactive-plan]` script elements in HTML
  the runtime did not author (initial boot, injected partials).
- **Self-stamped cleanup** — finding elements by a data attribute the runtime
  itself wrote (retry indicators and similar).

Any other wide query fails the architecture enforcement test. Its allowlist
is the only registry of boundary files; this document carries the principle,
never the file list.

**Sync and async lanes.** Sync reactions stay sync: property set, method
call, dispatch, branch evaluation, and component update. Async boundaries:
HTTP, parallel HTTP, remote triggers, confirm/user decisions, and partial
injection.

**HTTP, gather, response.** Gather is `target <- source`: payload, header,
and route-param targets read through `ValueExpression`. GET emits query
string values; POST, PUT, and DELETE emit a JSON or form-data body as
declared. Response routes create success and error scopes. The request
snapshot creates request scope. A chained request runs only after success and
may gather from that success response. Parallel starts branches concurrently
and runs completion after all settle.

**Conditions.** Conditions are deterministic graphs over the same value
sources. Condition blocks appear in authored order, mixed with sets, calls,
dispatches, HTTP, parallel, and injection. Else-if and default routing
preserve first-match behavior. Nested branch behavior exists only where the
DSL source defines it.

**Partial slots.** SSR composition joins plan scripts by `PlanId`. Browser
partial injection uses `SlotId` as the load and unload handle. Active Plans
are recomposed from the boot snapshot plus loaded slots. Component id,
vendor, and type remain the runtime object join keys. Slot unload aborts
slot-owned behavior and validation wiring, removes slot-owned components and
rules, and keeps boot-level and app-level objects mounted.

**Validation.** FluentValidation is the server authority. Browser validation
is client metadata recorded through `ReactiveValidator<T>` and DI. Async,
MustAsync, and server-only rules stay on the server. Client metadata stays
deterministic, supports arrays and objects, and binds by component ID. The
client rule layer does not rebuild FluentValidation and does not extract
rules through reflection.

**Plugins.** A plugin is the escape hatch where the JSON DSL cannot express
the work: URL APIs, DOM APIs, array manipulation beyond the DSL. Plugin names
may be strings at the plugin boundary; component APIs stay typed. Plugin
reads and calls integrate through object members and `ValueExpression`.

**App-level objects.** Drawer, Toast, Confirm, Loader, ActionLink, and
similar page services use fixed identifiers. They are browser objects with
typed members, not runtime globals.

## Build & Run

Use the root script wrappers. They are the supported CLI surface for
framework developers and LLM agents. The command guide is
`docs/developer-cli.md`.

```bash
scripts/doctor.sh          # read-only CLI preflight
scripts/build.sh           # npm deps -> browser assets -> dotnet build
scripts/run.sh             # browser assets -> sandbox at http://localhost:5220
scripts/test.sh            # full gate, including observable Playwright
scripts/test.sh --no-e2e   # skip only the browser leg
scripts/pack.sh <version>  # browser assets -> Release build -> six NuGets
```

Full gate order:

```text
npm run typecheck -> npm run build:all -> npm test -> dotnet build -> non-Playwright dotnet tests -> scripts/playwright.sh --no-build
```

Playwright runs through `scripts/playwright.sh`, never raw `dotnet test`. The
wrapper prints active-test progress markers, writes live log/TRX/diag
artifacts, and rejects browser assets or `--no-build` binaries that predate
the current source.

Watch loop for active editing:

```bash
npm run watch:runtime
npm run watch:design-system
dotnet watch --project Alis.Reactive.SandboxApp
```

For UI work, use `docs/developer-cli.md#ui-developer-workflows` to choose the
watcher and the proof command. Framework assets ship from
`Alis.Reactive.Assets/`; sandbox-only assets live under
`Alis.Reactive.SandboxApp/`.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| C# | C# 14, compiled for BOTH `net48` and `net10.0` — every shipped package dual-targets (TagHelpers: net10-only by design). A net48 language-feature error (CS8xxx/CS90xx) means rework the feature to what PolySharp polyfills — never work around it. Host-type splits go behind `#if NET48`. |
| TS | TypeScript + esbuild ESM bundles, Tailwind CSS v4. Versions pinned in `package.json`. |
| Components | Syncfusion EJ2 32.x (Fusion) + Native HTML. Always through DSL: `Html.InputField(plan, m => m.Name).NativeTextBox(build: b => ...)` |
| Validation | FluentValidation as server authority (version per target framework, pinned in the csproj); `ReactiveValidator<T>` records browser validation metadata through DI |
| Tests | NUnit, Vitest + jsdom, Playwright. Versions pinned in the test csproj and `package.json`. |

## Skills

Skills live in `.claude/skills/`. Load applicable skills when useful. DSL
source and this root file override skill guidance that has drifted.

| Skill | Use for |
|-------|---------|
| `reactive-dsl` | Plan, triggers, Element, Dispatch, Component, InputField, .Reactive() |
| `http-pipeline` | Get/Post, Gather, Response, Chained, Parallel, WhileLoading, Into |
| `conditions-dsl` | When/Then/ElseIf/Else, operators, guard composition, source types |
| `validation-rules` | FluentValidation rules, Validate, ValidationErrors, WhenField |
| `onboard-fusion-component` | Onboard/audit Syncfusion components — artifact gate chain + fail-closed verifier |
| `solid-ts-audit` | SOLID analysis of TypeScript runtime modules |
| `modern-csharp` | C# 14 patterns that clarify the plan domain model |
| `dotnet-xml-docs` | XML doc comments on the C# public surface — tags, formatting, Alis patterns |
| `bdd-testing` | Playwright BDD tests — nested vertical slices, 5 rules, 7-behavior contract, blind reviewer |

Two deterministic PreToolUse hooks are live in `.claude/settings.json`
(scripts in `.claude/hooks/`): generated files are protected from hand-edits
(`runtime/types/plan.ts`, generated onboarding JSON/traces), and Playwright
must go through `scripts/playwright.sh` instead of raw `dotnet test`. Hooks
gate; the one-line rules here teach.

Hookify rule templates live in `.claude/hookify.*.local.md`. **All are
disabled (`enabled: false`)** — they document candidate quality gates and
enforce nothing. Enable one by setting `enabled: true`. The
public-contract-freeze rules (`protect-api-surface`, `no-public-in-libraries`)
were removed as noise.

Localized `CLAUDE.md` files live beside the work they govern — Fusion slices,
Playwright tests, the sandbox, the TS runtime, and the onboarding artifact
tree. Each loads when files in its directory are touched and adds only
directory-specific constraints; this root file remains authoritative. Loading
is discovered at session start (verified): guidance created mid-session takes
effect in the NEXT session. Research subagents (Explore/Plan) do not load
this hierarchy at startup, though directory CLAUDE.mds still attach when the
subagents read files there — inline critical constraints into agent prompts
rather than relying on either.

## Rules

### 1. DSL Source Before Code

The public DSL source is the requirement. Samples, XML docs, old unit tests,
`.claude` memory, the current runtime, and schema history are not. Archived
design documents and remembered plans are historical context, never
requirements. For shared plan and runtime work: read the DSL source files,
write the matrix row (see Process), name the domain term and the runtime
behavior, then edit. Local cleanup may proceed when the matrix row is already
clear.

### 2. Plan Is the Only Contract

Views carry no JavaScript. No `document.addEventListener` in `.cshtml`, no
`window.alis`, no inline `<script>` blocks. `root.ts` discovers and boots
every plan.

### 3. New or Changed Primitive — All Layers

1. C# plan model class — sealed class, `internal` constructor
2. Polymorphic registration — `PlanNodeDiscriminator<T>` delegates to concrete type
3. Builder method — PipelineBuilder, ElementBuilder, or TriggerBuilder
4. Generated TS plan contract — regenerate; typecheck proves alignment with C#
5. Runtime handler — new switch case + `assertNever`
6. C# domain behavior test — prove DSL intent becomes the right plan model
7. TS runtime behavior test — `Alis.Reactive.Assets/runtime/__tests__/*.test.ts`
8. Playwright test — browser behavior with sandbox view
9. Sandbox view — demonstrate the primitive

### 4. Vertical Slices — Duplication Over Abstraction

Each component module is self-contained. Behavior never moves into shared
base classes. Duplication between slices is intentional.

### 5. Vendor Isolation

A new component is a C# vertical slice with `IInputComponent` and zero TS
runtime changes. Vendor knowledge lives in three runtime roles: the
per-vendor driver (vendor → component root + event wiring), the per-vendor
event adapter, and vendor component modules. Everything else stays
vendor-blind and dispatches through the registered driver. Adding a vendor
means registering a driver and adding its adapter. The architecture test
enforces this isolation; its allowlist is the only registry of exceptions.

### 6. Trust Generated Plans — Boundary Errors Only

The runtime and plan domain trust framework-generated plans. If the plan
says source A is assigned to target B, the runtime reads A and writes B. Do
not add defensive throws, validators, rejects, claims, or fallback paths for
shapes the typed DSL already controls. Errors belong at boundaries:
developer-authored DSL misuse, DOM/component/plugin lookup, browser APIs,
network, and JSON the framework did not produce. When such JSON reaches the
browser, the failure must expose the drift with context instead of becoming
control flow. Execution bookkeeping is not validation: names describe what is
remembered for execution or unload, never suspicion of the plan. A fallback
is a deliberate, justified exception, never the default response to
uncertainty.

**Null escape hatches require justification.** Every NEW per-property
`[JsonIgnore(WhenWritingNull)]`, nullable property declaration, or
`?? fallback` must be PROVEN necessary by answering in writing: "Could this
be a sentinel/empty/default instead? If yes, why am I taking the shortcut?
If no, what domain meaning would `Empty`/`None` collide with?" Mechanical
addition of null markers during a refactor is a recorded failure pattern —
when removing tech debt, if the count of null markers on ANY surface goes
UP, stop and audit each new marker for sentinel-replaceability before
committing.

### 7. Plan-Driven IDs — No DOM Scanning

The plan carries every ID the runtime needs (see Key Concepts → Plan-driven
IDs). The runtime resolves each one with `getElementById`. Non-input IDs are
the developer's choice. No fallback IDs. No scanning.

### 8. API Surface Is Frozen

No `public` constructors on plan model classes. All plan model class
constructors are `internal`. All plan model properties use `internal set`.
Developers interact through builder APIs and factory methods (`Html.On`,
`Html.InputField`, `p.Get`, `p.When`), never through constructors.

### 9. Root Cause, Not Patch

Trace the code path. Identify the line. Understand WHY before changing WHAT.
After 2 failed attempts: research online, save findings, dispatch agents with
input evidence and output criteria. Fix the cause. Report "done" in past
tense with what the browser showed: the gesture performed and the visible
result.

A patch fixes a symptom without understanding the cause. Ten patches are ten
mistakes. Leave every touched file cleaner than you found it; fix the code
smell you see instead of walking past it.

### 10. Tests Are Production Code

Tests prove behavior and protect domain language. Delete or rewrite a test
that pins helper classes, old JSON shape, dropped vocabulary, or internal
syntax. A test that changes whenever implementation changes is design debt.
Runtime tests cover executor behavior; Playwright covers page-visible DSL
behavior, on runtime assets built from the current source.

### 11. Code Hygiene

These practices compound — each prevents a category of bugs. Worked examples
live in `.claude/memory/coding-principles.md` (Code Hygiene).

- **Revealing names over complex conditions** — extract booleans into named variables; the name is the documentation.
- **Variables close to usage** — declare where first needed; if used once, inline it.
- **No nesting** — early returns and guard clauses; flat control flow.
- **No dead code** — delete unused/unreachable/commented-out code; git has history.
- **Small methods, single responsibility** — if a block needs a comment, extract it into a named method.

### 12. Prefer BDD Vertical Slice Playwright Tests

Every Playwright test is one user-visible behavior: isolated (its own page,
no ordering, no shared state), vertical (full stack, no mocking, no
`page.evaluate()` — the one exception: framework gather-pipeline tests may
assert `request.PostData`), and named for what the user sees. Load the
`bdd-testing` skill before writing any Playwright test — it carries the
nested-vertical-slice method and points to the 5 BDD rules and 7-behavior
contract;
`tests/Alis.Reactive.PlaywrightTests/CLAUDE.md` carries the local rules.

### 13. Quality Aspirations

Known weaknesses are tracked in `.claude/memory/quality-principles.md`
(Quality Aspirations ledger) — DDD depth, serialization contracts, TS tracing.

### 14. Git Worktrees for Feature Work

```bash
git worktree add .worktrees/<feature-name> -b feature/<feature-name>
cd .worktrees/<feature-name>
```

## Process

### Pass Protocol

Every plan or runtime pass starts with this row:

```text
Close matrix row: <DSL source call> -> <domain term> -> <runtime behavior>
```

The row names the source files, the developer intent, the C# domain term, the
JSON and generated TS term, the runtime executor behavior, the sync or async
lane, the behavior proof, and the commit boundary. If the row cannot be
written from source, stop and read more source.

Then list:

1. DSL source files used as requirements.
2. Sync or async lane expectation.
3. Code to delete or simplify.
4. Tests that prove behavior before commit.
5. The commit boundary.

A module is done when its matrix rows are covered, focused tests pass,
generated TS is checked when plan shape changed, and the slice is committed.
Do not start a second module before that commit.

### Source-Grounded Design Loop

1. Read the public DSL source for the row.
2. Draw/update the graph: trigger -> pipeline -> reaction, condition -> branch,
   request -> gather, gather target <- value source, response -> scope,
   partial slot load/unload -> Active Plan composition.
3. Fill the input/output matrix: DSL call -> developer intent -> C# domain ->
   JSON/generated TS -> runtime executor -> proof.
4. Delete helpers and tests that map to no graph node.
5. Implement the smallest domain/runtime change that closes the row.
6. Run focused runtime/domain tests.
7. Run `npm run typecheck` when C# plan or generated TS shape changed.
8. Build runtime assets before Playwright.
9. Commit the closed row.

### Wrong Plan Protocol

If a pass touches the same module repeatedly without closing it, stop editing
and redraw the graph from DSL source. Repeated local patches mean the design
is not clear enough.

If an implementation needs a fallback, registry, generated-plan validator, or
generic lifecycle concept, prove the DSL graph node that requires it. If none
exists, delete the concept.

If a pass touches an unexpected layer, the matrix row is incomplete. Stop
immediately, save what you learned (to memory — context loss is the real
cost), record the new edge, and redesign before continuing. Present the
problem concisely, step by step. Why: one session made 3 architecture changes
in 30 minutes with no plan; another took 26 fix commits in a day because
design was discovered by coding.

### Pre-Flight Checklist

- [ ] DSL source read, not inferred from tests or old docs.
- [ ] Matrix row written with source file, DSL input, domain term, generated TS term, runtime output.
- [ ] Sync/async lane named.
- [ ] API surface unchanged unless the task explicitly requires a public contract change.
- [ ] Code to delete/simplify identified.
- [ ] Behavior proof selected before editing.
- [ ] Rejected alternative named, with the one fact that killed it.

### Post-Flight Checklist

Each checked box carries its evidence: the command run and one line of what
it printed.

- [ ] Focused behavior tests pass.
- [ ] Generated TS checked when C# plan shape changed.
- [ ] Runtime assets rebuilt before Playwright when TS changed.
- [ ] Playwright behavior proved for page-visible changes.
- [ ] `git status` inspected.
- [ ] Commit message names the closed behavior row.
- [ ] No dropped vocabulary, dead code, defensive plan validation, or schema-contract references left behind.

### Review Loop — Every Change

**Review the live tree, and name it.** Every review — agent or human,
adversarial or routine — runs against the current branch's working tree at HEAD,
never a frozen historical commit. The reviewer confirms the target up front
(`git rev-parse --abbrev-ref HEAD`, `git rev-parse HEAD`, clean `git status`) and
states that branch + SHA in its output; a dispatched review agent is given the
branch and HEAD SHA and told to verify it before reading code. Why: a review
pinned to an already-merged commit raised four "unpaid debt" findings, every one
already closed by later commits on the branch — stale-tree false alarms and a
misleading verdict are the cost of an unnamed, out-of-date review target. A
finding only counts when its `file:line` still exists at HEAD.

Review against the matrix, not against implementation preference:

1. **Design Review**: Does the DSL graph fully explain the chosen domain terms?
2. **Implementation Review**: Does every new type/method map to a DSL node or edge?
3. **Behavior Review**: Do tests prove the row through the correct boundary?
