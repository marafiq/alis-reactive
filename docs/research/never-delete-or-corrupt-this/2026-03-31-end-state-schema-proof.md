# Source Of Truth

- Read in order:
  - [2026-03-31-session-transcript.md](./2026-03-31-session-transcript.md)
  - [2026-03-31-architecture-understanding.md](./2026-03-31-architecture-understanding.md)
  - [2026-03-31-session-transcript-continuation-02.md](./2026-03-31-session-transcript-continuation-02.md)
  - [2026-03-31-release-stack-plan-v2.md](./2026-03-31-release-stack-plan-v2.md)
  - [2026-03-31-end-state-schema-proof-matrix.md](./2026-03-31-end-state-schema-proof-matrix.md)
  - [2026-03-31-end-state-reactive-plan.schema.json](./2026-03-31-end-state-reactive-plan.schema.json)

# 2026-03-31 End-State Schema Proof

## Governing Rule

The refactor is only valid if every real use-case family in the current codebase can be narrated and expressed by the end-state plan contract itself.

If a use case only works because:

- runtime branches invent payload shape
- lazy enrichment secretly completes missing trigger data
- the schema cannot tell the story without prose

then the end-state schema is wrong.

## Final Contract Decisions

These decisions are locked in this proof package.

1. `components` stays a map keyed by model path.
The key is the stable correlation key for `IncludeAll`, validation, and component lookups after partial merges.

2. Component registrations stop emitting `readExpr` / `coerceAs`.
They emit:

```json
{
  "id": "Resident_Name",
  "vendor": "native",
  "componentType": "textbox",
  "value": {
    "path": "value",
    "shape": "string"
  }
}
```

3. Reads and carried values are separated cleanly.
- `ValueAccess` answers: where does the raw value come from?
- `PlanValue` answers: what value is being passed to a consumer?

4. Trigger payload and response payload are one source family.
- trigger payload reads use `source: { kind: "payload", scope: "trigger" }`
- response payload reads use `source: { kind: "payload", scope: "response" }`

5. `ComponentEventTrigger` must be self-sufficient.
It cannot depend on later enrichment because partial-injected entries are wired immediately.

6. Validation fields stay pure.
Validation does **not** carry enriched `fieldId`, `vendor`, `readExpr`, or `coerceAs`.
It stays:

```json
{
  "modelPath": "Resident.Address.ZipCode",
  "rules": [
    { "rule": "required", "message": "required" }
  ]
}
```

Runtime lookup can resolve the current component through `plan.components[modelPath]`.

7. `mutate-event` becomes `mutate-payload`.
The command acts on trigger payload, not just DOM event args. That matches current reachable use through custom events, fusion callback args, and native event-like payloads.

8. Request input, dispatch payload, method args, and condition operands all use `PlanValue`.
This is the unified value-flow center.

## Self-Sufficient vs Lazy-Resolvable

This distinction is required by the browser partial lifecycle.

```mermaid
flowchart TD
  A["Partial HTML + plan JSON arrives"] --> B["Inject DOM"]
  B --> C["Merge component registrations into plan.components"]
  C --> D["Re-resolve validation / IncludeAll against components map"]
  C --> E["Wire incoming entries immediately"]
  E --> F["Later trigger fires"]
```

- `ComponentEventTrigger` must be complete before `E`
- validation fields are allowed to stay pure and be resolved at `D`

## Activity Proofs

### 1. Native Readable Component Event

```mermaid
flowchart TD
  A["Native component change fires"] --> B["Trigger target resolves element + vendor root"]
  B --> C["Trigger payload object is projected from explicit fields"]
  C --> D["payload.trigger.value is available"]
  D --> E["Reaction consumes payload.trigger.value"]
  E --> F["Optional dispatch crosses document scope"]
```

Schema objects:
- `ComponentEventTrigger.target`
- `ComponentEventTrigger.payload.kind = object`
- `TriggerProjectionValue = ComponentReadValue`
- `ValueAccess.source.kind = component`
- `DispatchCommand.payload = ObjectValue`

### 2. Native Non-Readable Component Event

```mermaid
flowchart TD
  A["Native button click fires"] --> B["Trigger target resolves element + vendor root"]
  B --> C["Trigger payload.kind = none"]
  C --> D["Reaction still executes"]
  D --> E["Optional dispatch with no payload"]
```

This replaces the current runtime habit of manufacturing `{ event: e }` even when the DSL never reads it.

### 3. Fusion Callback Event

```mermaid
flowchart TD
  A["Fusion callback fires"] --> B["Trigger target resolves Syncfusion root"]
  B --> C["Trigger payload.kind = callback"]
  C --> D["payload.trigger exposes callback object directly"]
  D --> E["Reaction reads payload.trigger.newValue / count / selectedIndex"]
```

Schema objects:
- `ComponentEventTrigger.payload.kind = callback`
- `ValueAccess.source.kind = payload`
- `ValueAccess.source.scope = trigger`

### 4. HTTP Request Input And Response

```mermaid
flowchart TD
  A["Trigger fires"] --> B["Resolve GatherField values"]
  B --> C["Transport sink emits GET query / JSON body / form-data"]
  C --> D["Fetch executes"]
  D --> E["Success handler gets payload.response root"]
  E --> F["Commands / dispatch / chained request consume payload.response"]
```

Schema objects:
- `GatherField`
- `PlanValue`
- `RequestDescriptor.contentType`
- `StatusHandler.reaction`
- `PayloadValueSource.scope = response`

### 5. Chained Request Continuity

```mermaid
flowchart TD
  A["Request A succeeds"] --> B["payload.response root now points at response A"]
  B --> C["Chained request B resolves gather from payload.response"]
  C --> D["Request B executes"]
  D --> E["Success handler for B reads payload.response from response B"]
```

This is the concrete proof that response payload is not a side subsystem. It is the same unified payload-read model.

### 6. Partial Validation

```mermaid
flowchart TD
  A["Root plan boots with validation fields"] --> B["Some model paths have no registered components yet"]
  B --> C["Partial arrives later with Address.* component registrations"]
  C --> D["Validation lookup resolves modelPath against plan.components"]
  D --> E["Live-clear and validation rendering use the resolved component access"]
```

This removes the need to mutate validation fields with copied component metadata.

### 7. Payload Mutation During Filtering

```mermaid
flowchart TD
  A["Custom filtering trigger fires"] --> B["payload.trigger contains filter args"]
  B --> C["HTTP request executes"]
  C --> D["Success handler reads payload.response.items"]
  D --> E["mutate-payload call updates payload.trigger using response data"]
```

Schema objects:
- `MutatePayloadCommand`
- `CallMutation.args: PlanValue[]`
- `PayloadValueSource.scope = trigger | response`

This is the real proof case behind `PreventDefault` and `UpdateData`.

### 8. Server Push And SignalR

```mermaid
flowchart TD
  A["External message arrives"] --> B["Trigger wiring supplies payload.trigger object"]
  B --> C["Reaction reads payload.trigger fields"]
  C --> D["Dispatch / DOM update / conditional logic consume the same value model"]
```

No special read model is needed for SSE or SignalR. They are just trigger-payload providers.

## Result

With the contract in [2026-03-31-end-state-reactive-plan.schema.json](./2026-03-31-end-state-reactive-plan.schema.json), every current use-case family can be expressed without relying on hidden runtime payload invention.

The two most important architectural corrections are:

- `ComponentEventTrigger` now owns explicit payload projection instead of leaving it to vendor-specific runtime branching
- validation remains pure and resolves through `components` instead of lazy copying/mutation

## Review Notes

- `Carson` runtime review:
  - request input, response payload, and chained-request continuity are one value-flow family
  - partial-injected component-event triggers must stay self-sufficient
- `Descartes` public-DSL / writer review:
  - public placeholders like `ResponseBody<T>` and typed event args are compile-time DSL only
  - emitted projection contracts must be distinct from public DSL inputs and internal semantic records
