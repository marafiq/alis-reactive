---
title: Onboarding a Component
description: How to add a new component as a vertical slice in the V2 architecture.
sidebar:
  order: 8
---

Adding a component means adding one vertical slice that participates in the V2 model.

## The slice owns

- the component contract
- typed member helpers
- event payload shapes
- HTML helper / builder surface
- workflow wiring through `.Reactive(...)`
- unit tests
- runtime behavior only where the contract genuinely needs it

## Authoring checklist

1. Define the component class with its vendor and readable member paths.
2. Add builder and HTML helper types for rendering and fluent authoring.
3. Register runtime objects and bindings from the slice.
4. Add typed extension methods that emit V2 member access, actions, or reads.
5. Define event payload types and event catalogs.
6. Wire `.Reactive(...)` to create an object-event workflow scope.
7. Add unit tests, drift coverage, and browser coverage.

## Design rules

- Do not add a second schema path.
- Do not add vendor checks outside the slice or the shared resolver.
- Do not add runtime fallbacks for missing contract data.
- Do not serialize ad-hoc JSON just for this component.

## What success looks like

The runtime should be able to work with the new component using the same four ideas it already understands:

- contract
- object
- binding
- workflow

If the slice needs more than that, stop and redesign the contract instead of leaking a special case into the framework.
