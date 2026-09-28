# CI

Why the workflows look the way they do, in one place, so nobody has to reinvent it. Releases:
`docs/RELEASING.md`. Branches: `docs/BRANCHING.md`. Local commands: `docs/developer-cli.md`.

## Facts this design rests on

- **The local full gate is the release-grade browser proof.** `scripts/test.sh` runs typecheck,
  assets, vitest, the gate-script self-tests, `dotnet build`, the non-Playwright dotnet tests, the
  whole Playwright suite, and the behavioral-coverage gate (0b). `scripts/test.sh --parallel` runs
  the Playwright leg as the five CI shards side by side (each with its own sandbox) and hands every
  shard's TRX to 0b: about 20 minutes instead of about 60 on a 12-core machine. CI runs the same
  scripts; it never defines a second gate.
- **CI Playwright is single-worker.** `tests/Alis.Reactive.PlaywrightTests/GlobalUsings.cs:4-5`
  declares `[assembly: Parallelizable(ParallelScope.Fixtures)]` and
  `[assembly: LevelOfParallelism(1)]`; one Kestrel sandbox per test assembly run
  (`WebServerFixture.cs:10-33`, a `[SetUpFixture]` on a free port). Playwright's own CI guide
  recommends one worker on CI "to prioritize stability and reproducibility"
  ([playwright.dev/docs/ci](https://playwright.dev/docs/ci)).
- **The unsharded suite takes about 80 minutes on `ubuntu-latest`.** The last green CI run before
  this design (run [29164122260](https://github.com/marafiq/alis-reactive/actions/runs/29164122260),
  2026-07-11, job `playwright`): `scripts/build.sh` 76 s, browser install 28 s, `dotnet test`
  from 18:50:07 to 20:09:29 = 79.4 min, 1232 tests, all passed; the 1209 tests with
  `[playwright:end]` markers sum to 4697 s of test time. The deterministic job takes about 2 min
  (run 36232653109, job `test`: 1m59s).
- **Traces and screenshots already exist on failure.** `PlaywrightTestBase.cs:61` starts a
  Playwright trace for every test; `TearDown` (`PlaywrightTestBase.cs:109-132`) saves
  `TestResults/playwright-traces/<test>.zip` and `<test>.png` only when the test failed and attaches
  both to the TRX. `scripts/playwright.sh` writes a log, a TRX and a VSTest `--diag` file per run
  and prints `[playwright:start]` / `[playwright:end]` markers with elapsed time.
- **GitHub-hosted runners are choppy.** Between 2026-06-08 and 2026-06-10 every CI Playwright leg
  was red (the unlicensed EJ2 trial modal, fixed in `2d5f9562`); since then ten consecutive legs
  were green (2026-06-10 to 2026-07-11) plus the `v1.0.0-rc.2` publish leg. Green is the norm, but
  a single 80-minute job that reruns from scratch on any hiccup is the wrong shape for it.
- **Toolchain.** .NET SDK `10.0.101` or a newer 10.0 feature band (`global.json` sets
  `rollForward: latestFeature`, so a runner that already has a newer 10.0 SDK uses it: CI builds
  with 10.0.4xx while this repo's machines build with 10.0.101); Node 22 in CI;
  `Microsoft.Playwright.NUnit` 1.63.0, `NUnit` 4.6.1, `NUnit3TestAdapter` 5.2.0
  (`tests/Alis.Reactive.PlaywrightTests/Alis.Reactive.PlaywrightTests.csproj`).

## What each workflow is for, and what gates what

| Workflow | Trigger | Jobs | Gates | Publishes |
|----------|---------|------|-------|-----------|
| `gate.yml` (reusable, `workflow_call`) | called by the three below | `test`; `playwright (<shard>)` x5; `behavioral coverage (0b)` over all shards' TRX | the one gate definition | never |
| `ci.yml` | every pull request; pushes to `main` / `release/*` (path-filtered); manual | `gate` -> `gate / test`, `gate / playwright (...)`, `gate / behavioral coverage (0b)` | PR merge (required: `gate / test`) | never |
| `nightly.yml` | 03:30 UTC Mon-Fri on `main`; manual | `gate`; `report-failure` | the watched browser signal: opens/updates issue `ci-nightly-failure` on red | never |
| `nuget-publish.yml` | push of a `v*` tag; manual runs gate only | `verify-tag` -> `gate` -> `pack-and-publish` | the release: tag shape, both suites, six packages | nuget.org + GitHub Release |
| `verify-net48.yml` | pushes and PRs to `main` / `release/*`; manual | net48 build+pack on Windows; IIS Express boot proof | PR merge (both required) | never |
| `deploy-docs.yml` | pushes to `main` touching docs or public C#; manual | `build-docs`, `deploy` | Pages deployment | GitHub Pages |

## One gate definition

`ci.yml`, `nightly.yml` and `nuget-publish.yml` all `uses: ./.github/workflows/gate.yml`
([reusable workflows](https://docs.github.com/en/actions/sharing-automations/reusing-workflows):
a same-repository call runs the called file from the same commit as the caller). The gate has two
inputs that matter (`playwright`, `retry-failed`) and one secret (`SYNCFUSION_LICENSE_KEY`). Tool
versions live in one composite action, `.github/actions/setup-toolchain/action.yml`
([composite actions](https://docs.github.com/en/actions/sharing-automations/creating-actions/creating-a-composite-action)):
.NET from `global.json`, Node 22 with the npm cache, and a NuGet package cache. `verify-net48.yml`
uses the same action, so a toolchain bump is one edit. Every job still runs the root scripts
(`scripts/test.sh --no-e2e`, `scripts/build.sh`, `scripts/playwright.sh`), so local and CI stay
one entry point.

Check names on a pull request are `gate / test`, `gate / playwright (<shard>)` and
`gate / behavioral coverage (0b)` (GitHub names a reusable workflow's jobs
`<caller job> / <called job>`); confirm with `gh pr checks <n>` before wiring required checks.

The 0b job runs only when all five shards are green. Each shard uploads its TRX files (the first
attempt and any `-retryN` attempt) as `trx-<shard>`; the job downloads them all and runs
`verify-behavioral-coverage.mjs --all --trx <every file>`. For each test the highest attempt's
outcome stands. A lost shard cannot shrink the proof: the job only starts when every shard passed
(`needs`), each shard's upload fails when it finds no TRX (`if-no-files-found: error`), and a test
a coverage map names but no TRX contains is reported "not found" and turns 0b red. (The gate's own
exit 2 for a listed-but-missing file or an empty list guards local callers.) Because
`nuget-publish.yml` needs the whole gate, a release is blocked by 0b too.
The gate's own behavior is proven by `verify-behavioral-coverage.selftest.mjs`, which
`scripts/test.sh` runs in every `gate / test` job.

## Playwright on GitHub-hosted runners: designed for choppiness

### Sharding

`scripts/playwright.sh --shard <name>` maps a name to a fixed VSTest filter
([`dotnet test --filter` grammar](https://learn.microsoft.com/en-us/dotnet/core/testing/selective-unit-tests):
`~` contains, `!~` does not contain, `|` or, `&` and, parentheses). VSTest filters have no
wildcards, so Fusion component namespaces are split by their **first letter**
(`FullyQualifiedName~.Components.Fusion.A` matches `AutoComplete`, `AIAssistView`, `Accordion`);
every letter A-Z is listed across the three Fusion shards, so a new component lands in exactly one.
The last shard is the complement of the others, so a new top-level namespace can never fall outside
every shard. `scripts/playwright.sh --shard <name> --print-filter` prints the exact filter.
Sharding is the Playwright-recommended way to parallelize across machines
([playwright.dev/docs/test-sharding](https://playwright.dev/docs/test-sharding)).

Expected time per shard, from the per-test `elapsed=` markers of run 29164122260 (single worker,
`ubuntu-latest`, 2026-07-11):

| Shard | Filter (summary) | Tests (`--list`) | Test time (2026-07-11 log) | Expected job time |
|-------|------------------|------------------|----------------------------|-------------------|
| `fusion-a-f-and-core` | Fusion A-F, `CoreBehaviors` | 217 | 719 s + 129 s = 848 s | ~16 min |
| `fusion-g-l-and-http` | Fusion G-L, `HttpPipeline` | 256 | 543 s + 385 s = 928 s | ~18 min |
| `fusion-m-z` | Fusion M-Z | 213 | 809 s | ~16 min |
| `components-and-validation` | `Components.*` except Fusion (Native, AppLevel), `Validation` | 274 | 504 s + 553 s = 1057 s | ~20 min |
| `conditions-patterns-and-rest` | everything not in the others (`Conditions`, `Patterns`, future namespaces) | 272 | 574 s + 478 s = 1052 s | ~20 min |

The test counts are the `--list` output at the commit that introduced the shards: sum 1232 = the
discovered set, union identical to the full list, zero duplicates.

`scripts/playwright.sh --shard <name> --list --no-build` prints the fully qualified names a shard
selects without running them: `dotnet test --list-tests` (with the NUnit adapter's
`DisplayName=FullName`) gives the discovered set, and because `--list-tests` ignores `--filter`
(observed: every shard listed all 1232 tests), `scripts/vstest-filter.mjs` applies the shard's
expression with the VSTest grammar (`~ !~ = != | &`, parentheses, case-insensitive values). The
partition proof is: the five lists concatenated equal the full list
(`scripts/playwright.sh --list --no-build`), every test exactly once:

```bash
for s in fusion-a-f-and-core fusion-g-l-and-http fusion-m-z components-and-validation conditions-patterns-and-rest; do
  scripts/playwright.sh --shard "$s" --list --no-build > "/tmp/shard-$s.txt"; wc -l < "/tmp/shard-$s.txt"
done
scripts/playwright.sh --list --no-build > /tmp/all.txt
cat /tmp/shard-*.txt | sort > /tmp/union.txt
diff <(sort /tmp/all.txt) /tmp/union.txt && echo "no gaps"; sort /tmp/union.txt | uniq -d | wc -l   # 0 = no duplicates
```

Job time adds about 2 min of setup (checkout, toolchain, `scripts/build.sh` 76 s, browser install
28 s). Wall clock for the browser leg: about 20 minutes instead of about 80, at roughly the same
total runner minutes. Times are from one run and will drift. Re-balance by moving a letter or a
namespace between shards in `shard_filter()`; keep every letter present exactly once and one
complement shard, then re-run the partition proof above.

### Retries: only what failed, with artifacts only on failure

`scripts/playwright.sh --retry-failed 1` re-runs **only** the failed tests once
(`scripts/trx-failed-tests.mjs` reads the failures from the TRX; the re-run filter is
`FullyQualifiedName=<fqn>|...`). The gate passes `retry-failed: 1` on PRs, nightly and releases.
Every attempt keeps its own log, TRX and diag file. Tests that fail and then pass are the **flaky
list**: `[playwright:flaky]` lines, `TestResults/observable/flaky-<stamp>.txt`, and a block in the
GitHub job summary. The `if: failure()` artifact upload carries logs, TRX, diagnostics, and the
traces and screenshots `PlaywrightTestBase` saved for the failed tests. This is the
Playwright-recommended shape (retry in CI, keep the trace of the failure:
[playwright.dev/docs/test-retries](https://playwright.dev/docs/test-retries)) implemented at the
runner level, because NUnit's `[Retry]` only retries assertion failures unless every exception type
is listed in `RetryExceptions` (NUnit 4.6.1,
[RetryAttribute](https://docs.nunit.org/articles/nunit/writing-tests/attributes/retry.html)),
and Playwright timeouts are exceptions, not assertions.

PR #58 (2026-03-25) had a 4x retry loop living in `deploy-docs.yml`; it did not survive the
workflow split. The retry now lives in the wrapper so it is the same locally and in CI.

### Timeouts

Every job declares `timeout-minutes` (GitHub's default is 360 minutes,
[workflow syntax](https://docs.github.com/en/actions/writing-workflows/workflow-syntax-for-github-actions#jobsjob_idtimeout-minutes)):
`verify-tag` 10, `test` 30, each `playwright` shard 60 (3x the expected time, room for one re-run
pass on a slow runner), `pack-and-publish` 45, net48 jobs 30 and 40, docs 20 and 10, the nightly
reporter 10. A hung shard now costs an hour, not six. Inside the run, `scripts/playwright.sh` keeps
`--blame-hang --blame-hang-timeout 10m` per test.

### Results in the job summary

Each shard publishes its TRX files to the job summary with `dorny/test-reporter` (pinned to the
commit of `v3.0.0`, `reporter: dotnet-trx`, `use-actions-summary: true`, `fail-on-error: false`
because the test step already carries the verdict). The job needs `checks: write`
([job summaries](https://docs.github.com/en/actions/writing-workflows/choosing-what-your-workflow-does/workflow-commands-for-github-actions#adding-a-job-summary),
[dorny/test-reporter](https://github.com/dorny/test-reporter)). The wrapper appends the flaky list
to the same summary.

### Flaky tests: quarantine is temporary

A test that fails and passes on re-run is flaky. A test that fails twice in a row is a regression
until proven otherwise. The convention:

1. Open an issue with the trace, name the root cause hypothesis, and fix the wait strategy or the
   test. Widening a timeout is not a fix (CLAUDE.md Rule 9).
2. If it cannot be fixed within the PR, quarantine it: add `[Category("Quarantine")]`
   ([NUnit Category](https://docs.nunit.org/articles/nunit/writing-tests/attributes/category.html)),
   add the shard exclusion `&TestCategory!=Quarantine` to every `shard_filter()` case and a sixth,
   non-blocking `quarantine` shard with `TestCategory=Quarantine`, and add the row below. The PR that
   quarantines must show the shard's test count dropping by exactly the quarantined tests.
3. Quarantine expires after **14 days**: fix and remove the category, or delete the test with the
   reason in the commit. A quarantined test is never `[Ignore]`d and never has its assertions
   weakened (BDD Rule 3, `.claude/memory/bdd-principles.md`).

| Test (FQN) | Issue | Quarantined on | Expires | Owner |
|------------|-------|----------------|---------|-------|
| _none_ | | | | |

### Gating

- **Pull requests.** The browser shards run on every PR and are visible on it, but they are **not**
  required to merge; `gate / test` and the two net48 jobs are. Reason: on a choppy runner a
  required check that flakes trains people to click "re-run" without reading, and the merge gate
  should be the deterministic proof plus a human reading the shard results. The local full gate
  (`scripts/test.sh`) remains the rule before merge (`docs/developer-cli.md`, "Test Rules").
- **Nightly on `main`.** `nightly.yml` runs the full gate on `main`'s tip each weekday and opens or
  updates a `ci-nightly-failure` issue on red. This is the watched signal: a red nightly is a
  regression on the trunk or a flaky test to quarantine, and it is nobody's PR to ignore.
  Scheduled workflows run on the default branch only and are disabled after 60 days without
  repository activity ([schedule](https://docs.github.com/en/actions/writing-workflows/choosing-when-your-workflow-runs/events-that-trigger-workflows#schedule)).
- **Tag releases: the browser shards block publishing.** Recommendation and the workflow agree
  (`nuget-publish.yml`: `pack-and-publish` needs `gate`, which includes all five shards). Evidence:
  (1) ten consecutive green legs since the license fix (2026-06-10 to 2026-07-11) plus the
  `v1.0.0-rc.2` publish leg, which meets the repo's own re-arm criterion (`high-quality-bar-tasks.md`
  T10/T14: three consecutive green runs); (2) sharding plus one in-shard re-run changes the cost of
  a flake from an 80-minute re-run to a 20-minute re-run of the failed shard
  (`gh run rerun <run-id> --failed`); (3) publishing is idempotent (`--skip-duplicate`), so a re-run
  after a flake never double-ships; (4) the alternative, "the local gate is the proof", leaves no
  recorded evidence next to the GitHub Release, which the Definition of Done
  (`.claude/rules/definition-of-done.md`) does not accept. What would justify going back to
  non-blocking: a shard that fails on two consecutive releases with no test at fault and a
  documented runner-side cause. Until then, red means stop.

## Hygiene norms in the workflows

- **Least privilege.** Every workflow declares `permissions: contents: read` at the top; jobs widen
  only what they need (`checks: write` for the shard summary, `contents: write` for the GitHub
  Release, `issues: write` for the nightly reporter, Pages scopes in `deploy-docs.yml`)
  ([security hardening](https://docs.github.com/en/actions/security-for-github-actions/security-guides/security-hardening-for-github-actions)).
- **Pinning policy.** GitHub-owned `actions/*` stay on major tags (`@v4`), the accepted trade-off
  for first-party actions; every third-party action is pinned to a full commit SHA with the version
  in a trailing comment (`dorny/test-reporter@a43b3a5f... # v3.0.0`,
  `microsoft/setup-msbuild@6fb02220... # v2`). Dependabot keeps both current.
- **Concurrency.** `ci.yml` and `verify-net48.yml` group by pull request and cancel a superseded
  PR run (`cancel-in-progress: ${{ github.event_name == 'pull_request' }}`); pushes to `main` /
  `release/*` never cancel, and `nuget-publish.yml` serializes releases with
  `cancel-in-progress: false`
  ([concurrency](https://docs.github.com/en/actions/writing-workflows/choosing-what-your-workflow-does/control-the-concurrency-of-workflows-and-jobs)).
- **Caching.** npm through `actions/setup-node` (`cache: npm`, keyed on `package-lock.json`).
  NuGet through `actions/cache` on `~/.nuget/packages`, keyed on `**/*.csproj`,
  `Directory.Build.props` and `global.json`, because `actions/setup-dotnet`'s own cache requires
  committed `packages.lock.json` files and this repo has none; enabling those means
  `<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>` in `Directory.Build.props`, a
  `dotnet restore` to generate the lock files, committing them, and restoring with `--locked-mode`
  in CI ([caching](https://docs.github.com/en/actions/writing-workflows/choosing-what-your-workflow-does/caching-dependencies-to-speed-up-workflows),
  [setup-dotnet caching](https://github.com/actions/setup-dotnet#caching-nuget-packages)).
  Playwright browsers are **not** cached: "Caching browser binaries is not recommended, since the
  amount of time it takes to restore the cache is comparable to the time it takes to download the
  binaries" ([playwright.dev/docs/ci](https://playwright.dev/docs/ci)); the observed install is
  28 s.
- **Path filters.** `ci.yml` filters pushes (not pull requests) so a docs-only push to `main` does
  not run the gate; a required check that is path-filtered on `pull_request` would never report
  and block the merge, which is why the `pull_request` trigger has no filter. The filter lists the
  gate's own files (`gate.yml`, `.github/actions/**`) so a CI change is tested by CI.
- **Dependabot.** `.github/dependabot.yml`: monthly, grouped minor/patch updates for GitHub
  Actions, npm (root workspaces and `docs-site`) and NuGet. Details in "Dependency updates" below.

## Dependency updates

- **Syncfusion is upgraded with one command.** `node scripts/pin-syncfusion.mjs <version>` moves the
  three pins together (npm `@syncfusion/ej2`, NuGet `Syncfusion.EJ2.AspNet.Core` and `Syncfusion.EJ2.MVC5`),
  pins every control package to the exact version the umbrella was published with (root `overrides`),
  and re-resolves only the Syncfusion part of `package-lock.json`; then `npm ci` and
  `scripts/test.sh --parallel`. It exists because npm resolves the umbrella's `~` ranges to the newest
  patch, and ignores new `overrides` for packages an existing lock already resolved (seen in the
  32.2.8 -> 33.1.47 upgrade: 22 control packages stayed on the skipped 33.1.49). `scripts/test.sh` runs
  `--check` in every gate, so a half-done bump fails instead of drifting. A new Syncfusion major also
  needs a license key valid for it in user-secrets and in both the Actions and Dependabot secret stores:
  a sandbox page carries `window.syncfusion={isLicValidated:true}` when the key is accepted.
- **What Dependabot proposes.** One grouped PR per ecosystem per month: minor and patch for npm and
  NuGet; for GitHub Actions every version, majors included (they track the runner's Node runtime,
  and this PR's own CI run proves them). Never npm or NuGet majors,
  never Syncfusion (npm and NuGet move together, deliberately, one patch behind the newest weekly
  release, with a new license key per major), never `Microsoft.CodeAnalysis.*` (the analyzers'
  Roslyn version is the minimum compiler every consumer needs).
- **The secret it needs.** Workflows started by Dependabot read **Dependabot secrets**, not Actions
  secrets. Without `SYNCFUSION_LICENSE_KEY` in that store the job log shows
  `Syncfusion__LicenseKey:` empty and the InPlaceEditor and Drawer tests time out behind the
  unlicensed overlay (observed on PRs #141-#152, 2026-09-26). Set it once:
  `gh secret set SYNCFUSION_LICENSE_KEY --app dependabot`.
- **How to land them: one batch, one proof.** Rather than merging each PR on its own CI run, branch
  from `main`, cherry-pick the Dependabot commits worth taking, run `scripts/test.sh` locally, and
  open one PR; Dependabot closes the PRs whose updates reached `main`. GitHub Actions bumps can only
  be proven by CI, so that one PR's run is their proof. A major upgrade is its own PR with its own
  proof (for test-framework majors: `scripts/playwright.sh --shard <s> --list` still sums to the full
  suite).

## Required checks for `main` (after the cutover)

Required: `gate / test`, `Build + pack net48 on real .NET Framework 4.8 (Windows)`,
`net48 runtime boots under IIS Express (Playwright screenshot proof)`. Not required:
`gate / playwright (...)`. The exact `gh api` command and the ruleset JSON are in
`docs/BRANCHING.md` ("Protection"); the owner applies it once the first post-cutover PR has shown
the check names.

## Changing CI: the checklist

1. Read the workflow you are changing and this page. If the change is a new step in the gate, it
   goes in `gate.yml` (once), not in a caller.
2. A tool version changes in `.github/actions/setup-toolchain/action.yml` or `global.json`, nowhere else.
3. A new shard or a re-balance changes `shard_filter()` in `scripts/playwright.sh` **and** the matrix
   in `gate.yml` **and** the table above. Every letter A-Z must appear in exactly one Fusion shard,
   and one shard must remain the complement. Prove the partition with
   `scripts/playwright.sh --shard <name> --print-filter` for each shard.
4. A third-party action is pinned to a commit SHA with a version comment; resolve it with
   `gh api repos/<owner>/<repo>/git/ref/tags/<tag>` (peel annotated tags through `git/tags/<sha>`).
5. Every new job has `timeout-minutes` and the narrowest `permissions`.
6. Lint locally: `actionlint` on `.github/workflows/*.yml`; `bash -n` on scripts.
7. Prove by behavior: a workflow change is done when a real run shows the new step doing what it
   claims (CLAUDE.md, definition of done); YAML that parses is not evidence.
8. Update the "Facts" section when a number you relied on changes.

## Open decisions (owner)

- `npm run lint` as a blocking step in `gate / test` (`high-quality-bar-tasks.md` T7; the ledger
  records lint clean on 2026-06-09).
- `verify-net48.yml` packs and asserts the fixture version `1.0.0-rc.1` (`verify-net48.yml`,
  `examples/net48-mvc-smoke/Net48MvcSmoke.csproj:84`, `_Layout.cshtml:10,18`); rename to a fixture
  version such as `0.0.0-net48smoke` in a separate PR so CI logs stop reading like a release.
- Committed `packages.lock.json` files for locked NuGet restores (see "Caching").
- A `.nvmrc` (`22`) so local Node matches CI (the dev box runs 24).
