---
name: merge-requires-all-tests
enabled: false
event: bash
pattern: git\s+merge|gh\s+pr\s+merge
action: block
---

**BLOCKED: Merge requires ALL tests passing + explicit user permission**

Before any merge:

1. **Run the full gate** — `bash scripts/test.sh` (typecheck → build:all → npm test → dotnet build → Playwright).
2. **Every single test must pass.** No exceptions.
3. **Ask the user for explicit permission** before executing the merge. Never merge without confirmation.
