---
name: commit-requires-relevant-tests
enabled: true
event: bash
pattern: git\s+commit
---

**Tests must pass before committing**

Before any commit, run the relevant test suites for changed files:

- **Changed `Alis.Reactive/`** → `dotnet test tests/Alis.Reactive.UnitTests`
- **Changed `Alis.Reactive.Native/`** → `dotnet test tests/Alis.Reactive.Native.UnitTests`
- **Changed `Alis.Reactive.Fusion/`** → `dotnet test tests/Alis.Reactive.Fusion.UnitTests`
- **Changed `Alis.Reactive.FluentValidator/`** → `dotnet test tests/Alis.Reactive.FluentValidator.UnitTests`
- **Changed `Scripts/`** → `npm test`

Skip only if the user explicitly says to commit without tests.
