---
name: Wizard validation gap — single model doesn't fit multi-step forms
description: ReactiveValidator on one model fires ALL rules on Submit, even for hidden steps. Wizards need per-step validation with draft saves.
type: project
---

Single-model FluentValidation doesn't work for wizard forms — all WhenField rules evaluate on Submit regardless of which step is visible. Discovered during AdmissionAssessment flagship build.

**Why:** A 4-step wizard with conditional sections means only 1-2 steps are relevant per diagnosis path. But Validate<T> fires all rules including rules for hidden fields on other steps.

**How to apply:** For wizard forms, each step should be its own mini-form that POSTs independently (draft save pattern). The section-save buttons (SaveCognitive, SaveCardiac, etc.) already do this correctly. The final Submit should only validate that required drafts are saved (check hidden field IDs), not re-validate all fields. This is a framework evolution opportunity — per-step validation with draft persistence.
