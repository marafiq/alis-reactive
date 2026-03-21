---
title: Runtime
description: How the JavaScript runtime discovers plans, boots them, and executes commands.
sidebar:
  order: 1
---

The runtime is a plan executor. It reads JSON plans that the C# builders produce and executes the intent they describe. The C# side does the thinking. The runtime follows instructions.

---

## How does the runtime discover plans?

When a page loads, the entry point (`root.ts`) runs one query:

```typescript
const planEls = document.querySelectorAll<HTMLElement>("[data-reactive-plan]");
```

Every matching element contains a JSON plan. The runtime parses each one and calls `boot()`. If the element has a `data-trace` attribute, tracing is enabled before boot begins.

There is no `window.alis`. No global state. No inline `<script>` blocks in views. The layout loads the runtime once via `<script type="module" src>`, and boot handles the rest.

---

## What happens during boot?

Boot enforces one critical invariant: **custom-event listeners must be wired before dom-ready reactions execute.**

Without this, a plan that dispatches an event on page load would fire before anyone is listening:

```json
{
  "entries": [
    { "trigger": { "kind": "dom-ready" },
      "reaction": { "commands": [{ "kind": "dispatch", "event": "resident-loaded" }] } },
    { "trigger": { "kind": "custom-event", "event": "resident-loaded" },
      "reaction": { "commands": [{ "kind": "mutate-element", "target": "status", ... }] } }
  ]
}
```

The boot function solves this with two passes:

1. **Phase 1** -- Wire all non-dom-ready triggers. These become active listeners.
2. **Phase 2** -- Execute dom-ready entries. Any dispatches land on Phase 1 listeners.

This is not an optimization. It is a correctness requirement, verified at all three test layers.

---

## How does command execution work?

When a trigger fires, the runtime calls `executeReaction()`, which dispatches by the reaction's `kind`:

| Reaction Kind | What Happens |
|---------------|-------------|
| `sequential` | Executes each command in order |
| `conditional` | Runs pre-commands, evaluates guard branches, executes the first match |
| `http` | Validates, executes a fetch request, routes the response |
| `parallel-http` | Fires multiple requests concurrently via `Promise.all` |

Each command is then dispatched by its own `kind`:

| Command Kind | What It Does |
|-------------|-------------|
| `dispatch` | Fires a `CustomEvent` on `document` |
| `mutate-element` | Applies a mutation to a DOM element or component |
| `mutate-event` | Modifies the event args object in the execution context |
| `validation-errors` | Displays server-side validation errors |
| `into` | Injects HTML from an HTTP response into a container |

---

## What is bracket notation?

This is the core execution model. Every DOM mutation and every component interaction flows through the same code path:

```typescript
// Property set
root[prop] = val;

// Method call
root[method](arg1, arg2);
```

The runtime does not have a catalog of CSS operations or component API calls. The plan tells it the property or method name, and the runtime uses bracket notation to execute it.

Two mutation kinds make this work:

**`set-prop`** -- sets a property on the resolved root:

```json
{ "kind": "set-prop", "prop": "textContent" }
```

Runtime executes: `root["textContent"] = val`

**`call`** -- calls a method, optionally chained through a sub-object:

```json
{ "kind": "call", "method": "add", "chain": "classList", "args": [{ "kind": "literal", "value": "active" }] }
```

Runtime executes: `root["classList"]["add"]("active")`

Adding a new component method or DOM mutation requires zero runtime changes. The C# module puts the right name in the plan. The runtime executes it verbatim.

---

## How does vendor resolution work?

The runtime supports multiple component vendors. The difference between them is one thing: **where the root object lives.**

| Vendor | Root |
|--------|------|
| `native` | The DOM element itself |
| `fusion` | `el.ej2_instances[0]` (the Syncfusion component instance) |

One function handles this:

```typescript
function resolveRoot(el: HTMLElement, vendor: Vendor): unknown {
  switch (vendor) {
    case "native":  return el;
    case "fusion":  return el.ej2_instances[0];
    default:        throw new Error(`unknown vendor: "${vendor}"`);
  }
}
```

This is the only place in the entire runtime that knows about `ej2_instances`. Every other module calls `resolveRoot()` and works with the result through bracket notation. They never ask "is this Syncfusion?" or "is this a checkbox?".

---

## How does source resolution work?

Commands often need dynamic values -- an event payload field, a component's current value. The plan expresses these as structured `BindSource` objects with a `kind` discriminator.

**Event source** -- walks a dot-path against the execution context:

```json
{ "kind": "event", "path": "evt.address.city" }
```

The runtime walks `ctx.evt.address.city` by splitting on dots and traversing the object graph. If any segment is null, it returns `undefined`.

**Component source** -- reads a live value from a rendered component:

```json
{ "kind": "component", "componentId": "careLevel", "vendor": "fusion", "readExpr": "value" }
```

The runtime resolves the vendor root and walks the `readExpr` path. For a native checkbox, `readExpr` is `"checked"`. For a Syncfusion dropdown, it is `"value"`. The plan carries this. The runtime just walks.

---

## What about type coercion?

HTML form values arrive as strings. Component APIs expect typed arguments. The plan declares the expected type, and the runtime converts:

```typescript
coerce(null, "string")     // ""
coerce("42", "number")     // 42
coerce("false", "boolean") // false  (not true)
coerce(value, "raw")       // value unchanged
```

Six coercion types: `string`, `number`, `boolean`, `date`, `array`, `raw`.

---

## The whole picture

1. `root.ts` discovers `[data-reactive-plan]` elements
2. Each plan is parsed and passed to `boot()`
3. `boot()` enriches entries with component metadata
4. Two-phase wiring: listeners first, dom-ready second
5. When a trigger fires, `executeReaction()` dispatches by reaction kind
6. Commands execute via bracket notation against vendor-resolved roots
7. Source values are resolved from event context or live components
8. Trace output records every step in a testable format

The runtime is not a framework. It is a plan interpreter.
