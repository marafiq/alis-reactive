# Alis.Reactive Framework

Alis.Reactive is a clean-break V2 system.

C# builders capture typed browser intent and render one JSON contract:

- `version`
- `contracts`
- `objects`
- `bindings`
- `workflows`

The browser runtime is a dumb executor for that contract. There is no legacy plan, no compatibility layer, and no second schema to reason about.

## Quality Bar

- One active model only: V2.
- No adapter, bridge, fallback, shim, or parallel implementation.
- If a module makes the system harder to reason about than `main`, it is a bug.
- Any old vocabulary that teaches the removed design is a bug.
- Every class, helper, and test must satisfy SOLID and proper encapsulation.
- Tests are behavior-first vertical slices, not internal implementation probes.
- Runtime uses explicit object/member resolution only. No DOM scanning, no guessing.

## Build & Verify

```bash
npm run build:all
dotnet build /Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/schema-capability-design/Alis.Reactive.slnx -nologo -t:Rebuild
npm run typecheck
npm test

dotnet test tests/Alis.Reactive.UnitTests/Alis.Reactive.UnitTests.csproj -nologo
dotnet test tests/Alis.Reactive.Native.UnitTests/Alis.Reactive.Native.UnitTests.csproj -nologo
dotnet test tests/Alis.Reactive.Fusion.UnitTests/Alis.Reactive.Fusion.UnitTests.csproj -nologo
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests/Alis.Reactive.FluentValidator.UnitTests.csproj -nologo
dotnet test tests/Alis.Reactive.DriftDetection.Tests/Alis.Reactive.DriftDetection.Tests.csproj -nologo
dotnet test tests/Alis.Reactive.PlaywrightTests/Alis.Reactive.PlaywrightTests.csproj -nologo
```

For the full browser sweep with visible progress:

```bash
dotnet test tests/Alis.Reactive.PlaywrightTests/Alis.Reactive.PlaywrightTests.csproj -nologo --logger "console;verbosity=detailed"
```

## Mental Model

### C# authoring

- `ReactivePlan<TModel>` owns the document being authored.
- Builders create V2-native actions, predicates, requests, bindings, and workflows.
- Components register capability contracts and runtime objects.
- Typed expressions become value expressions and shapes.

### Rendered plan

- `contracts` describe vendor-specific members and events.
- `objects` name concrete runtime instances.
- `bindings` define canonical reads for typed model fields.
- `workflows` connect subscriptions to actions.

### Browser runtime

- Resolve object.
- Resolve member.
- Evaluate value/predicate.
- Execute action.

Nothing in the runtime invents missing information.

## Architectural Rules

### 1. The plan is the contract

No inline JavaScript in views. No hidden browser behavior outside the rendered plan and explicit component boot slices.

### 2. Vendor isolation

Vendor differences live in declared capability contracts and resolvers. They do not leak into unrelated modules.

### 3. Fail fast

Missing contract data is an error. Wrong shapes are errors. Unsupported paths are errors. Do not guess.

### 4. Surgical runtime

Use explicit ids and explicit object names. Do not introduce wide DOM queries when the plan can carry the information.

### 5. Vertical slices

Each component slice owns its contracts, event payloads, HTML helpers, builder surface, tests, and docs.

### 6. Observability

Tracing follows W3C/Otel-style context propagation. Requests carry `traceparent`. Logs and spans must describe real V2 behavior, not deleted concepts.

## Change Rules

When adding or changing a capability:

1. Model it in V2 terms first.
2. Keep the C# DSL compile-time safe.
3. Serialize directly to the V2 schema.
4. Keep the runtime dumb.
5. Add or update unit, runtime, and Playwright coverage.
6. Delete dead code immediately.

## Review Questions

Ask these every time:

- Does this reduce tech debt compared with `main`?
- Does this keep one active model only?
- Is this class or helper SOLID and properly encapsulated?
- Am I keeping code that should be deleted?
- Does this test prove user-visible behavior instead of internals?

If the answer is not clearly yes, stop and redesign.
