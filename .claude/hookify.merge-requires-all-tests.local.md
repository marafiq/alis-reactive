---
name: merge-requires-all-tests
enabled: true
event: bash
pattern: git\s+merge|gh\s+pr\s+merge
action: block
---

**BLOCKED: Merge requires ALL tests passing + explicit user permission**

Before any merge:

1. **Run the FULL test suite** (all 1,731+ tests):
   - `npm test` (TS unit tests)
   - `dotnet test tests/Alis.Reactive.UnitTests`
   - `dotnet test tests/Alis.Reactive.Native.UnitTests`
   - `dotnet test tests/Alis.Reactive.Fusion.UnitTests`
   - `dotnet test tests/Alis.Reactive.FluentValidator.UnitTests`
   - `dotnet test tests/Alis.Reactive.PlaywrightTests`
2. **Every single test must pass.** No exceptions.
3. **Ask the user for explicit permission** before executing the merge. Never merge without confirmation.
