# Alis.Reactive

Alis.Reactive lets developers author dynamic UI in ASP.NET MVC (.NET Framework 4.8 MVC 5 and
.NET 10) with fluent, fully typed C#. The aim is developer and agent productivity: typed
abstractions so the compiler catches mistakes before the browser does, and defaults so developers
type less.

This branch is the rewrite. `spec/` is its design, and `spec/REWRITE.md` is the repository it builds and the 26 units
it builds it in. Main, the redesign and `poc/` are evidence of features and of the mechanisms the POCs proved, never
the design: the unit that lands a door writes it from its spec lines.

## Spec-first

The rewrite is spec-first: `spec/BRIEF.md` states its thesis (section 2), its closed principles (section 3) and the
stability rules every test keeps (section 10); `spec/REWRITE.md` holds the repository it builds and its units; the 26
modules and the owner's decisions are `spec/CLAUDE.md`'s.
- A defect is prevented on the highest rung that holds it: the type, an analyzer, a refusal where the view renders,
  then behavior in the browser. A defect a type or an analyzer prevents gets no browser test.
- A unit runs in this order: its spec lines, TLC green with each mutant failing, its Playwright tests written from
  their promises and failing, the code until they pass, each test failing on its planted defect, commit.
- The server decides when async work happens and the test releases it through the harness; a test waits for the
  state it names, never for time, in a world of its own, and asserts literal outcomes.
- Every unit closes one loop, a spec or a rules unit as a rewrite unit, and each exit is a line a tool printed, read
  when it printed:
  1. Restate the goal in your own words; a new direction waits for the owner's yes.
  2. Read the owner's decisions (`spec/CLAUDE.md`), then every file the unit touches, whole, at HEAD, and the rules
     that file states. A line enters a spec file, a rule file or a test only after it is held against each rule.
  3. Decide from the proof in hand: the TLA+ specs, the principles and the owner's decisions. A decision on a
     principle, a closed rule or the scope goes to the owner as one line with its cost and its risk; no other does.
  4. Change one meaning in one commit, with every place that carries it.
  5. Prove it on the rung that holds it. Behavior: a `scripts/playwright.sh` result line, the test first failing on
     its planted defect. A rule (an analyzer, a test rule, a lint rule, an architecture test, a check): a refused
     case it reports by name and an accepted case one fact away, both kept in the repo. A pin: a restore or an
     install, and a run. Rules and docs: each rule found in one file, every removed line traced to its home. A
     failure goes back to 4, twice at most; then stop and research.
  6. Run the rules test on the staged unit (`tests/Alis.Reactive.ArchitectureTests/WhenARuleNamesTheCode.cs` and
     `WhenCodeIsWritten`), commit locally, and run the full check at that head (`python3 spec/tools/check.py`). Push
     on their two result lines alone, then the unit's STATE; `git ls-remote` equals HEAD after each push.
  7. The commit says what changed, its proof lines and what did not run. A slip is said in one line and fixed in
     every place that shares its cause, with no new mechanism built for it. Work an agent finished is landed or
     dropped, with the reason, in the same session.
- `spec/tools/check.py` and `spec/plants` change in two cases: a check reports wrongly on correct content, or it passes
  a defect met in content. A new check waits for the owner's yes.

## Playwright is the proof

Behavior is done when a Playwright test proves it in a real browser, and not before.

- A real gesture (click, type, select) and a visible outcome, on a page a developer would write. A test finds an
  element as a user does and expects a literal: no `page.evaluate()`, no read of a component's object, no DOM
  poking. The test rules are `.claude/rules/tests.md`; they load when you open a file under `tests/`.
- The test fails when the behavior breaks. Ask what defect would still pass it, plant that defect
  (`scripts/plant-defect.sh`) and read the test fail.
- Run through `scripts/playwright.sh`. A deny rule in `.claude/settings.json` refuses raw `dotnet test` on a
  Playwright project or the solution; a non-browser project runs when it is named first
  (`dotnet test tests/Alis.Reactive.ArchitectureTests`).
- A build, a typecheck and a feedback test are feedback while coding, never proof of behavior. Outside `spec/`,
  whose checks are `spec/tools/check.py`, build no other gate, coverage map, checklist or proof artifact. A done
  report names the run and the line of output that proves it, and what did not run. MVC 5 on net48 is proven only
  by the Windows job of CI (`spec/REWRITE.md`, section 2.8).

```bash
scripts/playwright.sh --project poc --filter "FullyQualifiedName~Properties"
scripts/playwright.sh --project poc
```

The POC's suites are the one browser run until U2 lands the conformance app's harness; a unit ports each proof it
replaces, and `poc/` leaves last (`spec/REWRITE.md`, section 3).

## Vision: typed access to JavaScript objects

Everything the browser runs (a Syncfusion component, a native input, a plugin, an app service like
Toast or Drawer) is a JavaScript object with public capabilities: properties (read, write), methods
(call, may return a value) and events (callbacks with payloads). The framework gives typed C# access
to those capabilities through one fluent API. Any member that returns a value can be a source
wherever the DSL takes one: reaction values, conditions, request parts, dispatch payloads and plugin
arguments.

**Components are consumers.** A component, the framework's own or an app's, is one declaration class that mirrors
the object's typed API member for member by its JavaScript names, and one render helper, built on the core's public
surface with zero TypeScript runtime changes: a component that seems to need one means the plan is missing
information, and the C# side carries it. The core names no consumer. Fusion and Native are built on the surface an
app uses and get nothing an app does not, and any developer onboards a component
(`.claude/skills/onboard-component/SKILL.md`).

## Architecture

C# builders write the plan, a closed grammar of sealed nodes; the view renders it as JSON; the TypeScript contract
is generated from the grammar; the TypeScript runtime executes the plan. There is one C# model: no semantic graph
and no lowering pass sit between the builders and the grammar. The core is one assembly, and each module is one
folder and one namespace of the same name in every tree. `spec/REWRITE.md` section 2 holds the tree, the slice
tables and the names; the module table is `spec/CLAUDE.md`'s.

- C# never executes browser behavior. The runtime never invents behavior the plan does not carry:
  what a sentence means when the page does not match its declaration (a member absent, its holder or
  its value missing, a promise where a value was declared) is stated by the C# and carried by the plan.
  A state left to a runtime default is a gap. Make invalid behavior unrepresentable in the plan domain.
- A meaning has one home: its D line in `spec/Core/DESIGN.md`, or its module's own in that module's folder under
  `spec/`. A rule file restates two, since every view meets them:
  - A member's name (D2.4 to D2.6, D2.9). A request body names each member as the endpoint's binder reads it: on
    net10 the name System.Text.Json's contract gives it under the app's JSON options, on net48 its C# name. Form-data
    and query keys are the member's C# path on both hosts. Data read from a response body or a pushed message is
    named as the endpoint writes it, under the app's JSON options. The naming is the app's: no name is camel-cased by
    a rule of the framework's own.
  - Absent data (D4.2). A member a present holder lacks reads its start, the value System.Text.Json binds for it
    under the app's options. A holder that holds null or reads nothing reads nothing, and so does every member past
    it.
- The generated contract is never hand-edited: `.claude/hooks/protect-generated.mjs` refuses the edit, and a change
  to it is a change to the C# grammar, regenerated.
- A new behavior is a builder method and the node it writes in the existing grammar. The grammar is closed
  (`spec/Core/schema.md`): a new production is the owner's decision.
- The runtime trusts framework-generated plans: no validators, fallbacks, registries or defensive
  throws for shapes the DSL controls. Errors belong at real boundaries: DOM lookup, browser APIs,
  network, component or plugin lookup, and JSON the framework did not produce. No field of the plan is optional or
  nullable, so plan data takes no default.
- Views carry no JavaScript: no inline `<script>`, no `addEventListener`, no `window.alis`.
- Every ordering of the async lanes is a TLA+ spec's (`spec/REWRITE.md`, section 2.6). A browser test holds the
  machine at one named action through the harness and never provokes a race by timing.

## Rules

- Read the spec lines and the source before an edit; old tests, docs and plans are history. Before editing, name
  the row: the spec line, the door, the node it writes, the runtime that executes it, the Playwright test.
- Typed APIs only. Analyzer ALIS009 fails the build on the whole public surface of the shipped packages and on
  every component declaration and render helper, shipped or brought: no `object`, no `System.Type`, no string that
  names a member, field, event, method or path. Strings stay for text people read, URLs, element ids the developer
  chose, header names, CSS classes, and JavaScript names at the vendor and plugin boundary. A deliberate exception
  carries the exemption `spec/Analyzers/module.md` keeps (M20.N1).
- The grammar's nodes are sealed, with `internal` constructors. Developers author through builders.
- The public surface of a shipped package is the text in its `PublicAPI/<tfm>/PublicAPI.Unshipped.txt`
  (`PublicAPI/<tfm>/PublicAPI.Shipped.txt` once released). A public symbol missing from it or a line without a symbol
  fails the build (RS0016, RS0017); a change to the surface is a reviewed diff of that file. A door is public only
  when a view author, a validator author or a component author needs it.
- Never lose typed nature or a feature. No door of either line is lost: one whose form is refused names the doors
  that carry its capability, and `spec/tools/check.py` holds features lost at zero. Code that new code replaces is
  evidence of intent, not the design: read what it does when the page does not match (each throw, guard and
  default) and give each a verdict in the replacement. "Nothing references it" is no verdict.
- No comments: code is the truth. Name the thing what it is, extract a function, guard the
  boundary. The one exception is an XML doc on the public surface of a shipped package (what its
  PublicAPI text lists): the developer's IntelliSense, written to `.claude/rules/docs.md`.
  A shebang, `#nullable` and the version Dependabot reads beside a pinned action are directives, not
  comments. `WhenCodeIsWritten` fails on any other comment, in every language the repo holds.
- Raise a gap, never invent. When a view, component or test needs a door the DSL or the vendor's
  public API lacks, name the gap with its use case; never invent a member, a syntax or a runtime
  behavior so a test passes. A member's real shape comes from the installed package or the vendor's
  source.
- Never read secrets: no `dotnet user-secrets list`, nothing that prints a secret's value, and no file that holds
  a credential, a machine's NuGet or npm configuration among them. When a secret is in doubt, say so without
  reading the store. Deny rules in `.claude/settings.json` refuse the first two; an agent's brief states all three.
- Slices are self-contained. Duplication between slices beats a shared base class.
- Root cause, not patch. A fix lands in the code that owns the meaning (the C# that states it), not
  where the symptom shows, and covers every place that shares its cause, each proven on its rung. A
  meaning is decided against these rules when a real case or one of the spec's matrices
  (`spec/Reference/`) raises it, never by analogy. After two failed attempts, stop and research. Leave
  touched files cleaner: delete private helpers no feature reaches.
- C# 14, compiled for `net48` and `net10.0`. A net48 language error means rework to what PolySharp polyfills;
  realtime is net10's alone.

## Workflow

- `main` is the trunk and the release line. A release is an annotated `vX.Y.Z[-rc.N]` tag on `main`:
  `scripts/verify-release-tag.sh` checks it and `.github/workflows/nuget-publish.yml` publishes it.
- The rewrite runs on `feature/rewrite` and lands on `main` by pull request; other work branches from
  `origin/main` in a worktree (`feature/`, `fix/`, `chore/`). A SessionStart hook installs the pre-commit guard
  that refuses commits on `main`. `git stash` is never run: `stash@{0}` holds work that stays put, and a deny rule
  refuses it.

  ```bash
  git worktree add .worktrees/<name> -b feature/<name> origin/main
  ```

- A unit of work closes the loop of Spec-first. Each exit shows in git or a run's log, never in a
  checklist or an artifact.
- A review names the branch and HEAD SHA it read. A finding counts only if its `file:line` exists
  at HEAD.
- `scripts/check.sh` (every check that needs no browser; its last line is "[check] every check passed."),
  `scripts/test.sh` (that, then every browser test), `scripts/doctor.sh` (every pin against what this machine
  runs), `scripts/build.sh`, `scripts/pack.sh <version>`. A script joins this list with the commit that writes it
  (`spec/REWRITE.md`, section 2.7).
- Every tool and package is pinned to one exact version, the latest stable when its unit lands it; a version held
  below the latest names what holds it in its row of `spec/REWRITE.md` section 2.7. Syncfusion's hold is the owner's:
  EJ2 33.1.47 with npm and NuGet pinned together; upgrade with `node scripts/pin-syncfusion.mjs <version>`.

## Local guidance

The spec is the reference for a door that is not built; the code and its XML docs are the reference for one that
is. Read the declaration, the builder and a page that uses it before writing a view, a component or a test.

Rules for one kind of file live in `.claude/rules/` and load when you open such a file: `tests.md`
(files under `tests/`), `views.md` (`.cshtml`), `csharp.md` (`.cs`), `analyzers.md` (an analyzer project
and its cases) and `docs.md` (docs, Markdown and XML docs). A folder's own `CLAUDE.md` holds what is true of that
folder; `spec/CLAUDE.md` holds the owner's decisions. This file wins on conflict. `AGENTS.md` is a symlink to this
file.
