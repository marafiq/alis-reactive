# Fusion Onboarding Artifacts

This is the committed root for deterministic Fusion onboarding and audit
artifacts.

Each component writes durable evidence under:

```text
tools/FusionOnboarding/wwwroot/onboarding/fusion/{componentName}/
```

Use `.claude/skills/onboard-fusion-component/SKILL.md` for the required stage
order. Do not place new workflow artifacts under `tools/SyncfusionOnboarding`;
that older tree is not authoritative unless a current proof pass validates a
specific file as vendor evidence.

Create or refresh Stage 1 inventory artifacts with:

```bash
node .claude/skills/onboard-fusion-component/scripts/inventory-fusion-components.mjs --write
```

Inventory artifacts are not API proof. They only record current repo surfaces
before raw EJ2 discovery, primitive mapping, implementation, and behavior proof.
