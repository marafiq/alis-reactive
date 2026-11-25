# Alis.Reactive

Alis.Reactive lets developers author dynamic UI in ASP.NET MVC (.NET Framework 4.8 MVC 5 and .NET 10) with fluent,
fully typed C#. The compiler catches a mistake before the browser does, and defaults mean less typing.

This branch is the rewrite. `spec/` is its design, and `spec/REWRITE.md` is the repository it builds and the 27 units
it builds it in. Read its section 1 first: the layout on one screen, the kinds of proof with their commands, and the
one place of every other answer. Main, the redesign and `poc/` are evidence of features and of the mechanisms the
POCs proved, never the design: the unit that lands a door writes it from its spec lines, and where the rewrite meets
an older line the rewrite's piece is written as its spec states it and the older line gives way.

## How you work

The bar comes before the mechanics: done for a unit is "Spec-first", and each section below is one part of the bar.
The standard of one kind of file is its file under `.claude/rules/` (`tests.md`, `csharp.md`, `views.md`, `docs.md`,
`analyzers.md`), which loads with such a file; a project's own standard is its own `CLAUDE.md`. The owner's decisions
are `spec/CLAUDE.md`, verbatim in `spec/NOTES.md`; each kind of mistake and what holds it is `spec/MISTAKES.md`; every
other answer is `spec/REWRITE.md`, section 1.3. These are read before the first edit of a unit: the commands carry the
work and never make it good (the owner, 2026-11-17). The spec is the reference for a door not built, the code and its
XML docs for one that is. This file wins, and `AGENTS.md` is a symlink to it.

The process is `.claude/rules/process.md`: one state machine, whose tables `scripts/state.py` reads and prints with
the command that leaves each state. A unit starts from its brief, whoever builds it; a note of the owner's is homed
the turn it arrives. Nothing a session reads to resume lives outside the repo (the owner, 2026-11-18).
```bash
scripts/doctor.sh
. scripts/toolchain-env.sh
python3 scripts/state.py
python3 scripts/open.py
```
Builds run one at a time, with a read-only consult beside the build (the owner, 2026-11-17).
Commands on this Mac: git grep's extended expressions take no \b and no \s; zsh ties the variable path to PATH, reads a letter after a bare $var and a colon as a modifier, stops a command whose glob matches no file and runs a command held in a variable as one word, so a script is bash and writes ${var}; BSD sed's in-place edit takes ''; a tool that reads its input is started with < /dev/null.
The browser runner takes --test or --all here and no --filter. A script bash is running is never edited; a planted file is restored by cp and touch, since mv keeps the old time.

## Vision: typed access to JavaScript objects

Everything the browser runs (a Syncfusion component, a native input, a plugin, an app service like Toast or Drawer) is a
JavaScript object with properties, methods and events, and the framework gives typed C# access to them through one
fluent API; any member that returns a value is a source wherever the DSL takes one. A component, the framework's own or
an app's, is a consumer: one declaration class that mirrors the object's typed API by its JavaScript names, and one
render helper, on the core's public surface with zero TypeScript runtime changes; one that seems to need a runtime
change means the plan is missing information, and the C# side carries it. The core names no consumer, Fusion and Native
get nothing an app does not, and any developer onboards a component (`.claude/skills/onboard-component/SKILL.md`).

## What governs what

- **TLA+ governs every ordering.** Each async lane is a spec under `spec/` that TLC checks, with the mutants that must
  fail (`spec/REWRITE.md`, section 2.6). A lane changes in its spec first; a browser test holds the machine at one
  named action through the harness and never provokes a race by timing; a question a spec answers is decided from it.
  A commit whose piece meets an ordering names the invariants it rests on, and an ordering no spec holds is written as
  one before its door (the owner, 2026-11-13).
- **The schema governs the seam.** C# builders write the plan, a closed grammar of sealed nodes (`spec/Core/schema.md`);
  the view renders it as JSON; the TypeScript contract is generated from the grammar; the runtime executes it and
  invents no behavior the plan does not carry. C# states what a sentence means when the page does not match its
  declaration: a state left to a runtime default is a gap, and no field of the plan is optional or nullable. The runtime
  trusts a framework-generated plan and fails loud only at real boundaries (`Alis.Reactive.Assets/CLAUDE.md`). A new
  behavior is a builder method and the node it writes; a new production is the owner's decision. Views carry no
  JavaScript: no inline `<script>`, no `addEventListener`, no `window.alis`.
- **The map governs where a thing lives.** The module table of `spec/CLAUDE.md` names 26 modules; the core is one
  assembly, a module one folder and one namespace of the same name in every tree, its slices in its project's slice
  table (`Alis.Reactive/SliceTree.txt` is the core's). A meaning has one home, its D line in `spec/Core/DESIGN.md` or
  its module's own under `spec/`; `.claude/rules/views.md` restates the two every view meets.
- **The doors govern the surface,** each public only when an author needs it, none lost (`spec/CLAUDE.md`,
  `spec/BRIEF.md`). A shipped package's surface is its `PublicAPI/<tfm>/PublicAPI.Unshipped.txt`, changed by a reviewed
  diff. A door is typed: no `object`, no `System.Type`, no string that names a member, field, event, method or path;
  strings stay for text people read, URLs, element ids the developer chose, header names, CSS classes, and JavaScript
  names at the vendor and plugin boundary; a deliberate exception carries the exemption of M20.N1. Code a door replaces
  is evidence of intent: each throw, guard and default it holds gets a verdict in the replacement.
- **Encapsulation is the heart.** One place owns each concept's data and meaning, internal by default. The grammar's
  nodes are sealed, with `internal` constructors, and developers author through builders. No internal is exposed to a
  test or to a consumer. Slices are self-contained: duplication between slices beats a shared base class.
- **A defect is prevented on the highest rung that holds it** (`spec/REWRITE.md`, section 1.2): a shape the compiler
  already refuses gets nothing below it, its refused case compiled once to read the compiler's error (the owner, 2026-11-14).

## Spec-first

`spec/BRIEF.md` states the thesis (section 2), the closed principles (section 3) and the stability rules every test
keeps (section 10); the modules and the owner's decisions are `spec/CLAUDE.md`'s.
- A unit runs in this order: its spec lines, TLC green with each mutant failing, its Playwright tests written from
  their promises and failing, the code until they pass, each test failing on its planted defect, commit.
- From U3 on every Playwright test stands written and failing, save one that waits on a later unit: a startup call
  that throws until its unit, a host its startup builds, a door or a harness verb it builds, or a fact no spec line
  states; that unit writes it first, failing, before its code (the owner, 2026-11-14). A module's unit writes its pure
  tests from the schema's lines before the half they prove.
- Every unit closes one loop, and each exit is a line a tool printed, read when it printed:
  1. Restate the goal in your own words; a new direction waits for the owner's yes.
  2. Read the owner's decisions (`spec/CLAUDE.md`), then every file the unit touches, whole, at HEAD, and the rules
     that file states: a line enters a spec file, a rule file or a test only after it is held against each. Before
     an edit, name its row: the spec line, the door, the node it writes, the runtime that executes it, the test.
  3. Decide from the proof in hand: the TLA+ specs, the principles and the owner's decisions. A decision on a
     principle, a closed rule or the scope goes to the owner as one line: what already holds, the one thing a yes
     changes, its cost and risk, then the lead's recommendation (the owner, 2026-11-14), from a design thought through
     whole, its family of cases, alternatives and counts read at the source, never ad hoc (the owner, 2026-11-23).
  4. Change one meaning in one commit, with every place that carries it; a batch of slices a builder finished lands
     as one commit, each slice named (the owner, 2026-11-17).
  5. Prove it on the rung that holds it, once: behavior by a `scripts/playwright.sh` result line, the test first failing
     on its planted defect (`scripts/plant-defect.sh`); a rule by a refused case it reports by name and an accepted case
     one fact away, both kept; a pin by a restore and a run. A failure goes back to 4 twice at most, then research.
  6. Land through the lane of `.claude/rules/process.md`, behind the gate's `"[check] every check passed."`, or for Markdown alone its `"[gate] the Markdown path: every check passed."`.
  7. The commit says what changed, its proof lines and what did not run; a unit's last commit rewrites where the
     rewrite stands (`spec/REWRITE.md`, section 4).
- `spec/tools/check.py` and `spec/plants` change in two cases: a check reports wrongly on correct content, or it passes
  a defect met in content. A new check waits for the owner's yes.

```bash
python3 spec/tools/check.py
python3 spec/tools/check.py --orient
scripts/playwright.sh --unit U2
```

Behavior is done when a Playwright test proves it in a real browser, and not before: a real gesture and a visible
outcome, on a page a developer would write, found as a user finds it, with no `page.evaluate()` and no read of a
component's object (`.claude/rules/tests.md`); a build, a typecheck and a module's pure tests are no proof of it. Every commit of more than Markdown waits on every written test at its tree, and a test that passed at the run before and
fails stops it (the owner, 2026-11-15; for Markdown, 2026-11-24). A non-browser project runs when it is named first, as `dotnet test tests/Alis.Reactive.ArchitectureTests`.

## What is forbidden, and what refuses it

A standard is held by the tool on its rung, with a refused case it reports by name and an accepted case. A rule in
prose alone is a gap to close; a guard for a defect nobody met is not built.

| Forbidden | Refused by |
|---|---|
| A comment in any language; an XML doc off a shipped package's public surface | `WhenCodeIsWritten` |
| A rule file that names code the repo does not hold | `tests/Alis.Reactive.ArchitectureTests/WhenARuleNamesTheCode.cs` |
| A hand edit of the generated contract | `.claude/hooks/protect-generated.mjs`, held by `WhenClaudeRunsAHook` |
| A command in a shape this machine gets wrong: a `git grep` extended expression with a class it lacks, the browser runner by a filter, `grep --include`, Codex started with its input open, a variable split into words as bash splits it, a letter after a bare variable and a colon, which zsh reads as a modifier, a file restored from the index where HEAD was meant (git checkout or git restore with no commit named), a backtick outside single quotes, which runs its text as a command | `.claude/hooks/refuse-known-traps.mjs`, held by `WhenClaudeRunsAHook` |
| A commit the landing script did not make, where `scripts/land.sh` stands | the pre-commit hook `scripts/install-git-hooks.sh` installs |
| An untyped public door; a door handed `dynamic` as a type argument, a plan opened over `object`, or a call bound at run time by a `dynamic` argument; a slice that uses a later row; a namespace off its folder; an open public class; a table off its project | `ALIS009`, `ALIS044`, `ALIS010`, `ALIS011`, `ALIS012`, `ALIS013` |
| A project that holds folders, or a shipped package, built without its slice table; a friend assembly, an internal opened to a test or to a consumer | `WhenAProjectHoldsFolders`, `WhenAPackageShips`, `WhenAnInternalIsExposed` |
| JavaScript in a view: an inline script, a handler attribute, a script URL | `WhenAViewIsWritten` |
| In a shipped project, a conditional directive inside a file: one host's code is a file of its own | `WhenCodeCompilesForBothHosts` |
| A public symbol outside the PublicAPI text, or a line with no symbol; an analyzer off its release row, or one that reads a file or the environment; in the core, reflection, a compiled expression, a regular expression, a read of type information off the owner's list | the build (RS0016, RS0017, RS2000, RS1035, RS0030); `WhenTheCoreIsCompiled` |
| In a browser test: a reference, a member of Playwright or an attribute of NUnit beyond what a test takes; a wait on time; a branch or a loop; an expected value that is computed, partial, what the test typed or only an absence; an assertion in a helper; no promise or no gesture of its own | `TEST001` to `TEST013` (`spec/REWRITE.md`, section 2.10) |
| In the runtime: a default for plan data, a floating promise, a switch that misses a kind, a condition that is no boolean, a loose equality, an enum, an inline lint directive, an import of a later row of its slice table, a pure test that imports past its module's index | the lint and the TypeScript compiler, held by `WhenTheLintIsConfigured` |
| A rule with no refused or no accepted case | `WhenAnArchitectureTestIsWritten`; `spec/plants` for a check |
| A door or a feature lost; a defect below its rung; a meaning with no carrier; a kind outside the grammar; a unit that proves nothing; a path with no verdict; a pin off its version; a generated binding off the spec; a hedge word in the spec | `spec/tools/check.py`, each check with its plants |
| C# off the formatter; a private member nothing uses or reads; an unread parameter | `dotnet format --verify-no-changes` and the build, in `scripts/check.sh` |
| A package off its lock, a tool off its pin, a package with a known vulnerability | the locked restore, `scripts/doctor.sh` and both audits, npm's past the advisories the owner excuses (`WhenThePackagesAreAudited`) |
| A browser run that ran other than the spec selected, retried a test, ran one over the measured budget or left a server alive | `scripts/playwright.sh`, held by `WhenAPlaywrightRunStarts` and `WhenAPlaywrightRunEnds` |
| A raw `dotnet test` of a Playwright project or the solution; `git stash`; a listing of user secrets; a commit on `main` | the deny rules of `.claude/settings.json`; the pre-commit hook `scripts/install-git-hooks.sh` installs |

Kept by the loop, with no tool: a new direction without the owner's yes; a gap worked around by an invented member,
syntax or runtime behavior, where the gap is raised with its use case and a member's real shape is read from the
installed package; a patch where the symptom shows; a meaning decided by analogy, where no real case and no matrix of
`spec/Reference/` raises it; a gate, coverage map, checklist or proof artifact outside `spec/`; reading a file that
holds a credential, a machine's NuGet or npm configuration among them.

## Confidence

The confidence of a piece of work is determined, never felt: the result line of the command that proves its kind
(`spec/REWRITE.md`, section 1.2), printed on the committed tree and read when it printed, standing until a fact changes.
Nothing is run twice to feel safer, nothing waits on a run that gates no unit, and a question the specs, the principles
or the owner's decisions answer is decided from them and held. A report says what was run, by its line, and what was
not; MVC 5 on net48 is CI's Windows job (the owner, 2026-11-20).

## Corrections

A correction has one home: an owner's correction, and a reversal of the lead's with no new fact, become that turn a
plain line, dated and attributed, of the rule file that owns the subject (`.claude/rules/`, a project's `CLAUDE.md`,
`spec/CLAUDE.md` for the owner's decisions, this file for the plan, the loop, a session and the team). A mistake, the
lead's or an agent's, is fixed where it is found and wherever its cause is shared, then counted in its kind in
`spec/MISTAKES.md` that turn, one count or one new row (the owner, 2026-11-14). A kind that grows while its holder
stands moves the holder up a rung that turn: a line of the rule file that loads with that kind of file, then a tool that
refuses it, as Claude Code's guidance moves a line Claude keeps missing into a hook (the owner, 2026-11-23: "its not log
and forgotten, its to enforce progressive learning"). A mistake is called a mistake, never a softer word (the owner,
2026-11-13).

## Sessions

- A session starts and ends at a stable state: every unit pushed, `git ls-remote` equal to HEAD, nothing uncommitted.
  The lead is started with claude inside the lane, ".worktrees/rewrite", so this file, `.claude/settings.json`,
  `.claude/agents/` and `.claude/skills/` load from the lane. The owner starts a new session and the lead ends none; a
  reply to continue a go or no-go ask is the go, work that needs no answer never waits, and a long run is wrapped in
  `caffeinate`.
- A status is what is completed and what is open in the plan's words, by count, never where a head is; what is open is
  a piece of a unit, never the lead's own checking. Progress is units closed, the open ones before another opens (the
  owner, 2026-11-11, 2026-11-14 and 2026-11-15).
- `HANDOFF.md`, written by `scripts/push.sh` at every push, holds the head, what landed with its proof line, what is
  next and one `date`. A session ends with it and the next prompt in the clipboard, read back. To begin, and after a
  compaction: `scripts/doctor.sh`, the orient command above, `HANDOFF.md` whole, `spec/REWRITE.md` sections 1 and 4,
  the latest standing decisions and the rule file of what the unit touches. A claim stands on what is run now.
- A move-in unit reads its brief and `spec/Reference/move-in.md` first, uses the framework's landed typed doors and
  MVC's `Html.*` partials, and records a missing door with its use in part 9 of the move-in spec; a framework change is
  a unit of its own (the owner, 2026-11-19: "you can not change framework now while working in conformance").

## The team

The lead plans, briefs, reads every change at its source and lands it; an agent's or Codex's answer is a lead, read at
the source. Before any agent or Codex takes work, the lead loads `.claude/skills/delegate/SKILL.md`: which definition
a task takes, how each is started, what a brief carries, and how the work is read and landed or dropped.

## Workflow

- `main` is the trunk and the release line; a release is an annotated `vX.Y.Z[-rc.N]` tag on `main` (`spec/REWRITE.md`, 2.8).
- Every unit lands on `feature/rewrite`; its one pull request ships 1.0 to `main` (the owner, 2026-11-23). `git stash`
  is never run: `stash@{0}` holds work that stays put. Other work branches from `origin/main` in a worktree
  (`feature/`, `fix/`, `chore/`): `git worktree add .worktrees/<name> -b feature/<name> origin/main`.
- Each script is `spec/REWRITE.md`, section 2.1, joined with the commit that writes it; each pin is section 2.7, and
  Syncfusion's is the owner's (`node scripts/pin-syncfusion.mjs <version>`).
- C# 14, compiled for `net48` and `net10.0`; `Alis.Reactive/CLAUDE.md` holds the one rule for a host's own code.
