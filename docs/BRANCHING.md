# Branching

One long-lived branch, short-lived everything else, releases by tag. The release runbook is
`docs/RELEASING.md`; the CI that gates every branch is `docs/CI.md`.

## The model (after the cutover below)

| Ref | Purpose | Rules |
|-----|---------|-------|
| `main` | the trunk **and** the release line | moves by pull request only; required checks green; no force-push; no deletion |
| `feature/*`, `fix/*`, `chore/*`, `docs/*` | one change each | branched from `main` in a worktree (CLAUDE.md Rule 14); landed by PR within days; deleted on merge |
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

## Why the state on 2026-09-26 confused

Facts, read from git and the GitHub API on that date:

- Two trunks. `main` (`f4083947`) was 56 commits behind `tiny-safe-but-important-refactorings`
  (`0a1dca3e`), whose PR #136 into `main` had been open since 2026-06-06. `CLAUDE.md` declared the
  feature branch the "active working branch", and `scripts/install-git-hooks.sh` refused commits
  anywhere else, so the feature branch became the de facto trunk.
- A branch per release candidate: `release/1.0.0-preview1` and `release/1.0.0-rc1` (both at
  `01c4b672`, already in `main`) and `release/1.0.0-rc3` (at `0a1dca3e`, created the same day as tag
  `v1.0.0-rc.3` at the same commit). Two names for one commit invite drift, and neither is what the
  publish workflow reads (it reads the tag).
- `v1.0.0-rc.3` was cut from the feature branch, so it is not reachable from `main`;
  `v1.0.0-rc.2` is a lightweight tag (no tagger, date or message).
- 28 remote branches fully merged into `main`, and eight open pull requests (#124, #126 to #131)
  whose heads are already in `main`; #122 is an abandoned redesign draft with three commits nowhere
  else.
- No branch protection, no rulesets (`gh api repos/marafiq/alis-reactive/rulesets` -> `[]`), and
  the `nuget-release` environment had no reviewer and no deployment policy.

## Local guard

`sh scripts/install-git-hooks.sh` installs a `pre-commit` hook that refuses a direct commit on
`main` or `release/*` and points at this page. It replaces the old guard, which pinned one working
branch through `git config alis.activeBranch` (the installer removes that setting). Override once,
deliberately: `ALIS_ALLOW_PROTECTED=1 git commit ...`. The server-side ruleset below is the real
enforcement; the hook only fails faster.

## Protection (the owner applies this; the PR that added this page changed no settings)

All commands are read from the GitHub REST docs
([rulesets](https://docs.github.com/en/rest/repos/rules),
[environments](https://docs.github.com/en/rest/deployments/environments),
[deployment branch policies](https://docs.github.com/en/rest/deployments/branch-policies)).
Apply them after cutover step 1, when `main` carries the new workflows.

### Ruleset: `main` moves by pull request with green required checks

Required checks are the deterministic legs plus the net48 proof. The five `gate / playwright (...)`
jobs run on every PR and are visible, but are **not** required (`docs/CI.md`, "Gating").

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
fewer re-runs). Check names are the job names GitHub shows on a PR; confirm them on the first PR
after cutover with `gh pr checks <n>` before applying.

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

## Cutover (ordered; exact commands; run them, in this order, once)

Preconditions: this page's PR has merged into `tiny-safe-but-important-refactorings`, and the local
full gate (`scripts/test.sh`) on that branch is green.

```bash
# 1. Make main the release line: merge PR #136 with a merge commit (the branch already contains
#    merge commits from main; squashing would rewrite 56 commits of history).
gh pr merge 136 --merge

# 2. Verify: main now contains the whole release line and the RC tag.
git fetch origin --tags
git rev-list --count origin/main..origin/tiny-safe-but-important-refactorings   # expect 0
git merge-base --is-ancestor v1.0.0-rc.3 origin/main && echo "rc.3 is on main"

# 3. Retire the old working-branch guard in every clone (installer removes alis.activeBranch and
#    installs the protected-branch guard).
rm -f "$(git rev-parse --git-path hooks)/pre-commit"
sh scripts/install-git-hooks.sh

# 4. Delete the per-RC / preview release branches: the tags already identify those commits.
for b in release/1.0.0-preview1 release/1.0.0-rc1 release/1.0.0-rc3; do
  git merge-base --is-ancestor "origin/$b" origin/main && echo "$b is contained in main"
done
git push origin --delete release/1.0.0-preview1 release/1.0.0-rc1 release/1.0.0-rc3

# 5. Close the stale pull requests whose heads are already in main (verified 2026-09-26).
for pr in 124 126 127 128 129 130 131; do
  gh pr close "$pr" --comment "Closing: every commit of this branch is already in main (merged through #136 / the release line). Branch model: docs/BRANCHING.md."
done
#    #122 (abandoned redesign, three commits not in main): close as abandoned; its branch
#    codex/schema-capability-design keeps the commits.
gh pr close 122 --comment "Closing as abandoned; the redesign commits stay on codex/schema-capability-design."

# 6. Delete remote branches that are fully merged into main (dry run first, then delete).
git for-each-ref --format='%(refname:short)' --merged origin/main refs/remotes/origin \
  | grep -v -E '^origin/(main|HEAD|tiny-safe-but-important-refactorings)$' | sed 's#^origin/##'
git for-each-ref --format='%(refname:short)' --merged origin/main refs/remotes/origin \
  | grep -v -E '^origin/(main|HEAD|tiny-safe-but-important-refactorings)$' | sed 's#^origin/##' \
  | xargs -n 20 git push origin --delete

# 7. Retire the old release line itself (its content is in main; v1.0.0-rc.3 preserves the commit).
git merge-base --is-ancestor origin/tiny-safe-but-important-refactorings origin/main && \
  git push origin --delete tiny-safe-but-important-refactorings

# 8. Apply the protection above (rulesets, environment, repository setting).

# 9. Update the CLAUDE.md transition note (the "Until PR #136 merges" block) in a small PR from main.

# 10. Cut the next release candidate from main per docs/RELEASING.md.
git tag -a v1.0.0-rc.4 -m "AlisReactive 1.0.0-rc.4" "$(git rev-parse origin/main)" && git push origin v1.0.0-rc.4
```

Ordering matters at one point: between merging this page's PR into the release line and step 1,
**do not push a release tag** — the new `verify-tag` gate rejects a tag whose commit is not on
`main`, and the release line is not `main` until step 1.

## After the cutover: the one-screen answer

```bash
git describe --tags --abbrev=0 main      # the current release line, by tag
git tag --points-at origin/main          # empty means main has unreleased commits
gh release list                          # what shipped
```
