# Issue #86 Reviewer Challenge

## Purpose

This note is for reviewers of the issue #86 end-state plan schema.

The goal is not to defend the current draft and not to invite arbitrary JS
counterexamples. The goal is to pressure-test the proposed final schema against
the real DSL and the real runtime algebra the framework actually supports.

Start here:

- [Issue #86 Final Schema Shape](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/docs/research/2026-03-31-issue-86-final-schema-shape.md)
- [Issue #86 Runtime / Schema Proof](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/docs/research/2026-03-31-issue-86-runtime-schema-proof.md)
- [Issue #86 Exhaustive Feature Proof](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/docs/research/2026-03-31-issue-86-exhaustive-feature-proof.md)

## What The DSL Supports

Review break cases only within these supported surfaces:

- top-level `reactions`
- `Request` as a first-class unit with:
  - `gather`
  - `as`
  - `whileLoading`
  - `validate`
  - `response.onSuccess`
  - `response.onError`
  - `response.chained`
- `Parallel` with `onAllSettled`
- component, element, document, trigger, and response roots
- value reads as compositional access steps:
  - member/path access
  - method invocation
  - ordered step chains over the same root, for example `invoke` then `member`
- the same member/path semantics apply on:
  - trigger payload roots
  - response roots
  - explicit component roots
  - component `binding` access
- mutations as:
  - set property
  - call method
- trigger payload as:
  - none
  - host
  - explicit build
- optional component `binding` participation for:
  - request gather
  - `includeAll`
  - validation

The current schema does **not** claim to support:

- arbitrary JS reflection
- dynamic property-name generation at runtime
- arbitrary method chaining
- making every component a `binding` participant without an explicit canonical
  semantic value contract

## Challenge Format

Please provide a **minimal failing use case** with all of:

1. the user-level DSL intent
2. why it is supported by the framework’s curated DSL surface
3. the exact schema object(s) that cannot express it cleanly
4. why the failure is architectural, not just a missing serializer/runtime case

## High-Value Break Areas

If this schema is wrong, the most valuable break cases are likely to be in one
of these places:

- a non-input component that still participates in `binding` cleanly
- a request chain that gathers from nested response data into another request
- a validation rule or condition that cannot be expressed without copied runtime
  enrichment
- a trigger payload that still requires runtime invention
- a case where compositional access still cannot express a curated DSL read
  cleanly without adding reflection or fallback behavior
- a case where `binding` participation cannot stay self-sufficient and still
  express the supported semantic value
- a partial lifecycle scenario where trigger wiring or `binding` lookup is not
  honest about wire-time vs lazy resolution

## Known Proved Cases

These already have direct proof and should not be treated as open gaps unless a
reviewer can show the proof is incomplete:

- nested response walking through arrays and objects, for example
  `responseBody.residents.1.meta.code`
- nested trigger-payload walking through arrays and objects, for example
  `evt.residents.1.meta.name`
- nested component-root walking through arrays and objects, for example
  `items.1.meta.name`
- explicit component prop read/write
- explicit component method call with and without args
- component event payload build from declared sources
- request gather from literal, trigger, response, component, and `includeAll`
- validation lookup through component registry plus optional `binding`

## The Question For Reviewers

Given the supported DSL above, what is the smallest real use case that this
schema cannot express without:

- adding fallback behavior
- duplicating semantics in a second DTO family
- inventing payload shape in runtime
- reintroducing input-vs-non-input split as separate top-level families
