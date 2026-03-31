# Issue #86 Release Stack Plan V2

> Source Of Truth: this plan supersedes the earlier
> [2026-03-31-task-level-plan.md](./2026-03-31-task-level-plan.md) for the
> stacked-release strategy. It is grounded in the later planning discussion
> preserved in
> [2026-03-31-session-transcript-continuation-02.md](./2026-03-31-session-transcript-continuation-02.md)
> and the refined architecture notes in
> [2026-03-31-architecture-understanding-continuation-02.md](./2026-03-31-architecture-understanding-continuation-02.md).
>
> Date: 2026-03-31

## Summary

Base the stack on the current `codex/issue-86-capability-matrix` head, then
create:

- `release/simplification-and-elegant-architecture`

The end state is:

- public C# DSL remains the only public grammar
- internal semantics become pure recorders of meaning
- JSON becomes explicit writer-owned projection
- runtime gets dumber and more mechanical
- HTTP request inputs, response-body reads, and chained-request continuity are
  treated as one value-flow family
- the final emitted contract moves toward value objects rather than ending on
  `readExpr` / `coerce` terminology

## End-State Contract Direction

### Stable concepts

- root resolution
- path access
- raw JS value
- shape value
- consume or emit

### Stable semantic objects

- `BindSource`
  - small locator of the underlying runtime root
- `ValueAccess`
  - internal seam for root + path + shape metadata
- `PlanValue`
  - recursive carried value model
- `GatherField`
  - named request value
- `PlanWriter`
  - one-way projection to emitted plan JSON

### Final emitted contract direction

The branch may temporarily keep some current wire names while the stack is in
flight, but the end-state contract should move toward value-object terms such
as:

- `root`
- `path`
- `shape`
- `value`
- `payload scope`

not finish with:

- `readExpr`
- `coerce`
- `coerceAs`

### HTTP / payload rule

- trigger payload and fetch response payload are one payload-read family
- response payload is not a separate source kind
- URL construction is a transport sink over named request values
- chained requests should inherit success context continuity

## Master Index

| Layer | Responsibility |
| --- | --- |
| Public DSL | curated compile-time grammar only |
| Internal semantics | pure meaning, no serializer leakage |
| Writer | explicit projection from semantics to JSON |
| Emitted plan | stable transport contract |
| TS runtime | resolve root, access path, shape value, execute sinks |

| Family | Unified rule |
| --- | --- |
| Guards | read from the same root/path/shape model |
| Gather / request input | named request values over the same model |
| Response-body reads | same payload-read model, different scope |
| Command consumers | same recursive carried value model |
| Dispatch | another consumer, not a special model |
| Validation | read-side consumer, projection-enriched, not a separate semantic lane |

## Stacked PRs

### PR1

`test: establish architecture BDD foundation`

End state after merge:

- trusted BDD coverage exists for the architecture-critical seams
- broad or serializer-accidental tests are replaced where needed

Primary modules:

- C# architecture/unit/drift tests
- TS architecture/runtime tests
- Playwright core behavior specs

Schema / public DSL:

- no schema delta
- no DSL change

Deletes:

- brittle tests that pin serializer detail instead of meaning

Red-first proof:

- root resolution
- path access
- shape step
- event scope boundary
- typed payload flow

### PR2

`refactor: introduce ValueAccess and unify typed lowering`

End state after merge:

- typed producer lowering is one internal seam
- guards, request inputs, and typed response reads all use the same lowering
  family

Primary modules:

- typed-source / condition builder seams
- element / request / response builder lowering seams
- guard-building seams

Schema / public DSL:

- no schema delta
- no DSL expansion

Deletes:

- repeated event/component/response-body lowering branches

Red-first proof:

- event, component, and response-body lower through the same conceptual seam
- source-vs-source guard cases remain correct
- typed response still lowers through `responseBody.*` current wire truth

### PR3

`refactor: normalize HTTP request semantics`

End state after merge:

- request input values become one named request-value family
- response handlers become reaction-only
- chained requests inherit success context continuity

Primary modules:

- request builders
- response builders
- gather builders
- request descriptors
- TS HTTP types
- TS gather / HTTP execution

Schema / public DSL:

- schema delta:
  - `GatherItem` becomes `AllGather | GatherField`
  - `StatusHandler` becomes reaction-only
- no public DSL growth

Deletes:

- `EventGather`
- `ComponentGather`
- `StaticGather`
- `StatusHandler.commands`
- chained request execution with original context after success

Red-first proof:

- static/event/component/include-all request inputs
- request values feeding GET, JSON, and form-data sinks
- success response continuity into chained request
- `responseBody.*` consumed in chained flows

### PR4

`refactor: replace flat carried values with recursive PlanValue`

End state after merge:

- one recursive value model carries both command values and request values

Primary modules:

- command and mutation descriptors
- request gather value shape
- TS command types
- TS request types
- TS carried-value resolution

Schema / public DSL:

- schema delta:
  - flat carried values become recursive `PlanValue`
  - object and array values become first-class

Deletes:

- flat `CommandValue` model
- dispatch-only value wrapper family

Red-first proof:

- primitive/object/array value flow across:
  - command consumers
  - request inputs
  - response-body sourced values

### PR5

`feat: complete source-backed dispatch on unified value flow`

End state after merge:

- dispatch becomes another consumer of the unified model

Primary modules:

- dispatch lowering
- dispatch runtime execution
- dispatch BDD suites

Schema / public DSL:

- no new semantic primitives
- no schema delta beyond recursive value support

Deletes:

- any dispatch-specific lowering or runtime special case that survives after
  implementation

Red-first proof:

- source-backed dispatch payloads
- object and array payload fields
- local component event remains local
- explicit dispatch remains the only scope crossing

### PR6

`refactor: project request and validation subtree`

End state after merge:

- request and validation semantics become truly immutable in practice because
  projection owns enrichment and emitted shape for that subtree

Primary modules:

- request semantics
- validation semantics
- validation resolver flow
- first `PlanWriter` subtree
- `ReactivePlan` subtree projection

Schema / public DSL:

- no schema delta
- no DSL change

Deletes:

- live-tree request / validation mutation as the active render contract for the
  projected subtree

Red-first proof:

- render-equivalent validation extraction
- render-equivalent component enrichment
- render-equivalent planId stamping
- unchanged emitted request / validation behavior

### PR7

`refactor: complete PlanWriter and flip emitted contract to clean value objects`

End state after merge:

- full plan projection is writer-owned
- the final emitted contract can now be renamed deliberately into value-object
  terms

Primary modules:

- full-plan writer
- triggers
- guards
- reactions
- entries
- top-level envelope
- component projection
- `ReactivePlan`

Schema / public DSL:

- schema delta:
  - finish the move from transport-era field names toward value-object terms
  - represent access/path/shape explicitly where the end-state contract needs it
- no DSL broadening

Deletes:

- remaining active serializer-driven plan emission
- end-state dependence on `readExpr` / `coerce` naming as architectural truth

Red-first proof:

- full mixed-plan render
- schema conformance for the final branch contract
- payload-read parity between trigger payload and response payload

### PR8

`refactor: internalize semantic records and tighten public DSL`

End state after merge:

- public DSL is the only public grammar left
- semantic records are internal

Primary modules:

- descriptor visibility
- friend assemblies
- analyzer/public API tests
- XML docs

Schema / public DSL:

- no schema delta
- public API becomes tighter and more correct

Deletes:

- accidental public semantic and serialization types that only existed because
  serialization previously needed them public

Red-first proof:

- public API compile-time tests
- analyzer tests
- unchanged emitted plan and runtime behavior

## Convergence Rules

After each merge, these truths must become real:

| After PR | Truth that must now hold |
| --- | --- |
| PR2 | one typed lowering story exists for reads, guards, request inputs, and typed responses |
| PR3 | HTTP no longer has separate request-input / response-handler lanes, and chained requests carry success context |
| PR4 | commands and request inputs share one recursive carried value model |
| PR5 | dispatch is just another consumer of that model |
| PR6 | request / validation projection owns emitted shape for that subtree |
| PR7 | full emitted JSON is writer-owned and cleaned toward value objects |
| PR8 | public DSL is the only public grammar left |

If a PR does not advance one of those truths, it is mis-sliced.

## Non-Negotiables

- no fallbacks
- no dual old/new lanes left behind
- red BDD first
- runtime gets dumber, not smarter
- source is truth
- request URL construction is a sink, not a subsystem
- response-body reads remain part of the same payload-read model
- public C# DSL remains frozen and curated

## Quality Gates

Per PR:

- `npm run typecheck`
- `npm test`
- `dotnet test` for unit, analyzers, and drift-detection
- focused Playwright suite for the touched slice

Per merge into `release/simplification-and-elegant-architecture`:

- full solution test run
- full Playwright run on a clean environment

If the harness is flaky:

- fix or quarantine the harness
- do not distort the architecture to satisfy bad tests
