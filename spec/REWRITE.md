# The rewrite

What the rewrite builds and the order it builds it in. The decisions behind each line are in `spec/CLAUDE.md`; the
meanings are in `spec/Core/DESIGN.md` and each module's folder. Two designs fed this file, one for finding a feature
in the fewest hops and one for the fastest honest feedback loop; where they disagreed, the choice and its reason are
in the line.

## 1. The view from above

One screen: what the rewrite builds, where each piece lives, what proves it, and where every other answer is. Each
later section is the detail of a line here.

### 1.1 The layout

```
spec/                               the design: one folder a module, its TLA+ specs, this plan, check.py
Alis.Reactive/                      the core, one assembly: sixteen rows of module folders, each one namespace
Alis.Reactive.Assets/               the core's browser code: runtime/<Module>/ runs what that module's C# writes
Alis.Reactive.Analyzers/            the compiler's rules, packed inside the core: <Module>/ holds that module's
Alis.Reactive.Native/               a consumer: the plain-DOM components
Alis.Reactive.Fusion/               a consumer: the Syncfusion slices
Alis.Reactive.OpenTelemetry/        a consumer: the trace sink
tests/
  Alis.Reactive.Conformance.World/            what a browser test and the app agree on, and nothing of the framework
  Alis.Reactive.Conformance.App/              the care home app every browser test runs on; .Net48 is its MVC 5 twin
  Alis.Reactive.Conformance.PlaywrightTests/  the browser tests, <Module>/<Screen>/When*.cs
  Alis.Reactive.TestRules/                    the browser tests' own compiler rules; .AnalyzerTests holds their cases
  Alis.Reactive.ModuleTests/                  <Module>/: a core module's pure tests of its public surface
  Alis.Reactive.AnalyzerTests/                <Module>/: each analyzer's cases
  Alis.Reactive.ArchitectureTests/            what the repository holds
  Alis.Reactive.Consumer.Net10/ and .Net48/   fresh apps on the packed packages
scripts/                            one command for each kind of proof
```

A dependency points one way. The core names no consumer. Each consumer stands on the core's public surface alone, as
an app's own component does. The app stands on the shipped projects and the world contract. A browser test stands on
the world contract alone and meets the app only through the browser. A module test stands on the shipped projects'
public surface alone. Inside a project a slice uses only the rows before its own (2.3), and a module's folder has one
name in every tree (2.2). Section 2.1 is this tree in full.

- **Six packages, one version.** AlisReactive (the core, its analyzers inside), AlisReactive.Native,
  AlisReactive.Fusion and AlisReactive.OpenTelemetry, the trace sink a page registers (N20.2), ship code;
  AlisReactive.DesignSystem and AlisReactive.NativeTagHelpers ship empty, so their ids stay, and nothing in the tree is
  kept for them. Main's AlisReactive.FluentValidator is not packed: the core's validation carries its doors (M8.F1;
  spec/CLAUDE.md, 2026-11-10).
- **One app proves the framework.** The care home conformance app (`spec/Reference/app.json`): care screens a care
  home's staff use, the component bench with the conformance library's declarations, and the refusal routes, run
  under the hosts app.json declares. Every browser test runs on one of its screens. No core test loads Syncfusion.
- **A feature is one path through the spec to the code.** `spec/<Module>/features.json` names its question, its
  sentences, its nodes, its TLA+ invariants, its defects and its tests; each test names its screen, hosts, parts and
  held steps in `spec/Reference/app.json`.

### 1.2 The kinds of proof

Each kind proves one thing, once, on the rung that holds it. A defect a higher row prevents gets no test on a lower
one.

| Kind | What it is | It proves | It prevents | Its standard | Its command | When |
|---|---|---|---|---|---|---|
| TLA+ | the specs of `spec/<Module>/*.tla` under TLC, each with the mutant configurations that must fail (2.6) | the design: every ordering of the async lanes keeps its invariants | an ordering decided in code, and a browser test that provokes a race by timing | `spec/CLAUDE.md`: an invariant that restates its action proves nothing, and each one is a named browser test | `python3 spec/tools/check.py --check tlc` | before a lane's tests are written, and on every changed spec |
| The compiler | the types of the public surface and the ALIS analyzers (`Alis.Reactive.Analyzers`) | that a view, a validator or a declaration cannot write the mistake | a defect that reaches a browser though the source states it | `.claude/rules/analyzers.md`, a refused and an accepted case a rule | `dotnet test tests/Alis.Reactive.AnalyzerTests`; `python3 spec/tools/check.py --check fluent` compiles `spec/api`, the surface and its refused sentences | every build; a rule lands with the module whose line it enforces |
| The runtime's compiler and lint | TypeScript at every strictness option and the lint of 2.7 | that the runtime invents no behavior the plan does not carry | a default for plan data, a floating promise, a switch that misses a kind | `Alis.Reactive.Assets/CLAUDE.md`, a refused and an accepted file a rule | `npx tsc -b Alis.Reactive.Assets`; `npx eslint Alis.Reactive.Assets --max-warnings 0` | every commit |
| Module tests | a core module's pure tests of its public surface, each half alone against the schema: in C#, a sentence written through public doors, the nodes it renders compared with literal nodes written from `spec/Core/schema.md` and read against `plan.schema.json`, and a validator's server verdict; in TypeScript, what a runtime module's index exports, run in node with no DOM | that a module's half writes, or executes, the schema's nodes as the spec states them | a half that drifts from the schema before a page shows it; it proves no behavior in a browser | the project's own rule file: the public surface alone, no internal exposed, no recorded snapshot | `dotnet test tests/Alis.Reactive.ModuleTests`; `npx vitest run` | in the module's own unit, with its code |
| Playwright tests | a gesture and its visible outcome in Chromium, on a screen of the conformance app, in a world of its own; a refusal the compiler cannot see is read on a refusal route | behavior, and that the code refines each TLA+ action a test holds | a framework that compiles and lies | `.claude/rules/tests.md`, held by the test rules TEST001 to TEST013 (section 2.10) and the bindings (2.5) | `scripts/playwright.sh` | written from its promise before its code; it passes in the unit check.py lands it in |
| The planted defect | the defect a test names, put in the code it proves | that the test fails for that defect | an assertion the broken code also satisfies | `.claude/rules/tests.md`, rule 5 | `scripts/plant-defect.sh` | before a test is kept |
| Architecture tests | rules over what the repository holds: no comment, every name a rule file cites, the hooks' refusals, the lint's own rules, the shipped packages and their slice tables | that the tree keeps its own rules | drift between the tree and the pages that govern it | the project's own rule file; a refused and an accepted case a rule, which WhenAnArchitectureTestIsWritten holds | `dotnet test tests/Alis.Reactive.ArchitectureTests` | every commit |
| The spec's checks | `spec/tools/check.py`: the spec against itself and against the tree | that every feature has its unit, every test its screen, every door its verdict, every pin its version | a plan that drifts from the design it builds | `spec/CLAUDE.md`; a refused and an accepted plant a check (`spec/plants`) | `python3 spec/tools/check.py` | at every committed head, before its push |
| Probes | a console or a browser script under `spec/probes/` | a fact of a browser, a host or a vendor the design rests on | a design built on a remembered fact | each probe pins the version of the fact it reads | `python3 spec/tools/check.py --check probes` | on a changed probe or pin |
| verify-net48 | CI's Windows job (2.8) | each test whose placement lists the twin, on MVC 5, and the packed packages on a fresh MVC 5 app | a net48 host that compiles and does not run | section 2.8 | the workflow | when the rewrite is done; it gates no unit (the owner, 2026-11-11) |

`scripts/check.sh` runs every row that needs no browser, in one command; `scripts/test.sh` adds the browser tests.
The confidence of a piece of work is the line its row's command printed, read when it printed; root CLAUDE.md says
how a unit collects those lines.

### 1.3 Where every answer is

| The question | Its one place |
|---|---|
| What is built, and where each piece lives | 1.1 and section 2 |
| The order it is built in, and when a unit is done | section 4 |
| What proves a piece of work, by what command | 1.2 |
| What governs what; each forbidden practice and the tool that refuses it; how a unit is worked; how confidence is determined; when a handoff is written and a session starts, how to begin and how to reorient; where a correction goes; when a fork is used | root CLAUDE.md |
| The standard of one kind of file | its file under `.claude/rules` |
| The standard, the command and the refusals of one project | that project's own CLAUDE.md |
| The owner's decisions, in the owner's words | `spec/CLAUDE.md`; verbatim in `spec/NOTES.md` |
| Each mistake, its class, what it cost and what holds the class | `spec/MISTAKES.md` |
| A module's doors, features, defects and tests | `spec/<Module>/module.md` and `features.json` |

## 2. The repository

### 2.1 The tree

```
Alis.Reactive.slnx                    the shipped projects, the generator and the net10 test projects
Directory.Build.props                 LangVersion 14.0, nullable on, warnings as errors, AlisSliceTree off by default
Directory.Build.targets               a project with AlisSliceTree on hands its SliceTree.txt and that property to the
                                      compiler
Directory.Packages.props              central package management: one exact version per package and framework, but
                                      Roslyn's second (2.7), which the projects that host the compiler take by
                                      VersionOverride; each project it manages holds a packages.lock.json
global.json                           SDK 11.0.100, rollForward disable
nuget.config                          nuget.org alone, every other source cleared
.node-version .python-version .java-version
package.json  package-lock.json  eslint.config.js   npm workspaces, locked; the lint, its import order generated from
                                      each SliceTree.txt
vitest.config.js                      the runtime's pure tests: node, the files that end in .test.ts under the runtime
build.mjs                             esbuild: one bundle a package, written into its project's static web assets
tools/PlanContractGenerator/          writes Alis.Reactive.Assets/runtime/Schema/plan.ts and plan.schema.json
.editorconfig .gitignore AGENTS.md CLAUDE.md
README.md                             every package's readme (Directory.Build.props packs it): the docs, beside the XML docs
.claude/                              the session's settings, hook, rules, skills and agent (below the tree)
.github/                              the toolchain action, Dependabot, the release notes and section 2.8's workflows
scripts/                              section 2.7's scripts

Alis.Reactive/                        net48;net10.0, PackageId AlisReactive, AlisSliceTree on
  SliceTree.txt BannedSymbols.txt PublicAPI/net48/ PublicAPI/net10.0/
  Schema/ TypeKnowledge/ PathsAndWire/ OwnershipAndState/ Tracing/ Execution/ ValuesAndSources/ Conditions/
  ArrayLambdas/ Declarations/ Vendors/ Plugins/ Elements/ InputsAndIds/ Validation/ RequestsAndResponses/
  Realtime/ TriggersAndPipelines/ PartialsAndSlots/ RenderingAndDelivery/
Alis.Reactive.Assets/                 NoTargets, not packed: the core's browser code
  tsconfig.json runtime/SliceTree.txt runtime/index.ts runtime/<Module>/ (with its *.test.ts beside it)
Alis.Reactive.Analyzers/              netstandard2.0, packed inside AlisReactive
  <Module>/                           the rules that enforce that module's lines, its generators and build files
  Analyzers/                          module 20's own rules, ALIS009 to ALIS013
Alis.Reactive.Native/                 net48;net10.0, PackageId AlisReactive.Native, on the core alone
  SliceTree.txt PublicAPI/ native.ts
  NativeComponents/ NativeComponents/<Slice>/ NativeAppLevelObjects/<Slice>/   declaration, render helper, its .ts
Alis.Reactive.Fusion/                 net48;net10.0, PackageId AlisReactive.Fusion, Syncfusion [33.1.47] per host
  SliceTree.txt PublicAPI/ fusion.ts
  FusionComponents/ FusionComponents/<Slice>/ FusionTemplates/ FusionAppLevelObjects/<Slice>/
Alis.Reactive.OpenTelemetry/          net48;net10.0, PackageId AlisReactive.OpenTelemetry, on the core alone
  SliceTree.txt PublicAPI/ opentelemetry.ts
  Tracing/                            the layout helper and the trace sink it registers (N20.2)
build/EmptyPackage.proj  build/EmptyPackage.md   packed once per empty id

tests/
  Alis.Reactive.Conformance.World/            net48;net10.0, on no Alis.Reactive assembly: the world's records, its
                                              typed seed builders, the gate names generated from the TLA+ specs,
                                              the promise constants generated from app.json, the FailsFor and
                                              Gesture marks
  Alis.Reactive.Conformance.App/              net10 MVC on the shipped projects: Care/<Screen>/, Bench/,
                                              Refusals/<Module>/, Shared/, Components/<Declaration>/, HostVariants/,
                                              World/
  Alis.Reactive.Conformance.App.Net48/        the MVC 5 twin: links the app's files, consumes the packed packages
  Alis.Reactive.Conformance.PlaywrightTests/  Harness/, <Module>/<Screen>/When*.cs, <Module>/Hosts/<Host>/,
                                              <Slice module>/<Slice>/ for a slice's own tests; on the world alone
  Alis.Reactive.TestRules/                    netstandard2.0, never packed: TEST001 to TEST013 on each Playwright
                                              project
  Alis.Reactive.TestRules.AnalyzerTests/      each test rule's refused case and accepted case
  Alis.Reactive.Consumer.Net10/               a fresh net10 app on the packed packages, outside the slnx
  Alis.Reactive.Consumer.Net48/               a fresh MVC 5 app on the packed packages, outside the slnx
  Alis.Reactive.ModuleTests/<Module>/         each core module's pure tests of its public surface (1.2)
  Alis.Reactive.AnalyzerTests/<Module>/       each analyzer's cases
  Alis.Reactive.ArchitectureTests/            WhenCodeIsWritten, WhenARuleNamesTheCode, WhenClaudeRunsAHook, the POC's
                                              WhenTheCoreIsCompiled and WhenAContractDriftIsSuppressed, over the
                                              conformance app as built WhenAnAppRendersItsPages, which opens every
                                              address it answers to a GET and every link its pages hold, and
                                              SandboxStaticStateTests, renamed WhenTheAppHoldsNoStaticState, which
                                              refuses a static a request can change, over each fresh app's build
                                              WhenAConsumerAppIsBuilt (section 2.9), and over the runner's selection
                                              and result WhenAPlaywrightRunStarts and WhenAPlaywrightRunEnds (section
                                              2.5), over the lint's rules
                                              WhenTheLintIsConfigured (section 2.7), and over the shipped packages
                                              and their slice tables WhenAPackageShips (M20.D1), each rule with its
                                              refused and accepted cases, which WhenAnArchitectureTestIsWritten holds; over
                                              the shipped projects' files WhenCodeCompilesForBothHosts (2.2); over
                                              every assembly WhenAnInternalIsExposed, which refuses a friend
                                              assembly; over the solution's projects WhenAProjectHoldsFolders
                                              (2.3); over the contract generator's reader
                                              WhenThePlanContractIsGenerated; over every view WhenAViewIsWritten, which refuses script
                                              in one; their
                                              helpers Repository, Hosts, Scripts and read-scripts
                                              and its packages.lock.json and CLAUDE.md; LintCases holds the lint's refused and
                                              accepted files; RefusedCaseAttribute and AcceptedCaseAttribute are
                                              its two case marks, and WrittenFolder holds the files a case writes
spec/                                         the design source: BRIEF.md, REWRITE.md (this file), CLAUDE.md,
                                              NOTES.md, MISTAKES.md, STATE.json, history.tsv, Core/, Fluent/, api/, inventory/,
                                              plants/, probes/, Reference/, tools/, one folder per module
```

Beside the tree the repository keeps `.claude/settings.json`, `.claude/hooks/protect-generated.mjs`,
`.claude/rules/analyzers.md`, `.claude/rules/csharp.md`, `.claude/rules/docs.md`, `.claude/rules/tests.md`,
`.claude/rules/views.md`,
`.claude/skills/onboard-component`, `.claude/skills/prove-behavior` and `.claude/agents/pragmatic-architect.md`, which
the cut rewrites for the new tree, and `.github/actions`, `.github/dependabot.yml` and `.github/release.yml`. Each project
holds a CLAUDE.md of its own: its standard, its command and what it refuses.

The Playwright project keeps the `PlaywrightTests` suffix, so the deny rule on a raw `dotnet test` of a Playwright
project holds. It names the app with `ReferenceOutputAssembly="false"`, so the app builds first and none of its types
reach a test, and the framework's analyzers the same way, as an analyzer, for its slice table; the app's `World/` holds the world's middleware and endpoints on the world contract. The architecture
tests reference the app whole, for the two rules that read it as built: WhenAnAppRendersItsPages runs it in their own
process (Microsoft.AspNetCore.Mvc.Testing, 2.7), which takes a public type of the app, so its entry class is public,
and WhenTheAppHoldsNoStaticState reads the statics of its assembly. The consumer apps
and the twin each hold an empty `Directory.Build.props` and a `Directory.Packages.props` that turns central management
off, so their builds see what a fresh app sees.

### 2.2 One name in five places

| Tree | Folder | Holds |
|---|---|---|
| C# | `Alis.Reactive/<Module>/` | its builders, objects and sources, the plan nodes it writes, its render-time refusals; namespace `Alis.Reactive.<Module>` |
| Browser | `Alis.Reactive.Assets/runtime/<Module>/` | the code that executes its nodes, and its pure tests beside it |
| Analyzer | `Alis.Reactive.Analyzers/<Module>/` | each rule that enforces one of its lines, the rule's id in the file name |
| Tests | `tests/Alis.Reactive.Conformance.PlaywrightTests/<Module>/`, `tests/Alis.Reactive.ModuleTests/<Module>/`, `tests/Alis.Reactive.AnalyzerTests/<Module>/` | its browser tests, its pure tests, its analyzer cases |
| Spec | `spec/<Module>/` | `module.md` and `features.json`; the TLA+ it owns (section 2.6) |

Four rules move a type out of its module's folder, each forced by a fact, and the feature row's `writes` field cites
the file it moved to:
- P1. A type lives in the folder of the module whose meaning it carries.
- P2. `Pipeline<TModel>` and each type that hands a pipeline back live in `TriggersAndPipelines`: a call that names only
  its own type argument binds only to an instance member (`spec/Core/DESIGN.md:2322-2331` at 924147dbb), so the request
  verbs, the stage types, `RequestDefaults` and `NoModel` are members or types of the pipeline's module.
- P3. Every core door on `HtmlHelper<TModel>` and every startup call lives in `RenderingAndDelivery`: `Html.InputField`
  takes the plan, which holds handlers.
- P4. A type whose module sits below a type it needs moves to the lowest module that can hold it: `PageUrl` and
  `BrowserFile` to `RequestsAndResponses`, `IAppLevelComponent` to `Vendors`.

The folder and namespace names are the module table's (spec/CLAUDE.md). DESIGN.md's `Grammar`, `Pipelines`,
`Requests` and `Hosting` read `Schema`, `TriggersAndPipelines`, `RequestsAndResponses` and `RenderingAndDelivery`: the
grammar's classes, every node and closed choice, are `Schema`'s, and a module's folder holds the nodes its builders
write as instances. A typed contract is its module's, generated beside the grammar when that module's unit writes
it (D19.9): `HttpSend` and `HttpResponse` are `RequestsAndResponses`' (N9.1, N10.2), and `FormCheck` and `FormPlace`
land with the unit that builds the form's check (D16.5, D16.13). The block writer, the POC's
`Writing/PlanBuild.cs`, goes to `Execution/` and reads no module above it: the view's name prefix is text it holds.
A file of a shipped project compiles for both hosts or for one: a one-host file is named `.Net48.cs` or `.Net10.cs` and
wrapped whole in `#if NET48` or `#if NET10_0_OR_GREATER`, and no other conditional directive is written; a member that
differs by host is a partial member, implemented in each host's file (the core's own rule file states it, and
WhenCodeCompilesForBothHosts holds it). Realtime's files are net10's, each so named and wrapped, so the net48 build
keeps the folder its row names.
Module 20 has no folder in the core: its one type there, `TypedDslExemptionAttribute`, lives in `Declarations`, the row
below the two slices whose doors carry it (M20.N1).

### 2.3 The SliceTree

`Alis.Reactive/SliceTree.txt`, first row the foundation, each slice using only the rows before its own (ALIS010; the
table's syntax and the four rules that read it are M20.D2 to M20.D6):

| Row | Slices | Why here |
|---:|---|---|
| 1 | Schema | the grammar depends on nothing |
| 2 | TypeKnowledge | reads the shape nodes |
| 3 | PathsAndWire | a member's four names take the JSON name type knowledge gives |
| 4 | OwnershipAndState Tracing | where culture and time zone live; the trace field the doors above record through |
| 5 | Execution | the block writer writes into the plan ownership holds |
| 6 | ValuesAndSources | the one read writes through the block writer |
| 7 | Conditions | a condition is a typed source of bool |
| 8 | ArrayLambdas | a list's predicate is built by the comparisons |
| 9 | Declarations | a declared member is a source, a declared list takes the operations |
| 10 | Vendors Plugins | live objects of row 9's vocabulary |
| 11 | Elements InputsAndIds | an element is a declaration whose vendor is the DOM; an input is built from a declared property and its commit event |
| 12 | Validation | a rule's condition reads the lambda terms and binds to row 11's ids |
| 13 | RequestsAndResponses Realtime | every part of a request takes every source; a reply is a live object; the hub and stream are plugins |
| 14 | TriggersAndPipelines | P2 |
| 15 | PartialsAndSlots | a plan holds handlers and names the pipeline |
| 16 | RenderingAndDelivery | P3 |

Native's table is `NativeComponents` over `NativeAppLevelObjects/* NativeComponents/*`; Fusion's is
`FusionComponents FusionTemplates` over `FusionAppLevelObjects/* FusionComponents/*`; OpenTelemetry's is `Tracing`
alone. The runtime has its own table,
`Alis.Reactive.Assets/runtime/SliceTree.txt`, since its evaluator dispatches every kind and sits on top, and its last row is the dot, the bundle's entry beside
the table: `Schema`, `ValuesAndSources`, `OwnershipAndState`, `Plugins`, `Vendors`, `TriggersAndPipelines`,
`Execution` and the dot today, a row entering with its first file. The test app
holds `Components`, what its components share, over `Components/ConformancePeer`, which ConformanceComponent's
events hand over, over the other components' folders, over `World HostVariants` over `Care/* Bench/* Refusals/*`, so
no screen reaches into another's model, and last the files beside the table, its entry point, a class in the app's namespace, since ALIS011 refuses a
type in no namespace; the Playwright project holds `Harness` and then one row of module folders, so no module's tests
use another's helpers; the analyzer cases and the module tests hold the files beside the table and then one row of
module folders. Every project of the solution that holds a folder of C# builds with its table, which
WhenAProjectHoldsFolders holds; the analyzers project is the one that cannot, since an analyzer cannot load into its
own build.
ALIS011 expects `<assembly>.<first folder>` for every file; ALIS013 reads the `AlisSliceTree` build property in place
of a list of assembly names (M20.X1); `eslint.config.js` writes ESLint's `no-restricted-imports` from each table.

A row enters a table with the first file of its folder, since ALIS013 refuses an entry that names no folder (M20.D6).
U1 writes the four shipped projects' tables and builds each with AlisSliceTree on, so from then on a file in a
folder with no row fails the build. The tables are empty but for the core's, which holds `Declarations` for the
exemption (M20.N1); U3 writes the core's 16 rows with their folders.

### 2.4 Naming

1. A module's folder is its module-table name, the same in every tree; a folder exists only where its module has code.
2. A core module's folder is flat, and a file's namespace is its project's root namespace and its first folder.
3. A component's folder is its class name and holds its declaration, its render helper and its TypeScript.
4. A Fusion class is `Fusion` and the EJ2 class; a Native class is `Native` and what it renders.
5. C# files are PascalCase after their type; TypeScript files are kebab-case, a component's after its class.
6. A shipped project is `Alis.Reactive[.<Package>]` with PackageId `AlisReactive[.<Package>]`; a test project is
   `Alis.Reactive.<Subject>.<Kind>`; the net48 twin adds `.Net48`.
7. A test class is `When<Situation>`, a method the test as a sentence; it sits in the folder of the module whose test
   it is, under the app.json screen it runs on.
8. A browser asset is `alis-reactive[-<package>][-<part>].js`, linked at `_content/<PackageId>/js/` on net10 and
   `Content/<PackageId>/js/` on net48.
9. No folder is named Shared, Common, Utils, Helpers, Support or Core, except MVC's `Shared/` for layouts and the
   spec's `Core/`.

### 2.5 The test harness

- **The world.** Each test makes a key and sets it as the cookie `world` on its own browser context before it opens a
  page; every request, stream and hub negotiation carries it. The app's world middleware, in Development only, scopes
  data, gates, hub groups, stream filters and caches to the key; a document request with none gets a new world, so a
  person can run the app. Teardown ends the world and fails any gate still parked. The World area serves two pages of
  its own for the harness's tests, app.json's `harnessTests`, which use no framework door: a records page listing the
  world's residents as text the server writes, and a word page whose link opens, in the page's frame, an endpoint that
  answers with the word the world's seed holds.
- **The verbs** are app.json's: seed, gate, push, drop, refuse, reopen, restart, clock, gc. A placement lists its
  verbs and its held steps, each held step with the TLA+ action it holds, and check_app refuses a verb with no step.
  Gates are generated from the `Next` of each spec a held step names into the world contract (`Gates.g.cs`, one enum
  per spec), so the app and the tests name a gate the same way without either seeing the other: the test arms a gate
  before its gesture, waits until the harness reports it held, asserts the held state, then releases, by its own
  request, the held gate the wait returns, so a release before the gate is reached does not compile. The harness's release
  and push carry the world contract's `[Gesture]` mark, which TEST013 reads (section 2.10). A gate parks its
  request before the endpoint runs, so a held answer is computed at its release, from the world as it is then. Restart
  is the test app's host script on the harness's word. Clock is `Page.Clock`, installed only by a test that names it.
  Gc is `Page.RequestGCAsync`, confirmed by the page's own weak hold.
- **Bindings.** `Promises.g.cs` is generated from app.json and the features: a test carries `[FailsFor(...)]` naming its
  feature, defect and index, its cell, or its harness test. `spec/tools/check.py --generate`, which U2 adds, writes
  `Gates.g.cs` and `Promises.g.cs` into the world contract, where they are committed, since check.py is the one reader
  of the spec's JSON and of a TLA+ spec's `Next`. `Gates.g.cs` holds one enum for each spec a held step names,
  `<Spec>Gate`, its members the actions of that spec's `Next` in their order. `Promises.g.cs` holds the class
  `Promises`, and in it one class for each feature (`M4_F1`) and the classes `Cells`, `OwnCells` and `Harness`; each
  test of the spec is a text constant there, named for it with `_` in place of every character a name cannot hold
  (`X6_3_0`), its value the test's id in the spec ("M4.F1 X6.3 0"), and two tests that would take one constant stop
  the generation. The world contract declares four marks: `FailsForAttribute`, which takes one of those constants;
  `GestureAttribute`; `BesideThePageAttribute`, on what the harness collected beside the page (TEST006); and
  `HoldsAttribute`, which `check.py --generate` writes on a constant once for each gate its test holds (TEST012). The bindings are held on the rung that
  reads each. check.py's `bindings` check fails when a generated file differs from what the spec writes, so a hand's
  edit, and a spec that moved without its constants, are both refused. TEST012 fails the build of a test that names
  no test of the spec, of two tests on one constant, of a test that never arms a gate its held steps name and, once
  every test is written, of a project that leaves a test of the spec unwritten (2.10). The harness releases a gate
  only through the held gate its wait returns, and its teardown fails a gate still parked. The check reads no
  view. A clause that held a view's module namespaces to its test's feature and parts stood here and is left out
  (the lead, 2026-11-12, `spec/CLAUDE.md`): every view that opens a plan names the two namespaces that hold every
  module's doors (2.2's P2 and P3), and app.json names no view for a test. A test landed before a door its page
  calls fails its unit's run, and its placement then lists the part that needs it.
- **Hosts.** A fixture takes its hosts from its placement (`TestFixtureSource`), each host an NUnit category. Each net10
  host is one server process, started on first use with `--urls http://127.0.0.1:0`, its address read from the
  "Now listening on:" line; it exits when its test host dies, teardown kills its process tree, and a run that leaves a
  server alive fails. The net10 clock is America/New_York, the twin's UTC; the browser context is en-US in
  America/Los_Angeles, so a date read in the wrong zone shows. The hosts consumerNet10 and consumerNet48 are the fresh
  apps' own servers, which hold no World area: a test under one runs on app.json's `fresh` screen, sets no world and
  uses no harness verb.
- **A run proves it ran.** Each run ends with
  `[playwright:result] <selection> <i>/<k>: N selected, N ran, N passed, 0 failed, 0 over budget, slowest <s>s <test>`,
  its selection the selector's own words (`U8`, `through U8`, `all`, a pool's name), its shard `1/1` when whole, and
  fails on no TRX, zero selected, ran other than selected, a failure, a test over budget, or a server left alive,
  each said on a line before it. What a run runs is decided before it starts. The runner joins the tests the spec
  lands (`spec/tools/landings.py`, which reads the spec through check.py: each test's promise, its unit, its screen
  and its hosts) with the tests the built assembly holds (`scripts/list-tests.cs`, each test's promise read from its
  `[FailsFor]`), refuses a selected test of the spec that nobody wrote, and writes the cases the run must run, each a
  fixture under one host with its test. The result is read against that list by name, so a test that ran twice, a
  test that was skipped and a fixture that runs under a host its placement does not list each fail the run.
  WhenAPlaywrightRunStarts holds the selection and WhenAPlaywrightRunEnds the reading, each over files its cases
  write: a refused case for each failure, and an accepted case for a green run. No retries, anywhere.
- **Waits and budget.** Action waits, navigation waits and the per-test budget are set from measurement, never chosen,
  on the machine the delivery loop runs on, this Mac (the owner, 2026-11-11): unit U2 runs the suite 20 times there
  and sets the budget at four times the slowest test it measured, the waits at four times the slowest step, and
  writes both into the harness with the run that measured them: `scripts/playwright.sh --measure` writes the budget
  and the two waits to `Harness/measured.json`, each with what set it, the slowest test and the slowest step of its
  kind, and the runs, the day and the machine. A step is one call of Playwright that a wait bounds: a navigation
  (`goto`, `reload`, `goBack`, `goForward`) under the navigation wait, and every other call on a frame, a gesture's
  and an expectation's, under the action wait. In a measured run the harness records each browser context with
  Playwright's own trace, which holds every call's start and end, and writes the calls beside that run's TRX; the
  reader takes the slowest step of each kind over the 20 runs. Outside a measured run the harness sets the action
  wait on every context and on every expectation and the navigation wait on every context; a measured run sets
  neither, so a suite that has grown slower is measured under Playwright's own waits. A test over the budget fails
  its run, and the budget and the waits move only by a new measurement of 20 green runs, written with that run: the
  result reader refuses a budget that is not four times its slowest test, a wait that is not four times its slowest
  step, and a measurement of fewer than 20 runs. CI measures its own numbers the same way when the rewrite is done;
  no number measured there gates a unit.
- **Syncfusion.** Only the Fusion pool's job receives the licence key; the app registers it at startup when its host
  serves a Fusion screen, and a Fusion screen refuses to render without it. A core test also fails at run time if its
  page requests the EJ2 script: the harness reads every request of a test's pages, a world's and a person's, and at
  the test's teardown fails a test whose page asked for `ej2.min.js`, naming the address. Through U24 that is every
  test; U25 lands, with the first test of a Fusion slice, the one way such a fixture takes the script.

### 2.6 TLA+

Each spec lives in its owning module's folder, `spec/<Module>/<Spec>.tla` beside its `.cfg`, `.violates.*.cfg` and
`.holds.*.cfg` files; the owner is the module whose unit changes it. A feature in any module names the invariants it
rests on in its `checker` field, and a test names the action it observes.

| Spec | Owner | Models |
|---|---|---|
| `Schema.tla` | Schema | the closed node set and its handlers |
| `DateWire.tla` | PathsAndWire | a date's text on each host |
| `Absence.tla` | ValuesAndSources | what a missing member, holder or value reads |
| `Ownership.tla` | OwnershipAndState | page, plan and slot holds, listens, destroys and collection |
| `Run.tla` | Execution | a handler run's awaits, settles and stops |
| `Lanes.tla` | Execution | the async lanes and their policies |
| `Tracing.tla` | Tracing | spans, attempts and propagation |
| `CallbackProperty.tla` | Vendors | the callback property's listener chain |
| `Validation.tla`, `ValidationToday.tla` | Validation | the rewrite's checks, and the redesign's for contrast |
| `Request.tla` | RequestsAndResponses | a request's loading, guard, routes and finally |
| `Composition.tla` | RequestsAndResponses | chained and parallel requests and their counts |
| `Realtime.tla` | Realtime | connections, drops, give-ups and the reconnect offer |
| `Partials.tla` | PartialsAndSlots | slot loads and unloads |

A feature a month later starts here: its module's `module.md` and `features.json`, the specs its owner holds, the
actions its tests will observe. A change to an async lane is a spec change first, TLC green with each mutant's
`.violates` config failing on its invariant, then code.

### 2.7 Tools and pins

| Tool | Pin | One job |
|---|---|---|
| .NET SDK | 11.0.100 (`global.json`, rollForward disable) | compile, test, pack, for net48 and net10.0; the net10 hosts run on the 10.0 runtime, 10.0.15, which CI installs with SDK 10.0.404, that line's latest, beside the pinned one |
| NuGet | the SDK's, 11.0.100's; `Directory.Packages.props`, lock files restored locked in CI, `nuget.config` with nuget.org alone | restore; a finding of its audit fails the restore, and so does a second source on a machine, which central package management refuses (NU1507) |
| net48 references | System.Text.Json 10.0.15, Microsoft.Extensions.DependencyInjection.Abstractions 10.0.15, System.Diagnostics.DiagnosticSource 10.0.15, Microsoft.AspNet.Mvc 5.3.0 | the core's net48 dependencies (2.9), restored together at these exact versions; net10 takes the first three from the framework reference; held on the 10.0 line, the net10 host's, so both hosts run one line of System.Text.Json (each one's latest is 11.0.0) |
| net48 build | PolySharp 1.17.0, Microsoft.NETFramework.ReferenceAssemblies 1.0.3, both private | the attributes C# 14's features need on net48, written as source, and the reference assemblies a build on any OS compiles against |
| MSBuild SDK | Microsoft.Build.NoTargets 3.7.134 | the Assets project |
| Node | 26.13.0 (`.node-version`) | esbuild, tsc, eslint, node probes |
| TypeScript | 6.0.3 | `tsc -b`, no emit, every strictness option on: strict, exactOptionalPropertyTypes, noUncheckedIndexedAccess, noImplicitOverride, noPropertyAccessFromIndexSignature, noImplicitReturns, noFallthroughCasesInSwitch, noUnusedLocals, noUnusedParameters, noUncheckedSideEffectImports, verbatimModuleSyntax, isolatedModules and erasableSyntaxOnly, which refuses an enum (S8), with allowUnreachableCode and allowUnusedLabels off; held by typescript-eslint 8.72.2, whose peer range ends below 6.1.0 (the latest is 7.0.2) |
| esbuild | 0.28.2 | one root `build.mjs`, one bundle per package; `package.json` denies its install script, since its platform package holds the binary |
| ESLint | eslint 10.14.0, @eslint/js 10.0.1, typescript-eslint 8.72.2, with minimatch 11.0.7 by an override of the 10.2.6 they bring, which npm's audit reports (GHSA-g3hv-75wg-m249) | `strictTypeChecked` and `stylisticTypeChecked` on the runtime and every package's TypeScript, each of their rules an error, `--max-warnings 0`, no rule turned off or down and no inline configuration (`noInlineConfig`); the rules beyond the presets are listed below the table |
| Vitest | vitest 5.1.0, with vite 8.3.8, the peer it requires; node environment, no jsdom | a runtime module's pure tests of what its index exports (1.2): the files that end in .test.ts under the runtime, which the root's `vitest.config.js` names |
| @microsoft/signalr | 10.0.11 | the hub's own asset; held on the 10.0 line, the line of the hub it talks to (the latest is 11.0.0) |
| Probe packages | jsdom 31.0.0, parse5 8.1.1, ws 8.22.2 | what the node probes load beside the pins they read: a DOM for EJ2's code, HTML's parser and a WebSocket server; no test loads them |
| OpenTelemetry | @opentelemetry/api 1.10.0, @opentelemetry/sdk-trace 3.1.1, @opentelemetry/exporter-trace-otlp-http 0.301.1 | the adapter's bundle (N20.2); sdk-trace replaces sdk-trace-web, which its registry entry retires in its favor; the three move together, each peering on the API's 1.10 |
| Roslyn | Microsoft.CodeAnalysis.CSharp 4.14.0 for the analyzer projects; 5.9.0 for the projects that host the compiler | 4.14.0 is the floor, the lowest compiler the packed analyzers load in: a compiler below an analyzer's Roslyn refuses to load it (CS9057), and 4.14.0 is Visual Studio 2022's last, so an MVC 5 app built there runs every rule; the analyzer cases, the test-rule cases and the architecture tests host 5.9.0, the latest, which parses C# 14 (`LanguageVersion.CSharp14`) |
| Analyzer authoring | Microsoft.CodeAnalysis.Analyzers 5.9.0, private | the compiler's own rules for an analyzer project, release tracking among them (`.claude/rules/analyzers.md`) |
| Analyzer cases | Microsoft.CodeAnalysis.CSharp.Analyzer.Testing 1.1.4, with Microsoft.CodeAnalysis.CSharp.Workspaces 5.9.0 and System.Security.Cryptography.Pkcs 10.0.15 | every analyzer's, suppressor's and test rule's cases; the library takes the workspaces package at any version, and without its pin the restore brings 1.0.1 and fails (NU1701); the Pkcs pin replaces the 9.0.6 that the library's NuGet.Packaging 7.0.3 brings, which NuGet's audit refuses (NU1903) |
| PublicApi and BannedApi analyzers | Microsoft.CodeAnalysis.PublicApiAnalyzers 5.6.0, Microsoft.CodeAnalysis.BannedApiAnalyzers 5.6.0 | the surface text; no run-time reflection |
| NUnit, adapter, test SDK | NUnit 5.0.0, NUnit3TestAdapter 6.3.1, Microsoft.NET.Test.Sdk 18.11.1 | tests |
| Plan schema | JsonSchema.Net 9.6.0 | the module tests and the architecture tests read every plan against `plan.schema.json` |
| App sweep | Microsoft.AspNetCore.Mvc.Testing 10.0.15 | the architecture tests' sweep of the conformance app's pages; held by net10.0, which its 11.0.0 does not support (NU1202) |
| Playwright | Microsoft.Playwright 1.64.1, CI's image `mcr.microsoft.com/playwright/dotnet:v1.64.1-noble` by digest | every browser test and probe |
| Java, TLC | OpenJDK 27.0.1, tla2tools v1.7.4 (TLC 2.19) by SHA-256 in `spec/tools/tla2tools.lock` | model checking; v1.8.0 is a prerelease |
| Python | 3.15.2, standard library | `spec/tools/check.py`, and `spec/tools/landings.py`, which prints where the spec lands each test |
| Syncfusion | 33.1.47: Syncfusion.EJ2.AspNet.Core on net10, Syncfusion.EJ2.MVC5 on net48, @syncfusion/ej2, together | `node scripts/pin-syncfusion.mjs <version>` moves both; the owner's hold (the latest is 35.1.43) |
| Actions | by SHA with Dependabot's version comment | CI |

A pin is the latest stable version of its package or tool on the day its unit lands it (the owner, 2026-11-11), looked
up at its own registry and proven by a restore or an install and a run. A pin below the latest names what holds it in
its row, and the holds are this table's and no other: Syncfusion's, the owner's; the analyzers' Roslyn floor; the 10.0
line for what runs in a host (the net48 references, the Pkcs pin, the hub's client); TypeScript, by typescript-eslint's
peer range; Microsoft.AspNetCore.Mvc.Testing, by net10.0. Each hold is the owner's to lift. `.github/dependabot.yml`
proposes every newer stable version each week, majors among them, of every NuGet and npm package, every action, the SDK
and the Playwright image, and ignores this table's holds and nothing else; `scripts/doctor.sh --latest`, which the
nightly workflow runs, compares Node, Python, Java and TLC with their registries, which Dependabot does not read. A
finding of NuGet's audit or of `npm audit` fails `scripts/check.sh`, and a package it names moves to its patched
version by a pin or an override in this table.

The lint beyond its presets is closed. Each rule enforces one line of the spec, which names it, and a rule joins the
list only with its line, in the unit that brings that line's code:
- `@typescript-eslint/switch-exhaustiveness-check` refuses a switch over a union that has no case for one of its
  members, a `default` beside it or not (S4).
- `@typescript-eslint/strict-boolean-expressions`, with every one of its allowances off, refuses a condition and an
  operand of `&&`, `||` or `!` that is no boolean: a text, a number or a nullable value never stands for true or false,
  and a missing value is compared with `null` or `undefined` by name (S2).
- `eqeqeq` refuses `==` and `!=`, a second conversion beside the one (BRIEF.md, Principles 7).
- `no-restricted-syntax` refuses a call of `querySelector`, `querySelectorAll`, `getElementsByClassName`,
  `getElementsByTagName`, `getElementsByName`, `closest` or `matches`: an element is found by its id (BRIEF.md,
  Principles 8; M5.N1). It is lifted in one file, the one of `runtime/Execution/` that finds a root's plans.
- `no-restricted-imports`, which `eslint.config.js` writes from each `SliceTree.txt` of a TypeScript tree, the
  runtime's today, refuses in a slice's file every relative import but one of a file of its own folder or of a
  slice on a row before its own: a later row, a slice of its own row and a folder named in place of its file are
  refused, and so is every relative import in a file whose folder has no row (section 2.3, M20.D3). It reads a
  table's folder entries and its dot; an entry with a star is read from U23 on, with the first TypeScript tree that
  writes one, and before that a file under one stands as a file with no row.
- `no-restricted-imports`, over a module's pure tests, the files that end in .test.ts, refuses an import of the
  runtime other than the test's own module's index (`./index`), so a pure test reads a module's public surface and
  never its internals (1.2).

The presets already hold `no-floating-promises` (D4.4), `no-unnecessary-condition` and `no-non-null-assertion` (S2):
since no field of the plan is optional or nullable, a default for plan data is a condition the types call unnecessary.
Principles 8's other lines (what a listener received read by its constructor, a catch that goes on) are about what a
value means, which no selector states: the POC's runtime, the proven design, holds `instanceof` 12 times, `try` 8 and
`??` 8 at feb301c88, so a blanket selector would refuse the design itself. WhenTheLintIsConfigured, an architecture
test, proves each rule of the list as section 2.10 proves every rule: it lints a refused file and an accepted file
for the rule with the repository's own configuration and expects the rule's id at the refused file's line and no
message for the accepted one, so a selector that matches nothing fails its refused case; its last case holds that an
inline disable changes no result. Its files are kept in its `LintCases` folder, which the configuration covers
beside the runtime; a file that holds a comment, the inline disable's, is written by its case and removed. It holds
the three rules the presets carry for a spec line the same way, and the compiler's refusal of an enum (S8) by a file
its case writes. U1 lands the presets, the ban on inline configuration and the three rules that read no path (the
switch, the boolean conditions, the equality); the wide query's refusal and the import rows read a path of the
runtime, and landed in U3: the import rows with the runtime's first row, and the wide query's refusal with the file
that finds a root's plans, `runtime/Execution/plan-scripts.ts`, the one file it is lifted in. The pure tests' import rule
is landed with its refused and accepted files, before U3 writes the first of them.

Scripts: `scripts/check.sh` (the contract regenerated and diffed, from U3 on; `npm audit`; the Syncfusion pin's
agreement in NuGet, npm and the lock; `tsc -b`; eslint; vitest over the runtime's pure tests; the browser bundle, `node build.mjs`; a locked `dotnet restore`; `dotnet build -c Release`;
`dotnet format --verify-no-changes` over the solution; the architecture tests, the analyzer cases, the test
rules' cases and the module tests; `check.py --ci`, which
its `--no-spec` leaves to CI's own job; its last line is "[check] every check passed."); `scripts/playwright.sh` (the bundle and the build, then run, with the selectors
below, the result line and the leftover-server check), with `scripts/list-tests.cs`, `scripts/playwright-select.mjs`
and `scripts/playwright-result.mjs`, which list the built tests, select a run's cases and read its result;
`scripts/test.sh` (both, its browser leg every test or, with `--through`, the tests landed through a unit); `scripts/build.sh`,
`scripts/run.sh`, `scripts/pack.sh`, `scripts/doctor.sh` (every pin against what the machine runs: the SDK, Node,
Python, Java from `JAVA_HOME`, the TLC jar by its SHA-256 and `node_modules` by the lock, exit 1 on a difference),
`scripts/plant-defect.sh`, which plants one defect and reads the runner's result line for its one test,
`scripts/verify-release-tag.sh`, `scripts/pin-syncfusion.mjs`, `scripts/install-git-hooks.sh`.
MSBuild never runs npm or node: `build.mjs` writes each bundle, the core's to `Alis.Reactive/wwwroot/js/alis-reactive.js`,
which git ignores and the core's static web assets serve at `_content/AlisReactive/js/alis-reactive.js`, and each
script that builds runs it first (`scripts/check.sh`, `scripts/playwright.sh`, `scripts/build.sh`, `scripts/run.sh`).
An app whose bundle is missing refuses to start.

`check.py --ci`, which U1 adds, runs every check but survival, a spec session's own hook and branch, and probes,
which run nightly and on a changed probe or pin, and the plants of those two. It runs a TLC config only when the
cache file the `TLC_CACHE` variable names holds no result for its hash, and writes every result back to that file;
with no cache file every config runs, the nightly run's way. It reads java from `JAVA_HOME` and the jar from
`TLA2TOOLS_JAR`, as every mode does where they are set, and refuses to run with either unset; it refuses a jar whose
SHA-256 is not the lock's, writes no `spec/STATE.json`, and exits 1 on any failure. The checks that read code keep reading the SHAs their cites
pin, except kinds, which from U3 on reads the rewrite's grammar at HEAD: a kind `spec/Core/schema.md` does not list
fails, and from U27 on so does a kind it lists that HEAD lacks.

`scripts/playwright.sh` takes one selector a run: a unit (`--unit U8`, the tests check.py lands in it), every unit
through one (`--through U8`), a feature (`--feature M6.F1`), a screen (`--screen residents`), one test by its promise
constant (`--test M3_F1.X8_27_2`), a folder or a fixture of the project by its name (`RequestsAndResponses`), or
everything the project holds (`--all`). `--host snakeCase` narrows any of them to one host; without it a run takes
the hosts the conformance app serves on the machine, never the twin's or a fresh app's. A pool's shard (`--pool core
--shard 1/2`) is section 2.8's, taken from the tests the project holds as `--all` is, and a workflow is its one caller. `--measure` writes the budget and the waits (2.5),
and `--measure-parallelism` the count of tests a run takes at once (2.8).

### 2.8 Shards and CI

- **The shard unit** is a module folder; the net10 host-variant fixtures form one unit, so each variant's server starts
  once per run; FusionComponents splits by slice. Pools: core, hosts and fusion; a test under a fresh app's host runs in its delivery job, never in a pool. The fusion
  pool holds the tests of the three Fusion module folders under every host a pool serves, the hosts pool every other
  test under a host variant, and the core pool every other test under net10. Shards are balanced longest-first
  from the TRX-measured weights in `shard-weights.json`, so a stale weight unbalances and never drops a test:
  `scripts/playwright.sh --measure` writes there, for each unit of its selection, the seconds its tests took in one
  run, and a unit with no weight is placed last, on the lightest shard. A shard that takes no unit is refused. Unit U2
  measures, on this Mac, one test at a time against 2, 4 and 8 per process, 20 runs each, and keeps parallelism only
  when every run is green and the wall time falls by at least 1.6 times: `scripts/playwright.sh
  --measure-parallelism` runs them, each a measured run under no measured wait, reads a run's wall time from its
  TRX, and writes to `Harness/measured.json` the count it keeps with the wall time of each count: the quickest of
  2, 4 and 8 whose 20 runs are green and took, together, no more than the 20 runs at one divided by 1.6, else one.
  Fixtures run beside each other and a fixture's tests one after another. Every run takes the kept count, and the
  budget and the waits are measured at it: the result reader refuses a count a hand chose and a budget measured at
  another count.
- **`.github/workflows/ci.yml`** on every pull request and push to main, and called by the nightly and the publish
  workflows: `pack`, once, at the version its caller names or one of the run's own, its expected ids 2.9's six, whose
  files verify-net48 and delivery-net10 restore; `checks` (`scripts/check.sh` without check.py); `spec`
  (`check.py --ci`, TLC only for a spec whose hash CI has not checked); `playwright` per pool and shard in the
  Playwright image; `verify-net48` on Windows (pack, the twin and the fresh MVC 5 app built against the packs with
  warnings as errors, WhenAConsumerAppIsBuilt over the fresh app, the tests under net48 and consumerNet48 under IIS
  Express); `delivery-net10` (pack, the fresh net10 app restored from the packs, WhenAConsumerAppIsBuilt over it, the
  tests under consumerNet10); `proof`, the one required status: every shard has a TRX with tests, none failed, none
  over budget, and the executed names equal the full list, each once. Each run uploads its folder, its selection
  and its TRX; `proof` reads them with `scripts/playwright-result.mjs --proof` against the full list, which
  `scripts/playwright-select.mjs --all --every-host` writes: every case the spec lands that a test carries, the
  twin's and a fresh app's among them.
- **`.github/workflows/nightly.yml`**: everything, every TLC config uncached, every probe, `scripts/doctor.sh --latest`; an issue on failure.
- **`.github/workflows/syncfusion-upgrade.yml`**, on a change to the pins: `pin-syncfusion.mjs --check`, the EJ2 browser probes, then the
  fusion pool.
- **`.github/workflows/nuget-publish.yml`**, on a `v*` tag: `verify-release-tag.sh`; ci on the tag; one pack job, its expected ids 2.9's six; verify-net48 and
  delivery-net10 restore those exact packages; publish pushes the same files by trusted publishing, in the
  `nuget-release` environment: nuget.org's policy names this repository, this workflow's file and that environment, so
  neither is renamed.

### 2.9 Packages and delivery

| Package | lib | analyzers | build | browser files | dependencies |
|---|---|---|---|---|---|
| AlisReactive | net48, net10.0 | the ALIS analyzers and the validator generator, packed from the analyzer project's own output path | `build/net48` copies the browser files; the vendor contract target, opt-in | `alis-reactive.js`; `alis-reactive-hub.js` on net10 | net48: System.Text.Json 10.0.15, Microsoft.Extensions.DependencyInjection.Abstractions 10.0.15, System.Diagnostics.DiagnosticSource 10.0.15 (D20.8), Microsoft.AspNet.Mvc 5.3.0; net10: the framework reference |
| AlisReactive.Native | net48, net10.0 | none | `build/net48` copy | `alis-reactive-native.js` | AlisReactive at the same version, exactly |
| AlisReactive.Fusion | net48, net10.0 | none | `build/net48` copy | `alis-reactive-fusion.js`, `alis-reactive-fusion-templates.js` | AlisReactive at the same version, exactly; Syncfusion.EJ2.AspNet.Core [33.1.47] on net10, Syncfusion.EJ2.MVC5 [33.1.47] on net48 |
| AlisReactive.OpenTelemetry | net48, net10.0 | none | `build/net48` copy | `alis-reactive-opentelemetry.js`, its bundle holding 2.7's OpenTelemetry pins | AlisReactive at the same version, exactly |
| AlisReactive.DesignSystem | `lib/net48/_._`, `lib/net10.0/_._` | none | none | none | none |
| AlisReactive.NativeTagHelpers | `lib/net10.0/_._` | none | none | none | none |

- Each package carries its own bundle; the core's bundle holds no component code. No CSS ships: an app names its own
  field classes (M7.N4), the Native components carry no stylesheet (NativeAppLevelObjects/module.md), and Fusion
  carries none (FusionComponents/module.md): an app links EJ2's theme and styles the dialog form FusionGrid writes and
  its own invalid class.
- Every link carries the SHA-256 of the bytes served: `asp-append-version` on net10, `Url.AppendVersion` on net48 over
  the files the package's build target copies into `Content\<PackageId>\` beside a `.gitignore` of `*`.
- Script order in a layout, on both hosts: `ej2.min.js` (Fusion only), the Fusion templates script, each package's
  script, then `alis-reactive.js` last, every one a classic script without `async`.
- A fresh net10 app: `dotnet add package`, the startup calls, a `@using` per module namespace a view uses, the links.
  A fresh MVC 5 app: PackageReference (packages.config retires, section 3), the startup calls and the resolver in
  `Application_Start`, the namespaces in `Views/Web.config`, the links.
- The delivery tests run on both fresh apps: app.json's placements on its `fresh` screen under the hosts consumerNet10
  and consumerNet48, apps that README.md's steps alone wire on the packed packages, each booting its plan (M19.X5); and
  WhenAConsumerAppIsBuilt, whose cases hold that git lists no file a build wrote into either app (M19.D2), that a file an
  ALIS analyzer refuses fails a fresh app's build with that analyzer's id, so the analyzers packed inside AlisReactive
  reach an app on both hosts, and that the fresh net10 app's static web assets serve each package's browser file under
  `_content/<PackageId>/js/`, which only a fresh app reads, since the conformance app takes the shipped projects. Each other
  delivery behavior is proven once where its defect shows: a link's version is the content hash of the bytes served on
  the twin (M19.X1), which runs on the packs, a validator refuses in the browser and the server saves a valid post on
  the conformance app and the twin (M8.F1's X16.10), and each time zone shows its own day there (M15.F1's X3.7 and
  X3.9, M18.F1's M18.X1); on net10 the version is ASP.NET Core's own `asp-append-version` (M19.N1).
- Native, Fusion and OpenTelemetry depend on AlisReactive at the exact version: pack writes `[x.y.z]`, and U22's
  delivery job reads it back from each nuspec.

### 2.10 The test rules

A test written after its code, by an author who read that code, drifts toward what the code does. On the redesign at
65fdf118d, 51 Playwright test files find their inputs with the framework's own `IdGenerator.For` (14 on main at
7b9e987cc), so the id the generator writes is the id the test looks for and a wrong one passes; rule 7 of
`.claude/rules/tests.md` forbade it in prose. In the rewrite every rule and smell of that file is held by a compiler
rule or a named check:
- **The reference cut.** A Playwright project compiles against the target framework, Playwright, NUnit and its test
  platform, and `Alis.Reactive.Conformance.World`, and nothing else (TEST001). A test finds an element by its role,
  label or text, or by an id written as a literal; the InputsAndIds tests assert each derived id as a literal, so a
  wrong id fails them.
- **Test first.** A unit's tests are written from their promises in `features.json` and their screens in `app.json`
  before the doors they need, and run red; the screen and the doors make them pass; each then fails on its planted
  defect. The reference cut is what lets a test compile before its door exists.
- **The rules project.** `tests/Alis.Reactive.TestRules` holds the rules as Roslyn analyzers on every Playwright
  project, apart from the framework's analyzers, which ship inside AlisReactive;
  `tests/Alis.Reactive.TestRules.AnalyzerTests` holds each rule's refused case and accepted case. Unit U2 lands both
  before the first feature test, and check.py's `test_rules` binds each line of the second table to
  `.claude/rules/tests.md`.
- **A rule's own test.** An analyzer, a test rule, an architecture test and a check.py check are each a rule over the
  inputs it is given, and each is proven the same way: a refused case, a planted input the rule must report by name,
  and an accepted case, a legitimate input beside it (for WhenCodeIsWritten a shebang, `#nullable` and a pinned
  action's version), which the rule must pass, so a rule that reports nothing and a rule that fires on a rewording
  both fail. An architecture test's `[Test]` runs its rule over the tracked repository and its cases run it over files
  the case writes; a scan of a clean repository alone passes whether the rule works or not, and at 2f9f9b1a2 that is
  all WhenCodeIsWritten, WhenARuleNamesTheCode, WhenAnAppRendersItsPages and SandboxStaticStateTests hold, while
  WhenClaudeRunsAHook already refuses an edit to the generated contract and allows one beside it. A case is a test
  marked `[RefusedCase]` or `[AcceptedCase]`, the project's two marks, each a test attribute, so the runner runs a
  case as it runs a rule; WhenAnArchitectureTestIsWritten reads the project's fixtures through Roslyn and refuses a
  fixture that holds a rule, a `[Test]`, with no case of each mark. The analyzers' and the test rules' case projects
  mark their cases the same way, and the same test holds every analyzer and suppressor of the two rules projects to
  a refused and an accepted case that runs it. U1 lands the cases
  of every architecture test it keeps, and WhenAnArchitectureTestIsWritten refuses a rule with no refused or no
  accepted case. check.py's checks keep theirs in `spec/plants/`, which its `plants` check runs in throwaway
  worktrees.
- **How an analyzer is written.** `.claude/rules/analyzers.md` holds it for every analyzer, suppressor and test rule,
  and loads when a file of an analyzer project or of its cases is opened: one id for one line of the spec, tracked in
  the project's release files, which the build compares with every descriptor; semantic reads only; cases on
  Microsoft.CodeAnalysis.Testing (2.7), a refused case for each message and an accepted case one fact away for each
  condition. The unit that lands a rule writes it to that file. The analyzers of main, of the redesign and of the POC
  are evidence of the rules and of the cases they met, never code the rewrite moves: all three switch release tracking
  off (`Alis.Reactive.Analyzers/Alis.Reactive.Analyzers.csproj:11` at 7b9e987cc and at 65fdf118d,
  `poc/Alis.Reactive.Poc.Analyzers/Alis.Reactive.Poc.Analyzers.csproj:8` at feb301c88), main's analyzers have no cases, and the
  redesign's and the POC's cases each run on a fixture of their own that builds every compilation by hand.

| Id | Refuses |
|---|---|
| TEST001 | a package, project or file reference of the project beyond the packages Microsoft.Playwright, NUnit, NUnit3TestAdapter and Microsoft.NET.Test.Sdk, the rules project, the framework's analyzers taken as an analyzer, which give a test no type, and the world contract: the framework, the app, a mocking or a snapshot library |
| TEST002 | a member of Microsoft.Playwright outside the members a test takes, listed below the tables: script run in the page (`EvaluateAsync`, `WaitForFunctionAsync`), a stubbed network (`RouteAsync`), a one-shot read of the page (`InnerTextAsync`, `IsVisibleAsync`), an event no gesture raised (`DispatchEventAsync`) and every member a later version adds; and a literal that names `ej2_instances` or `window.alis` |
| TEST003 | a wait on time: `WaitForTimeoutAsync`, `Task.Delay`, `Thread.Sleep`, `SpinWait`, and outside `Harness/` a `Timeout` a test sets on a Playwright call, since every wait is the harness's, set from measurement (2.5) |
| TEST004 | a skipped, repeated, ordered or parameterized test: an NUnit attribute outside `[Test]`, `[TestFixtureSource]` and `[Category]` (`[Ignore]`, `[Explicit]`, `[Retry]`, `[Repeat]`, `[Order]`, `[TestCase]` among them), and a member of `Assert`, `Assume` or `Warn` outside `Assert.That` (`Assert.Ignore`, `Assert.Inconclusive`, `Assert.Pass`, `Assume.That` among them), and every legacy assertion of NUnit's (`ClassicAssert`, `StringAssert`, `CollectionAssert`) |
| TEST005 | a branch or a loop in a test method: `if`, `switch` as a statement or an expression, `for`, `foreach`, `while`, `do`, `try`, `goto`, and a conditional operator, listed below the tables |
| TEST006 | an `Assert.That` over anything but what the browser reports beside the page: a request the page sent, a console message, an uncaught error, a download and a page a gesture opened; the page itself is asserted through `Expect` alone |
| TEST007 | an expected value, or a locator's selector, role name, label or text, that is neither a compile-time constant nor a generated promise constant |
| TEST008 | a partial match on the page: `ToContainTextAsync`, or a `Regex` in a page matcher; a regex reads only a request's header |
| TEST009 | an expected value equal to a constant the same test typed into the same locator |
| TEST010 | a test whose every assertion is an absence (`ToBeHiddenAsync`, a `Not` matcher, `ToHaveCountAsync(0)`, `ToBeEmptyAsync`, an empty text) |
| TEST011 | `Expect` or `Assert` outside a test method: a helper builds a page or performs a gesture, and never asserts |
| TEST012 | a test method without exactly one `[FailsFor]` taking a generated promise constant; a fixture not named `When` and its situation; a promise constant two tests take; a gate its promise holds that the test never arms; and, once every test is written, a promise constant no test takes |
| TEST013 | a test method that performs no gesture itself: a gesture member of the list below the tables, or a harness release or push |

| `.claude/rules/tests.md` | Held by |
|---|---|
| 1. One promise, named as the promise | TEST012, the bindings check |
| 2. The public surface only | TEST001 |
| 3. Real collaborators | TEST001, TEST002 |
| 4. An outcome someone observes | TEST002, TEST006 |
| 5. Fails for the defect it names | the planted-defect run, TEST010 |
| 6. Both sides of every promise | check_promises |
| 7. No tautology | TEST001, TEST007, TEST008, TEST009 |
| 8. Independent and deterministic | TEST003, TEST004, TEST005, the harness |
| 9. Readable as the specification | TEST005, TEST011, TEST013 |
| 10. Complete | the bindings check, check_app |
| A mock, stub, spy or fake, of framework code or of the boundary it talks to | TEST001, TEST002 |
| A page arranged through internals, internal constructors or reflection | TEST001 |
| jsdom, or any stand-in for the browser, offered as proof | TEST001 |
| A golden, snapshot or plan-JSON assertion offered as proof of behavior | TEST001, TEST007 |
| Asserting the arrange: typing "Helen" and asserting the input says "Helen" | TEST009 |
| An expected value computed by framework code | TEST001, TEST007 |
| An assertion the defect also satisfies, such as an empty element with no positive control | TEST010, the planted-defect run |
| Only the happy path | check_promises |
| One test proving several promises, or two tests proving one | TEST012, the bindings check |
| A helper that asserts, or hides the gesture under test | TEST011, TEST013 |
| A sleep, a retry, or an order between tests | TEST003, TEST004 |
| A name that describes the code instead of the promise | the bindings check |
| A component library on a framework test's page | check_app, the harness |

check_promises gives every promise its opposite case as a promise of its own; the bindings check is section 2.5's; the
planted-defect run is `scripts/plant-defect.sh`, its red line in the unit's commit; the harness gives each test its
own world and fails a core test whose page requests the EJ2 script. A test's name is the author's sentence: the
bindings check makes its promise, not its name, the specification it is held to.

The members a test takes of Microsoft.Playwright 1.64.1 are closed, and TEST002 refuses every other one, so a member a
later version adds is refused until this list takes it. A member TEST003 or TEST008 names is reported by that rule
alone.
- **Gestures**, the ones TEST013 counts: a page's `GotoAsync`, `ReloadAsync`, `GoBackAsync` and `GoForwardAsync`; a
  locator's `ClickAsync`, `DblClickAsync`, `FillAsync`, `ClearAsync`, `PressAsync`, `PressSequentiallyAsync`,
  `SelectOptionAsync`, `CheckAsync`, `UncheckAsync`, `SetCheckedAsync`, `DragToAsync`, `SetInputFilesAsync` and
  `HoverAsync`; `Keyboard.PressAsync` and `Mouse.MoveAsync`.
- **Finding**: `GetByRole`, `GetByLabel`, `GetByText` and `Locator`, its selector one id (`#` and the id), of a page, a
  locator and a frame locator; a locator's `First`, `Last`, `Nth`, `Filter`, `And` and `Or`; `FrameLocator` and `ContentFrame`.
- **Expecting**, through `Assertions.Expect` alone: a locator's `ToHaveTextAsync`, `ToHaveValueAsync`,
  `ToHaveValuesAsync`, `ToBeCheckedAsync`, `ToBeVisibleAsync`, `ToBeHiddenAsync`, `ToBeFocusedAsync`,
  `ToHaveClassAsync`, `ToHaveAttributeAsync`, `ToHaveIdAsync`, `ToHaveCountAsync`, `ToBeEmptyAsync` and `Not`; a
  page's `ToHaveURLAsync` and `Not`.
- **Beside the page**, what TEST006 lets `Assert.That` read: a page's `RunAndWaitForRequestAsync`,
  `RunAndWaitForDownloadAsync` and `RunAndWaitForPopupAsync`, their gesture written in the test; a request's `Url`,
  `Method`, `PostData`, `Headers` and `HeaderValueAsync`; a console message's `Type` and `Text`; a download's
  `SuggestedFilename`; and what the harness collected of the page's requests, console and uncaught errors.

The project's `Harness/` takes besides: `Playwright.CreateAsync`, `Chromium`, `LaunchAsync`, `NewContextAsync` with its
locale and time zone, `NewPageAsync`, `AddCookiesAsync`, `CloseAsync`, `SetDefaultTimeout`,
`SetDefaultNavigationTimeout`, `Assertions.SetDefaultExpectTimeout`, a context's `Tracing` with its `StartAsync` and
`StopAsync`, which a measured run records its steps with (2.5), the page's events `Request`, `Console`, `PageError`,
`Download`, `Popup` and `Close`,
`RequestGCAsync`, and the clock's `InstallAsync`, `PauseAtAsync`, `FastForwardAsync`, `RunForAsync` and `ResumeAsync`;
and of NUnit `[SetUp]`, `[TearDown]`, `[OneTimeSetUp]`, `[OneTimeTearDown]`, `[SetUpFixture]` and `[Parallelizable]`,
and an attribute that is a test action of NUnit's, by which the harness opens a run once for every namespace.
TEST005's conditional operators are C#'s: `?:`, `??`, `??=`, `?.`, `?[]`, `&&` and `||`.

How the rules read a project and a test. The test rules run where a project takes them: the Playwright project
imports the rules project's build file, `Alis.Reactive.TestRules.targets`, which adds the rules as an analyzer and hands
the compiler two properties, and no rule asks whether its compilation is a Playwright project. The part of a rule
that reads a type the compilation lacks, NUnit's, Playwright's or the world contract's, does not run, and the rest
of the rule does: with no world contract TEST013 still counts Playwright's gestures.
- **The project (TEST001).** The build file hands the compiler the project's own reference items as one property,
  each with its kind: a package, a framework, a project that gives the test its types, a project taken as an
  analyzer, a project named for its build alone (`ReferenceOutputAssembly="false"`), a file. An assembly that an
  allowed package brings is no reference of the project's. The closed list: the four packages of the row; the
  framework the target framework brings; the world contract for its types; the rules project and the framework's
  analyzers as analyzers; the conformance app for its build alone. Each other item is reported with no location, by
  its name, and so is a project that takes the rules and hands the compiler no list.
- **A test method** is a method marked `[Test]`. What a rule reads in a test is the method's body, a lambda or a
  local function of it included; a call of a helper is read as a call, never as what the helper does.
- **A file of the harness** is one under the project's `Harness` folder; the members the harness takes besides, and
  NUnit's set-up attributes, are accepted there alone.
- **A member of Playwright (TEST002)** is a method, a property or an event of one of its interfaces or static
  classes, called, named, read or subscribed anywhere in the project: a method named and never called, a method
  group, is read by TEST002 as a call is. An options object and an enum value are the data a listed member takes
  and are not read, but for the two below. `Page.Keyboard`, `Page.Mouse` and, in the harness,
  `Page.Clock` and a context's `Tracing` are read as the way to the members listed under them. `Locator` and `FrameLocator` take one id, `#`
  and the id's letters, digits, `_` and `-`, as a constant: any other constant selector is refused by TEST002, and
  one that is no constant by TEST007.
- **A wait (TEST003)** is a call of a listed member, wherever it is written, and outside the harness a `Timeout`
  assigned on a Playwright options object.
- **NUnit (TEST004).** An attribute is NUnit's when NUnit declares it, a class it derives from or an interface it
  implements, so an attribute a project builds on one of NUnit's is read as NUnit's and refused. Every such attribute
  applied in the project is read, and every member of `Assert`, `Assume` and `Warn` and of NUnit's legacy
  assertions, the assembly beside its framework's, that is called or named.
- **A branch (TEST005)** is read as the compiler reads it, by its operation, where the file writes it: a branch the
  compiler adds for a `using` or an `await` is none, and neither is `&`, `|`, a pattern's `or`, `break` or
  `continue`. It is reported at its keyword or operator.
- **What `Assert.That` reads (TEST006)** is its first argument: a listed member beside the page, or a member that
  carries `[BesideThePage]`, the world contract's mark for what the harness collected of the page's requests,
  console and uncaught errors; or a call or a member read whose receiver, an extension's first argument included,
  leads to one (`request.Headers`, a projection of the console's messages), through an `await` and a cast. A local, a
  parameter, an operator's result and a value computed from the page are refused: the fact is asserted where it is
  read.
- **A constant (TEST007)** is what the compiler folds to one, a generated promise constant among them, and an array
  or a collection expression of constants. The values read: each argument of a finding or an expecting member, each
  text, number, boolean and enum an options object passed to one is given, and each argument inside the constraint
  `Assert.That` takes second. An options object is written where it is passed, with no argument of its own: one
  built before the call is refused, since its values are set where the rule does not read. A list written as
  neither an array nor a collection expression is refused whole. The rule reads wherever such a member is called, a
  helper and the harness included.
- **A partial match (TEST008)** is `ToContainTextAsync`, and a `Regex` passed to a member of Playwright or assigned
  to one of its options, read by the parameter's or the option's type, in the harness too.
- **The arrange (TEST009).** A test types with `FillAsync` and `PressSequentiallyAsync`. An expected value is the
  arrange when it equals a constant the same test typed into the same locator, the same expression, by its text, or
  the same local, field or property. A matcher reached through `Not` is read, and no order between the typing and
  the expecting is asked.
- **An absence (TEST010)** is `ToBeHiddenAsync`, a matcher reached through `Not`, `ToHaveCountAsync` of zero,
  `ToBeEmptyAsync`, an expected text or value that is empty, and an `Assert.That` whose constraint is `Is.Empty` or
  `Is.Null`. A test whose assertions are all absences is refused at its name, and so is a test that asserts nothing.
  An assertion is an expecting member of the list or an `Assert.That`; an `Assert.That` with no constraint, and one
  with a compound constraint (`Is.Not.Null`), assert a presence.
- **Outside a test (TEST011)** is every member that carries no `[Test]`, a method, a constructor, a property and a
  field's initializer: a call of `Expect` or of a member of `Assert` there is refused, in the harness too.
- **The binding (TEST012).** The one `[FailsFor]` takes a constant declared in a class of `Promises`. A constant
  whose test holds a gate carries `[Holds]`, the world contract's mark, once for each gate, and the test's body names
  each of those gates. Two tests on one constant are reported at the compilation's end, at the later one's mark, and
  so is a constant no test takes, with no location, in a project that does not set the build property
  `TestsAreStillBeingWritten`; the Playwright project sets it until U3 writes its last test.
- **A gesture (TEST013)** counts where it is written inside the test method's body, as a member of `IPage`,
  `ILocator`, `IKeyboard` or `IMouse`; a call of a helper that performs one counts as none. The harness's release and
  push carry `[Gesture]`, a mark of the world contract, and TEST013 counts a call of a member that carries it.

A test rule's title and message are its author's, written to the rule file's second rule: this section gives what
each rule refuses, never its words.

The lists are read from the tests the spec holds and from the pinned packages. Of the gestures the promises
describe, none drags, taps or scrolls; one hovers (M25.F5's X17.9); a right click (M21.F14's M21.X405), a click with
Control (M25.F3's X17.35) and one with the middle button are options `ClickAsync` takes. Their outcomes read, beside
the page, the console (M4.F1's X6.58), a download (M21.F23's M21.X201) and a page a click opened (M25.F3's X17.34),
each one of rule 4's three kinds (what the user sees, what the server received, what the developer reads), and none
reads whether an element is enabled. The refused lists this section held before named neither
`WaitForFunctionAsync`, `EvalOnSelectorAsync`, `EvaluateAllAsync`, `DispatchEventAsync`, `SetContentAsync`,
`ToHaveJSPropertyAsync`, `ToMatchAriaSnapshotAsync` nor `IsDisabledAsync`, each a member of Playwright 1.64.1, and
TEST006 refused the console's errors that the spec's own tests read.

## 3. What retires

Each retirement waits for `features_lost` at 0 and the rule files that name the item rewritten; "nothing references it"
is never the reason.
- `Alis.Reactive.SandboxApp` with `tests/Alis.Reactive.PlaywrightTests`; `tests/Alis.Reactive.Framework.App` with its
  16 components and `tests/Alis.Reactive.Framework.PlaywrightTests`; `tests/Alis.Reactive.ClassNames.App` and
  `tests/Alis.Reactive.ClassNames.PlaywrightTests`, now the host `classNames`; `scripts/framework-coverage.*`,
  `check_ci_shard_matrix`.
- `poc/`, the POC's test projects (`tests/Alis.Reactive.Poc.*`) and `tests/Alis.Reactive.Playwright.TestHost` they build
  on, with the runner that runs them and what it reads, `poc/playwright.sh`, `poc/vstest-filter.mjs` and
  `poc/trx-failed-tests.mjs` (main's `scripts/vstest-filter.mjs` and `scripts/trx-failed-tests.mjs`), and each
  `playwright.project.sh`, last: each proof ported by its unit, TestHost's harness merged into the Playwright project's `Harness/`, the POC's
  analyzers rewritten in their modules' folders to section 2.10's authoring rules, their rules and cases the evidence,
  and its two architecture tests moved to the architecture tests,
  `poc/VendorContract.targets` and `poc/tools/vendor-contract.mjs` to `Alis.Reactive.Analyzers/Vendors/`,
  `spec/api/HttpSurface` retargeted to the core, and every cite of `poc/` pinned to a SHA.
- The sources of `Alis.Reactive.DesignSystem` and `Alis.Reactive.NativeTagHelpers`, their workspaces and asset lines, since
  their ids ship from `build/EmptyPackage.proj`; `examples/net48-mvc-smoke`, which becomes
  `tests/Alis.Reactive.Consumer.Net48`; the build targets `build/AlisReactiveBrowserAssets.targets`,
  `build/AlisReactiveNetFrameworkContent.targets`, `build/AlisReactiveStaticWebAssets.targets` and main's
  `build/AlisReactiveAssets.targets`, which each package's own build target and static web assets replace (section 2.9,
  U22); `Alis.Reactive.Assets/components/`, whose code moves into its package; Tailwind, and jsdom as a test's
  browser, which the node probes alone still load.
- Main's packages.config layout, by the owner's word on main (spec/CLAUDE.md, 2026-11-10). Main's build target reads a
  version from a packages.config folder too (`build/AlisReactiveAssets.targets:41-50` at 7b9e987cc), a path main never
  ran: its own MVC 5 app restores by PackageReference "so NuGet resolves transitive deps"
  (`examples/net48-mvc-smoke/Net48MvcSmoke.csproj:29-31` at 7b9e987cc), and `RC1_RELEASE_READINESS.md:170-171` at
  7b9e987cc calls it "not yet observed". NuGet gives a packages.config project a package's analyzers only through an `install.ps1` that Visual
  Studio runs, which main does not ship, so the rules the compiler holds would not reach that app. One layout is
  proven by one fresh app. An MVC 5 app on packages.config moves to PackageReference by hand, since NuGet's migrator
  refuses ASP.NET projects, and README.md gives the steps (U22).
- Main's `Alis.Reactive.FluentValidator`, its line in the pack script and its id in the publish workflow's expected
  list, which the merge of main brings in; M8.F1 holds its doors' verdicts.
- The sandbox's worked example in the docs rule file: the cut removes it, and U18 writes the conformance app's.
- The docs site, by the owner's answer (spec/CLAUDE.md, 2026-11-10): `docs-site/`, the workflow that deploys it,
  `.github/workflows/deploy-docs.yml`, and the generator of its API page, `tools/ApiDocGenerator`. The docs are README.md,
  packed as every package's readme, and the XML docs on the shipped surface.
- Main's working notes and another editor's rules, readable at 7b9e987cc: `docs/` (process notes, decision records and
  handoffs; root CLAUDE.md's Workflow and section 2.8 hold the process), `issues/`, `.cursor/`,
  `RC1_RELEASE_READINESS.md`, `high-quality-bar-tasks.md`, `pure-gold-refacorts.md`, and
  `.github/PULL_REQUEST_TEMPLATE.md`, whose bar is main's definition-of-done rule.
- SonarQube, `docker-compose.sonarqube.yml` and `scripts/sonar-analyze.sh`: the rewrite's quality is section 1's layers,
  and no check reads Sonar's result.
- Main's artifact-gated Fusion onboarding, removed on purpose (root CLAUDE.md's vision): `tools/FusionOnboarding`,
  `tools/FusionCoverage`, `.claude/workflows/*` and `.claude/skills/onboard-fusion-component`.
- Main's `.claude/` beyond what section 2.1 keeps, which the redesign's rules and two skills replaced: `.claude/commands/*`,
  `.claude/memory/*`, `.claude/hookify.*.local.md`, `.claude/hooks/require-playwright-wrapper.mjs`, whose job the deny
  rule in the session's settings does, the rules `.claude/rules/agent-dispatch.md`, `.claude/rules/definition-of-done.md`,
  `.claude/rules/plan-contract-boundary.md`, `.claude/rules/process-pipeline.md` and `.claude/rules/process-task-types.md`,
  and the skills `.claude/skills/bdd-testing`, `.claude/skills/conditions-dsl`, `.claude/skills/dotnet-xml-docs`,
  `.claude/skills/http-pipeline`, `.claude/skills/modern-csharp`, `.claude/skills/reactive-dsl`,
  `.claude/skills/solid-ts-audit` and `.claude/skills/validation-rules`.
- `tests/Alis.Reactive.Playwright.Extensions`, main's locators over Fusion components' markup, which TEST001 refuses: a
  Playwright project compiles against the world contract alone.
- Main's example app, `examples/resident-intake` with `scripts/rebuild-example-app.sh`: the conformance app and the fresh
  consumer apps (section 2.1) replace it.
- `.github/workflows/gate.yml` and `.github/workflows/verify-net48.yml`, whose jobs become ci's (section 2.8), which the
  publish workflow runs on the tag.

## 4. Units

The rewrite runs on `feature/rewrite`, branched from `feature/spec-first`'s tip with main at 7b9e987cc merged in, the
tip whose delta from 623dd4a27 check.py's `main_delta` holds to verdicts, and lands on main by pull request. A later
main commit enters only when `MAIN_TIP` moves to it and `main_delta` passes; the redesign's tip is still 65fdf118d, and
a later commit of it needs the same before it enters. The merge raises six conflicts, each resolved here: `drawer.ts`,
`FusionToastExtensions.cs`, `FusionScheduleExtensions.cs` and `FusionTemplateExpression.cs` take this line's side, since
the cut removes both lines' code and main_delta holds main's changes to them as verdicts; main's `docs/RELEASING.md`
stays deleted (section 3); `nuget-publish.yml` takes main's trusted publishing, its OIDC login and `id-token: write`,
into this line's file, which U2 rewrites to section 2.8's. U1's first commit is the cut: it removes both lines' code that
section 2.1 does not hold under the same path, the shipped projects' sources and every item of section 3 but the `poc/`
bullet's, and in the same commit rewrites root CLAUDE.md, within 200 lines and with its loop kept, `.claude/rules/`, the two skills and the agent section 2.1
keeps, and the folder CLAUDE.md files for the new tree, the meanings list
stated from D2.4 to D2.6, D2.9 and D4.2 (D2.8), since `WhenARuleNamesTheCode` fails on a rule that names what the cut
removed. Each removed line stays readable at 65fdf118d or at main's SHA, which every cite pins, and `features_lost` at
0 is each removed door's verdict. Until the `poc/` bullet's items leave, they keep their own package versions: those of
their projects that write a version inline, which the central management the cut turns on refuses (NU1008), so the cut
writes a `Directory.Packages.props` that turns it off in `poc/`, in each of their folders under `tests/`, and in
`spec/api/` and `spec/probes/`: the spec's own projects take the solution's versions and lock files from neither, and
a probe pins the version of the fact it reads.

What the cut leaves of a path section 2.1 holds:
- Each shipped project that exists, the core, Native and Fusion, stays as an empty project: its project file and its
  surface text, emptied, since `WhenCodeIsWritten` reads which packages ship from their `PublicAPI` folders. The
  solution holds those three and the architecture tests.
- A file whose whole content was the old lines' leaves and returns with the commit or unit that writes it: the
  analyzers and their cases, the Assets project, the module tests and the contract generator; `eslint.config.js` with the
  lint; `scripts/test.sh` with `scripts/check.sh`; `scripts/run.sh` and the two architecture tests that sweep an app,
  WhenAnAppRendersItsPages and SandboxStaticStateTests, in U2 with the conformance app.
- Every other script and build file is reduced to what is true of the tree the cut leaves. `scripts/playwright.sh`
  is section 2.7's runner since U2 wrote it, the rewrite's alone, and reads no descriptor: the runner the cut kept
  for the POC's suites is `poc/playwright.sh`, with what it reads, and leaves with `poc/` (above), since the rewrite
  wins where the two meet (the owner, 2026-11-11). `scripts/pin-syncfusion.mjs` reads the NuGet pin in
  `Directory.Packages.props` and the npm pin in the root `package.json`, which drops its workspaces and declares what
  the kept code loads, each at the version the tree held: TypeScript and esbuild for the POC's build and the rules
  test, and EJ2, SignalR, jsdom, parse5 and ws for the probes.
- The hook that refuses a hand edit of the generated contract follows it to `Alis.Reactive.Assets/runtime/Schema/`,
  and WhenClaudeRunsAHook holds it there by the path, before U3 writes the two files.
- `ci.yml`, `nightly.yml` and the publish workflow called the gate the cut removed, so each failed closed until U2
  wrote section 2.8's. Since then `proof` stays red, and no tag publishes, until the units land what its jobs name:
  the tests of the hosts and fusion pools from U3 on, and in U22 the two empty packages, the twin and the two fresh
  apps. Each pool runs as one shard, and every run reads this Mac's budget and waits, until CI measures its own
  when the rewrite is done (2.5); no run of a workflow gates a unit.

A unit ends when every test check.py lands in it passes on `scripts/playwright.sh`, each written from its promise
before the code it needs and failing then, and failing again on its planted defect; its module's pure tests pass, each
written from the schema's lines; the specs its module owns are green under TLC with each mutant failing; and its
analyzer cases pass. The commit names each run and its result line.

The order is the owner's of 2026-11-11 (spec/CLAUDE.md): outside in, then one module a unit in the slice table's
order. U3 lands the seam and the surface whole: the schema's nodes and the generated contract, every door of the
public surface as a signature, the runtime's dispatch with a case for every kind, every screen's view, and every
browser test, written from its promise and failing. In the same unit it makes one thin path real, a plan that
renders, a runtime that boots and a load trigger whose pipeline sets an element's text, since every door that renders
a plan sits in the table's last core row and the runtime's code that touches the DOM has no proof but a browser test.
Each unit after it owns one module, in the table's order (2.3) and a row's modules in the order the row lists them:
the C# half that writes its nodes, the runtime half that executes them, its analyzers and its pure tests. Then Native,
then Fusion. ALIS010 holds at every commit.

A browser test passes in the unit check.py lands it in: its feature's unit, or a later unit that holds the module of
one of its screen parts (check.py prints each unit's tests). So a module's unit proves its two halves by its pure
tests and its analyzer cases and turns green the browser tests whose pages need nothing later; a test whose page
needs a later module, a Native input or a request among them, turns green in that module's unit and fails on its
planted defect there. A unit's Doors line names the doors it makes real ahead of their features' units, as U3's thin
path does, and a part that lists its doors, each on a Doors line, lands with the latest unit that brings one of them.

**Where the rewrite stands.** U1 and U2 are landed. U2 left the world contract with the generated gates and
promises, the harness with its three tests, the app's World area, `scripts/run.sh`, the test rules TEST001 to
TEST013 on the Playwright project, the bindings check, section 2.7's runner with its measured budget, waits and
count of tests at once, a pool's shard and CI's proof, the two architecture tests over the app, the harness's
refusal of the EJ2 script, and section 2.8's four workflows, which no run has read. U3 is in work. Landed of it:
the schema's nodes, the 51 kinds as sealed classes of `Alis.Reactive/Schema`, with the contract generator, its
cases, its step in `scripts/check.sh` and the generated `plan.ts` and `plan.schema.json`, and the kinds check
reading the rewrite's grammar. In work: the core's sixteen rows and every door as a signature, of which
every door `spec/api` compiles for the core is declared, in eleven rows, the plan's two doors among them
(`Html.ReactivePlan`, `Html.RenderPlan`; D14.7 as the owner corrected it on 2026-11-12). The rows of
`TypeKnowledge`, `InputsAndIds`, `Realtime` and `PartialsAndSlots` hold no door `spec/api` compiles and
enter the table with their first file, as `PathsAndWire`, `OwnershipAndState`, `Tracing` and `Execution` did with
the thin path. Landed after
that: the runtime's first row, `Schema`, which reads a plan script's text into its plan or the error it is refused
with (D19.10, M14.D1), with vitest, its first pure tests and its step in `scripts/check.sh`, and the lint's import
rows, written from each slice table, with their cases; and the core's banned symbols with WhenTheCoreIsCompiled,
whose list of the owner's type reads holds the plan's id; the module tests' project,
`tests/Alis.Reactive.ModuleTests`, with its first tests; and the thin path's C# half: the plan's two doors,
which a view renders as M19.D3's script with the document's every field, the load trigger (M1.N1), `pipeline.Element(id)`
(M5.N5) and a set of an element's text to a text literal (M5.N1), written by the block writer of `Execution`. What
the sentence does not need of those declarations stays unbuilt. The thin
path's runtime is written under the compiler and the lint, and no browser test has run it: seven rows of
`Alis.Reactive.Assets/runtime` and its entry, the evaluator's two switches with a case for every statement and
expression kind, an unbuilt kind failing loud, the one switch of the object locators, of a vendor's listening and of
a handler's policy, the file that finds a root's plans, and the lint's refusal of a wide query in every other.
The core's bundle is built by `build.mjs` at the root, which each script that builds runs first. The unit's one
test, X19.3's, is green on the refusals screen's ward page, whose layout renders a fragment the app's cache holds,
and fails on each of its two planted defects; it proves the runtime's thin path in a browser through a click
handler of a seeded plan, and no test of U3 asserts the load trigger there.
ALIS041 holds the plan's two doors at build, in a view through the code Razor's generator writes for it and at the
view's own line: a plan no line hands to `Html.RenderPlan`, a plan rendered twice and a door that takes the plan
after it rendered. Where the build does not follow a view's code, a section among it, the render refuses the last
two with the same sentence (D18.6).
WhenAnAppRendersItsPages states its next cases: each plan a page renders is read against `plan.schema.json`, an
MVC action is opened at its route, a route's parameter takes the value a link gives it, a page of the refusals
area that refuses is accepted as refused by design, and a page whose door is not built is listed apart from a
problem until U27.
The conformance library is declared in the app, `Components/` and a folder for each component: its six components,
their payloads and live objects, written member for member from `spec/Reference/native.json`, and the app's data
classes and enums the members name; no door of them is built, and their TypeScript is U13's. `Field`, the typed
member name a declared method takes (M11.F1), is declared with them.
The first view on the doors is the ledger of `Refusals/ValuesAndSources`, with the conformance component's render
helper and X7.13's test, which is red: its page answers 500 for a door that is not built, and the sweep of the
app's pages lists it apart.
The app event family is declared, M1.N2 to M1.N5 with the data a handler reads (N5.6), doors `spec/api` compiles
no sentence with, and the bench's first cell, prop.scalar.read, stands on them with its test red.
The refusals index and X3.12's linked record page stand, its test red, and so does M12.X2's call panel, an element
named by two declarations.
Open of it, in order: the doors a view meets that are not yet declared, each with that view; every screen's view;
every browser test, failing; the Playwright project's switch. The bindings check takes no view clause (section
2.5).
The commit that lands a piece rewrites this paragraph.

### U1. Tools, pins and tables
Features: M20.F1

The cut and the rewritten rule files (section 4's opening), the pins of section 2.7 with the TLC lock, central package
management, the SliceTree tables, the analyzers project with module 20's rules (ALIS010 to ALIS013, and ALIS009 over a
compilation that carries a slice table, with its exemption and WhenAPackageShips, M20.D7), the lint at its
strictest with its rules' cases (WhenTheLintIsConfigured), `check.py --ci`, `scripts/check.sh` green on a core that
holds the exemption alone, with `scripts/test.sh`, and every architecture test it keeps with its
refused and accepted cases and WhenAnArchitectureTestIsWritten (section 2.10).

### U2. The harness, the conformance app's shell and CI
Features: none

The app's areas, hosts, world, verbs, generated gates and bindings; the world contract; the Playwright project's
harness, result line and leftover-server check; the test rules TEST001 to TEST013 with their cases (section 2.10),
before any feature test is written; the CI workflows; `scripts/run.sh` and the two architecture tests over the app
(section 2.1). Its own tests, app.json's `harnessTests`, prove the harness: two
worlds isolated, and a held answer that reads the world as it is at its release; the held gate's type keeps a release
before reached from compiling, and the cases of WhenAPlaywrightRunStarts and WhenAPlaywrightRunEnds prove the runner's
selection and its reading, a run that selected no test failing among them. It measures and writes the waits, the budget and the
parallelism (section 2.5, 2.8).

### U3. The seam, the surface, every view, every test and one thin path
Features: M14.F1
Doors: N14.2, M1.N1, M5.N5, M5.N1

The schema's nodes, sealed, with the generated contract and its step in `scripts/check.sh`; the core's sixteen rows
with their folders; every door of the public surface as a signature, as `spec/api` compiles it; the runtime's rows
and its dispatch, a case for every kind; the harness's remaining verbs; every screen's view; every browser test, from
its promise, failing. The module tests land here with their first tests, the schema's: the project
`tests/Alis.Reactive.ModuleTests` with its rule file and its slice table, and vitest with the runtime's first
`*.test.ts`. The thin path: one plan, opened and rendered by its view's two doors (N14.2), which ALIS041 holds and lands here
(N18.15), with a load trigger (M1.N1) whose pipeline, one
run of one block, sets an element's text (M5.N5, M5.N1), compiled for both hosts. The lint's two rules that read a
path of the runtime land here, each with its refused and accepted file (section 2.7), and so do the core's banned
symbols with WhenTheCoreIsCompiled, before the core's first code that reads a type. WhenAnAppRendersItsPages, which U2 lands over the World
area's three addresses, meets its next cases here and states each with a refused and an accepted case: every plan a
page renders read against `plan.schema.json` (JsonSchema.Net, 2.7), the address of an MVC action, the value of a
route's parameter, which the sweep refuses as unopened until then, and the refusal routes, whose pages refuse by
design.

### U4. Type knowledge
Features: M16.F1

### U5. Paths and wire
Features: M15.F1

### U6. Ownership and state
Features: M18.F1

### U7. Tracing
Features: M26.F1, M26.F2, M26.F3

AlisReactive.OpenTelemetry's trace sink and layout helper (N20.2), its bundle built on 2.7's OpenTelemetry pins, and
README.md's startup example setting `console` from the host's development setting (N20.3); D20.8's
net48 fact, joining an inbound traceparent through the app's ASP.NET instrumentation module, probed on verify-net48
before the unit claims it.

### U8. Execution
Features: M17.F1

### U9. Values and sources
Features: M2.F1, M2.F2

### U10. Conditions
Features: M3.F1, M3.F2

### U11. Array lambdas
Features: M4.F1, M4.F2

### U12. Declarations
Features: M11.F1, M11.F2, M11.F3

### U13. Vendors
Features: M12.F1, M12.F2

The conformance library's declarations land here with the doors that reach them; the bench's cells run from here on.
ALIS009's reach to a component declaration and a render helper in a compilation with no slice table (M20.D1, M20.D7)
lands here with its cases, since it reads the types this unit and U12 land.
README.md and the onboarding skill teach a component's slice as the app's standard (spec/CLAUDE.md, 2026-11-06).

### U14. Plugins
Features: M13.F1, M13.F2

### U15. Elements
Features: M5.F1

### U16. Inputs and ids
Features: M7.F1, M7.F2, M7.F3

The core renders no input: `Html.InputField` builds on a component's render helper (M7.N2), so each behavior test of
this unit's features lists the component that renders its field, a conformance declaration of U13 or a Native input of
U23 (check_app), and lands with the last door its page uses.

### U17. Validation
Features: M8.F1, M8.F2, M8.F3, M8.F4

### U18. Requests and responses
Features: M6.F1, M6.F2

The request door's XML doc names MVC's binding of a parameter prefix and of a parameter named like its type's member (C25).

### U19. Realtime
Features: M10.F1, M10.F2

### U20. Triggers and pipelines
Features: M1.F1, M1.F2

### U21. Partials and slots
Features: M9.F1, M9.F2

### U22. Rendering and delivery
Features: M19.F1

The last core row whole, with the six packages packed, the delivery tests green on both fresh apps and the twin
built. README.md, every package's readme, holds what a fresh app of section 2.9 does on each host, a net48 app's move
to PackageReference among it; its first code example lands with the architecture test that holds each example to a
view a browser test runs (`.claude/rules/docs.md`).

### U23. Native components
Features: M24.F1, M24.F2, M24.F3, M24.F4, M24.F5, M24.F6, M24.F7, M24.F8, M24.F9

Written to the highest bar: a component-writing expert's review of each slice before its commit, bounded as root
CLAUDE.md bounds every review, and the strictest lint and TypeScript options of section 2.7 with no exemption.

### U24. Native app-level objects
Features: M25.F1, M25.F2, M25.F3, M25.F4, M25.F5, M25.F6, M25.F7

NativeDrawer, NativeToast, NativeConfirm, NativeLoader, the reconnect offer and the action link, each rendered into
the target a legacy layout holds by the helper the layout calls once; the same review and lint as U23.

### U25. Fusion's package, its reference and templates
Features: M21.F1, M21.F2, M22.F1, M22.F2, M22.F3

FusionMultiSelect's own tests, syncfusion.json's `own`, and the templates. The harness's refusal of the EJ2 script
(section 2.5) gains here, with the first of those tests, the one way a Fusion slice's fixture takes that script.

### U26. The Fusion slices
Features: M21.F3, M21.F4, M21.F5, M21.F6, M21.F7, M21.F8, M21.F9, M21.F10, M21.F11, M21.F12, M21.F13, M21.F14,
M21.F15, M21.F16, M21.F17, M21.F18, M21.F19, M21.F20, M21.F21, M21.F22, M21.F23, M21.F24, M21.F25, M21.F26, M21.F27,
M21.F28, M21.F29, M21.F30, M21.F31, M21.F32, M21.F33, M21.F34, M21.F35, M21.F36, M21.F37, M21.F38, M21.F39, M21.F40,
M21.F41, M21.F42, M21.F43, M21.F44, M21.F45, M21.F46, M21.F47, M21.F48, M21.F49, M21.F50, M21.F51, M21.F52

One commit per slice, each with its own tests, which `slice_tests` holds.

### U27. Fusion's app-level objects
Features: M23.F1, M23.F2

FusionConfirm and FusionToast, each with its own tests. This unit builds the last door, so from it on
WhenAnAppRendersItsPages takes a page whose door is not built as a problem; until it the sweep lists such a page
apart.
