# Alis.Reactive.TestRules

The browser tests' own compiler rules, TEST001 to TEST013: each is one row of `spec/REWRITE.md` section 2.10 that the
build of a Playwright project runs, so a rule of `.claude/rules/tests.md` is refused where the test is written and
never left to a reader. The project is never packed, and it stands apart from the framework's analyzers, which ship.

## The standard

`.claude/rules/analyzers.md` holds how a rule is written and proven, and loads with any file here. What this project
adds:
- A rule's row and its reading are in section 2.10: what it refuses, what a test method, a file of the harness, a
  constant and a gesture are, and where it reports. A rule decides nothing that section does not state; a reading
  the section lacks is written there first.
- A rule reads what its row names and no more. `TestCode.cs` holds what every rule reads alike: what a test method
  is, which file is the harness's, which member is Playwright's. `Taken.cs` holds section 2.10's closed lists once,
  as data, each member by its interface and its name; a rule never writes a list of its own.
- A Playwright project takes the rules by importing `Alis.Reactive.TestRules.targets` after its last reference; the
  file adds the rules as an analyzer and hands the compiler the project's own references, which `TEST001` reads.
- A rule's cases are in `tests/Alis.Reactive.TestRules.AnalyzerTests`, a refused case for each message and an
  accepted case one fact away for each condition.

## Commands

```bash
dotnet build tests/Alis.Reactive.TestRules -c Release
dotnet test tests/Alis.Reactive.TestRules.AnalyzerTests
dotnet build tests/Alis.Reactive.Conformance.PlaywrightTests -c Release
```
