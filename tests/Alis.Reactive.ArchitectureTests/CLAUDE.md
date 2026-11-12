# Alis.Reactive.ArchitectureTests

Rules over what the repository holds. Each fixture is one rule's situation, `When` and what is written, and holds
three kinds of test: a `[Test]` that runs the rule over the tracked repository (`Repository.cs` lists what git
tracks) or over the conformance app as built, a `RefusedCaseAttribute` case that runs it over what the case writes,
text or a small app, and expects each refusal by name, and an `AcceptedCaseAttribute` case one fact away that
expects none.

## The standard

- A scan of a clean repository passes whether its rule works or not, so a fixture that holds a rule holds a case of
  each mark; `WhenAnArchitectureTestIsWritten` refuses one that does not.
- A rule reads C# through Roslyn and a script through `read-scripts.mjs`, never by a pattern over their text; a
  project file as XML and JSON through a reader; markup, a stylesheet and a shell script by a scanner that knows
  their quoting.
- A rule fires on a real defect alone: it pins no wording a legitimate edit moves, and its refusal names where the
  defect is and the cure: the file and the line of what is tracked, the member or the address of what the app holds.
- A rule reads tracked files, so a unit is staged before the rules run on it.
- A defect planted in a rule's reader fails a case named for it; one that no case fails on names a missing case.
  The build refuses an unread member or parameter, so a planted defect that leaves one does not compile and proves
  nothing: it is redone as a mutation that compiles. A copy the sweep restores is touched, since the build reads a
  file's time, and a stale build passes for the restored code (the lead's slips, 2026-11-11). The copy is built and
  read green before the first plant, with what the fixture runs beside the code, `node_modules` among it: a plant
  on a copy that already fails proves nothing (the lead's slip, 2026-11-11).
- `LintCases` holds the lint's refused and accepted files, which `WhenTheLintIsConfigured` lints; `LintCases/rows` is
  a slice table of its own, for the import rows.
- `WhenAnAppRendersItsPages` and `WhenTheAppHoldsNoStaticState` read the conformance app as built, which this project
  references for the two of them. The first runs the app in this process and opens every address it answers to a
  GET, and every link its pages hold, as a browser opens a document; its cases sweep an app the case maps. The
  second reads every static of the types the app's authors wrote; its cases read an assembly built from their text.
  What the app holds as it runs is read from its assembly, the one place a view's and a generator's statics are.
- `WhenTheCoreIsCompiled` compiles the core's tracked C# as its net10 host does, against the references this
  project's build writes beside the tests, and holds every read of type information to the owner's list, by file
  and member; its cases compile small sources the same way. Its last two cases run the build's own analyzer over
  `Alis.Reactive/BannedSymbols.txt`, so an entry that names no symbol fails the case of its use.
- `WhenThePlanContractIsGenerated` runs the contract generator's reader, which this project references for it, over
  small grammars its cases declare, and expects each refusal's own words and the TypeScript and the schema of an
  accepted one as literals. The core's own contract is held by the first step of `scripts/check.sh`.
- `WhenAPlaywrightRunStarts` and `WhenAPlaywrightRunEnds` run the browser runner's two readers, the selection and the
  result, over the tables and TRX files their cases write, and expect each refusal's own line.

```bash
dotnet test tests/Alis.Reactive.ArchitectureTests
```
