# Drift Detection Plan: C# -> Schema -> TS Boundaries

## Problem

The pipeline is C# Descriptors -> JSON Schema -> TS Types -> TS Runtime. Three historical
drift incidents (commits `b5bb10b`, `d1fa967`, `4be3e5e`) were discovered by accident.
The `componentType` field was missing from TS types for weeks while present in C# and schema.

### What Exists Today

**C# -> Schema (partial coverage):**
- `AssertSchemaValid()` in `PlanTestBase.cs` validates that specific hand-written test plans
  conform to `reactive-plan.schema.json` using `Json.Schema.Net`.
- ~310 calls across test suites, 26 focused tests in `AllPlansConformToSchema.cs`.
- **Gap:** Tests only validate plans that someone wrote. If a C# descriptor has a property
  that no test exercises, schema drift is invisible. The tests prove "these plans are valid"
  not "the schema covers everything C# can produce."

**Schema -> TS (zero coverage):**
- TS types in `Scripts/types/` are hand-written to match the schema.
- No automation validates alignment. The `componentType` gap proves this.

## Approach: Task 10 -- C# -> Schema Completeness

### Options Considered

1. **Reflection-based exhaustive generation** -- Scan all sealed descriptor subclasses via
   reflection, construct instances with all optional properties populated, serialize to JSON,
   validate against schema. Catches any property the schema doesn't know about.
   - Pro: Systematic, catches exactly the historical drift patterns.
   - Con: Requires constructing instances of internal types. But test projects already
     reference the core project and 53 tests already construct internal types directly.

2. **Enhanced hand-written tests** -- Write tests that exercise every combination.
   - Pro: Uses existing pattern.
   - Con: Same gap as today -- relies on human completeness. Won't catch unknown unknowns.

3. **MSBuild target / pre-commit hook** -- Post-build step.
   - Pro: Automatic.
   - Con: Duplicates test infrastructure, harder to debug.

### Chosen: Option 1 -- Reflection-Based Schema Completeness Test

A single NUnit test class that:
1. Finds all concrete (sealed) types that participate in the plan JSON (triggers, reactions,
   commands, guards, sources, mutations, method args, gather items).
2. For each type, constructs an instance with ALL properties populated (no nulls for
   optional properties) using internal constructors.
3. Wraps each in a minimal valid plan structure.
4. Serializes with the same `JsonSerializerOptions` as `ReactivePlan.Render()`.
5. Validates against the schema.

This catches:
- `b5bb10b` (planId added to C# but not schema) -- the generated plan would include planId,
  schema validation would fail on `additionalProperties: false`.
- `d1fa967` (enriched props) -- same mechanism.
- `4be3e5e` (componentType) -- would include componentType in ComponentEntry, schema would
  need to have it.

### Why Not Reflection on Properties Directly?

Comparing C# property lists to schema property lists is fragile because:
- C# uses `JsonIgnore`, `JsonPropertyName`, `JsonPropertyOrder` attributes.
- The naming policy is `CamelCase`, so `Target` becomes `target`.
- Some properties are computed (`Kind => "mutate-element"`).
- Some are conditionally serialized (`WhenWritingNull`).

It's simpler and more reliable to test what C# actually serializes rather than what it declares.

## Approach: Task 11 -- Schema -> TS Type Completeness

### Options Considered

1. **Generate TS from schema** (json-schema-to-typescript) -- Auto-generate types.
   - Pro: Perfect alignment by construction.
   - Con: Adds dependency, generated types may not match hand-written style/imports,
     loses the developer-friendly type names and JSDoc. The existing types are well-crafted.

2. **Conformance test (vitest)** -- Read schema JSON at test time, extract property names
   and discriminated union members, compare to what TS types declare.
   - Pro: No new dependencies. Catches property additions/removals. Runs in existing `npm test`.
   - Con: Can't validate deep type correctness (e.g., `string` vs `number`), only structural.

3. **Schema-driven factory test** -- Create JSON objects matching schema, verify TS types.
   - Pro: Tests runtime compatibility.
   - Con: Same human-completeness gap as C# tests.

### Chosen: Option 2 -- Schema-to-TS Conformance Test (vitest)

A vitest that:
1. Reads `reactive-plan.schema.json` at test time.
2. For each `$defs` entry in the schema, extracts:
   - Required and optional property names.
   - Enum values (for `Vendor`, `CoercionType`, `GuardOp`, `ValidationRuleType`).
   - Discriminated union members (`oneOf` with `const` kind values).
3. Compares against the actual TS type definitions by:
   - Importing the TS types and using `keyof` / type-level checks where possible.
   - For discriminated unions: checking that each schema `kind` value has a corresponding
     TS interface.
   - For enums: checking that TS union type values match schema enum values.
4. Reports any mismatches clearly.

This catches the `componentType` gap: schema has `componentType` as required in
`ComponentEntry`, but if TS `ComponentEntry` interface lacks it, the test fails.

### Implementation Strategy

The vitest reads schema JSON directly (it's a static file). For each schema definition,
it creates assertions about what the TS types should contain. This means the test is
driven by the schema -- when the schema changes, the test automatically checks TS alignment.

The comparison works by maintaining a mapping file that connects schema `$defs` names
to their TS interface names and file locations. The test validates:
- Every required property in schema exists in the TS interface.
- Every optional property in schema exists as optional in the TS interface.
- Every enum value in schema exists in the TS union type.
- Every `oneOf` discriminant in schema has a matching TS variant.

## Implementation Steps

### C# Side (Task 10)

1. Create `tests/Alis.Reactive.UnitTests/Schema/WhenDetectingSchemaCompleteness.cs`
2. Build helper methods to construct descriptor instances with all properties populated
3. Wrap each descriptor in a minimal plan JSON
4. Validate each against the schema
5. Verify the test passes on current codebase
6. Verify it would fail on historical drift (temporary property test)

### TS Side (Task 11)

1. Create `Scripts/__tests__/when-detecting-schema-ts-drift.test.ts`
2. Read schema JSON file in test
3. Build assertions for each schema definition against TS type structure
4. Verify the test passes on current codebase
5. Verify it would fail if a property were missing from TS

## Success Criteria

- Both tests pass on the current codebase.
- Introducing a fake property in C# that's not in schema causes the C# test to fail.
- Removing a property from TS that exists in schema causes the TS test to fail.
- No new external dependencies.
- Runs as part of existing `dotnet test` and `npm test` pipelines.
