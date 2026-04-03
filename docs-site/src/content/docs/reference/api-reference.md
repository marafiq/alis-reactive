---
title: API Reference
description: Public entry points and core concepts for the V2 authoring surface.
sidebar:
  order: 0
---

This page is a curated reference for the active V2 API surface.

For architecture and examples, see:

- [Plan Model](../../csharp-modules/plan-model/)
- [Subscriptions and Workflows](../../csharp-modules/reactivity/subscriptions-and-workflows/)
- [The JSON Plan Contract](../../architecture/the-contract/)

## Core plan types

### `ReactivePlan<TModel>`

Owns plan authoring for one Razor view or partial.

Responsibilities:

- register contracts
- register runtime objects
- register bindings
- add workflows
- render the V2 JSON document

### `ComponentRef<TComponent, TModel>`

Typed handle for authoring component member access.

Responsibilities:

- target a named runtime object
- flow through component-specific extension methods
- preserve compile-time safety for reads, writes, and calls

### `PipelineBuilder<TModel>`

Primary workflow authoring surface.

Responsibilities:

- build actions
- build requests
- build predicates
- compose nested branches and parallel execution

## Top-level HTML helpers

### `Html.ReactivePlan<TModel>()`

Creates a root plan for a page.

### `Html.ResolvePlan<TModel>()`

Creates a partial-owned plan that merges into an existing root plan by `planId`.

### `Html.RenderPlan(plan)`

Serializes the current V2 document into the page.

### `Html.On(plan, t => ...)`

Adds document-level subscriptions and workflows.

## Workflow authoring

### Subscriptions

Supported subscription families:

- `DomReady`
- `CustomEvent`
- `ServerPush`
- `SignalR`
- component `.Reactive(...)`

### Actions

Supported action families:

- element and component member writes
- method calls
- dispatch
- request execution
- partial injection
- validation display
- branching and parallel composition

## Request authoring

`Get`, `Post`, `Put`, and `Delete` author V2 request actions.

Request composition supports:

- `query`
- `json`
- `form-data`
- before/onSuccess/onError/onSettled actions
- chained requests
- parallel requests
- validation by binding

## Validation authoring

Validation flows through bindings and field rules, not late runtime enrichment.

Key ideas:

- rules are extracted on the C# side
- bindings identify readable field values
- runtime evaluates the rendered rule set directly

## Component vertical slices

Each component slice should contribute:

- capability contract
- runtime object registration
- typed member helpers
- event payload shapes
- workflow wiring
- vertical-slice tests

If a feature needs a second schema path or a runtime fallback, the design is wrong.
