# ADR-0001: DesignSystem is a lockstep-versioned shared kernel

**Status:** accepted
**Date:** 2026-06-08
**Domain:** components | process

## Context

`AlisReactive.DesignSystem` (tokens + layout class helpers + the Tailwind CSS bundle) is a
separate NuGet package. Question raised: keep it, and should it get its own version line or
ship lockstep with the rest of the packages?

Evidence from the repo (verified at source):

- **Versioning is hard-locked.** One `<VersionPrefix>` in `Directory.Build.props`; all six
  packable projects inherit it, zero per-package overrides.
- **Dependency direction is clean.** DesignSystem has no internal dependencies. Native, Fusion,
  and NativeTagHelpers depend on it. `NativeTagHelpers` depends on it **without** the reactive
  core — a real "wants-A-without-B" consumer.
- **Cadence is asymmetric.** Last 18 months: ~9 DesignSystem commits vs ~615 core commits;
  only ~4 commits touched both. It is a stable, slow-moving foundation.
- **The markup→CSS contract is mostly typed.** Class names flow through `TokenMap`/`*Css`
  helpers (~27 call-sites). Only a couple of raw brand-class string literals remain (the
  `_optionCssClass` defaults in `NativeCheckListBuilder` and `NativeRadioGroupBuilder`).

## Decision

**Keep the package and version it in lockstep with the platform.** Treat it explicitly as a
*shared kernel*, not an independently-released library.

Independent versioning's only payoff — shipping a CSS change without rev'ing the engine — is
rarely exercised (≈9 releases would have diverged), while its cost is continuous: a
DesignSystem↔component compatibility matrix and cross-version style-drift risk. The 68:1
cadence makes DesignSystem a stable foundation, which is exactly the kind of thing you
semver-lock to the platform so consumers never have to reason about compatibility.

## Consequences

- **Positive:** No compatibility matrix. Every package always moves as one version. The split
  still pays off for the one standalone consumer (`NativeTagHelpers`).
- **Negative / accepted:** Cannot hotfix CSS independently of core. Accepted given the cadence.
- **Action taken with this decision:** Route the ~2 remaining raw class-string literals through
  the typed `TokenMap`/`CssUtils` helpers so a token rename becomes a compile error, not silent
  style drift. This hardens the contract regardless of versioning and keeps independence cheap
  to adopt later.

## Revisit trigger

Flip to an independent version line only when **either** is true:

1. DesignSystem gains an **external** consumer (outside this solution) upgrading on its own schedule, or
2. its cadence rises to routine hotfixes between core releases.

Bar for the *next* package split: another genuine standalone consumer like `NativeTagHelpers` —
not "it feels like a separate concern." This keeps the package set from sliding into a
thin-package distributed monolith.

## Alternatives Considered

- **Independent version line for DesignSystem.** Rejected now: continuous compatibility-matrix
  cost for a benefit (independent releases) the cadence data says is rarely collected.
- **Merge DesignSystem back into core.** Rejected: breaks the `NativeTagHelpers` consumer that
  wants styling without the reactive engine, and couples brand/visual change to engine change.

## References

- `Directory.Build.props` — single `VersionPrefix`.
- `Alis.Reactive.DesignSystem/` — tokens (`TokenMap`, `CssUtils`) and layout `*Css` helpers.
- `Alis.Reactive.NativeTagHelpers/*.csproj` — DesignSystem consumer with no core reference.
