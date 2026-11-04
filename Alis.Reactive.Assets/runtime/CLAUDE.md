# TS Runtime: the grammar interpreter

Root CLAUDE.md is authoritative. This directory runs the plans C# renders. The plan is a small
grammar (the sealed nodes of `Alis.Reactive/Grammar`, generated into `types/plan.ts`): objects located on elements or made by registered
plugins, handlers of their events, six statements and ten expressions. The interpreter names no
feature; every feature is a plugin.

## Layout and import directions

- `grammar/` evaluates expressions (`evaluate.ts`, operators, shapes, paths) and executes
  statements (`execute.ts`). It imports nothing outside `grammar/`, `types/`, `shared/` and
  `diagnostics/` (traces). The architecture test enforces every layer's import direction.
- `objects/` locates objects (element locators through a vendor's data, plugin locators through a
  registered plugin), and holds owners (the page, a slot) with their disposers.
- `plans/` discovers plan scripts, starts an owner (contribute plugin data, subscribe handlers,
  run `ready`), and announces `alis:mount` / `alis:unmount`.
- `plugins/` are the framework's built-in objects: `alis.document`, `alis.http`, `alis.model`,
  `alis.form`, `alis.slot`. `realtime/` holds `alis.stream` and `alis.hub`.
- `root.ts` registers the plugins, drains `window.__alisPlugins`, announces `alis:mount`, and boots
  the page.
- The framework's own component code (Native and Fusion widgets, Confirm, Drawer, Loader) is not
  here: it lives in `../components/`, constructs on `alis:mount` and queues plugins, as an app's
  does. `../bundle.ts` imports it before this runtime. Neither side imports the other.

## Posture

The runtime executes; it does not defend (root CLAUDE.md, Architecture). A checked member path is a
real boundary too; an error at one names the object, the path and the plan.

- `objects/vendor-driver.ts` follows a vendor's plan data for every component.
- `grammar/paths.ts`: a required read or call takes any present value as its holder, since text and
  numbers have members in JavaScript; a write needs an object. An optional one reads nothing past a
  missing holder.
- Sync stays sync: a handler with no awaited statement never returns a promise, so a vendor reads
  a payload's `cancel` in the same turn. Only `do`/`let` with `await` wait.
- One switch per generated union, ending in `assertNever`, so a node added in C# fails typecheck here
  before it fails in a browser. An `if` over a kind narrows to one case; it never stands in for the switch.

## Harness

`npm run typecheck` regenerates `types/plan.ts` from the C# grammar first. `npm test` runs in jsdom,
and several suites stub the network or fake a vendor. Rebuild the bundle (`scripts/build.sh`) before
`scripts/playwright.sh`.

The console log shows errors and warnings by default; a page sets another level once, with
`data-alis-trace` on `<html>` (`off`, `error`, `info`, `debug` or `trace`). The sandbox layout sets
`trace`. The Playwright base waits for `data-alis-booted` on `<html>`, which the runtime sets once the
page has booted.
