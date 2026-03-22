# Alis.Reactive Architecture Review — March 2026

## Overall Scores

| Layer | Score | Verdict |
|-------|-------|---------|
| Core Descriptors | 8/10 | Strong fundamentals, minor immutability gap |
| Builders + Extensions | 8.5/10 | Excellent fluent design, minor DRY gap |
| Vertical Slices | 9/10 | Exemplary isolation, 23 components prove the pattern |
| **Overall** | **8.5/10** | Production-ready, scales to ~50 components comfortably |

## Top 5 Strengths

1. **Zero runtime changes** — 23 components, identical TS runtime. Adding a component = new C# vertical slice only.
2. **Phantom type pattern** — vendor abstraction via instance properties (`IComponent.Vendor`, `IInputComponent.ReadExpr`), no reflection, no magic strings.
3. **Vertical slice isolation** — zero cross-slice references across 23 components, 7-file pattern enforced by convention.
4. **Open/Closed polymorphism** — `WriteOnlyPolymorphicConverter<T>` avoids `[JsonDerivedType]` attribute bloat. New commands added without touching base.
5. **Type-safe condition chain** — `When().Eq().Then().Else()` with compile-time type inference via `TypedSource<TProp>`.

## Issue Files

Each issue is documented in detail:

1. [command-guard-mutability](./command-guard-mutability-design-issues.md) — `Command.GuardWith()` mutates after construction
2. [reactive-plan-interface-breadth](./reactive-plan-interface-breadth-design-issues.md) — `IReactivePlan<T>` is too broad
3. [component-registration-enforcement](./component-registration-enforcement-design-issues.md) — Silent failures when registration is skipped
4. [condition-guard-composition-duplication](./condition-guard-composition-duplication-design-issues.md) — 3x duplicated composition logic
5. [gather-extensions-vendor-duplication](./gather-extensions-vendor-duplication-design-issues.md) — Near-identical gather logic across vendors

## Scalability Assessment

| Scale | Status |
|-------|--------|
| Current (23 components) | Excellent |
| 50 components | Good — PipelineBuilder gets wide, extension duplication grows |
| 100+ components | Needs registration enforcement, gather extraction, builder strategy |
