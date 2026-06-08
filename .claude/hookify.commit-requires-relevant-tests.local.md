---
name: commit-requires-relevant-tests
enabled: false
event: bash
pattern: git\s+commit
---

**Tests must pass before committing**

Before any commit, run the relevant gate for the changed area:

- **Changed runtime TS (`Alis.Reactive.Assets/runtime/`)** → `npm test`
- **Changed framework C# or views** → `bash scripts/playwright.sh` (the C# + page-behavior proof), or the full `bash scripts/test.sh`

Skip only if the user explicitly says to commit without tests.
