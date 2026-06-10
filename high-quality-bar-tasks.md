# RC1 High-Quality-Bar Tasks

Audit date: 2026-06-09. Branch: `tiny-safe-but-important-refactorings` (PR #136, HEAD `cd095962`).
Every finding below was verified by a direct read, grep, command run, or CI log on this machine —
file:line evidence is cited per task. No task is speculative.

Priority levels:

- **MUST** — do not tag `v1.0.0-rc.1` until done. Consumer-visible breakage, unverified release
  evidence, or a standing standard of this repo violated.
- **SHOULD** — high bar for an RC; do before or immediately after tagging, with a written reason
  if deferred.
- **NICE** — quality polish; schedule freely.

Every task ends with **Done means** — observable outcomes only. If a criterion cannot be
observed (command output, file content, CI run URL, package content), the task is not done.

---

## MUST

### T1 — Make the browser suite green on the release commit

**Evidence:** Both `playwright` legs on PR #136 are red. CI run 27191010764 has **28 distinct
failing tests** (extracted from the run log), clustered — not random flake:

- 19 of 28 are `Components.Fusion.InPlaceEditor.*` (nearly the whole InPlaceEditor suite:
  edit-mode entry, lifecycle events, quick-edit commit/cancel, validation block, server-error
  surface, saved-indicator, profile-form gather/validate).
- A second cluster is the `clearing_then_refilling_*_updates_condition_both_ways` /
  `clearing_*_toggles_indicator` family across DatePicker, DateRangePicker, InputMask,
  RichTextEditor, AutoComplete (live-clear + condition re-evaluation behavior).
- Plus `WhenDrawerOpensAndCloses.drawer_header_close_button_closes_the_drawer` (failed at 124s — timeout)
  and `WhenUsingFusionContextMenu.right_click_target_opens_the_context_menu`.

`nuget-publish.yml:44` sets the Playwright job `continue-on-error: true`, so **tagging today
would publish RC1 with zero green browser evidence in the release path** — for a framework whose
only end-to-end plan-shape harness is this suite (`.claude/rules/process-layers.md`, Layer 1
"Known gaps").

**Local adjudication (observed 2026-06-09):** the full suite was run locally on this commit via
`scripts/test.sh` — **1214/1214 passed, exit 0** (TRX:
`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260609-194510.trx`).
So the 28 CI failures are environment-dependent (2-core runner timing), not a functional
regression reproducible on a dev machine. That narrows the work to wait-strategy/runner-timing
root cause — it does not lower the bar: the *release path* still has no green browser run, and
clustered timing failures (a whole component suite) mean the tests' synchronization is fragile
exactly where production consumers on slow machines would be.

**Work:** Diagnose the two clusters (InPlaceEditor suite-wide failure suggests one shared
fixture/page/timing root cause, not 19 independent bugs). Fix the wait strategies — not by
widening timeouts blindly.

**Done means:**
- [ ] One full Playwright run **on the exact commit to be tagged** completes with **0 failed**
      (full current count passed), evidenced by a CI run URL or a committed/linked TRX from
      `scripts/playwright.sh`.
- [ ] Each of the 28 named tests passes in that run (the list is reproducible via
      `gh run view 27191010764 --log-failed | grep -oE "Failed:Error elapsed=[0-9.]+s [A-Za-z0-9._]+"`).
- [ ] Zero tests were deleted, skipped, or quarantined to achieve this — or every such test is
      individually named with a written justification in the PR.
- [ ] A one-paragraph root-cause note per cluster (InPlaceEditor; clear/refill-condition family)
      exists in the PR or commit message — "flaky" alone is not a root cause.

### T2 — Fix the stale asset links in the README that ships inside every package

**Evidence:** `README.md:120-121` hardcodes `design-system.1.0.0-preview.2.css` and
`syncfusion.1.0.0-preview.2.css`. `Directory.Build.props:35` packs this README into all six
nupkgs. At RC1 the targets bake `*.1.0.0-rc.1.*` filenames, so the nuget.org instructions 404
for every consumer who copies them.

**Done means:**
- [ ] `grep -n "preview.2" README.md` returns nothing.
- [ ] The stylesheet-link instructions are version-neutral (explain that the `.targets` bakes the
      installed package version into the filename, show the pattern) — no literal version that
      can go stale again.
- [ ] After `scripts/pack.sh 1.0.0-rc.1`, unzip `nupkgs/AlisReactive.1.0.0-rc.1.nupkg` and read
      the packed `README.md`: the fix is present in the artifact, not just the repo.

### T3 — Document the `AlisReactive.FluentValidator` public API

**Evidence:** It is the only shipped package without `<GenerateDocumentationFile>` (the other
five set it: `Alis.Reactive.csproj:7`, `Fusion:6`, `Native:6`, `DesignSystem:9`,
`NativeTagHelpers:10`), and the project contains **zero** `/// <summary>` comments across ~89
public members (`ReactiveValidator<T>`, `ReactiveClientRules` with 50+ extension methods,
`FieldGuard<T>`, DI extensions). Invisible because `Directory.Build.props:18` suppresses CS1591
solution-wide. Consumers of the validation entry point get no IntelliSense at all.

**Done means:**
- [ ] `Alis.Reactive.FluentValidator.csproj` sets `<GenerateDocumentationFile>true</GenerateDocumentationFile>`.
- [ ] With CS1591 temporarily removed from `NoWarn`, `dotnet build Alis.Reactive.FluentValidator -f net10.0`
      reports **0 CS1591 warnings** for this project (run the same for `-f net48`).
- [ ] The repacked `AlisReactive.FluentValidator.*.nupkg` contains
      `lib/net48/Alis.Reactive.FluentValidator.xml` and `lib/net10.0/Alis.Reactive.FluentValidator.xml`
      (observed by listing the package contents).
- [ ] Docs follow the repo's `dotnet-xml-docs` conventions (dev-facing voice, no em-dashes).

### T4 — Pin all floating dependency versions in shipped packages

**Evidence (violates the standing production standard: pin and align, no floating `*`):**
- `Alis.Reactive.csproj:23-25` — `System.Text.Json 9.*`, `M.E.DependencyInjection.Abstractions 9.*`, `PolySharp 1.*`
- `Alis.Reactive.FluentValidator.csproj:16-17,21-22,24` — `FluentValidation 12.*` / `11.*`,
  `M.E.DependencyInjection.Abstractions 10.*` / `9.*`, `PolySharp 1.*`
- `Alis.Reactive.Native.csproj:20`, `Fusion.csproj:25`, `DesignSystem.csproj:18` — `PolySharp 1.*`

Consequence: two builds of the same tag can restore different dependency versions — the release
is not dependency-reproducible despite `Deterministic`/`ContinuousIntegrationBuild` being set.

**Done means:**
- [ ] `grep -rn 'Version="[0-9]*\.\*"' */[A-Z]*.csproj` over the six shipped projects returns
      **nothing** — every `PackageReference` names an exact version (e.g. `12.0.0`, not `12.*`).
      If `PolySharp` (build-time only, `PrivateAssets="all"`) is deliberately left floating, that
      exception is written into the csproj comment and into this file — otherwise pin it too.
- [ ] `dotnet build -c Release` green for all projects on both TFMs after pinning.
- [ ] The `.nuspec` inside each repacked nupkg shows exact dependency lower bounds (observed, not
      inferred).

### T5 — Realign CLAUDE.md and `.claude/rules` with the codebase

**Evidence — every row verified against source today (zero-tolerance item):**

| Doc says | Reality |
|---|---|
| CLAUDE.md: wide DOM queries at `root.ts:25`, `inject.ts:16`, `retry-indicator.ts:53` | `root.ts:57`, `inject.ts:21`, `retry-indicator.ts:22,48` |
| CLAUDE.md: "Scoped querySelector calls exist in error-display.ts and orchestrator.ts" | False — neither file contains `querySelector` (both use `getElementById`) |
| CLAUDE.md: "`IdGenerator` (`Alis.Reactive/IdGenerator.cs`)" | Actual: `Alis.Reactive/PlanAuthoring/ExpressionPaths/IdGenerator.cs` |
| CLAUDE.md §13: "`core/trace.ts` is 38 lines" | Actual: `runtime/diagnostics/trace.ts`, 54 lines |
| CLAUDE.md / rules: generator is "PlanTypeGenerator" | Tool project is `tools/PlanTypeGenerator`; emitted header (`plan.ts:2`) says `Alis.Reactive.PlanModel.PlanContractGenerator` — one concept, three names |
| `process-layers.md` known gaps: vendor leaks at `trigger.ts:45`, `live-clear.ts:44`; "3 ForTests functions" | Those leaks no longer exist; ForTests functions number **5**, aggregated under `resetBootStateForTests` |
| `process-layers.md`: "5 docs-site pages reference deleted IReactivePlan; 50/78 docs obsolete" | 0 `IReactivePlan` references in the active docs-site (archive-history only) |
| `ci.yml:54-55` comment: lint skipped due to "10 pre-existing eslint errors" | `npm run lint` is clean today (0 errors) |

**Done means:**
- [ ] Every row above is corrected in one commit (CLAUDE.md + `.claude/rules/process-layers.md`;
      the `ci.yml` comment is handled by T7).
- [ ] CLAUDE.md contains **no hand-maintained line numbers** — file paths and function/anchor
      names only (line numbers rot by design; this is the recurring root cause of "many tries had
      been made to fix it").
- [ ] One generator name is chosen and used consistently in CLAUDE.md, the rules files, the tool
      project, and the generated-file header.
- [ ] Re-running the eight checks above (each is a single grep/read) shows ALIGNED on all eight.

### T6 — Tag from a clean, committed tree

**Evidence:** `git status --porcelain` shows **94 uncommitted files** (7 modified, 87 untracked).
All verified to be `.claude/` skill tooling, `tools/FusionOnboarding/` discovery artifacts, and
`RC1_RELEASE_READINESS.md` — none affect pack inputs. But a release tag must be reproducible from
a clean checkout, and the readiness doc itself says it is "persisted (uncommitted)".

**Done means:**
- [ ] `git status --porcelain` prints **nothing** on the commit that gets tagged.
- [ ] Each of the 94 files was deliberately committed or deliberately deleted — no file left in
      limbo. The Fusion-onboarding artifacts and skill scripts are committed if they are part of
      the deterministic-onboarding work; otherwise removed.
- [ ] `RC1_RELEASE_READINESS.md` is either committed (as a historical audit record, updated to
      reflect Task 1 = done) or superseded by this file and deleted — decision recorded in the
      commit message.

---

## SHOULD

### T7 — Enable `npm run lint` as a blocking CI step

**Evidence:** `ci.yml:54-55` says lint is skipped because of "10 pre-existing eslint errors";
`npm run lint` ran clean today (exit 0, zero errors). The gate is free — not taking it invites
regression of finished cleanup.

**Done means:**
- [ ] `ci.yml` test job runs `npm run lint` as a blocking step; the stale comment is deleted.
- [ ] One green CI run on the PR shows the lint step executed and passed.

### T8 — Resolve the two vendor-isolation violations in the runtime

**Evidence (Rule 5: vendor knowledge only in `resolver.ts` + `event-{vendor}.ts`):**
- `runtime/execution/partials/inject.ts:31-36` — Syncfusion `ej.base.append` detection (with
  native-DOM `else` branch) inside the generic partial-injection module.
- `runtime/execution/requests/request-payload-writer.ts:143-146` — unwraps Syncfusion's
  `{ rawFile: File }` shape inside the generic gather/payload module.

Both have real reasons to exist; as written they contradict "adding a third vendor must only
touch resolver.ts." Silent rule violation is the worst state given T5.

**Done means (one of the two, per site):**
- [ ] The logic moves behind the vendor boundary (resolver-owned adapter), and
      `grep -rn "Syncfusion\|ej\.base\|rawFile" Alis.Reactive.Assets/runtime --include="*.ts"`
      hits only `resolver.ts`, `resolution/event-*.ts`, vendor adapter modules, and tests; **or**
- [ ] CLAUDE.md's vendor-isolation rule names exactly these two sites as justified exceptions
      with their rationale (Syncfusion must initialize injected HTML; uploader events carry
      `rawFile`).
- [ ] `npm run typecheck`, `npm test`, and the Playwright partial-injection + uploader tests pass
      after the change.

### T9 — Perform the nupkg content inspection (readiness-doc Task 2) on the release pack

**Evidence:** Task 1 (Fusion `net48;net10.0`) is done (commit `c06b173d`; csproj verified).
Tasks 3a/3b are proven by the green `verify-net48` CI jobs (assets land in
`Content\alisreactive`; MVC5 app boots under IIS Express with screenshot). **Task 2 — opening
the six nupkgs and checking contents bullet-by-bullet — has not been observed by anyone.**

**Done means:**
- [ ] On the pack output of the release commit (`scripts/pack.sh 1.0.0-rc.1`), each of the six
      nupkgs is unzipped and checked against the Task 2 bullet list in `RC1_RELEASE_READINESS.md`
      (per-TFM `lib/`, `build/` + `buildTransitive/` targets, version-stripped asset names,
      `analyzers/dotnet/cs/`, per-TFM dependency groups, NativeTagHelpers net10-only).
- [ ] The observed result is recorded as a checklist (PR comment or `docs/release-evidence/`)
      with each bullet marked observed-pass/observed-fail — not inferred from csproj reading.

### T10 — Restore the Playwright leg as a blocking publish gate

**Evidence:** `nuget-publish.yml:44` (`continue-on-error: true`) was justified by CI-runner
flakiness. With T1 done, the remaining work is stability, then re-arming the gate. A release
pipeline whose only browser harness is advisory will eventually ship a browser regression.

**Done means:**
- [ ] Root cause of the slow-runner failures is addressed (wait strategies, runner sizing, test
      sharding, or per-suite timeout design — documented, not guessed).
- [ ] Three consecutive green `playwright` CI runs on the main/release branch (run URLs listed).
- [ ] `nuget-publish.yml`: `continue-on-error` removed and `pack-and-publish` has
      `needs: [test, playwright]`; `docs/releasing.md` updated to match.

### T11 — Justify or remove the `?? emptyPlan` fallback in plan composition

**Evidence:** `runtime/lifecycle/applied-plans.ts:142` —
`activePlans.get(bootPlan.planId) ?? emptyPlan(bootPlan.planId)`. Rule 6 (trust generated plans)
requires every fallback to be a proven, deliberate exception; this one's invariant is unstated.

**Done means:**
- [ ] Either the fallback is removed and `npm test` stays green, **or** a comment at the site
      names the exact DSL graph node / composition path that requires it (per the Wrong Plan
      Protocol), and a vitest exercises that path by name.

---

## NICE

### T12 — Remove redundant null-coalescing noise in the runtime

**Evidence:** `runtime/browser-objects/runtime-plan.ts:146` (`getElementById(...) ?? undefined`),
`runtime/validation/orchestrator.ts:33` (`?? undefined`), `runtime/execution/requests/http.ts:251`
(`?.trim().toLowerCase() ?? ""` on a chain that cannot be undefined at that point).

**Done means:**
- [ ] The three sites read without dead coalescing; null/undefined convention is consistent per
      module; `npm run typecheck` and `npm test` green.

### T13 — Per-package READMEs for nuget.org listings

**Evidence:** `Directory.Build.props:35` packs the repo-root README into all six packages — every
listing shows identical content, including Fusion-specific instructions on non-Fusion packages.

**Done means:**
- [ ] Each shipped project packs its own `README.md` (install + minimal usage for that package);
      observed inside each repacked nupkg.

### T14 — Decide and document analyzer delivery

**Evidence:** Only `AlisReactive` bundles `Alis.Reactive.Analyzers`; consumers referencing only
`.Fusion`/`.Native` get no analyzer. Existing, deliberate behavior — but undocumented.

**Done means:**
- [ ] A written decision: either the analyzer is packed into the other packages (observed in
      their nupkgs), or the install docs state that the analyzer ships with `AlisReactive` only.

---

## Evidence status at audit time

**Observed directly (this machine, 2026-06-09):**
- PR #136 checks: deterministic `test` gate green; `verify-net48` both Windows jobs green
  (including the IIS Express boot proof); both `playwright` legs red with the 28 failures named
  in T1.
- Local `scripts/test.sh` (full gate): **all legs passed** — typecheck (regenerates `plan.ts`),
  asset build, vitest, `dotnet build`, non-Playwright dotnet tests, and the full Playwright suite
  (**1214/1214 passed**, exit 0, "All gates green"; TRX
  `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260609-194510.trx`).
  The 28 CI failures in T1 therefore do not reproduce locally — they are CI-runner
  timing-dependent, and T1's criteria target the release path's missing green run plus the
  fragile wait strategies the clusters expose.
- `npm run lint`: clean.
- Every file:line citation in T2–T14.

**Not yet observed by anyone (tracked above):** nupkg content inspection (T9); a green full
browser run on the release commit (T1).
