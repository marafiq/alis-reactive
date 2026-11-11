# The rewrite

What the rewrite builds and the order it builds it in. The decisions behind each line are in `spec/CLAUDE.md`; the
meanings are in `spec/Core/DESIGN.md` and each module's folder. Two designs fed this file, one for finding a feature
in the fewest hops and one for the fastest honest feedback loop; where they disagreed, the choice and its reason are
in the line.

## 1. The view from above

- **Six packages, one version.** AlisReactive (the core, its analyzers inside), AlisReactive.Native,
  AlisReactive.Fusion and AlisReactive.OpenTelemetry, the trace sink a page registers (N20.2), ship code;
  AlisReactive.DesignSystem and AlisReactive.NativeTagHelpers ship empty, so their ids stay, and nothing in the tree is
  kept for them. Main's AlisReactive.FluentValidator is not packed: the core's validation carries its doors (M8.F1;
  spec/CLAUDE.md, 2026-11-10).
- **One app proves the framework.** The care home conformance app (`spec/Reference/app.json`): care screens a care
  home's staff use, the component bench with the conformance library's declarations, and the refusal routes, run
  under the hosts app.json declares. Every browser test runs on one of its screens. No core test loads Syncfusion.
- **Each layer proves one thing, once.**

  | Layer | Proves | Runs |
  |---|---|---|
  | TLA+ (`spec/<Module>/*.tla`) | the design: every ordering of the async lanes | TLC, on a changed spec and nightly |
  | Probes (`spec/probes/`) | a fact of a browser, a host or a vendor the design rests on | on a changed probe or pin, nightly |
  | The compiler | the type stages and the ALIS analyzers: what a view, a validator or a declaration cannot write | every build |
  | The render | a refusal the compiler cannot see, on the refusal routes | Playwright |
  | Playwright | behavior: a gesture and its visible outcome, in Chromium | every pull request |
  | The test rules | that a browser test cannot lean on the code it tests: TEST001 to TEST013 (section 2.10) | every build of a Playwright project |
  | Architecture tests | what the repository holds: no comment, every name a rule file cites, the hooks' refusals, the app's pages and statics, the runner's result line, a fresh app's git status | every pull request |
  | Cases and plants | that each analyzer, test rule, architecture test and check.py check reports its refused case by name and passes its accepted case (section 2.10) | with the rule each proves |
  | verify-net48 | each test whose placement lists the twin, on MVC 5 (no stream or hub: realtime is net10's, check_app), and the packed packages on a fresh MVC 5 app | every pull request |
  | Vitest, PlanTests | feedback while coding (BRIEF.md, Principles 10); never counted as proof | the inner loop |

- **Tests stand apart from the code.** A Playwright project references no assembly of the framework or of the
  conformance app; it reaches a page only through the browser and the world contract, so a test cannot compute what
  the code computes. Its quality rules are compiler rules in a project of their own. The feedback projects, vitest
  beside the runtime and PlanTests, reference the code they give feedback on and are never counted as proof.
- **A feature is one path through the spec to the code.** `spec/<Module>/features.json` names its question, its
  sentences, its nodes, its TLA+ invariants, its defects and its tests; each test names its screen, hosts, parts and
  held steps in `spec/Reference/app.json`. A new feature runs: spec line, TLC green, its tests written from their
  promises and failing, the screen and the doors until they pass, each test failing on its planted defect, commit.

## 2. The repository

### 2.1 The tree

```
Alis.Reactive.slnx                    the shipped projects, the generator and the net10 test projects
Directory.Build.props                 LangVersion 14.0, warnings as errors, AlisSliceTree off by default
Directory.Packages.props              central package management: one exact version per package and framework, but
                                      Roslyn's second (2.7), which the architecture tests take by VersionOverride
global.json                           SDK 10.0.101, rollForward disable
.node-version .python-version .java-version
package.json  package-lock.json  eslint.config.js   npm workspaces, locked; the lint, its import order generated from
                                      each SliceTree.txt
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
  runtime/SliceTree.txt runtime/index.ts runtime/<Module>/ (with its *.test.ts beside it)
Alis.Reactive.Analyzers/              netstandard2.0, packed inside AlisReactive
  <Module>/                           the rules that enforce that module's lines, its generators and build files
  Analyzers/                          module 20's own rules, ALIS010 to ALIS013
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
                                              typed seed builders, the gate names generated from the TLA+ specs
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
  Alis.Reactive.PlanTests/<Module>/           feedback
  Alis.Reactive.AnalyzerTests/<Module>/       each analyzer's cases
  Alis.Reactive.ArchitectureTests/            WhenCodeIsWritten, WhenARuleNamesTheCode, WhenClaudeRunsAHook, the POC's
                                              WhenTheCoreIsCompiled and WhenAContractDriftIsSuppressed, over the
                                              conformance app WhenAnAppRendersItsPages and SandboxStaticStateTests,
                                              renamed WhenTheAppHoldsNoStaticState, over each fresh app's build
                                              WhenAConsumerAppIsBuilt (section 2.9), and over the runner's result
                                              WhenAPlaywrightRunEnds (section 2.5), each rule with its refused and
                                              accepted cases, which WhenAnArchitectureTestIsWritten holds; their
                                              helpers Repository, Scripts and read-scripts
spec/                                         the design source: BRIEF.md, REWRITE.md (this file), CLAUDE.md,
                                              NOTES.md, STATE.json, history.tsv, Core/, Fluent/, api/, inventory/,
                                              plants/, probes/, Reference/, tools/, one folder per module
```

Beside the tree the repository keeps `.claude/settings.json`, `.claude/hooks/protect-generated.mjs`,
`.claude/rules/csharp.md`, `.claude/rules/docs.md`, `.claude/rules/tests.md`, `.claude/rules/views.md`,
`.claude/skills/onboard-component`, `.claude/skills/prove-behavior` and `.claude/agents/pragmatic-architect.md`, which
the cut rewrites for the new tree, and `.github/actions`, `.github/dependabot.yml` and `.github/release.yml`.

The Playwright project keeps the `PlaywrightTests` suffix, so the deny rule on a raw `dotnet test` of a Playwright
project holds. It names the app with `ReferenceOutputAssembly="false"`, so the app builds first and none of its types
reach a test; the app's `World/` holds the world's middleware and endpoints on the world contract. The consumer apps
and the twin each hold an empty `Directory.Build.props` and a `Directory.Packages.props` that turns central management
off, so their builds see what a fresh app sees.

### 2.2 One name in five places

| Tree | Folder | Holds |
|---|---|---|
| C# | `Alis.Reactive/<Module>/` | its builders, objects and sources, the plan nodes it writes, its render-time refusals; namespace `Alis.Reactive.<Module>` |
| Browser | `Alis.Reactive.Assets/runtime/<Module>/` | the code that executes its nodes, and its vitest feedback |
| Analyzer | `Alis.Reactive.Analyzers/<Module>/` | each rule that enforces one of its lines, the rule's id in the file name |
| Tests | `tests/Alis.Reactive.Conformance.PlaywrightTests/<Module>/`, `tests/Alis.Reactive.AnalyzerTests/<Module>/` | its browser tests, its analyzer cases |
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

The folder and namespace names are the module table's (spec/CLAUDE.md). DESIGN.md's `Pipelines`, `Requests` and
`Hosting` read `TriggersAndPipelines`, `RequestsAndResponses` and `RenderingAndDelivery`. The block writer, the POC's
`Writing/PlanBuild.cs`, goes to `Execution/` and reads no module above it: the view's name prefix is text it holds.
Realtime's files are wrapped whole in `#if NET10_0_OR_GREATER`, so the net48 build keeps the folder its row names.

### 2.3 The SliceTree

`Alis.Reactive/SliceTree.txt`, first row the foundation, each slice using only rows above it (ALIS010):

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
`Alis.Reactive.Assets/runtime/SliceTree.txt`, since its evaluator dispatches every kind and sits on top. The test app
holds `Components/*` over `World HostVariants` over `Care/* Bench/* Refusals/*`, so no screen reaches into another's
model, and the Playwright project holds one row of module folders, so no module's tests use another's helpers.
ALIS011 expects `<assembly>.<first folder>` for every file; ALIS013 reads the `AlisSliceTree` build property in place
of a list of assembly names (M20.X1); `eslint.config.js` writes ESLint's `no-restricted-imports` from each table.

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
  world's residents as text the server writes, and an echo endpoint that answers a GET with its query's word.
- **The verbs** are app.json's: seed, gate, push, drop, refuse, reopen, restart, clock, gc. A placement lists its
  verbs and its held steps, each held step with the TLA+ action it holds, and check_app refuses a verb with no step.
  Gates are generated from the `Next` of each spec a held step names into the world contract (`Gates.g.cs`, one enum
  per spec), so the app and the tests name a gate the same way without either seeing the other: the test arms a gate
  before its gesture, waits until the harness reports it held, asserts the held state, then releases it by its own
  request; a release before the gate is reached fails the test. Restart is the test app's host script on the harness's
  word. Clock is `Page.Clock`, installed only by a test that names it. Gc is `Page.RequestGCAsync`, confirmed by the
  page's own weak hold.
- **Bindings.** `Promises.g.cs` is generated from app.json and the features: a test carries `[FailsFor(...)]` naming its
  feature, defect and index, its cell, or its harness test. The bindings check fails when the generated files differ
  from what the spec writes, when a test names no test of the spec or a test of the spec has no test, when a test never
  reached the gate its held steps name, and when a screen's view uses a module namespace that neither the test's
  feature nor a part it lists belongs to, since check.py lands each test in the latest unit of those modules.
- **Hosts.** A fixture takes its hosts from its placement (`TestFixtureSource`), each host an NUnit category. Each net10
  host is one server process, started on first use with `--urls http://127.0.0.1:0`, its address read from the
  "Now listening on:" line; it exits when its test host dies, teardown kills its process tree, and a run that leaves a
  server alive fails. The net10 clock is America/New_York, the twin's UTC; the browser context is en-US in
  America/Los_Angeles, so a date read in the wrong zone shows. The hosts consumerNet10 and consumerNet48 are the fresh
  apps' own servers, which hold no World area: a test under one runs on app.json's `fresh` screen, sets no world and
  uses no harness verb.
- **A run proves it ran.** Each test process ends with
  `[playwright:result] <pool> <i>/<k>: N selected, N ran, N passed, 0 failed, 0 over budget, slowest <s> <name>` and
  fails on no TRX, zero selected, ran other than selected, a failure, a test over budget, or a server left alive.
  WhenAPlaywrightRunEnds runs that reading over the TRX and the result line each case gives: a refused case for each
  failure, and an accepted case for a green run. No retries, anywhere.
- **Waits and budget.** Action waits, navigation waits and the per-test budget are set from measurement, never chosen:
  unit U2 runs the suite 20 times on ubuntu-latest and sets the budget at four times the slowest test it measured, the
  waits at four times the slowest step, and writes both into the harness with the run that measured them.
- **Syncfusion.** Only the Fusion pool's job receives the licence key; the app registers it at startup when its host
  serves a Fusion screen, and a Fusion screen refuses to render without it. A core test also fails at run time if its
  page requests the EJ2 script.

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
| .NET SDK | 10.0.101 (`global.json`, rollForward disable) | compile, test, pack |
| NuGet | the SDK's, 10.0.101's; `Directory.Packages.props`, lock files restored locked in CI | restore |
| net48 references | System.Text.Json 10.0.12, Microsoft.Extensions.DependencyInjection.Abstractions 10.0.12, System.Diagnostics.DiagnosticSource 10.0.12, Microsoft.AspNet.Mvc 5.3.0 | the core's net48 dependencies (2.9), restored together at these exact versions; net10 takes the first three from the framework reference |
| net48 build | PolySharp 1.16.0, Microsoft.NETFramework.ReferenceAssemblies 1.0.3, both private | the attributes C# 14's features need on net48, written as source, and the reference assemblies a build on any OS compiles against |
| MSBuild SDK | Microsoft.Build.NoTargets 3.7.0 | the Assets project |
| Node | 24.19.0 (`.node-version`) | esbuild, tsc, eslint, node probes |
| TypeScript | 5.9.3 | `tsc -b`, no emit, the strictest options (strict, exactOptionalPropertyTypes, noUncheckedIndexedAccess, noImplicitOverride, noPropertyAccessFromIndexSignature) |
| esbuild | 0.28.2 | one root `build.mjs`, one bundle per package |
| ESLint | eslint 9.39.4, @eslint/js 9.39.5, typescript-eslint 8.70.1 | `strictTypeChecked` and `stylisticTypeChecked` on the runtime and every package's TypeScript, `--max-warnings 0`; `switch-exhaustiveness-check`, `no-floating-promises`, `no-unnecessary-condition`, and `no-restricted-syntax` on wide DOM queries |
| Vitest | 3.2.7, node environment, no jsdom | feedback beside the runtime's code |
| @microsoft/signalr | 10.0.11 | the hub's own asset |
| OpenTelemetry | @opentelemetry/api 1.9.1, @opentelemetry/sdk-trace-web 2.12.1, @opentelemetry/exporter-trace-otlp-http 0.223.1 | the adapter's bundle (N20.2); the web SDK 2.12.1 takes the API below 1.10.0, so the three move together |
| Roslyn | Microsoft.CodeAnalysis.CSharp 4.12.0; 5.0.0 for the architecture tests | the analyzers and their cases; the architecture tests parse C# 14 (`LanguageVersion.CSharp14`), which 4.12.0 does not know |
| PublicApi and BannedApi analyzers | Microsoft.CodeAnalysis.PublicApiAnalyzers 5.6.0, Microsoft.CodeAnalysis.BannedApiAnalyzers 5.6.0 | the surface text; no run-time reflection |
| NUnit, adapter, test SDK | NUnit 4.6.1, NUnit3TestAdapter 5.2.0, Microsoft.NET.Test.Sdk 17.14.1 | tests |
| Plan schema | JsonSchema.Net 9.1.2 | PlanTests and the architecture tests read every plan against `plan.schema.json` |
| App sweep | Microsoft.AspNetCore.Mvc.Testing 10.0.1 | the architecture tests' sweep of the conformance app's pages |
| Playwright | Microsoft.Playwright 1.63.0, CI's image `mcr.microsoft.com/playwright/dotnet:v1.63.0-noble` by digest | every browser test and probe |
| Java, TLC | OpenJDK 27.0.1, tla2tools v1.7.4 (TLC 2.19) by SHA-256 in `spec/tools/tla2tools.lock` | model checking |
| Python | 3.14.7, standard library | `spec/tools/check.py` |
| Syncfusion | 33.1.47: Syncfusion.EJ2.AspNet.Core on net10, Syncfusion.EJ2.MVC5 on net48, @syncfusion/ej2, together | `node scripts/pin-syncfusion.mjs <version>` moves both |
| Actions | by SHA with Dependabot's version comment | CI |

Scripts: `scripts/check.sh` (the contract regenerated and diffed, `tsc -b`, eslint, `dotnet build -c Release`, the
architecture and analyzer tests, `check.py --ci`); `scripts/playwright.sh` (build, then run, with the selectors below,
the result line and the leftover-server check); `scripts/test.sh` (both); `scripts/build.sh`, `scripts/run.sh`,
`scripts/pack.sh`, `scripts/doctor.sh` (every pin), `scripts/plant-defect.sh` with `scripts/trx-failed-tests.mjs`, which
reads a TRX's failed and passed tests, `scripts/verify-release-tag.sh`, `scripts/pin-syncfusion.mjs`,
`scripts/install-git-hooks.sh`.
MSBuild never runs npm; an app whose bundle is missing refuses to start.

`check.py --ci`, which U1 adds, runs every check but survival, a spec session's own hook and branch, and probes,
which run nightly and on a changed probe or pin. It runs a TLC config only when the CI cache holds no result for its
hash, reads java from `JAVA_HOME` and the jar from `TLA2TOOLS_JAR`, refuses a jar whose SHA-256 is not the lock's,
writes no `spec/STATE.json`, and exits 1 on any failure. The checks that read code keep reading the SHAs their cites
pin, except kinds, which from U3 on reads the rewrite's grammar at HEAD: a kind `spec/Core/schema.md` does not list
fails, and from U26 on so does a kind it lists that HEAD lacks.

`scripts/playwright.sh` selects a module (`RequestsAndResponses`), a screen (`--screen Residents`), a feature
(`--feature M6.F1`), one test (`--test M3_F1_X8_27_2`), one fixture, one host (`--host snakeCase`), a pool's shard
(`--pool core --shard 1/2`) or everything (`--all`).

### 2.8 Shards and CI

- **The shard unit** is a module folder; the net10 host-variant fixtures form one unit, so each variant's server starts
  once per run; FusionComponents splits by slice. Pools: core, hosts and fusion; a test under a fresh app's host runs in its delivery job, never in a pool. Shards are balanced longest-first
  from the TRX-measured weights in `shard-weights.json`, so a stale weight unbalances and never drops a test. Unit U2
  measures one test at a time against 2, 4 and 8 per process, 20 runs each, and keeps parallelism only when every run
  is green and the wall time falls by at least 1.6 times.
- **`.github/workflows/ci.yml`** on every pull request and push to main: `checks` (`scripts/check.sh` without check.py); `spec`
  (`check.py --ci`, TLC only for a spec whose hash CI has not checked); `playwright` per pool and shard in the
  Playwright image; `verify-net48` on Windows (pack, the twin and the fresh MVC 5 app built against the packs with
  warnings as errors, WhenAConsumerAppIsBuilt over the fresh app, the tests under net48 and consumerNet48 under IIS
  Express); `delivery-net10` (pack, the fresh net10 app restored from the packs, WhenAConsumerAppIsBuilt over it, the
  tests under consumerNet10); `proof`, the one required status: every shard has a TRX with tests, none failed, none
  over budget, and the executed names equal the full list, each once.
- **`.github/workflows/nightly.yml`**: everything, every TLC config uncached, every probe; an issue on failure.
- **`.github/workflows/syncfusion-upgrade.yml`**, on a change to the pins: `pin-syncfusion.mjs --check`, the EJ2 browser probes, then the
  fusion pool.
- **`.github/workflows/nuget-publish.yml`**, on a `v*` tag: `verify-release-tag.sh`; ci on the tag; one pack job, its expected ids 2.9's six; verify-net48 and
  delivery-net10 restore those exact packages; publish pushes the same files by trusted publishing, in the
  `nuget-release` environment: nuget.org's policy names this repository, this workflow's file and that environment, so
  neither is renamed.

### 2.9 Packages and delivery

| Package | lib | analyzers | build | browser files | dependencies |
|---|---|---|---|---|---|
| AlisReactive | net48, net10.0 | the ALIS analyzers and the validator generator, packed from the analyzer project's own output path | `build/net48` copies the browser files; the vendor contract target, opt-in | `alis-reactive.js`; `alis-reactive-hub.js` on net10 | net48: System.Text.Json 10.0.12, Microsoft.Extensions.DependencyInjection.Abstractions 10.0.12, System.Diagnostics.DiagnosticSource 10.0.12 (D20.8), Microsoft.AspNet.Mvc 5.3.0; net10: the framework reference |
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
- Native, Fusion and OpenTelemetry depend on AlisReactive at the exact version: pack writes `[x.y.z]`, and U3's
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
  WhenClaudeRunsAHook already refuses an edit to the generated contract and allows one beside it. U1 lands the cases
  of every architecture test it keeps, and WhenAnArchitectureTestIsWritten refuses a rule with no refused or no
  accepted case. check.py's checks keep theirs in `spec/plants/`, which its `plants` check runs in throwaway
  worktrees.

| Id | Refuses |
|---|---|
| TEST001 | a reference beyond the target framework's assemblies, Playwright, NUnit and its test platform, and the world contract: the framework, the app, a mocking or a snapshot library |
| TEST002 | script run in the page or a stubbed network: `EvaluateAsync`, `EvaluateHandleAsync`, `AddInitScriptAsync`, `ExposeFunctionAsync`, `ExposeBindingAsync`, `RouteAsync`, `RouteFromHARAsync`, `RouteWebSocketAsync`, and a literal that names `ej2_instances` or `window.alis` |
| TEST003 | a wait on time: `WaitForTimeoutAsync`, `Task.Delay`, `Thread.Sleep`, `SpinWait` |
| TEST004 | a skipped, repeated or ordered test: `[Ignore]`, `[Explicit]`, `[Retry]`, `[Repeat]`, `[Order]`, `Assert.Ignore`, `Assert.Inconclusive`, `Assert.Pass`, `Assume.That` |
| TEST005 | a branch or a loop in a test method: `if`, `switch`, `?:`, `for`, `foreach`, `while`, `do`, `try`, `goto` |
| TEST006 | a one-shot read of the page (`InnerTextAsync`, `TextContentAsync`, `InputValueAsync`, `GetAttributeAsync`, `InnerHTMLAsync`, `IsVisibleAsync`, `IsHiddenAsync`, `IsCheckedAsync`, `IsEnabledAsync`, `CountAsync`, `AllInnerTextsAsync`, `AllTextContentsAsync`); a page is asserted through `Expect` alone, and `Assert.That` reads only a request the page sent, its body and its headers |
| TEST007 | an expected value, or a locator's selector, role name, label or text, that is neither a compile-time constant nor a generated promise constant |
| TEST008 | a partial match on the page: `ToContainTextAsync`, or a `Regex` in a page matcher; a regex reads only a request's header |
| TEST009 | an expected value equal to a constant the same test typed into the same locator |
| TEST010 | a test whose every assertion is an absence (`ToBeHiddenAsync`, a `Not` matcher, `ToHaveCountAsync(0)`, `ToBeEmptyAsync`, an empty text) |
| TEST011 | `Expect` or `Assert` outside a test method: a helper builds a page or performs a gesture, and never asserts |
| TEST012 | a test method without exactly one `[FailsFor]` taking a generated promise constant, or a fixture not named `When` and its situation |
| TEST013 | a test method that performs no gesture itself: a navigation, click, fill, press, select, check, drag or file, or a harness release or push |

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

## 3. What retires

Each retirement waits for `features_lost` at 0 and the rule files that name the item rewritten; "nothing references it"
is never the reason.
- `Alis.Reactive.SandboxApp` with `tests/Alis.Reactive.PlaywrightTests`; `tests/Alis.Reactive.Framework.App` with its
  16 components and `tests/Alis.Reactive.Framework.PlaywrightTests`; `tests/Alis.Reactive.ClassNames.App` and
  `tests/Alis.Reactive.ClassNames.PlaywrightTests`, now the host `classNames`; `scripts/framework-coverage.*`,
  `scripts/vstest-filter.mjs`, every `playwright.project.sh`, `check_ci_shard_matrix`.
- `poc/`, the POC's test projects (`tests/Alis.Reactive.Poc.*`) and `tests/Alis.Reactive.Playwright.TestHost` they build
  on, last: each proof ported by its unit, TestHost's harness merged into the Playwright project's `Harness/`, the POC
  analyzers moved to their modules' folders and its two architecture tests to the architecture tests,
  `poc/VendorContract.targets` and `poc/tools/vendor-contract.mjs` to `Alis.Reactive.Analyzers/Vendors/`,
  `spec/api/HttpSurface` retargeted to the core, and every cite of `poc/` pinned to a SHA.
- The sources of `Alis.Reactive.DesignSystem` and `Alis.Reactive.NativeTagHelpers`, their workspaces and asset lines, since
  their ids ship from `build/EmptyPackage.proj`; `examples/net48-mvc-smoke`, which becomes
  `tests/Alis.Reactive.Consumer.Net48`; the build targets `build/AlisReactiveBrowserAssets.targets`,
  `build/AlisReactiveNetFrameworkContent.targets`, `build/AlisReactiveStaticWebAssets.targets` and main's
  `build/AlisReactiveAssets.targets`, which each package's own build target and static web assets replace (section 2.9,
  U3); `Alis.Reactive.Assets/components/`, whose code moves into its package; jsdom and Tailwind.
- Main's packages.config layout, by the owner's word on main (spec/CLAUDE.md, 2026-11-10). Main's build target reads a
  version from a packages.config folder too (`build/AlisReactiveAssets.targets:41-50` at 7b9e987cc), a path main never
  ran: its own MVC 5 app restores by PackageReference "so NuGet resolves transitive deps"
  (`examples/net48-mvc-smoke/Net48MvcSmoke.csproj:29-31` at 7b9e987cc), and `RC1_RELEASE_READINESS.md:170-171` at
  7b9e987cc calls it "not yet observed". NuGet gives a packages.config project a package's analyzers only through an `install.ps1` that Visual
  Studio runs, which main does not ship, so the rules the compiler holds would not reach that app. One layout is
  proven by one fresh app. An MVC 5 app on packages.config moves to PackageReference by hand, since NuGet's migrator
  refuses ASP.NET projects, and README.md gives the steps (U3).
- Main's `Alis.Reactive.FluentValidator`, its line in the pack script and its id in the publish workflow's expected
  list, which the merge of main brings in; M8.F1 holds its doors' verdicts.
- The sandbox's worked example in the docs rule file: the cut removes it, and U12 writes the conformance app's.
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
bullet's, and in the same commit rewrites root CLAUDE.md, `.claude/rules/`, the two skills and the agent section 2.1
keeps, and the folder CLAUDE.md files for the new tree, the meanings list
stated from D2.4 to D2.6, D2.9 and D4.2 (D2.8), since `WhenARuleNamesTheCode` fails on a rule that names what the cut
removed. Each removed line stays readable at 65fdf118d or at main's SHA, which every cite pins, and `features_lost` at
0 is each removed door's verdict. Until the `poc/` bullet's items leave, they keep their own package versions: those of
their projects that write a version inline, which the central management the cut turns on refuses (NU1008), so the cut
writes a `Directory.Packages.props` that turns it off in `poc/` and in each of their folders under `tests/`.

A unit ends when every test check.py assigns it passes on `scripts/playwright.sh`, each written from its promise
before the doors it needs and failing then, and failing again on its planted defect, the specs its module owns are
green under TLC with each mutant failing, and its analyzer cases pass; the commit names the run and its result line.

The order is the order pages need doors, not the SliceTree's. A unit for Schema alone has no page to prove it on, so
U3 is a walking skeleton that gives every row its folder and the thinnest door, and each unit after it widens the
rows its features need; ALIS010 holds at every commit. A test lands in the unit that brings the last door its page
uses: its feature's unit, or a later unit that holds the module of one of its screen parts (check.py prints each
unit's tests). A unit's Doors line names the doors it brings ahead of their features' units, as U3's skeleton does, and
a part that lists its doors, each on a Doors line, lands with the latest unit that brings one of them.

### U1. Tools, pins and tables
Features: M20.F1

The cut and the rewritten rule files (section 4's opening), the pins of section 2.7 with the TLC lock, central package
management, the SliceTree tables, the analyzers project with ALIS010 to ALIS013 and module 20's rules, the lint at its
strictest, `check.py --ci`, `scripts/check.sh` green on an empty core, and every architecture test it keeps with its
refused and accepted cases and WhenAnArchitectureTestIsWritten (section 2.10).

### U2. The harness, the conformance app's shell and CI
Features: none

The app's areas, hosts, world, verbs, generated gates and bindings; the world contract; the Playwright project's
harness, result line and leftover-server check; the test rules TEST001 to TEST013 with their cases (section 2.10),
before any feature test is written; the CI workflows. Its own tests, app.json's `harnessTests`, prove the harness: two
worlds isolated, a gate held then released, a release before reached failing; WhenAPlaywrightRunEnds's cases prove the
runner, a run that selected no test failing among them. It measures and writes the waits, the budget and the
parallelism (section 2.5, 2.8).

### U3. The walking skeleton, the packs and delivery
Features: M19.F1
Doors: N14.2, M1.N1, M5.N5, M5.N1

One plan, the block of its view (N14.2), with a load trigger (M1.N1) that sets an element's text (M5.N5, M5.N1),
through every row, on both hosts; the six packages packed; the delivery tests green on both fresh apps; the twin built. README.md, every package's
readme, holds what a fresh app of section 2.9 does on each host, a net48 app's move to PackageReference among it.

### U4. Triggers and pipelines
Features: M1.F1, M1.F2

### U5. Values and sources
Features: M2.F1, M2.F2

### U6. Elements
Features: M5.F1

### U7. Declarations and vendors
Features: M11.F1, M11.F2, M11.F3, M12.F1, M12.F2

The conformance library's declarations land here with the doors that reach them; the bench's cells run from here on.
README.md and the onboarding skill teach a component's slice as the app's standard (spec/CLAUDE.md, 2026-11-06).

### U8. Inputs and ids
Features: M7.F1, M7.F2, M7.F3

The core renders no input: `Html.InputField` builds on a component's render helper (M7.N2), so each behavior test of
this unit's features lists the component that renders its field, a conformance declaration of U7 or a Native input of
U9 (check_app), and lands with the last door its page uses.

### U9. Native components
Features: M24.F1, M24.F2, M24.F3, M24.F4, M24.F5, M24.F6, M24.F7, M24.F8, M24.F9

Written to the highest bar: a component-writing expert's review of each slice before its commit, and the strictest
lint and TypeScript options of section 2.7 with no exemption.

### U10. Conditions
Features: M3.F1, M3.F2

### U11. Plugins
Features: M13.F1, M13.F2

### U12. Requests and responses
Features: M6.F1, M6.F2

The request door's XML doc names MVC's binding of a parameter prefix and of a parameter named like its type's member (C25).

### U13. Partials and slots
Features: M9.F1, M9.F2

### U14. Native app-level objects
Features: M25.F1, M25.F2, M25.F3, M25.F4, M25.F5, M25.F6, M25.F7

NativeDrawer, NativeToast, NativeConfirm, NativeLoader, the reconnect offer and the action link, each rendered into
the target a legacy layout holds by the helper the layout calls once; the same review and lint as U9.

### U15. Array lambdas
Features: M4.F1, M4.F2

### U16. Validation
Features: M8.F1, M8.F2, M8.F3, M8.F4

### U17. Schema
Features: M14.F1

### U18. Type knowledge
Features: M16.F1

### U19. Paths and wire
Features: M15.F1

### U20. Ownership and state
Features: M18.F1

### U21. Execution
Features: M17.F1

### U22. Tracing
Features: M26.F1, M26.F2, M26.F3

AlisReactive.OpenTelemetry's trace sink and layout helper (N20.2), its bundle built on 2.7's OpenTelemetry pins, and
README.md's startup example setting `console` from the host's development setting (N20.3); D20.8's
net48 fact, joining an inbound traceparent through the app's ASP.NET instrumentation module, probed on verify-net48
before the unit claims it.

### U23. Realtime
Features: M10.F1, M10.F2

### U24. Fusion's package, its reference and templates
Features: M21.F1, M21.F2, M22.F1, M22.F2, M22.F3

FusionMultiSelect's own tests, syncfusion.json's `own`, and the templates.

### U25. The Fusion slices
Features: M21.F3, M21.F4, M21.F5, M21.F6, M21.F7, M21.F8, M21.F9, M21.F10, M21.F11, M21.F12, M21.F13, M21.F14,
M21.F15, M21.F16, M21.F17, M21.F18, M21.F19, M21.F20, M21.F21, M21.F22, M21.F23, M21.F24, M21.F25, M21.F26, M21.F27,
M21.F28, M21.F29, M21.F30, M21.F31, M21.F32, M21.F33, M21.F34, M21.F35, M21.F36, M21.F37, M21.F38, M21.F39, M21.F40,
M21.F41, M21.F42, M21.F43, M21.F44, M21.F45, M21.F46, M21.F47, M21.F48, M21.F49, M21.F50, M21.F51, M21.F52

One commit per slice, each with its own tests, which `slice_tests` holds.

### U26. Fusion's app-level objects
Features: M23.F1, M23.F2
