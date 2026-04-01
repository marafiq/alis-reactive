# Issue #86 Final Schema Shape

## Governing Decisions

- `reactions` are the top-level behavior unit because they match the public DSL.
- The schema is surface-first, not input-first: every component resolves the
  same way from `id + vendor`.
- Bound/model participation is not a second top-level family; it is an optional
  `binding` facet on a component.
- `Request` stays a first-class DSL unit and keeps the public stage names:
  `gather`, `as`, `whileLoading`, `validate`, `response.onSuccess`,
  `response.onError`, `response.chained`.
- Each request unit is immutable after `gather` resolves: later source changes
  do not rewrite the in-flight payload, which keeps future retry support
  open without recollection.
- Pipeline order is first-class: conditions, requests, parallel blocks, and
  commands keep declaration order instead of being flattened into one stage.
- `reactions[]` is the post-lowering execution surface. If the public DSL
  declares multiple same-trigger segments, lowering groups them into one ordered
  reaction before the hidden schema is emitted.
- One value-flow law governs the whole runtime: resolve root, access from root,
  shape if needed, consume.
- A component opts into `binding` only when its vertical slice can declare a
  self-sufficient canonical semantic value. Current proof coverage for this is
  centered on readable bound components, not arbitrary non-input widgets.
- Member paths are plain dotted JS paths and may walk arrays and nested objects,
  for example `items.0.meta.name`.
- The same access algebra governs payload building, request/command values, and
  explicit `component` / `response` / `trigger` reads; validation deliberately
  uses a constrained subset for purity.
- Generic read access is compositional: an ordered chain of `member` and
  `invoke` steps.
- Generic effect access stays `set` and `call`.
- Validation targets canonical `componentId` and resolves through the component
  registry instead of carrying copied runtime enrichment.
- `includeAll` walks only components that opt into `binding`.
- The final schema makes trigger payload shape explicit; the current runtime
  still carries legacy/native-invented payloads, which remains an implementation
  gap until the hidden end-state contract is executed directly.

## Final Nouns

- `Plan`: root contract for one reactive surface.
- `Component`: one resolvable component surface, with optional binding
  participation.
- `Binding`: canonical semantic field participation for request/validation use.
- `ComponentRef`: explicit runtime root identity for a component surface.
- `Reaction`: one trigger-attached behavior.
- `Trigger`: wake-up source plus explicit trigger payload contract.
- `TriggerPayload`: the carried payload root for a trigger, built from the same
  value/access algebra using payload-scoped roots.
- `Pipeline`: ordered executable steps.
- `PipelineStep`: one ordered command, condition, request, or parallel block.
- `When`: guarded branching stage.
- `Request`: full HTTP unit.
- `Response`: success/error/chained stages owned by a request.
- `Parallel`: concurrent request unit with `onAllSettled`.
- `Value`: one shared value/access algebra, parameterized by which roots are
  legal in that scope.
- `BindingAccess`: the self-sufficient canonical value contract for a binding.
- `AccessStep`: one composable read step over a root or intermediate value.
- `Access`: generic read access over a resolved root.
- `ReadRoot`: one readable runtime root surface.
- `ApplyTarget`: one writable/callable runtime root surface.
- `Command`: one imperative effect.
- `Mutation`: `set` or `call` against an already-resolved root.
- `Validation`: request-time validation contract.
- `ValidationTarget`: rules for one canonical component id.
- `ValidationRule`: one validation rule.
- `ValidationCondition`: composable rule gate.

## Proof Anchors

The current design is now anchored by two deliberately separate proof layers:

- runtime execution proof in
  [proof-surfaces.ts](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive.SandboxApp/Scripts/components/lab/proof-surfaces.ts)
  and
  [when-proving-canonical-proof-surfaces.test.ts](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive.SandboxApp/Scripts/__tests__/when-proving-canonical-proof-surfaces.test.ts)
  for a controlled native/fusion shared surface
- C# lowering proof in
  [WhenLoweringDslSurfacesToRuntimeAlgebra.cs](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/tests/Alis.Reactive.UnitTests/Architecture/WhenLoweringDslSurfacesToRuntimeAlgebra.cs)
  for bound refs, explicit refs, app-level refs, gather participation, command
  values, guards, and request-owned stage lowering

## Final Shape

```ts
type Vendor = "native" | "fusion";

type Shape =
  | "raw"
  | "string"
  | "number"
  | "boolean"
  | "date"
  | "object"
  | { kind: "array"; of: Shape };

type BindingLiteral = { kind: "literal"; value: unknown };

type AccessStep<TValue> =
  | { kind: "member"; path: string }
  | { kind: "invoke"; method: string; args?: TValue[] };

type Access<TValue> = {
  steps: AccessStep<TValue>[]; // empty = resolved root itself
  rawShape?: Shape;
  shape?: Shape;
};

type BindingAccess = Access<BindingLiteral>; // self-sufficient: no external roots

type ComponentRef = {
  id: string;
  vendor: Vendor;
};

type Binding = {
  path: string;      // semantic field name/path, e.g. "Address.City"
  access: BindingAccess; // self-sufficient canonical semantic value for this component
};

type Component = {
  vendor: Vendor;
  binding?: Binding; // present only if this component participates in model/request/validation semantics
};

type Plan = {
  planId: string;
  sourceId?: string;
  components: Record<string, Component>; // key = componentId
  reactions: Reaction[]; // already grouped by trigger signature + declaration order
};

type Reaction = {
  on: Trigger;
  pipeline: Pipeline;
};

type Trigger =
  | { kind: "domReady" }
  | { kind: "documentEvent"; event: string; payload: TriggerPayload }
  | { kind: "componentEvent"; target: ComponentRef; event: string; payload: TriggerPayload }
  | { kind: "sse"; url: string; event?: string; payload: TriggerPayload }
  | { kind: "signalR"; hubUrl: string; method: string; payload: TriggerPayload };

type TriggerPayload =
  | { kind: "none" }
  | { kind: "host" }
  | { kind: "build"; value: PayloadValue };

type RuntimeRoot =
  | { kind: "trigger" }
  | { kind: "response" }
  | { kind: "component"; target: ComponentRef }
  | { kind: "element"; id: string }
  | { kind: "document" };

type PayloadRoot =
  | { kind: "payloadHost" }
  | { kind: "payloadTarget" };

type Value<TRoot> =
  | { kind: "literal"; value: unknown }
  | { kind: "bindingValue"; componentId: string }
  | { kind: "access"; root: TRoot; access: Access<Value<TRoot>> }
  | { kind: "object"; fields: Record<string, Value<TRoot>> }
  | { kind: "array"; items: Value<TRoot>[] };

type RuntimeValue = Value<RuntimeRoot>;
type PayloadValue = Value<PayloadRoot>;

type PipelineStep = Command | When | Request | Parallel;

type Pipeline = {
  steps: PipelineStep[];
};

type When = {
  kind: "when";
  branches: WhenBranch[];
  otherwise?: Pipeline;
};

type WhenBranch = {
  guard: Guard;
  pipeline: Pipeline;
};

type Guard =
  | { kind: "check"; left: RuntimeValue; op: GuardOp; right?: RuntimeValue }
  | { kind: "all"; guards: Guard[] }
  | { kind: "any"; guards: Guard[] }
  | { kind: "not"; guard: Guard }
  | { kind: "confirm"; message: string };

type GuardOp =
  | "eq"
  | "neq"
  | "gt"
  | "gte"
  | "lt"
  | "lte"
  | "truthy"
  | "falsy"
  | "isNull"
  | "notNull"
  | "isEmpty"
  | "notEmpty"
  | "in"
  | "notIn"
  | "between"
  | "arrayContains"
  | "contains"
  | "startsWith"
  | "endsWith"
  | "matches"
  | "minLength";

type Request = {
  kind: "request";
  method: "GET" | "POST" | "PUT" | "DELETE";
  url: string;
  gather?: GatherItem[];
  as?: "json" | "formData"; // GET implies query-string sink
  whileLoading?: Command[]; // current public DSL keeps this commands-only
  validate?: Validation;
  response?: Response;
};

type Response = {
  onSuccess?: Pipeline[];
  onError?: ErrorHandler[];
  chained?: Request;
};

type ErrorHandler = {
  status: number;
  pipeline: Pipeline;
};

type Parallel = {
  kind: "parallel";
  requests: Request[];
  onAllSettled?: Command[]; // current public DSL keeps this commands-only
};

type GatherItem =
  | { kind: "field"; name: string; value: RuntimeValue }
  | { kind: "includeAll" };

type Command =
  | { kind: "apply"; target: ApplyTarget; mutation: Mutation }
  | { kind: "dispatch"; event: string; payload?: RuntimeValue }
  | { kind: "validationErrors"; formId: string }
  | { kind: "into"; target: string };

type ApplyTarget =
  | { kind: "trigger" }
  | { kind: "component"; target: ComponentRef }
  | { kind: "element"; id: string }
  | { kind: "document" };

type Mutation =
  | { kind: "set"; path: string; value: RuntimeValue }
  | { kind: "call"; path: string; args?: RuntimeValue[] };

type Validation = {
  formId: string;
  targets: ValidationTarget[];
};

type ValidationTarget = {
  componentId: string;
  rules: ValidationRule[];
};

type ValidationRule = {
  rule:
    | "required"
    | "empty"
    | "minLength"
    | "maxLength"
    | "email"
    | "regex"
    | "url"
    | "creditCard"
    | "range"
    | "exclusiveRange"
    | "min"
    | "max"
    | "gt"
    | "lt"
    | "equalTo"
    | "notEqual"
    | "notEqualTo"
    | "atLeastOne";
  message: string;
  constraint?: unknown;
  peerComponentId?: string;
  shape?: Shape;
  when?: ValidationCondition;
};

type ValidationCondition =
  | { kind: "check"; left: ValidationValue; op: ValidationOp; right?: ValidationValue }
  | { kind: "all"; conditions: ValidationCondition[] }
  | { kind: "any"; conditions: ValidationCondition[] }
  | { kind: "not"; condition: ValidationCondition };

type ValidationValue = Extract<
  RuntimeValue,
  { kind: "bindingValue" } | { kind: "literal" }
>;

type ValidationOp = GuardOp;
```

## Minimal Example

```json
{
  "planId": "MyApp.Models.OrderModel",
  "components": {
    "MyApp_Models_OrderModel__Address_City": {
      "vendor": "fusion",
      "binding": {
        "path": "Address.City",
        "access": {
          "steps": [
            { "kind": "member", "path": "value" }
          ],
          "shape": "string"
        }
      }
    },
    "MyApp_Models_OrderModel__Age": {
      "vendor": "native",
      "binding": {
        "path": "Age",
        "access": {
          "steps": [
            { "kind": "member", "path": "value" }
          ],
          "rawShape": "string",
          "shape": "number"
        }
      }
    },
    "resident-tabs": {
      "vendor": "fusion"
    },
    "alisFusionToast": {
      "vendor": "fusion"
    }
  },
  "reactions": [
    {
      "on": {
        "kind": "componentEvent",
        "target": {
          "id": "MyApp_Models_OrderModel__Age",
          "vendor": "native"
        },
        "event": "change",
        "payload": {
          "kind": "build",
          "value": {
            "kind": "object",
            "fields": {
              "Value": {
                "kind": "bindingValue",
                "componentId": "MyApp_Models_OrderModel__Age"
              }
            }
          }
        }
      },
      "pipeline": {
        "steps": [
          {
            "kind": "request",
            "method": "POST",
            "url": "/orders/save",
            "gather": [
              { "kind": "includeAll" }
            ],
            "validate": {
              "formId": "order-form",
              "targets": [
                {
                  "componentId": "MyApp_Models_OrderModel__Address_City",
                  "rules": [
                    {
                      "rule": "required",
                      "message": "City is required"
                    }
                  ]
                }
              ]
            },
            "response": {
              "onSuccess": [
                {
                  "steps": [
                    {
                      "kind": "apply",
                      "target": {
                        "kind": "component",
                        "target": { "id": "alisFusionToast", "vendor": "fusion" }
                      },
                      "mutation": {
                        "kind": "set",
                        "path": "content",
                        "value": {
                          "kind": "access",
                          "root": { "kind": "response" },
                          "access": {
                            "steps": [
                              { "kind": "member", "path": "resident.name" }
                            ],
                            "shape": "string"
                          }
                        }
                      }
                    },
                    {
                      "kind": "apply",
                      "target": {
                        "kind": "component",
                        "target": { "id": "alisFusionToast", "vendor": "fusion" }
                      },
                      "mutation": {
                        "kind": "call",
                        "path": "show"
                      }
                    }
                  ]
                }
              ]
            }
          }
        ]
      }
    }
  ]
}
```

## Explicit ComponentRef Examples

Non-input and app-level components do not need `binding`. They are still normal
component surfaces.

Read from a non-input component property:

```json
{
  "kind": "access",
  "root": {
    "kind": "component",
    "target": { "id": "resident-tabs", "vendor": "fusion" }
  },
  "access": {
    "steps": [
      { "kind": "member", "path": "selectedItem" }
    ],
    "shape": "number"
  }
}
```

Read from a non-input component method:

```json
{
  "kind": "access",
  "root": {
    "kind": "component",
    "target": { "id": "resident-tabs", "vendor": "fusion" }
  },
  "access": {
    "steps": [
      { "kind": "invoke", "method": "getSelectedItems" }
    ],
    "shape": { "kind": "array", "of": "raw" }
  }
}
```

Call a non-input component method:

```json
{
  "kind": "apply",
  "target": {
    "kind": "component",
    "target": { "id": "resident-tabs", "vendor": "fusion" }
  },
  "mutation": {
    "kind": "call",
    "path": "select",
    "args": [{ "kind": "literal", "value": 2 }]
  }
}
```

Illustrative end-state extension: a non-input component could participate in
request gather if a future vertical slice declares a canonical binding:

```json
{
  "components": {
    "edit-button": {
      "vendor": "native",
      "binding": {
        "path": "EditContext",
        "access": {
          "steps": [
            { "kind": "invoke", "method": "getPayload" }
          ],
          "shape": "object"
        }
      }
    }
  }
}
```

Walk an array item object path from a response or component root:

```json
{
  "kind": "access",
  "root": {
    "kind": "component",
    "target": { "id": "resident-grid", "vendor": "fusion" }
  },
  "access": {
    "steps": [
      { "kind": "member", "path": "items.1.meta.name" }
    ],
    "shape": "string"
  }
}
```

Invoke a method, then keep walking inside what it returned:

```json
{
  "kind": "access",
  "root": {
    "kind": "component",
    "target": { "id": "country-ddl", "vendor": "fusion" }
  },
  "access": {
    "steps": [
      { "kind": "invoke", "method": "getItems" },
      { "kind": "member", "path": "3.disabled" }
    ],
    "shape": "boolean"
  }
}
```

This is part of the locked end-state design, but it is still a design
requirement rather than a current executable-proof case because the current TS
runtime still executes the older path model directly.

`shape` describes the terminal semantic value after all steps run. It does not
carry the path structure. So for an array of objects where the final leaf is a
string, the access is:

- `steps = [{ kind: "member", path: "items.1.meta.name" }]`
- `shape = "string"`

If the final leaf itself is an array, the shape stays explicit:

```json
{ "kind": "array", "of": "object" }
```

That component is still not an HTML input. The schema leaves room for future
vertical slices to opt such a surface into `includeAll`, validation, and
request gather by declaring a canonical binding value, but current proof
coverage and the current public C# gather surface are still centered on
readable bound/input components.

## Why This Is The Final Center

- `components` is the one surface registry.
- `binding` is an optional capability, not a second top-level object family.
- generated ids and manual ids are both just `componentId` at schema level; how
  the DSL produced them is not a runtime-schema concern.
- `bindingValue(componentId)` gives request/validation/includeAll one canonical
  participation lane.
- ordered `pipeline.steps[]` keeps `When -> Request -> When -> Parallel`
  sequences honest instead of flattening them into one stage.
- generic `access(root, access)` gives triggers, responses, components,
  elements, and document one shared read language.
- compositional `access.steps[]` means future slice onboarding can add
  method-return walking without redesigning the schema.
- generic `apply(target, mutation)` gives triggers, components, elements, and
  document one shared effect language.
- non-input widgets like tabs, accordion, toast, confirm, and custom buttons do
  not need special schema treatment.
- the schema leaves extension room for non-input binding participation later;
  current proof coverage for binding participation is intentionally narrower.
- the runtime stays dumb:
  - resolve root
  - execute `access.steps[]` in order
  - shape if needed
  - consume
  - apply `set` / `call` only on explicit targets

Native action-link remains a constrained projection over the same `When` +
`Request` core, not a second top-level plan family.

## Stage Mix Rules

This is the stage-mix contract the schema now claims.

- outer `pipeline.steps[]`
  - supports ordered mixing of `Command`, `When`, `Request`, and `Parallel`
  - example: `Command -> When -> Request -> When -> Parallel -> Command`
- `When.branches[].pipeline`
  - full nested `Pipeline`
  - preserves order inside each branch
- `Request.whileLoading`
  - commands only
  - this matches the current public DSL and builder enforcement
- `Request.response.onSuccess[]`
  - full nested `Pipeline`
  - can mix commands, conditions, requests, and parallel blocks in order
- `Request.response.onError[]`
  - full nested `Pipeline`
  - same capability as `onSuccess`, with required status matching
- `Request.response.chained`
  - exactly one `Request`
  - stays a request-owned continuation stage, not a generic pipeline slot
- `Parallel.requests[]`
  - each branch is a full `Request`
- `Parallel.onAllSettled`
  - commands only
  - this matches the current public DSL and builder enforcement

So the honest architecture is:

- one ordered outer pipeline language
- full nested pipelines in response handlers and conditional branches
- request-owned transport stages stay request-owned
- a few deliberately commands-only slots stay commands-only
