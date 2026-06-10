---
name: fusion-artifact-sync
enabled: false
event: bash
pattern: git\s+commit
---

**Fusion onboarding artifact sync must be checked before commits that touch Fusion rows.**

This hook is intentionally narrow. It applies only when the staged change set
includes one of these paths:

- `.claude/skills/onboard-fusion-component/`
- `Alis.Reactive.Fusion/Components/Fusion*/`
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/`
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/Fusion/`
- `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/`
- `tools/FusionOnboarding/wwwroot/onboarding/fusion/`

For each changed component artifact root, run:

```bash
node .claude/skills/onboard-fusion-component/scripts/verify-fusion-artifact-gates.mjs --component <component>
```

The gate must be allowed to fail closed while a component audit is incomplete.
Do not bypass a stale generated matrix, missing count summary, missing proof
TRX, or unlinked required artifact by editing the hook. Fix the row artifacts
or keep the work uncommitted.
