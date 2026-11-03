# Branching

One long-lived branch, short-lived everything else, releases by tag. The release runbook is
`docs/RELEASING.md`; the CI that gates every branch is `docs/CI.md`.

## The model

| Ref | Purpose | Rules |
|-----|---------|-------|
| `main` | the trunk **and** the release line | moves by pull request only; required checks green; no force-push; no deletion |
| `feature/*`, `fix/*`, `chore/*`, `docs/*` | one change each | branched from `main` in a worktree (CLAUDE.md, Workflow); landed by PR within days; deleted on merge |
| `release/X.Y` | post-GA stabilization only | created from tag `vX.Y.0` only when a hotfix must ship without `main`'s newer work; PRs into it; hotfix tags `vX.Y.Z` on it; every fix forward-ported to `main` |
| `vX.Y.Z[-rc.N]` tags | the only release trigger | annotated; immutable; commit on `main` or `release/X.Y` |

Nothing else is long-lived. There is no "active working branch": the trunk is `main`, and the
question "which branch is the RC?" has no answer because an RC is a tag
(`git describe --tags --abbrev=0 main`).

Norms followed: trunk-based development with short-lived branches
([DORA](https://dora.dev/capabilities/trunk-based-development/),
[trunkbaseddevelopment.com](https://trunkbaseddevelopment.com/)); pull-request flow
([GitHub flow](https://docs.github.com/en/get-started/using-github/github-flow)); SemVer prerelease
tags ([SemVer 2.0.0](https://semver.org/spec/v2.0.0.html)); annotated tags for releases
([Pro Git](https://git-scm.com/book/en/v2/Git-Basics-Tagging)); `release/X.Y` servicing branches
after GA ([dotnet/runtime branches](https://github.com/dotnet/runtime/branches/all?query=release%2F));
branch rulesets and required status checks
([about rulesets](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/about-rulesets),
[required status checks](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/defining-the-mergeability-of-pull-requests/about-protected-branches#require-status-checks-before-merging));
protected deployment environments with required reviewers
([using environments](https://docs.github.com/en/actions/deployment/targeting-different-environments/using-environments-for-deployment));
a local "no commit on the trunk" hook
([pre-commit-hooks `no-commit-to-branch`](https://github.com/pre-commit/pre-commit-hooks#no-commit-to-branch)).

## Local guard

`sh scripts/install-git-hooks.sh` installs a `pre-commit` hook that refuses a direct commit on
`main` or `release/*` and points at this page. Override once,
deliberately: `ALIS_ALLOW_PROTECTED=1 git commit ...`. The server-side ruleset below is the real
enforcement; the hook only fails faster.

## Protection (the owner applies it)

All commands are read from the GitHub REST docs
([rulesets](https://docs.github.com/en/rest/repos/rules),
[environments](https://docs.github.com/en/rest/deployments/environments),
[deployment branch policies](https://docs.github.com/en/rest/deployments/branch-policies)).
Read what is applied with `gh api repos/marafiq/alis-reactive/rulesets` and
`gh api repos/marafiq/alis-reactive/environments/nuget-release`: until both show the rules below,
`main` takes direct pushes, tags can move, and a pushed `v*` tag publishes without an approval.

### Ruleset: `main` moves by pull request with green required checks

Required checks are the deterministic legs plus the net48 proof. The `gate / playwright (...)` jobs
and `gate / example` run on every PR and are visible, but are **not** required (`docs/CI.md`, "Gating").

```bash
cat > /tmp/main-ruleset.json <<'JSON'
{
  "name": "main: pull requests only, required checks",
  "target": "branch",
  "enforcement": "active",
  "bypass_actors": [],
  "conditions": { "ref_name": { "include": ["~DEFAULT_BRANCH"], "exclude": [] } },
  "rules": [
    { "type": "deletion" },
    { "type": "non_fast_forward" },
    { "type": "pull_request", "parameters": {
        "required_approving_review_count": 0,
        "dismiss_stale_reviews_on_push": true,
        "require_code_owner_review": false,
        "require_last_push_approval": false,
        "required_review_thread_resolution": false } },
    { "type": "required_status_checks", "parameters": {
        "strict_required_status_checks_policy": true,
        "required_status_checks": [
          { "context": "gate / test" },
          { "context": "Build + pack net48 on real .NET Framework 4.8 (Windows)" },
          { "context": "net48 runtime boots under IIS Express (Playwright screenshot proof)" } ] } }
  ]
}
JSON
gh api -X POST repos/marafiq/alis-reactive/rulesets --input /tmp/main-ruleset.json
```

`required_approving_review_count` is 0 because a single maintainer cannot approve their own PR;
the status checks are the gate. `strict_required_status_checks_policy: true` requires the PR
branch to be current with `main` before merging (the norm; set `false` to trade that safety for
fewer re-runs). Check names are the job names GitHub shows on a PR; confirm them with
`gh pr checks <n>` before applying.

### Ruleset: release tags are immutable

```bash
cat > /tmp/tag-ruleset.json <<'JSON'
{
  "name": "release tags are immutable",
  "target": "tag",
  "enforcement": "active",
  "bypass_actors": [],
  "conditions": { "ref_name": { "include": ["refs/tags/v*"], "exclude": [] } },
  "rules": [ { "type": "deletion" }, { "type": "update" } ]
}
JSON
gh api -X POST repos/marafiq/alis-reactive/rulesets --input /tmp/tag-ruleset.json
```

Creating `v*` tags stays open (that is how releases happen); deleting or moving one is blocked
for everyone, including admins. A mistaken tag that fails `verify-tag` stays in history harmlessly;
the fix is the next version, not a re-used tag.

### Environment: `nuget-release` needs an approval and accepts only `v*` tags

```bash
gh api -X PUT repos/marafiq/alis-reactive/environments/nuget-release --input - <<'JSON'
{
  "wait_timer": 0,
  "prevent_self_review": false,
  "reviewers": [ { "type": "User", "id": 9109259 } ],
  "deployment_branch_policy": { "protected_branches": false, "custom_branch_policies": true }
}
JSON
gh api -X POST repos/marafiq/alis-reactive/environments/nuget-release/deployment-branch-policies \
  -f name='v*' -f type=tag
```

`9109259` is the owner's user id (`gh api user --jq .id`). `prevent_self_review` is `false` because
the only reviewer is also the person who pushes the tag; the approval is still a deliberate second
step. The tag policy means the environment (and its secret-bearing job) can only ever run for a
`v*` ref.

### Repository settings

```bash
gh repo edit marafiq/alis-reactive --delete-branch-on-merge   # merged PR branches disappear by themselves
```
