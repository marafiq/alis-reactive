# archive-history

Historical documentation moved out of the active tree on **2026-06-01** so that
`docs/design/redesign/` holds **only the 54 rewrite-final docs** that kick off the
Alis.Reactive 1.0.0 green-field rewrite. Nothing in this folder is on the rewrite
critical path. Files were moved with `git mv` — full history is preserved; names are
unchanged, only the path prefix.

## What's here

### `rc1-docs/` — the RC1 / 1.0-era documentation (28 files)
The design + status docs that described the **shipped RC1 system**. Superseded for the
rewrite by the consolidated corpus in `docs/design/redesign/` (`REWRITE-SPEC.md` is the
authority). Kept because RC1 is the differential **oracle** the rewrite proves against, so
its domain language and matrices remain useful reference. Includes:
`reactive-plan-source-blueprint.md`, `reactive-plan-domain-language.md`,
`design/dsl-graph-coverage-matrix.md`, `design/reactive-plan-domain-design.md`,
`design/dsl-architecture-atlas.md`, `design/dsl-atlas/`, the `adr/`, `reviews/`,
`superpowers/` plans + specs, `test-coverage/`, validation guides, and the RC1 status docs.

### `redesign-scratch/` — design-session working artifacts (54 tracked files)
The **superseded / CUT** by-products of the 2026-05/06 design sessions. The *consolidated*
outputs they fed into are the kept docs (`11-dsl-grammar-hardening.md`, `REWRITE-SPEC.md`,
`governance-gaps.md`, `08-determinism-formalization.md`). Includes:
- `grammar-critique-*.md` (5) + `grammar-hardening-review.md` — per-cluster PL critiques that
  `11-dsl-grammar-hardening.md` consolidates.
- The **Phase-D HTML simulators** — `governance-simulation.html`, `dsl-ast-tree.html`,
  `dsl-playground.html`, `_ast_data.json`, `_shell-*.html`, `playground/`. Phase D is **CUT**
  (a browser drawing is a non-falsifiable second implementation; the proof is the math in
  `08` run by harnesses). Retired as a gate, never on the unlock path.
- `dogfood/` — the determinism dogfood **experiment projects** (Shape / ShapeMath / repro /
  determinism-domain, 40 tracked source files; build artifacts were not archived). The
  *pattern* to replicate per module is described in the kept corpus; this is the raw run.

## Reference resolution
Any path in the rewrite corpus to `dogfood/`, `playground/`, `governance-simulation.html`,
`dsl-ast-tree.html`, `dsl-playground.html`, or a `grammar-critique-*.md` resolves under
`redesign-scratch/`. Any path to an RC1 doc (e.g. `reactive-plan-source-blueprint.md`,
`docs/design/dsl-graph-coverage-matrix.md`) resolves under `rc1-docs/` at the same relative
sub-path. Inline provenance citations in the kept docs were **not** rewritten one-by-one —
this map is the single resolver.
