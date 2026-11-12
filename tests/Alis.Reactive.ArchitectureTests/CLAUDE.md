# Alis.Reactive.ArchitectureTests

Rules over what the repository holds. Each fixture is one rule's situation, `When` and what is written, and holds
three kinds of test: a `[Test]` that runs the rule over the tracked repository (`Repository.cs` lists what git
tracks), a `RefusedCaseAttribute` case that runs it over text the case writes and expects each refusal by name, and
an `AcceptedCaseAttribute` case one fact away that expects none.

## The standard

- A scan of a clean repository passes whether its rule works or not, so a fixture that holds a rule holds a case of
  each mark; `WhenAnArchitectureTestIsWritten` refuses one that does not.
- A rule reads C# through Roslyn and a script through `read-scripts.mjs`, never by a pattern over their text; a
  project file as XML and JSON through a reader; markup, a stylesheet and a shell script by a scanner that knows
  their quoting.
- A rule fires on a real defect alone: it pins no wording a legitimate edit moves, and its refusal names the file,
  the line and the cure.
- A rule reads tracked files, so a unit is staged before the rules run on it.
- A defect planted in a rule's reader fails a case named for it; one that no case fails on names a missing case.
  The build refuses an unread member or parameter, so a planted defect that leaves one does not compile and proves
  nothing: it is redone as a mutation that compiles. A copy the sweep restores is touched, since the build reads a
  file's time, and a stale build passes for the restored code (the lead's slips, 2026-11-11).
- `LintCases` holds the lint's refused and accepted files, which `WhenTheLintIsConfigured` lints.
- `WhenAPlaywrightRunStarts` and `WhenAPlaywrightRunEnds` run the browser runner's two readers, the selection and the
  result, over the tables and TRX files their cases write, and expect each refusal's own line.

```bash
dotnet test tests/Alis.Reactive.ArchitectureTests
```
