# Alis.Reactive

Alis.Reactive lets developers author dynamic UI in ASP.NET MVC (.NET Framework 4.8 MVC 5 and .NET 10) with fluent,
fully typed C#. The compiler catches a mistake before the browser does, and defaults mean less typing.

This branch is the rewrite. `spec/` is its design, and `spec/REWRITE.md` is the repository it builds and the 27 units
it builds it in. Read its section 1 first: the layout on one screen, the kinds of proof with their commands, and the
one place of every other answer. Main, the redesign and `poc/` are evidence of features and of the mechanisms the
POCs proved, never the design: the unit that lands a door writes it from its spec lines, and where the rewrite meets
an older line the rewrite's piece is written as its spec states it and the older line gives way.

## Vision: typed access to JavaScript objects

Everything the browser runs (a Syncfusion component, a native input, a plugin, an app service like Toast or Drawer)
is a JavaScript object with properties, methods and events. The framework gives typed C# access to them through one
fluent API, and any member that returns a value is a source wherever the DSL takes one. A component, the framework's
own or an app's, is a consumer: one declaration class that mirrors the object's typed API member for member by its
JavaScript names, and one render helper, built on the core's public surface with zero TypeScript runtime changes. A
component that seems to need one means the plan is missing information, and the C# side carries it. The core names
no consumer, Fusion and Native get nothing an app does not, and any developer onboards a component
(`.claude/skills/onboard-component/SKILL.md`).

## What governs what

- **TLA+ governs every ordering.** Each async lane is a spec under `spec/` that TLC checks, with the mutants that must
  fail (`spec/REWRITE.md`, section 2.6). A lane changes in its spec first, a browser test holds the machine at one
  named action through the harness and never provokes a race by timing, and a question a spec answers is decided
  from it. A commit whose piece meets an ordering names the invariants it rests on, and an ordering no spec holds
  is written as one before its door (the owner, 2026-11-13: "I'm not seeing any traces of it").
- **The schema governs the seam.** C# builders write the plan, a closed grammar of sealed nodes
  (`spec/Core/schema.md`); the view renders it as JSON; the TypeScript contract is generated from the grammar; the
  runtime executes it and invents no behavior the plan does not carry. C# states what a sentence means when the page
  does not match its declaration: a state left to a runtime default is a gap, and no field of the plan is optional
  or nullable. The runtime trusts a framework-generated plan and fails loud only at real boundaries: DOM lookup,
  browser APIs, the network, component or plugin lookup, and JSON the framework did not produce. A new behavior is a
  builder method and the node it writes; a new production is the owner's decision. Views carry no JavaScript: no
  inline `<script>`, no `addEventListener`, no `window.alis`.
- **The map governs where a thing lives.** The module table (`spec/CLAUDE.md`) names 26 modules. The core is one
  assembly; a module is one folder and one namespace of the same name in every tree; a slice uses only the rows
  before its own in its project's slice table (`Alis.Reactive/SliceTree.txt` is the core's). A meaning has one home,
  its D line in `spec/Core/DESIGN.md` or its module's own under `spec/`; `.claude/rules/views.md` restates the two every
  view meets, a member's name and absent data.
- **The doors govern the surface.** A door is public only when a view author, a validator author or a component
  author needs it. A shipped package's surface is the text in its `PublicAPI/<tfm>/PublicAPI.Unshipped.txt`, and a
  change to it is a reviewed diff of that file. A door is typed: no `object`, no `System.Type`, no string that names a
  member, field, event, method or path; strings stay for text people read, URLs, element ids the developer chose,
  header names, CSS classes, and JavaScript names at the vendor and plugin boundary, and a deliberate exception
  carries the exemption of M20.N1. No door of either line is lost: one whose form is refused names the doors that
  carry its capability. Code that new code replaces is evidence of intent: each throw, guard and default it holds
  gets a verdict in the replacement, and "nothing references it" is none.
- **Encapsulation is the heart.** One place owns each concept's data and meaning, internal by default. The grammar's
  nodes are sealed, with `internal` constructors, and developers author through builders. No internal is exposed to a
  test or to a consumer. Slices are self-contained: duplication between slices beats a shared base class.
- **A defect is prevented on the highest rung that holds it:** the type, an analyzer, a refusal where the view
  renders, then behavior in the browser. A defect a type or an analyzer prevents gets no browser test.

## Spec-first

`spec/BRIEF.md` states the thesis (section 2), the closed principles (section 3) and the stability rules every test
keeps (section 10); the modules and the owner's decisions are `spec/CLAUDE.md`'s.
- A unit runs in this order: its spec lines, TLC green with each mutant failing, its Playwright tests written from
  their promises and failing, the code until they pass, each test failing on its planted defect, commit.
- From U3 on every Playwright test stands written and failing, save one that waits on a later unit: a startup call
  that throws until its unit, a host its startup builds, a door or a harness verb it builds, or a fact no spec line
  states; that unit writes it first, failing, before its code (the owner, 2026-11-14). A module's unit writes its pure
  tests from the schema's lines before the half they prove, and turns green the browser tests `spec/tools/check.py`
  lands in it.
- The server decides when async work happens and the test releases it through the harness; a test waits for the
  state it names, never for time, in a world of its own, and asserts literal outcomes.
- Every unit closes one loop, and each exit is a line a tool printed, read when it printed:
  1. Restate the goal in your own words; a new direction waits for the owner's yes.
  2. Read the owner's decisions (`spec/CLAUDE.md`), then every file the unit touches, whole, at HEAD, and the rules
     that file states: a line enters a spec file, a rule file or a test only after it is held against each. Before
     an edit, name its row: the spec line, the door, the node it writes, the runtime that executes it, the test.
  3. Decide from the proof in hand: the TLA+ specs, the principles and the owner's decisions. A decision on a
     principle, a closed rule or the scope goes to the owner as one line with its cost and its risk; no other does.
     The line opens with what already holds and the one thing a yes changes, then the lead's recommendation; code
     follows it (the owner, 2026-11-14: "you made it confusing for me").
  4. Change one meaning in one commit, with every place that carries it.
  5. Prove it on the rung that holds it, once. Behavior: a `scripts/playwright.sh` result line, the test first failing
     on its planted defect (`scripts/plant-defect.sh`). A rule (an analyzer, a test rule, a lint rule, an architecture
     test, a check): a refused case it reports by name and an accepted case one fact away, both kept in the repo.
     A pin: a restore or an install, and a run. A failure goes back to 4, twice at most; then stop and research.
  6. Stage the unit by path and run `scripts/check.sh --no-spec`; commit behind its last line,
     `"[check] every check passed."`; run the full check once at that head; push behind its first line, then the
     unit's STATE. `git ls-remote` equals HEAD after each push.
  7. The commit says what changed, its proof lines and what did not run, and a unit's last commit rewrites where
     the rewrite stands (`spec/REWRITE.md`, section 4). A mistake is said in one line and fixed in every place
     that shares its cause, with no new mechanism built for it.
- `spec/tools/check.py` and `spec/plants` change in two cases: a check reports wrongly on correct content, or it passes
  a defect met in content. A new check waits for the owner's yes.

```bash
python3 spec/tools/check.py
python3 spec/tools/check.py --orient
scripts/playwright.sh --unit U2
```

Behavior is done when a Playwright test proves it in a real browser, and not before: a real gesture and a visible
outcome, on a page a developer would write. A test finds an element as a user does and expects a literal: no
`page.evaluate()`, no read of a component's object, no DOM poking (`.claude/rules/tests.md`). A build, a typecheck and
a module's pure tests are no proof of behavior. A non-browser project runs when it is named first
(`dotnet test tests/Alis.Reactive.ArchitectureTests`). A unit runs the tests the spec lands in it (`--unit`) and, before
its commit, every test landed through it (`--through`). `poc/playwright.sh` runs the POC's suites, and `poc/` leaves last.

## What is forbidden, and what refuses it

A standard is held by the tool on its rung, with a refused case it reports by name and an accepted case. A rule in
prose alone is a gap to close; a guard for a defect nobody met is not built.

| Forbidden | Refused by |
|---|---|
| A comment in any language; an XML doc off a shipped package's public surface | `WhenCodeIsWritten` |
| A rule file that names code the repo does not hold | `tests/Alis.Reactive.ArchitectureTests/WhenARuleNamesTheCode.cs` |
| A hand edit of the generated contract | `.claude/hooks/protect-generated.mjs`, held by `WhenClaudeRunsAHook` |
| An untyped public door | `ALIS009` |
| A slice that uses a later row; a namespace off its folder; an open public class; a table off its project | `ALIS010`, `ALIS011`, `ALIS012`, `ALIS013` |
| A project that holds folders, or a shipped package, built without its slice table | `WhenAProjectHoldsFolders`, `WhenAPackageShips` |
| A friend assembly: an internal opened to a test or to a consumer | `WhenAnInternalIsExposed` |
| JavaScript in a view: an inline script, a handler attribute, a script URL | `WhenAViewIsWritten` |
| In a shipped project, a conditional directive inside a file: one host's code is a file of its own | `WhenCodeCompilesForBothHosts` |
| A public symbol outside the PublicAPI text, or a line with no symbol; an analyzer off its release row, or one that reads a file or the environment; in the core, reflection, a compiled expression, a regular expression, a read of type information off the owner's list | the build (RS0016, RS0017, RS2000, RS1035, RS0030); `WhenTheCoreIsCompiled` |
| In a browser test: a reference, a member of Playwright or an attribute of NUnit beyond what a test takes; a wait on time; a branch or a loop; an expected value that is computed, partial, what the test typed or only an absence; an assertion in a helper; no promise or no gesture of its own | `TEST001` to `TEST013` (`spec/REWRITE.md`, section 2.10) |
| In the runtime: a default for plan data, a floating promise, a switch that misses a kind, a condition that is no boolean, a loose equality, an enum, an inline lint directive, an import of a later row of its slice table, a pure test that imports past its module's index | the lint and the TypeScript compiler, held by `WhenTheLintIsConfigured` |
| A rule with no refused or no accepted case | `WhenAnArchitectureTestIsWritten`; `spec/plants` for a check |
| A door or a feature lost; a defect below its rung; a meaning with no carrier; a kind outside the grammar; a unit that proves nothing; a path with no verdict; a pin off its version; a generated binding off the spec; a hedge word in the spec | `spec/tools/check.py`, each check with its plants |
| C# off the formatter; a private member nothing uses or reads; an unread parameter | `dotnet format --verify-no-changes` and the build, in `scripts/check.sh` |
| A package off its lock, a tool off its pin, a package with a known vulnerability | the locked restore, `scripts/doctor.sh` and both audits |
| A browser run that ran other than the spec selected, retried a test, ran one over the measured budget or left a server alive | `scripts/playwright.sh`, held by `WhenAPlaywrightRunStarts` and `WhenAPlaywrightRunEnds` |
| A raw `dotnet test` of a Playwright project or the solution; `git stash`; a listing of user secrets; a commit on `main` | the deny rules of `.claude/settings.json`; the pre-commit hook `scripts/install-git-hooks.sh` installs |

Kept by the loop, with no tool: a new direction without the owner's yes; a gap worked around by an invented member,
syntax or runtime behavior, where the gap is raised with its use case and a member's real shape is read from the
installed package; a patch where the symptom shows; a meaning decided by analogy, where no real case and no matrix of
`spec/Reference/` raises it; a gate, coverage map, checklist or proof artifact outside `spec/`; reading a file that
holds a credential, a machine's NuGet or npm configuration among them.

## Confidence

The confidence of a piece of work is determined, never felt: it is the result line of the command that proves its
kind (`spec/REWRITE.md`, section 1.2), printed on the committed tree and read when it printed, and it stands until a
fact changes. Nothing is run twice to feel safer, nothing waits on a run that gates no unit, and a question the TLA+
specs, the principles or the owner's decisions answer is decided from them and held. A report says what was run, by
its line, and what was not: MVC 5 on net48 is run by CI's Windows job when the rewrite is done, and this Mac is the
loop until then. Drift is stopped where it starts: one meaning changes in one commit, with every place it is in.

## Sessions

- A session starts and ends at a stable state: every unit pushed, `git ls-remote` equal to HEAD, nothing uncommitted.
  The owner starts a new session; the lead ends none, and a reply to continue, given to a go or no-go ask, is the go:
  work that needs no answer never waits. A status to the owner says what is completed and what is open, in the plan's
  own words, never where a head is (the owner, 2026-11-11). It says what works, how many of the plan's units are left
  and what of the unit in work is left, each by its count, and a TLA+ run by its spec, what it reads and its count.
  What is open is a piece of a unit; the lead's own checking is no handoff's next and holds no finished piece (the
  owner, 2026-11-14: "you keep saying that it's open when your behavior must be finishing the rewrite", then "you can
  keep reporting [TLA+], but you gotta be specific what you're saying and how many units are left"). Work is pushed
  when a unit lands and when a session ends, each push behind the full check; a commit between pushes is behind the
  gate. A long run is wrapped in `caffeinate`.
- A handoff is written after every push, since the harness compacts without warning. It holds the head, what landed
  with its proof line, what is next, and no time but one `date` printed then (the lead's mistake, 2026-11-11);
  the lead's memory names its folder, and a session ends with it and the next prompt in the clipboard, read back.
- To begin: `scripts/doctor.sh`, then the orient command above, which prints the branch against its remote, what is
  uncommitted, the last committed STATE, the owner's latest decisions and the handoff. Then read this file,
  `spec/REWRITE.md` sections 1 and 4, `spec/CLAUDE.md`'s standing decisions, the latest first, and the rule file of
  what the unit touches. Section 4 says where the rewrite stands and which unit is next.
- To reorient after a compaction: read what the compact hook printed, the same orient output, whole, then the owner's
  latest decisions and the rule file of the file in hand, before the next edit. A claim stands on what is run now.

## Corrections

A correction has one home, where the work happens. An owner's correction and a reversal of the lead's with no new fact
each become, the turn they happen, a line of the rule file that owns the subject: a test's in `.claude/rules/tests.md`
or its project's own file, an analyzer's in `.claude/rules/analyzers.md`, C#'s in `.claude/rules/csharp.md` or its
project's own file, the runtime's in `Alis.Reactive.Assets/CLAUDE.md`, the spec's and the owner's decisions in
`spec/CLAUDE.md`, the plan's, the loop's, a session's and a fork's here. The line states the rule as a plain fact,
with its date and whose words it is. A mistake, the lead's, a fork's or an agent's, is fixed where it is found and
counted in its kind in `spec/MISTAKES.md` the same turn: one count, or one new row, and nothing more (the owner,
2026-11-14: "Mistakes are supposed to be caught very quickly, not that they become their own maintenance"). A kind
whose count keeps growing gets one fix, in the tool or the rule file that owns it. A mistake is called a mistake, in a
rule line, a log entry, a commit and a message to the owner, never by a softer word (the owner, 2026-11-13: "know that
slip is about you invented so you can avoid accountability about mistakes").

## Forks, agents and reviews

- A fork or an agent takes one bounded task that has a spec line, a landed exemplar and a command that proves it: one
  test rule to a landed rule's pattern, one module's half against the schema. Three at a time at most. None decides or
  words a rule. Its brief names the base SHA and carries the full bar, finished before anything reads it, and every
  spec file, D line and TLA+ action it cites is found at that SHA by a command before it goes out (the lead's mistake,
  2026-11-14); it reads no secret or file that holds a credential, runs no git command beyond its own worktree and
  branch, and runs no command its brief does not name (the lead's correction of a fork, 2026-11-13).
- An agent starts from a definition of `.claude/agents/`, which carries the harness its kind of task meets and the
  reply it returns: `.claude/agents/designer.md` for a complex task or a critical decision, on the latest Fable model
  at xhigh, and `.claude/agents/page-writer.md` for pages and their tests. It reads this file, `spec/CLAUDE.md`'s
  owner decisions, the kinds of `spec/MISTAKES.md`, and the rule file and the project's own `CLAUDE.md` of each kind
  of file it reads, writes or proposes, and its reply opens with the rule lines it applied, by file and line at the
  base SHA, which the lead reads at the source before a word of it lands. A brief adds the task alone, and a task no
  definition covers gets its definition first (the owner, 2026-11-13: "Are you making your agents actually read the
  harness, like all the setup we have, or you are just going, do whatever you want?", and the owner's yes to the
  definitions the same night). A definition loads as an agent type from the `.claude/agents/` of the folder the
  session starts in, and a session started in the main checkout lists the architect's alone; until the owner chooses
  where the definitions load, a brief names its definition file as the agent's first read and the call passes the
  model and effort the definition names (the lead's mistake, 2026-11-14).
- A complex task or a critical decision, one whose error would drift the schema, spread across tests, a harness door
  or a spec line, or lower the bar, goes first to an agent on the latest Fable model at xhigh effort, briefed with its
  spec lines, the files and the constraints; the lead lands it from its answer, read at the source (the owner,
  2026-11-13: "for complex tasks, use fabe xhigh, also for critical decisions use fable xhigh"). While the owner is
  away, a decision the work needs is made so: by quality, from the TLA+ specs, the agent's answer read at the source,
  and never by a shortcut around the spec (the owner, 2026-11-14: "if a decision is needed, Quality and fable xhigh
  will decide with tla+ - step back and do the right - never do the hack"). One such agent runs at a time, on the
  task the specs leave open; what a spec line and a landed exemplar already shape, an analyzer or a test among
  them, the lead writes itself (the owner, 2026-11-14: "U r overusing fable").
- The lead runs the task's proof command once, where the result lands, and lands or drops the work in the same
  session, with the reason. A task whose check costs what the work costs is the lead's own.
- No review is planned behind a writer. A review that runs is bounded before it starts: a fixed list of questions,
  each with the answer that passes; a finding is an answer that fails; one fix in the page it was read from, one
  second reading, then it ends, and what still fails, or anything raised outside the list, is one line to the owner.
  It names the branch and HEAD SHA it read, and a finding counts only if its `file:line` exists at HEAD.

## Workflow

- `main` is the trunk and the release line. A release is an annotated `vX.Y.Z[-rc.N]` tag on `main`:
  `scripts/verify-release-tag.sh` checks it and `.github/workflows/nuget-publish.yml` publishes it.
- The rewrite runs on `feature/rewrite` and lands on `main` by pull request; other work branches from `origin/main` in
  a worktree (`feature/`, `fix/`, `chore/`): `git worktree add .worktrees/<name> -b feature/<name> origin/main`. A
  SessionStart hook installs the pre-commit guard. `git stash` is never run: `stash@{0}` holds work that stays put.
- `scripts/check.sh` (every check that needs no browser), `scripts/test.sh` (that, then the browser tests),
  `scripts/doctor.sh` (every pin against this machine), `scripts/build.sh`, `scripts/run.sh` (the conformance app at
  http://localhost:5220), `scripts/pack.sh <version>`. A script joins this list with the commit that writes it.
- Every tool and package is pinned to one exact version, the latest stable when its unit lands it; a hold names its
  reason in `spec/REWRITE.md` section 2.7, and Syncfusion's is the owner's (`node scripts/pin-syncfusion.mjs <version>`).
- C# 14, compiled for `net48` and `net10.0`; `Alis.Reactive/CLAUDE.md` holds the one rule for a host's own code.

## Local guidance

The spec is the reference for a door that is not built; the code and its XML docs are the reference for one that is.
Rules for one kind of file live in `.claude/rules/` and load with such a file: `tests.md` (`tests/`), `views.md`
(`.cshtml`), `csharp.md` (`.cs`), `analyzers.md` (an analyzer project and its cases) and `docs.md` (docs). A project's
own `CLAUDE.md` holds its standard, its command and what it refuses. This file wins. `AGENTS.md` is a symlink to it.
