# Session: Schema + TS Drift Detection Tools

Branch: create `feature/drift-detection` from `refactor/api-surface-xml-docs`

## Goal

Build automated tools that catch C# → Schema and Schema → TS type drift before commit.

## Task 10: Schema Drift Detection (Layer 1→2)

**Problem**: Schema drifted 3 times in 33 revisions (M21). Each discovered by accident.

**Approach options** (decide in planning):
1. Build-time MSBuild target — serialize every descriptor class sample, validate against schema
2. Standalone tool — like ApiDocGenerator, run via `npm run` script
3. Hook — hookify rule that checks schema alignment on edit

**Input**: Current 310 `AssertSchemaValid()` tests, schema at `reactive-plan.schema.json`
**Output**: Automated check that runs on build or commit, catches drift before merge

## Task 11: TS-to-Schema Validation (Layer 2→3)

**Problem**: TS types diverge silently from schema. `componentType` was missing from TS while present in C# and schema for weeks (M23).

**Approach options** (decide in planning):
1. Generate TS types from schema (json-schema-to-typescript)
2. Validate existing TS types against schema at test time
3. Manual conformance test suite (vitest asserts TS type shape matches schema)

**Input**: Current TS types in `Scripts/types/`, schema, 4 known discrepancies
**Output**: Automated check that TS types match schema

## Context

- Forensic index: M21 (3 schema drifts), M22 (TODO never built), M23 (TS type divergence), M24 (MutateElement value too narrow)
- Current gap: zero automation validates TS types match schema
- Session todo: `.claude/memory/session_2026_03_28_todo.md` — Tasks 10-11
- These are foundational tools — once built, they prevent entire categories of bugs
