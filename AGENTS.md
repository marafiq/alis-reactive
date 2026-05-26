# Agent Guidance

Primary architecture rule: DSL -> Rich Plan Domain -> Generated Rich TS Contract -> Runtime Executioner.

The public DSL is frozen except plugin improvements. Read the DSL before changing the plan model or runtime. Do not infer missing behavior from runtime code when the DSL already expresses the intent.

Rich domain model is not permission to invent fluff. Add a concept only when it directly names a real DSL behavior and makes code simpler. Delete wrappers that only carry parameters, rename branches, or need explanation to justify their existence.

Runtime code executes framework-generated plans. Do not add defensive preflight, rollback, fallback, or speculative recovery for impossible bad plans. Put invalid behavior in the C# PlanModel where it can be made unrepresentable. Runtime checks are for real external boundaries only: DOM lookup, browser API failure, network, and malformed non-framework input.

Do not model normal execution bookkeeping as validation, claims, rejects, lifecycle gates, or registries. If the server-generated plan says source A is assigned to target B, the runtime reads A and writes B. Bookkeeping names should describe what is remembered for execution or unload, not imply the plan is suspicious.

Tests are a thinking tool. Write behavior tests that prove DSL intent becomes deterministic plan/runtime behavior. Do not write tests around helper classes or invented abstractions.

Partial plans are simple: boot composes plan documents by `planId`; browser injection replaces or unloads the declared partial slot. Component ids and type keys remain runtime join keys. Slot identity is only the handle for removing the artifacts loaded into that slot.

When in doubt, delete code before adding code. If a concept does not directly map to frozen DSL intent or a browser boundary the runtime must touch, it is probably noise.
