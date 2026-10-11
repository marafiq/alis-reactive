# TS Runtime: the grammar interpreter

Root CLAUDE.md is authoritative. This directory runs the plans C# renders. The plan is a small
grammar (`docs/design/grammar.md`, normative): objects located on elements or made by registered
modules, handlers of their events, six statements and nine expressions. The interpreter names no
feature; every feature is a module. Load `solid-ts-audit` for structural work.

## Layout and import directions

- `grammar/` evaluates expressions (`evaluate.ts`, operators, shapes, paths) and executes
  statements (`execute.ts`). It imports nothing outside `grammar/`, `types/` and `shared/`.
- `objects/` locates objects (element locators through a vendor's data, plugin locators through a
  registered module), and holds owners (the page, a slot) with their disposers.
- `plans/` discovers plan scripts, starts an owner (contribute module data, subscribe handlers,
  run `ready`), and announces `alis:mount` / `alis:unmount`.
- `modules/` are the framework's built-in objects: `alis.document`, `alis.http`, `alis.model`,
  `alis.form`, `alis.slot`. `realtime/` holds `alis.stream` and `alis.hub`.
  `components/` holds the app-level objects (Confirm, Drawer, Loader).
- `root.ts` registers the modules, drains `window.__alisPlugins`, and boots the page.

## Posture

The runtime executes; it does not defend. Trust framework-generated plans: no validators,
fallbacks, registries of shapes, or recovery for what the typed DSL controls. Errors belong at real
boundaries: object lookup, a checked member path, an unknown plugin, browser APIs, the network, and
JSON the framework did not produce. Each names the object, the path and the plan.

- `getElementById` only; the plan carries every id. Wide queries are allowed only for plan
  discovery and self-stamped cleanup (the architecture test's allowlist).
- A vendor is plan data (object path, subscribe, unsubscribe, payload); `objects/vendor-driver.ts`
  follows it for every component. Vendor names appear only in `components/`.
- Sync stays sync: a handler with no awaited statement never returns a promise, so a vendor reads
  a payload's `cancel` in the same turn. Only `do`/`let` with `await` wait.
- One evaluator. A second way to read a value is a defect.

## Component tripwire

Adding or changing a component requires ZERO changes here. If component work edits this
directory, the plan is missing information: make the C# side carry it.

## Harness

`npm run typecheck` (regenerates `types/plan.ts` from the C# grammar first) and `npm test` are
feedback while coding. Playwright is the only verification: rebuild the bundle, then run
`scripts/playwright.sh`. Never hand-edit `types/plan.ts`.
