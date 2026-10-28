# Framework Playwright tests

Root CLAUDE.md is authoritative. These tests prove the framework itself, and they follow
`.claude/memory/framework-test-principles.md`, not the component rules in `bdd-principles.md`.

- The app is `tests/Alis.Reactive.Framework.App`: the core and the validator package, and the components
  of `docs/design/framework-test-components.md`, brought the way any app brings one. No component
  library, no test doubles, no log-line assertions.
- A fixture derives from `AppPageTest` and opens its page with `Open(path)`. A slice's fixture sits at
  the same path as the slice's folder in the app (`Members/` here, `Members/` there).
- Each capability row of `docs/design/framework-test-components.md` is proven by a test here; the test
  names the row in its fixture's summary.
- Run through `scripts/playwright.sh --project framework`.
