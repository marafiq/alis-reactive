# ADR-0002: The design system is the app's choice, not a component dependency

**Status:** accepted
**Date:** 2026-10-27
**Domain:** components | packaging
**Supersedes:** the "Dependency direction" of ADR-0001 (its lockstep versioning stands)

## Context

`AlisReactive.Native`, `AlisReactive.Fusion` and `AlisReactive.NativeTagHelpers` referenced
`AlisReactive.DesignSystem`. Their markup carried the design system's Tailwind utility classes,
some of which the design system's Tailwind build generated only because it scanned the component
packages' C#, and some of which no stylesheet generated at all. The drawer and loader, whose CSS
decides whether they are open, took that CSS from the design system. The Syncfusion stylesheet
carried the Alis brand. A component could not be used without the Alis look, and the look could
not be swapped.

## Decision

- No component package references the design system. The core and each component package ship the
  styling their own markup needs, under class names they own (`alis-field`, `alis-has-error`,
  `alis-drawer`, `alis-check-list`, `alis-card`, `alis-hstack`, `alis-grid-dialog-form`, ...), plain
  and complete: the core's `alis-reactive.css` is token-free, the components' read the design
  system's tokens (`--color-*`, `--font-display`) with neutral fallbacks. Fusion ships only the
  styling of its own markup; the Syncfusion theme and script are the app's, at the version Fusion
  pins.
- The design system is two stylesheets and no code. `theme.css` is one `@theme static` block of
  tokens, shipped as written and copied into every referencing project by the package's
  `.targets`, for `@import` into the app's own Tailwind build; `design-system.css` is the compiled
  default for an app with none: the tokens on `:root`, the page base, the look of the core's field
  and validation markup and the brand on Syncfusion components, scoped to `.alis-root`. It carries
  no Tailwind base and no utilities; an app that wants utilities builds them itself.
- One cascade contract for every framework stylesheet: it opens with
  `@layer theme, base, alis, components, utilities;` and keeps its looks in `alis`, so Tailwind's
  Preflight sits below them and an app's utilities above; a runtime state mark (`alis-has-error`,
  Fusion's wrapper tokens) stays unlayered, above utilities. The brand on Syncfusion components is
  declared on `.alis-root`, not `:root`, so it wins by inheritance whatever the link order.
- Every package ships its browser assets as static web assets (`_content/{PackageId}/...`);
  on net48 a build target only copies the same files into `Content\{PackageId}\`.

## Consequences

- An app references `AlisReactive.DesignSystem` (or sets the same tokens itself), links the compiled
  default or imports the theme into its own build, and puts `alis-root` on `<body>`; the sandbox
  imports the theme.
- `AlisReactive.NativeTagHelpers` has its own enums (`SpacingScale`, `ElementSpacing`, and one per
  tag helper), not the design system's. Razor attributes (`gap="Base"`) are unchanged; C# that named
  the design system's token enums for a tag helper property must use the new types.
- The design system ships no C#: an app writes utility classes against the theme in its own build.
