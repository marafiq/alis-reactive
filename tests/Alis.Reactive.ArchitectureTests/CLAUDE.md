# Alis.Reactive.ArchitectureTests

Rules over what the repository holds. Each fixture is one rule's situation, `When` and what is written, and holds
three kinds of test: a `[Test]` that runs the rule over the tracked repository (`Repository.cs` lists what git
tracks), a `RefusedCaseAttribute` case that runs it over text the case writes and expects each refusal by name, and
an `AcceptedCaseAttribute` case one fact away that expects none.

## The standard

- A scan of a clean repository passes whether its rule works or not, so a fixture that holds a rule holds a case of
  each mark; `WhenAnArchitectureTestIsWritten` refuses one that does not.
- A rule reads C# through Roslyn, a script through `read-scripts.mjs`, a project file as XML and JSON through a
  reader. It never reads structure with a pattern over source text.
- A rule fires on a real defect alone: it pins no wording a legitimate edit moves, and its refusal names the file,
  the line and the cure.
- A rule reads tracked files, so a unit is staged before the rules run on it.
- A defect planted in a rule's reader fails a case named for it; one that no case fails on names a missing case.
- `LintCases` holds the lint's refused and accepted files, which `WhenTheLintIsConfigured` lints.

```bash
dotnet test tests/Alis.Reactive.ArchitectureTests
```
