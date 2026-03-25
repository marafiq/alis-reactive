---
title: JSON Plan Schema
description: Complete reference for the reactive plan JSON structure — every kind, every field.
sidebar:
  order: 9
---

The plan JSON is the contract between the C# DSL and the JS runtime. Every plan is validated against the schema at `Alis.Reactive/Schemas/reactive-plan.schema.json`.

## How do I find the schema?

The schema file lives at:
```
Alis.Reactive/Schemas/reactive-plan.schema.json
```

C# unit tests validate rendered plans against this schema using `AssertSchemaValid()`.

---

## What is the top-level structure?

```json
{
  "planId": "MyApp.Models.ResidentModel",
  "components": {
    "Physician": {
      "id": "physician-field",
      "vendor": "fusion",
      "readExpr": "value",
      "componentType": "autocomplete",
      "coerceAs": "string"
    }
  },
  "entries": [
    {
      "trigger": { "kind": "dom-ready" },
      "reaction": { "kind": "sequential", "commands": [...] }
    }
  ]
}
```

| Field | Required | Description |
|-------|----------|-------------|
| `planId` | Yes | `typeof(TModel).FullName` -- used as the runtime merge key |
| `components` | Yes | Map of binding path to component registration. Used by gather and validation. |
| `entries` | Yes | Array of trigger + reaction pairs |

Each component entry carries:

| Field | Required | Description |
|-------|----------|-------------|
| `id` | Yes | DOM element ID |
| `vendor` | Yes | `"native"` or `"fusion"` — determines `resolveRoot()` strategy |
| `readExpr` | Yes | Dot-path from vendor root to readable value (e.g., `"value"`, `"checked"`) |
| `componentType` | Yes | Semantic label (e.g., `"datepicker"`, `"numerictextbox"`) |
| `coerceAs` | Yes | Coercion type inferred from `typeof(TProp)` — `"string"`, `"number"`, `"boolean"`, `"date"`, `"array"`, `"raw"`. Used by gather (daterange decomposition) and validation (field enrichment). |

---

## What trigger kinds exist?

| Kind | Required Fields | Description |
|------|----------------|-------------|
| `dom-ready` | -- | Fires once when the page loads |
| `custom-event` | `event` | Fires when a named `CustomEvent` is dispatched on `document` |
| `component-event` | `componentId`, `jsEvent`, `vendor` | Fires when a component emits a vendor-specific event |

Optional fields on `component-event`: `bindingPath`, `readExpr`.

---

## What reaction kinds exist?

| Kind | Required Fields | Description |
|------|----------------|-------------|
| `sequential` | `commands` | Executes commands in order |
| `conditional` | `branches` | Evaluates guards, executes the first matching branch |
| `http` | `request` | Sends an HTTP request with gather, handlers, and optional chaining |
| `parallel-http` | `requests` | Sends multiple requests concurrently via `Promise.all` |

### Conditional reaction

```json
{
  "kind": "conditional",
  "commands": [...],
  "branches": [
    { "guard": { "kind": "value", ... }, "reaction": { "kind": "sequential", ... } },
    { "guard": null, "reaction": { "kind": "sequential", ... } }
  ]
}
```

A branch with `"guard": null` is the else branch.

---

## What command kinds exist?

| Kind | Required Fields | Description |
|------|----------------|-------------|
| `dispatch` | `event` | Dispatches a `CustomEvent` on `document` |
| `mutate-element` | `target`, `mutation` | Mutates a DOM element or component |
| `mutate-event` | `mutation` | Mutates the event args object in the execution context |
| `validation-errors` | `formId` | Triggers client-side validation for a form |
| `into` | `target` | Injects HTML response into a target element |

All commands accept an optional `when` guard.

---

## What mutation kinds exist?

| Kind | Required Fields | Description |
|------|----------------|-------------|
| `set-prop` | `prop` | Property assignment: `root[prop] = value` |
| `call` | `method` | Method call: `root[method](...args)` |

### set-prop

```json
{ "kind": "set-prop", "prop": "textContent", "coerce": "number" }
```

Optional `coerce` applies type coercion to the value before assignment.

### call

```json
{
  "kind": "call",
  "method": "add",
  "chain": "classList",
  "args": [{ "kind": "literal", "value": "active" }]
}
```

Optional `chain` navigates to a sub-object before calling the method. Each arg is either a `LiteralArg` (`kind: "literal"`) or a `SourceArg` (`kind: "source"`) with optional coercion.

---

## How are values resolved?

### Static values

The `value` field on `mutate-element` provides a static value:

```json
{ "kind": "mutate-element", "target": "status",
  "mutation": { "kind": "set-prop", "prop": "textContent" },
  "value": "loaded" }
```

### Source binding

The `source` field provides a `BindSource` that the runtime resolves at execution time:

| Source Kind | Fields | Description |
|-------------|--------|-------------|
| `event` | `path` | Dot-path into the execution context (e.g., `evt.address.city`) |
| `component` | `componentId`, `vendor`, `readExpr` | Reads a live component value |

When `source` is present, it takes priority over `value`.

---

## What coercion types are available?

| Type | Behavior |
|------|----------|
| `string` | `null` becomes `""` |
| `number` | `NaN` becomes `0` |
| `boolean` | `"false"` becomes `false`, `""` becomes `false` |
| `date` | Parses as date |
| `raw` | No coercion, pass through |
| `array` | Ensures the value is an array |

---

## What vendor values are valid?

| Vendor | Root Resolution |
|--------|----------------|
| `native` | The DOM element itself (`el`) |
| `fusion` | The Syncfusion instance (`el.ej2_instances[0]`) |

---

## What gather kinds exist?

Used in HTTP reactions to collect values for the request:

| Kind | Fields | Description |
|------|--------|-------------|
| `component` | `componentId`, `vendor`, `name`, `readExpr` | Reads a component value |
| `static` | `param`, `value` | Includes a fixed value |
| `event` | `param`, `path` | Reads from the event payload |
| `all` | -- | Gathers all registered components |

---

## What guard kinds exist?

Used in `when` conditions and conditional reaction branches:

| Kind | Fields | Description |
|------|--------|-------------|
| `value` | `source`, `coerceAs`, `op` | Evaluates an operator against a source value |
| `all` | `guards` (min 2) | All guards must pass (logical AND) |
| `any` | `guards` (min 2) | At least one guard must pass (logical OR) |
| `not` | `inner` | Inverts the inner guard |
| `confirm` | `message` | Shows a confirmation dialog, blocks on user response |

See the [Guard Operators](../../reference/guard-operators/) page for the full operator reference.
