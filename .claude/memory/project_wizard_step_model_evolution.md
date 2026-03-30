---
name: Wizard steps need per-step models — refactor planned for next session
description: Multi-step wizards should have one model per step (own partial, own save, own server state). Flat forms stay single-model. Each step loads its saved state from server on Next/Previous.
type: project
---

Current AdmissionAssessment uses a single flat model across 4 wizard steps. This works for conditions/visibility but breaks for validation (all rules fire on Submit regardless of which step is visible).

**Correct architecture for multi-step wizards:**
- Each step gets its own model class
- Each step is a partial with its own save endpoint
- Saving a step persists to server (draft pattern)
- Next/Previous reloads the step's partial with server state (edit scenario)
- Components already have built-in binding via property expressions — model + ViewBag just need correct values
- Final Submit only validates that all required steps are saved (check IDs)

**Key insight:** Flat forms (no steps) = single model is perfect. Multi-step = each step owns its own model and can mix partials in its own domain.

**How to apply:** Refactor AdmissionAssessment in a future session using existing framework primitives. No framework changes needed — just proper modeling with per-step models, per-step controllers, and server-side draft persistence. The partial merging (ResolvePlan) already supports this.
