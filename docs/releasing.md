# Releasing

Releases follow **[Semantic Versioning 2.0](https://semver.org)** and are driven entirely by **git
tags**. The tag is the single source of truth for the published version — there is no version to bump
in code, and merging to `main` never publishes anything.

To release **any** version, push a tag `vMAJOR.MINOR.PATCH[-prerelease]`. The
`Publish NuGet packages` workflow (`.github/workflows/nuget-publish.yml`) then:

1. validates the tag is a real SemVer version (a typo'd tag fails fast, before anything is packed),
2. runs the full gate (non-browser tests + Playwright),
3. packs all six NuGets at the tag's version (with `.snupkg` symbols),
4. pushes them to nuget.org (`--skip-duplicate`; symbols ride along), and
5. cuts a GitHub Release with auto-generated notes, marked **pre-release** when the version has a
   `-` suffix.

## Every version kind

| Kind | Tag | Published version | NuGet pre-release? |
|------|-----|-------------------|--------------------|
| Alpha | `v1.0.0-alpha.1` | `1.0.0-alpha.1` | yes |
| Beta | `v1.0.0-beta.2` | `1.0.0-beta.2` | yes |
| Release candidate | `v1.0.0-rc.1` | `1.0.0-rc.1` | yes |
| **Stable / GA** | `v1.0.0` | `1.0.0` | no |
| Patch | `v1.0.1` | `1.0.1` | no |
| Minor | `v1.1.0` | `1.1.0` | no |
| Major | `v2.0.0` | `2.0.0` | no |

NuGet auto-detects pre-release from the `-suffix`, and SemVer precedence holds
(`1.0.0-alpha.1 < 1.0.0-beta.1 < 1.0.0-rc.1 < 1.0.0`). Consumers only receive a pre-release when they
opt in (`--prerelease` / "Include prerelease"); the default `dotnet add package AlisReactive` resolves
the latest **stable** version.

## How to cut a release

```bash
# 1. Make sure the commit you are releasing is on main and CI is green
#    (ci.yml + verify-net48 must be green).
git checkout main && git pull

# 2. Tag it and push the tag — this is the only thing that publishes.
git tag v1.0.0-rc.1
git push origin v1.0.0-rc.1
```

Watch it: `gh run watch $(gh run list --workflow=nuget-publish.yml --limit 1 --json databaseId -q '.[0].databaseId')`

## Rules

- **The tag is authoritative.** `VersionPrefix` in `Directory.Build.props` is only a dev/local default
  for `scripts/pack.sh` and `dotnet build`; the tag overrides it via `-p:Version` at publish.
- **Merging to `main` never publishes.** CI (`ci.yml`) only gates; publishing requires a `v*` tag.
- **Malformed tags fail fast.** A tag that is not `vMAJOR.MINOR.PATCH[-prerelease]` errors before packing.
- **Re-running a tag is safe.** Publish uses `--skip-duplicate`, so an already-published version is skipped.
- **Optional approval gate.** The publish job runs in the `nuget-release` GitHub Environment; add
  required reviewers there (Settings → Environments) if you want a manual approval click before publish.

## Robustness — handled for you (you should not need to revisit this)

| Scenario | What the pipeline does |
|---|---|
| Typo'd / non-SemVer tag (`vfoo`, `v1.0`, `v1.0.0.0`) | Fails fast before packing — nothing is published |
| Re-run the same tag, or re-push it | `--skip-duplicate` skips already-published versions; the GitHub Release is **updated**, not duplicated |
| Pack regression (missing/extra package, wrong version) | A pre-publish check requires **all six** packages at the tagged version, or the run fails **before** pushing |
| Two releases triggered at once | Serialized by a `concurrency` group; the second waits, and an in-flight publish is never cancelled |
| `NUGET_API_KEY` not configured | Fails immediately with a clear message, before packing |
| Tests red on the tagged commit | `pack-and-publish` needs `test` + `playwright` green first — nothing publishes on red |
| Partial publish (network drop mid-push) | Just re-run the workflow: published packages are skipped, the rest retry — idempotent |
| Push / merge to `main` | Publishes nothing — only a `v*` tag does |
| Reproducibility | SDK pinned via `global.json`; deterministic build (`Directory.Build.props`) |

## First publish notes

`AlisReactive.DesignSystem` is a new package ID. The first push auto-creates it on nuget.org. If you
hold the `AlisReactive.*` ID-prefix reservation, the new ID inherits it and shows as owner-verified.
