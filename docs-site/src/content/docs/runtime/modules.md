---
title: Runtime Modules
description: Every module group in the runtime, what it does, and where it lives.
sidebar:
  order: 2
---

The runtime is organized into seven module groups under `Scripts/`. Each group has a clear boundary.

```
Scripts/
├── root.ts                     ← ESM entry point
├── core/                       ← Pure utilities, zero side effects
├── resolution/                 ← Where values come from
├── execution/                  ← Where behavior happens
├── lifecycle/                  ← Boot, merge, enrichment
├── conditions/                 ← Guard evaluation
├── validation/                 ← Form rules and error display
├── components/                 ← Component-specific side effects
│   ├── native/                 ← Drawer, loader, checklist, action-link
│   ├── fusion/                 ← Confirm dialog
│   └── lab/                    ← Test widget (architecture tests)
└── types/                      ← TypeScript interfaces
```

---

## Core

Pure utilities with no DOM access and no state.

| Module | Purpose |
|--------|---------|
| `walk.ts` | Traverses a dot-notation path on any object. `walk(root, "a.b.c")` returns `root.a.b.c`. Returns `undefined` if any segment is null. |
| `coerce.ts` | Type coercion: `string`, `number`, `boolean`, `date`, `array`, `raw`. Handles HTML form string values (e.g., `"false"` coerces to `false`, not `true`). |
| `trace.ts` | Scoped loggers producing deterministic single-string output: `[alis:scope] message {"data":"here"}`. Six levels: `off` through `trace`. |
| `assert-never.ts` | Exhaustive switch helper. Catches unhandled `kind` values at both compile time (TypeScript) and runtime (thrown error). |

---

## Resolution

Two modules that resolve where values come from.

| Module | Purpose |
|--------|---------|
| `resolver.ts` | Dispatches by `BindSource` kind. Event sources walk the execution context via `walk()`. Component sources delegate to `component.ts`. Also provides `resolveSourceAs()` for resolution + coercion in one call. |
| `component.ts` | Vendor root resolution -- the only module that knows about `ej2_instances`. Exports `resolveRoot(el, vendor)` and `evalRead(id, vendor, readExpr)`. Unknown vendors throw immediately. |

---

## Execution

Modules that turn plan commands into DOM mutations, events, HTTP requests, and injections.

| Module | Purpose |
|--------|---------|
| `execute.ts` | Reaction dispatcher. Routes `sequential`, `conditional`, `http`, and `parallel-http` reactions to their handlers. Fully async -- any branch may contain an HTTP reaction. |
| `commands.ts` | Command dispatcher. Routes `dispatch`, `mutate-element`, `mutate-event`, `validation-errors`, and `into` to their handlers. Supports per-command `when` guards. |
| `element.ts` | Bracket-notation execution. Handles `set-prop` (property assignment) and `call` (method invocation with per-argument resolution and coercion). |
| `trigger.ts` | Event listener wiring for `dom-ready`, `custom-event`, and `component-event` triggers. All listeners use `AbortSignal` for clean teardown. |
| `gather.ts` | Collects values from components and events for HTTP request bodies. Supports URL parameters, JSON body, and FormData transport. |
| `http.ts` | Builds and executes `fetch()` calls. Routes responses to `onSuccess`/`onError` handlers. Supports chained requests. |
| `pipeline.ts` | Orchestrates the HTTP lifecycle: pre-fetch commands, validation gate, request execution, parallel request coordination via `Promise.all`. |
| `inject.ts` | Injects HTML into a container. Extracts any `[data-reactive-plan]` elements from the injected HTML and merges them into the booted plan. |

---

## Lifecycle

Boot, plan composition, and pre-wiring enrichment.

| Module | Purpose |
|--------|---------|
| `boot.ts` | Two-phase boot: wire listeners first, execute dom-ready second. Also handles `mergePlan()` for partial view updates and `resetBootStateForTests()` for vitest cleanup. |
| `merge-plan.ts` | Composes partial plans. Multiple `[data-reactive-plan]` elements with the same `planId` are merged before boot. Handles both initial composition and runtime merges (e.g., AJAX partial loads). |
| `enrichment.ts` | Resolves component references in plan entries to concrete IDs, vendors, and read expressions from the `ComponentsMap`. Runs before trigger wiring. |
| `walk-reactions.ts` | Traverses the reaction tree to find validation descriptors and wire live-clear listeners. |

---

## Conditions

| Module | Purpose |
|--------|---------|
| `conditions.ts` | Evaluates guard expressions. Five guard kinds: `value` (20+ operators), `all` (AND), `any` (OR), `not` (invert), `confirm` (async user dialog). Both sync and async evaluation paths exist. |

---

## Validation

Client-side validation driven by plan descriptors.

| Module | Purpose |
|--------|---------|
| `orchestrator.ts` | Entry point for form validation. Reads field values, evaluates rules, displays errors. Fail-closed: every declared field must be accounted for. |
| `rule-engine.ts` | Pure rule evaluation. Takes a value and a rule, returns pass/fail. No DOM, no vendor. Supports `required`, `minLength`, `maxLength`, `range`, `email`, `regex`, cross-field comparisons, and more. |
| `condition.ts` | Evaluates conditional validation rules (e.g., "Phone required when Contact Method is Phone"). |
| `error-display.ts` | Renders inline errors next to fields and summary errors in a panel. Handles clearing and server-side error display. |
| `live-clear.ts` | Wires one-shot listeners that clear field errors when the user corrects an invalid input. |
| `index.ts` | Barrel export for the validation module. |

---

## Components

Side-effect modules for specific component behaviors that cannot be expressed as plan commands.

| Module | Purpose |
|--------|---------|
| `native/drawer.ts` | Wires close button and Escape key for native drawer component. |
| `native/loader.ts` | Handles target positioning and timeout for loader overlays. |
| `native/native-action-link.ts` | Intercepts action link clicks and dispatches plan events. |
| `fusion/confirm.ts` | Initializes the Syncfusion confirm dialog component. |
| `lab/test-widget.ts` | Test component with `ej2_instances` pattern for architecture regression tests. |

---

## Types

Nine domain-specific type files plus a barrel export. TypeScript interfaces mirror the JSON plan schema exactly: `plan.ts`, `triggers.ts`, `reactions.ts`, `commands.ts`, `sources.ts`, `guards.ts`, `context.ts`, `http.ts`, `validation.ts`.
