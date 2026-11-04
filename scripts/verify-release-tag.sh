#!/usr/bin/env bash
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
