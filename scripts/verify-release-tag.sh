#!/usr/bin/env bash
# Verifies that a git tag is fit to publish a release. nuget-publish.yml runs it in the verify-tag
# job, and it runs identically on any clone:
#   scripts/verify-release-tag.sh v1.0.0-rc.1
#
# Checks, in order, each with its own exit code so a failure is unambiguous:
#   2  usage: no tag name given
#   3  the tag does not exist in this clone (CI fetches it first; locally: git fetch origin --tags)
#   4  the tag is lightweight; release tags are annotated (git tag -a <tag> -m '<message>' <commit>)
#   5  the version is not SemVer 2.0.0 (vMAJOR.MINOR.PATCH[-prerelease])
#   7  refs/remotes/origin/main is missing, so reachability cannot be checked (git fetch origin main)
#   6  the tagged commit is not reachable from refs/remotes/origin/main or any
#      refs/remotes/origin/release/* branch; releases are cut from main or a release/X.Y hotfix line
#
# The script reads local refs only and never fetches, so it behaves the same in CI (after the
# workflow's fetch step) and on a developer machine. It prints ::error:: / ::notice:: lines that
# GitHub renders as annotations and, when GITHUB_OUTPUT is set, writes the step outputs
# version, prerelease, commit and reachable_from. Proving it bites: docs/RELEASING.md,
# "Prove the guard".
set -euo pipefail

tag="${1:-}"
if [ -z "$tag" ] || [ "$tag" = "-h" ] || [ "$tag" = "--help" ]; then
  echo "Usage: scripts/verify-release-tag.sh <tag>    e.g. scripts/verify-release-tag.sh v1.0.0-rc.4" >&2
  exit 2
fi

fail() {
  local code="$1"
  shift
  echo "::error::$*"
  exit "$code"
}

emit() {
  echo "[verify-tag] $1=$2"
  if [ -n "${GITHUB_OUTPUT:-}" ]; then
    echo "$1=$2" >> "$GITHUB_OUTPUT"
  fi
}

ref="refs/tags/$tag"
if ! git rev-parse --verify --quiet "$ref" >/dev/null; then
  fail 3 "Tag '$tag' does not exist in this clone. CI fetches it first; locally run: git fetch origin --tags"
fi

kind="$(git cat-file -t "$ref")"
if [ "$kind" != "tag" ]; then
  fail 4 "'$tag' is a lightweight tag (object type '$kind'). Release tags must be annotated so they carry a tagger, a date and a message: git tag -a $tag -m '<message>' <commit>"
fi

version="${tag#v}"
# SemVer 2.0.0: MAJOR.MINOR.PATCH with an optional -prerelease label (alpha/beta/rc/...).
if [[ ! "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?$ ]]; then
  fail 5 "Tag '$tag' is not a SemVer version. Use vMAJOR.MINOR.PATCH[-prerelease], e.g. v1.0.0-alpha.1, v1.0.0-rc.1 or v1.0.0."
fi
prerelease=false
if [[ "$version" == *-* ]]; then
  prerelease=true
fi

if ! git rev-parse --verify --quiet refs/remotes/origin/main >/dev/null; then
  fail 7 "refs/remotes/origin/main is missing, so there is nothing to check reachability against. Run: git fetch origin main"
fi

commit="$(git rev-parse "$ref^{commit}")"
reachable_from=""
for branch_ref in refs/remotes/origin/main $(git for-each-ref --format='%(refname)' 'refs/remotes/origin/release/*'); do
  if git merge-base --is-ancestor "$commit" "$branch_ref"; then
    reachable_from="${branch_ref#refs/remotes/origin/}"
    break
  fi
done
if [ -z "$reachable_from" ]; then
  fail 6 "Commit $commit (tag $tag) is not reachable from origin/main or any origin/release/* branch. Releases are cut from main (or from a release/X.Y hotfix line): merge first, then tag the merged commit. See docs/RELEASING.md."
fi

echo "[verify-tag] $tag is annotated: $(git for-each-ref "$ref" --format='%(taggername) %(taggeremail) %(taggerdate:iso8601) -- %(contents:subject)')"
emit version "$version"
emit prerelease "$prerelease"
emit commit "$commit"
emit reachable_from "$reachable_from"

summary="Release $version (prerelease=$prerelease) from annotated tag $tag at $commit, reachable from $reachable_from."
echo "::notice::$summary"
if [ -n "${GITHUB_STEP_SUMMARY:-}" ]; then
  echo "$summary" >> "$GITHUB_STEP_SUMMARY"
fi
