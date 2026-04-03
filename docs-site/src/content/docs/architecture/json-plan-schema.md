---
title: JSON Plan Schema
description: Complete reference for the V2 reactive plan JSON contract.
sidebar:
  order: 9
---

The plan JSON is the contract between the C# DSL and the browser runtime. V2 has one active model only:

- `contracts` describe capabilities
- `objects` name rendered runtime instances
- `bindings` name readable values
- `workflows` subscribe and run actions

Every rendered plan is validated against `Alis.Reactive/Schemas/reactive-plan.schema.json`.

## Where is the schema?

The schema file lives at:

```text
Alis.Reactive/Schemas/reactive-plan.schema.json
```

C# tests validate rendered plans against this schema using `AssertSchemaValid()`.

## Top-level structure

```json
{
  "version": 2,
  "planId": "MyApp.Models.ResidentModel",
  "contracts": {
    "fusion.dropdownlist": {
      "kind": "component",
      "resolver": "fusion-instance",
      "members": {
        "value": {
          "kind": "property",
          "path": [{ "prop": "value" }],
          "shape": { "kind": "scalar", "type": "string" },
          "access": "readwrite"
        }
      }
    }
  },
  "objects": {
    "statusField": {
      "contract": "fusion.dropdownlist",
      "elementId": "Resident_Status"
    }
  },
  "bindings": {
    "Resident.Status": {
      "object": "statusField",
      "valueMember": "value",
      "shape": { "kind": "scalar", "type": "string" }
    }
  },
  "workflows": [
    {
      "when": { "kind": "dom-ready" },
      "run": {
        "kind": "set",
        "target": { "object": "statusField", "member": "value" },
        "value": { "kind": "literal", "value": "Active" }
      }
    }
  ]
}
```

| Field | Required | Description |
|-------|----------|-------------|
| `version` | Yes | Schema version. V2 plans always render `2`. |
| `planId` | Yes | Stable merge key for the logical plan. |
| `sourceId` | No | Partial owner key used for merge and removal. |
| `contracts` | Yes | Capability contracts keyed by contract name. |
| `objects` | Yes | Runtime objects keyed by object name. |
| `bindings` | Yes | Named readable values keyed by binding path. |
| `workflows` | Yes | Subscription + action pairs. |

## Contracts

Contracts are the only place allowed to know vendor resolution details and raw member paths.

| Contract Kind | Description |
|---------------|-------------|
| `component` | A rendered UI object such as a native element or Syncfusion instance. |
| `element` | A plain DOM element used as a target or readable object. |
| `event-object` | A vendor event payload exposed during a workflow. |
| `service` | A runtime service object such as toast or confirm infrastructure. |

| Resolver | Description |
|----------|-------------|
| `native-element` | Resolve the DOM element itself. |
| `fusion-instance` | Resolve `ej2_instances[0]`. |
| `event-object` | Resolve the current event payload object. |
| `context-object` | Resolve an application-owned runtime object. |

Members are either:

- `property`
- `method`

Each member declares its runtime `path`, `shape`, and access semantics.

## Objects

Objects bind a contract to a concrete runtime instance.

| Field | Required | Description |
|-------|----------|-------------|
| `contract` | Yes | Contract name from `contracts`. |
| `elementId` | No | DOM id used by native and Fusion resolvers. |

## Bindings

Bindings are the canonical readable values used by validation, requests, and predicates.

| Field | Required | Description |
|-------|----------|-------------|
| `object` | Yes | Object name from `objects`. |
| `valueMember` | Yes | Readable member name from the object's contract. |
| `shape` | Yes | The declared value shape. |

## Workflow subscriptions

Each workflow has:

- `when`: a subscription
- `run`: an action

Supported subscription kinds:

| Kind | Required Fields | Description |
|------|-----------------|-------------|
| `dom-ready` | None | Runs once after the plan is booted. |
| `document-event` | `name` | Subscribes to a named document event. |
| `object-event` | `object`, `event` | Subscribes to a contract-defined event on a named object. |
| `server-push` | `url` | Subscribes to Server-Sent Events. |
| `signalr` | `hubUrl`, `method` | Subscribes to a SignalR hub method. |

`server-push` may also include `eventType`.

## Action kinds

Supported action kinds:

| Kind | Description |
|------|-------------|
| `sequence` | Runs actions in order. |
| `branch` | Evaluates predicate cases and runs the first match. |
| `parallel` | Runs actions concurrently and can include `onSettled`. |
| `set` | Writes a value to an object member. |
| `call` | Calls an object method with arguments. |
| `dispatch` | Dispatches a document event with optional detail. |
| `request` | Executes an HTTP request plan. |
| `inject` | Injects rendered HTML into a target object. |
| `show-validation-errors` | Displays server validation results for a form. |

## Value expressions

Actions, predicates, event payload shaping, and request bodies all use the same expression system.

Supported value-expression kinds:

- `literal`
- `binding`
- `member`
- `context`
- `object`
- `array`
- `convert`
- `binding-map`

This is the core clean-break rule of V2: the runtime reads values through one expression model instead of inventing separate gather, source, and payload shapes.

## Shapes and conversion

V2 uses declared shapes rather than ad-hoc coercion flags.

Supported shape kinds:

- `scalar`
- `array`
- `object`
- `any`

Supported scalar types:

- `string`
- `number`
- `boolean`
- `date`
- `raw`

Use `convert` when an action or predicate needs a different runtime shape than the original value.

## Request plans

`request` actions contain a `request` object with:

| Field | Description |
|-------|-------------|
| `method` | HTTP verb |
| `url` | Absolute or relative endpoint |
| `input` | Query, JSON, or form-data payload |
| `validation` | Binding-based client validation |
| `before` | Actions that run before the request |
| `onSuccess` | Response handlers keyed optionally by `statusCode` |
| `onError` | Error handlers keyed optionally by `statusCode` |
| `onSettled` | Actions that always run after completion |
| `next` | Chained follow-up request |

Supported input transports:

- `query`
- `json`
- `form-data`

## Predicates

Branches and validation rules use predicates.

Supported predicate kinds:

- `compare`
- `all`
- `any`
- `not`
- `confirm`

See the [Guard Operators](../../reference/guard-operators/) page for the compare operator reference. The fluent C# API still reads naturally, but the serialized contract is now V2-native.
