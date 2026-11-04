# Framework Playwright tests

Root CLAUDE.md is authoritative. These tests prove the framework itself: `.claude/rules/tests.md`,
its Framework tests section included.

- The app is `tests/Alis.Reactive.Framework.App`: the core (validation included), and the components
  of `docs/design/framework-test-components.md`, brought the way any app brings one. It consumes the design system as an app does (the
  compiled default linked, the theme in its own Tailwind build, `alis-root` on `body`); `/Validation/Notice`
  renders on `_CoreOnly`, the core's stylesheet alone, for the promise that a refusal is complete without one.
- A fixture derives from `AppPageTest` and opens its page with `Open(path)`. A slice's fixture sits at
  the same path as the slice's folder in the app (`Members/` here, `Members/` there).
- Each capability row of `docs/design/framework-test-components.md` is proven by a test here; the test
  names the row in its fixture's summary.
- Run through `scripts/playwright.sh --project framework`.
- What these tests run of the framework: `scripts/framework-coverage.sh` (npm run coverage:framework)
  records the runtime's block coverage per page and instruments the core's assemblies with
  dotnet-coverage, then writes `tests/TestResults/framework-coverage/<stamp>/report.md`: the share of
  `Alis.Reactive.Assets/runtime/**` and of the core's C# the tests ran, and every line they did not. It is feedback while
  coding, like typecheck, never a gate or a proof (root CLAUDE.md): a line it reports not run points at
  a missing test or at code the framework does not need, and a Playwright test settles which.
