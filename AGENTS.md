# Agent Guidance

Primary architecture rule: DSL -> Rich Plan Domain -> Generated Rich TS Contract -> Runtime Executioner.

Blueprint-first rule: do not make plan-model or runtime refactor choices from the current helper code alone. Before editing a module, ground the change in the frozen DSL source and the source blueprint/matrix (`docs/reactive-plan-source-blueprint.md`, `docs/reactive-dsl-feature-atlas.md`). If the change cannot be traced as `DSL API -> developer intent -> domain concept -> JSON/TS term -> runtime execution`, do not make the edit.

The public DSL is frozen except plugin improvements. Read the DSL before changing the plan model or runtime. Do not infer missing behavior from runtime code when the DSL already expresses the intent.

Module closure rule: touching a module means closing that module, not leaving a local improvement surrounded by stale names, stale tests, or stale docs. A module is not done until the DSL row, C# domain terms, generated TypeScript contract, runtime executor vocabulary, behavior tests, and Playwright-facing behavior all use the same language and prove the same intent.

Drift guard: the source DSL is the requirement, not samples, XML docs, old unit tests, the current runtime, or remembered clues. Before coding a surface, update the blueprint row that proves its input/output path. The proof shape is `source file + DSL input -> rich domain output -> JSON/TS term -> runtime output`. If the row cannot be written from source, keep reading source instead of editing locally.

Stale-work guard: after every rename or concept change, audit the surrounding source, generated contract, runtime tests, Playwright tests, blueprint, and glossary for the old term. Do not let one corrected slice make another slice silently wrong.

Doubt rule: whenever there is even slight doubt about a behavior, name, edge case, or module boundary, stop inferring and go back to the actual DSL source. Add or correct the input/output matrix row before changing code.

Rich domain model is not permission to invent fluff. Add a concept only when it directly names a real DSL behavior and makes code simpler. Delete wrappers that only carry parameters, rename branches, or need explanation to justify their existence.

Design-before-edit rule: if the current change depends on cross-module behavior such as conditions mixed with HTTP, gather sources, validation, component events, or partial load/unload, first walk the DSL input through the domain output, JSON/TS term, and runtime effect. Local edits without that walk are fake progress.

Runtime code executes framework-generated plans. Do not add defensive preflight, rollback, fallback, or speculative recovery for impossible bad plans. Put invalid behavior in the C# PlanModel where it can be made unrepresentable. Runtime checks are for real external boundaries only: DOM lookup, browser API failure, network, and malformed non-framework input.

Do not model normal execution bookkeeping as validation, claims, rejects, lifecycle gates, or registries. If the server-generated plan says source A is assigned to target B, the runtime reads A and writes B. Bookkeeping names should describe what is remembered for execution or unload, not imply the plan is suspicious.

Tests are a thinking tool. Write behavior tests that prove DSL intent becomes deterministic plan/runtime behavior. Do not write tests around helper classes or invented abstractions.

Partial plans are simple: boot composes plan documents by `planId`; browser injection replaces or unloads the declared partial slot. Component ids and type keys remain runtime join keys. Slot identity is only the handle for removing the state loaded into that slot.

When in doubt, delete code before adding code. If a concept does not directly map to frozen DSL intent or a browser boundary the runtime must touch, it is probably noise.
