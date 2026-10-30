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

- No component package references the design system. Each ships the styling its own markup needs,
  under class names it owns (`alis-drawer`, `alis-check-list`, `alis-card`, `alis-hstack`,
  `alis-grid-dialog-form`, ...), and reads the design system's tokens (`--color-*`,
  `--font-display`) with neutral fallbacks. Fusion ships the Syncfusion Tailwind3 theme with its
  own defaults; the brand on it is the design system's `design-system-fusion.css`.
- The core's field markup names its parts (`alis-field`, `alis-field__label`,
  `alis-field__required`, `alis-field__message`); the design system styles them.
- The design system depends on nothing. It emits every token on `:root` (`@theme static`) because
  the component stylesheets read them by name. An app references it to get the look.
- Every package ships its browser assets as static web assets (`_content/{PackageId}/...`);
  on net48 a build target only copies the same files into `Content\{PackageId}\`.

## Consequences

- An app references `AlisReactive.DesignSystem` (or sets the same tokens itself) and links its
  stylesheets; the sandbox does both.
- `AlisReactive.NativeTagHelpers` has its own enums (`SpacingScale`, `ElementSpacing`, and one per
  tag helper), not the design system's. Razor attributes (`gap="Base"`) are unchanged; C# that named
  `Alis.Reactive.DesignSystem.Tokens.*` for a tag helper property must use the new types.
- The design system's C# class helpers (`CardCss`, `TokenMap`, ...) stay, for apps that write raw
  markup in its utility classes.
