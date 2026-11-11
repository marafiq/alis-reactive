# Alis.Reactive.TestRules.AnalyzerTests

Every test rule's cases. A case is whole C# source, a browser test or a file of its harness, run through one rule by
`Case.cs` on Microsoft.CodeAnalysis.Testing; it compiles against Playwright, NUnit and the world contract, as a
Playwright project does, and against nothing else. `.claude/rules/analyzers.md` holds the standard of a case.

- A case is a test marked `RefusedCaseAttribute` or `AcceptedCaseAttribute`. A refused case marks the reported text
  and expects the id, the severity and the message as a literal sentence; an accepted case differs from a refused
  one by one fact and expects no report.
- A fixture is named `When` and its situation, one for each rule; one accepted case of each holds source that does
  not compile, and a rule that reads a type of Playwright, NUnit or the world contract has a case without it.
- A rule is done when each defect planted in it fails a case named for that defect, on a copy of the projects
  outside the worktree. A defect planted in a project file leaves its trace in the project's lock file: both are
  put back.

```bash
dotnet test tests/Alis.Reactive.TestRules.AnalyzerTests
```
