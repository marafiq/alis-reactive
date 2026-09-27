# Releasing

Releases follow [Semantic Versioning 2.0.0](https://semver.org/spec/v2.0.0.html) and are driven
entirely by **annotated git tags whose commit is on `main`**. The tag is the single source of truth
for the published version: nothing is bumped in code, no branch is created per release candidate,
and merging to `main` never publishes. The publish workflow is
`.github/workflows/nuget-publish.yml`; the branch model and the one-time cutover are in
`docs/BRANCHING.md`; the CI that gates every release is described in `docs/CI.md`.

> **Cutover status.** `main` became the release line on 2026-09-26 09:55Z: it was fast-forwarded to
> `0a1dca3e`, the commit `v1.0.0-rc.3` was cut from, so that tag is reachable from `main` and this
> page applies verbatim. The remaining one-time steps (retire the per-RC branches, apply the
> protection) are in `docs/BRANCHING.md`. The publish workflow rejects a tag whose commit is not on
> `main` (or on a `release/*` branch): such a tag fails in seconds and publishes nothing.

## Where is the release candidate? (one screen)

A release is identified by its **tag**, never by a branch. There is no `release/1.0.0-rc.N`
branch; the answer to "which commit is the RC?" is always a tag lookup:

```bash
git fetch origin --tags --prune --prune-tags
git describe --tags --abbrev=0 main            # newest tag reachable from main = the current release line
git tag --points-at origin/main                 # tags exactly on main's tip (empty = main has moved since the last tag)
git tag --contains <sha>                        # every tag whose history contains that commit
git branch -r --contains v1.0.0-rc.3            # branches whose history contains a tag
git for-each-ref --sort=-creatordate --format='%(refname:short)  %(objecttype)' 'refs/tags/v*'
                                                # objecttype "tag" = annotated (required), "commit" = lightweight (rejected)
gh release list                                 # what was actually published, newest first
gh release view v1.0.0-rc.3 --json tagName,targetCommitish,isPrerelease,publishedAt,url
gh run list --workflow=nuget-publish.yml --limit 5   # one publish run per tag
```

Reading it: when `git describe --tags --abbrev=0 main` and the first line of `gh release list`
name the same tag, `main` and nuget.org agree. When `git tag --points-at origin/main` is empty,
`main` carries unreleased commits: the next release candidate is a **new tag**, not a branch.

## The model

- `main` is the trunk and the release line. Every release tag points into its history.
- `vX.Y.Z-rc.N` is an **annotated** tag on `main` and the only thing that publishes a release
  candidate. `vX.Y.Z` is the GA tag, cut the same way.
- `release/X.Y` exists only **after** `vX.Y.0` has shipped, and only when a hotfix must ship
  without `main`'s newer work. Hotfix tags `vX.Y.Z` are cut on it; the fix is forward-ported to
  `main` by pull request. Never one branch per release candidate, never one per prerelease.
- Every tag gets a GitHub Release with notes generated from merged pull requests, grouped by label
  (`.github/release.yml`). GitHub Releases are the changelog; there is no `CHANGELOG.md` to bump.

Norms followed: [SemVer 2.0.0](https://semver.org/spec/v2.0.0.html) items 9 and 11 for
prerelease identifiers and precedence; [git annotated tags](https://git-scm.com/book/en/v2/Git-Basics-Tagging)
("annotated tags are meant for release"); trunk-based development
([DORA](https://dora.dev/capabilities/trunk-based-development/),
[trunkbaseddevelopment.com](https://trunkbaseddevelopment.com/)); `release/X.Y` servicing branches
as used by [dotnet/runtime](https://github.com/dotnet/runtime/branches/all?query=release%2F);
[GitHub auto-generated release notes](https://docs.github.com/en/repositories/releasing-projects-on-github/automatically-generated-release-notes).

## Every version kind

| Kind | Tag | Published version | NuGet pre-release? |
|------|-----|-------------------|--------------------|
| Alpha | `v1.0.0-alpha.1` | `1.0.0-alpha.1` | yes |
| Beta | `v1.0.0-beta.2` | `1.0.0-beta.2` | yes |
| Release candidate | `v1.0.0-rc.4` | `1.0.0-rc.4` | yes |
| **Stable / GA** | `v1.0.0` | `1.0.0` | no |
| Patch (hotfix) | `v1.0.1` | `1.0.1` | no |
| Minor | `v1.1.0` | `1.1.0` | no |
| Major | `v2.0.0` | `2.0.0` | no |

NuGet detects a pre-release from the `-suffix`, and SemVer precedence holds
(`1.0.0-alpha.1 < 1.0.0-beta.1 < 1.0.0-rc.1 < 1.0.0`). Consumers receive a pre-release only when
they opt in (`--prerelease` / "Include prerelease"); `dotnet add package AlisReactive` resolves the
latest **stable** version.

## Cut a release candidate

```bash
# 0. Preconditions: the commit is main's tip, and its checks are green.
git fetch origin
git log -1 --oneline origin/main
gh run list --branch main --limit 6          # CI (gate) and verify-net48 green for that commit

# 1. Tag main's tip. Annotated (-a) with a message, and push ONLY the tag.
git tag -a v1.0.0-rc.4 -m "AlisReactive 1.0.0-rc.4" "$(git rev-parse origin/main)"
git push origin v1.0.0-rc.4

# 2. Watch the publish run: verify-tag -> gate (test + five browser shards + 0b) -> pack-and-publish.
gh run watch "$(gh run list --workflow=nuget-publish.yml --limit 1 --json databaseId -q '.[0].databaseId')"
```

When the `nuget-release` environment has a required reviewer (`docs/BRANCHING.md`, "Protection"),
the run pauses before `pack-and-publish` with status `waiting`. Approve it on the run page
("Review deployments"), or from the CLI:

```bash
gh api -X POST "repos/marafiq/alis-reactive/actions/runs/<run-id>/pending_deployments" \
  --input - <<'JSON'
{ "environment_ids": [16430325768], "state": "approved", "comment": "Release approved" }
JSON
```

What gets rejected, in seconds, before any suite runs: a lightweight tag (`git tag v1.0.0-rc.4`
without `-a`), a non-SemVer tag (`v1.0`, `v1.0.0.0`, `vfoo`), and a tag whose commit is not
reachable from `origin/main` or an `origin/release/*` branch.

## Prove the guard

`verify-tag` is `scripts/verify-release-tag.sh`, so it can be shown to bite on any clone without
pushing anything. It reads local refs only (`refs/tags/*`, `refs/remotes/origin/main`,
`refs/remotes/origin/release/*`) and never fetches. Exit codes: `0` fit to release, `2` usage,
`3` tag missing, `4` lightweight, `5` not SemVer, `6` not reachable from `main` / `release/*`,
`7` `origin/main` missing.

```bash
git fetch origin --tags

# 1. A good tag: annotated and reachable from main -> exit 0 and the release summary line.
scripts/verify-release-tag.sh v1.0.0-rc.3; echo "exit=$?"

# 2. A LOCAL lightweight probe tag on a commit that is not on main -> exit 4, "lightweight tag".
OFF="$(git rev-parse HEAD)"   # any commit not on main or release/*, e.g. your feature branch tip
git tag v0.0.0-probe-light "$OFF" && scripts/verify-release-tag.sh v0.0.0-probe-light; echo "exit=$?"
git tag -d v0.0.0-probe-light

# 3. A LOCAL annotated probe tag on that same commit -> exit 6, "not reachable from origin/main".
git tag -a v0.0.0-probe-annotated -m "probe" "$OFF" && scripts/verify-release-tag.sh v0.0.0-probe-annotated; echo "exit=$?"
git tag -d v0.0.0-probe-annotated

# 4. A LOCAL annotated tag with a non-SemVer name -> exit 5.
git tag -a vprobe -m "probe" origin/main && scripts/verify-release-tag.sh vprobe; echo "exit=$?"
git tag -d vprobe
```

Probe tags are local and deleted right after; never push one. Real tags today: `v1.0.0-rc.2` exits
`4` (it is lightweight); `v1.0.0-rc.1` and `v1.0.0-rc.3` exit `0` with `reachable_from=main`. Until
the per-RC branch `release/1.0.0-rc3` is deleted (`docs/BRANCHING.md`, step 4) it also vouches for
the rc.3 commit, because the guard accepts `release/*` for hotfix lines; after that step only `main`
and true hotfix lines can vouch for a tag. Proof run on 2026-09-26: rc.3 `0`, a local lightweight
probe off `main` `4`, a local annotated probe off `main` `6`.

## Cut GA

Identical to a release candidate with a stable version: `git tag -a v1.0.0 -m "AlisReactive 1.0.0"
"$(git rev-parse origin/main)" && git push origin v1.0.0`. The GitHub Release is created without
the pre-release flag and becomes "Latest"; nuget.org lists `1.0.0` as the stable version.

## Hotfix through `release/X.Y`

Only after `vX.Y.0` exists, and only when `main` already carries work that must not ship yet.

```bash
# 1. Create the stabilization branch ONCE, from the GA tag's commit.
git push origin "v1.0.0^{commit}:refs/heads/release/1.0"

# 2. Fix on a short-lived branch, open the PR INTO release/1.0 (ci.yml and verify-net48 run on it).
git switch -c fix/1.0-plan-boot-order origin/release/1.0
# ... commit, push ...
gh pr create --base release/1.0 --fill

# 3. After the merge, tag the branch tip: an annotated tag on a release/* branch passes verify-tag.
git fetch origin
git tag -a v1.0.1 -m "AlisReactive 1.0.1" "$(git rev-parse origin/release/1.0)"
git push origin v1.0.1

# 4. Forward-port the fix to main by pull request (cherry-pick), so main never lags a shipped fix.
git switch -c fix/forward-port-1.0.1 origin/main && git cherry-pick <fix-sha> && gh pr create --base main --fill
```

## What each workflow does and gates on

| Workflow | Runs on | Gates | Publishes? |
|----------|---------|-------|------------|
| `ci.yml` | every pull request; pushes to `main` and `release/*` (path-filtered) | `gate / test` (typecheck, assets, vitest, gate-script self-tests, dotnet build, dotnet tests), five `gate / playwright (<shard>)` jobs, then `gate / behavioral coverage (0b)` over all shards | never |
| `verify-net48.yml` | pushes and pull requests to `main` and `release/*` | net48 build + pack on real .NET Framework 4.8 (Windows); IIS Express boot proof of the net48 sample app | never (packs a local fixture feed) |
| `nightly.yml` | 03:30 UTC Monday to Friday on `main`; manual | the full gate; opens or updates a `ci-nightly-failure` issue when red | never |
| `nuget-publish.yml` | push of a `v*` tag; manual runs exercise the gates only | `verify-tag` (annotated, SemVer, reachable from `main`/`release/*`) -> `gate` (test + all shards + 0b, **blocking**) -> `pack-and-publish` in the `nuget-release` environment | **only on a tag** |
| `deploy-docs.yml` | pushes to `main` touching the docs site or the public C# surface | docs-site build | GitHub Pages |

`nuget-publish.yml`'s `pack-and-publish` job asserts that exactly the six expected packages exist at
the tagged version before pushing, pushes with `--skip-duplicate` (re-runs are idempotent), then
creates or updates the GitHub Release with the packages attached and notes generated per
`.github/release.yml`.

## Verify a release

```bash
gh run view <run-id>                           # verify-tag, gate / test, all gate / playwright (...), pack-and-publish green
gh release view v1.0.0-rc.4                    # six .nupkg assets, pre-release flag, generated notes

# nuget.org's flat container lists every version of a package id (lower-case id; indexing lags a few minutes):
curl -s https://api.nuget.org/v3-flatcontainer/alisreactive/index.json | jq -r '.versions[-3:][]'
for id in alisreactive alisreactive.native alisreactive.fusion alisreactive.fluentvalidator alisreactive.designsystem alisreactive.nativetaghelpers; do
  printf '%-34s ' "$id"; curl -s "https://api.nuget.org/v3-flatcontainer/$id/index.json" | jq -r '.versions | index("1.0.0-rc.4") != null'
done
```

The package page is `https://www.nuget.org/packages/AlisReactive/1.0.0-rc.4`. The consumer-side
proof is a scratch project: `dotnet add package AlisReactive --version 1.0.0-rc.4` followed by a
build that lands `wwwroot/scripts/alis-reactive.1.0.0-rc.4.js`
([flat container API](https://learn.microsoft.com/en-us/nuget/api/package-base-address-resource)).

## Roll back

nuget.org [does not delete published versions](https://learn.microsoft.com/en-us/nuget/nuget-org/policies/deleting-packages);
it **unlists** them: hidden from search and from the package page, still restorable by exact
version so nobody's build breaks.

```bash
# Unlist every one of the six package ids at the bad version (dotnet nuget delete unlists on nuget.org).
for id in AlisReactive AlisReactive.Native AlisReactive.Fusion AlisReactive.FluentValidator AlisReactive.DesignSystem AlisReactive.NativeTagHelpers; do
  dotnet nuget delete "$id" 1.0.0-rc.4 --source https://api.nuget.org/v3/index.json --api-key "$NUGET_API_KEY" --non-interactive
done

# Mark the GitHub Release accordingly; never delete or move the tag (the tag ruleset blocks it anyway).
gh release edit v1.0.0-rc.4 --prerelease --notes "Unlisted from nuget.org on <date>: <reason>. Use 1.0.0-rc.5 or later."
```

Then fix forward: the next tag (`v1.0.0-rc.5`) is the remedy. Re-tagging a used version is
impossible by design (nuget.org rejects a re-push of an existing version, and the tag is immutable).
Reference: [`dotnet nuget delete`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-nuget-delete).

## Rules

- **The tag is authoritative.** `VersionPrefix` in `Directory.Build.props` is a dev/local default for
  `scripts/pack.sh` and `dotnet build`; the tag overrides it through `-p:Version` at publish.
- **Annotated only.** `git tag -a ... -m ...`. A lightweight tag is rejected by `verify-tag`
  (`git cat-file -t` must print `tag`), because a release needs a tagger, a date and a message.
- **On `main` (or `release/X.Y`) only.** `verify-tag` requires the tagged commit to be reachable
  from `origin/main` or an `origin/release/*` branch. Merge first, then tag the merged commit.
- **Merging to `main` never publishes.** CI only gates; publishing requires a `v*` tag.
- **The browser suite blocks a release.** All five shards must be green; the evidence for making
  it blocking is in `docs/CI.md` ("Gating"). A red shard is re-run with `gh run rerun <run-id> --failed`.
- **Re-running a tag is safe.** `--skip-duplicate` skips already-published versions and the GitHub
  Release is updated, not duplicated.
- **Approval gate.** `pack-and-publish` runs in the `nuget-release` environment; with a required
  reviewer configured (`docs/BRANCHING.md`), nothing reaches nuget.org without a click.
- **No branch per release.** The RC is the tag. `release/X.Y` is a post-GA hotfix line only.

## Robustness: handled for you

| Scenario | What the pipeline does |
|---|---|
| Lightweight tag (`git tag v1.0.0-rc.4` without `-a`) | `verify-tag` fails in seconds; nothing runs, nothing is published |
| Tag on a feature branch, or on a commit not merged to `main` | `verify-tag` fails: commit not reachable from `origin/main` / `origin/release/*` |
| Typo'd / non-SemVer tag (`vfoo`, `v1.0`, `v1.0.0.0`) | `verify-tag` fails before packing |
| Deterministic suite red on the tagged commit | `gate / test` red; `pack-and-publish` never starts |
| A browser shard red | Failed tests are re-run once inside the shard; still red = the release stops; re-run failed jobs after fixing or quarantining (`docs/CI.md`) |
| Re-run the same tag, or re-push it | `--skip-duplicate` skips published versions; the GitHub Release is updated, not duplicated |
| Pack regression (missing/extra package, wrong version) | The six-package assertion fails **before** anything is pushed |
| Two releases triggered at once | Serialized by the `nuget-release` concurrency group; an in-flight publish is never cancelled |
| `NUGET_API_KEY` not configured | Fails immediately with a clear message, before packing |
| Partial publish (network drop mid-push) | Re-run the workflow: published packages are skipped, the rest are pushed |
| Push / merge to `main` | Publishes nothing; only a `v*` tag does |
| Reproducibility | SDK pinned by `global.json`; deterministic build (`Directory.Build.props`); the release summary names the exact commit |
