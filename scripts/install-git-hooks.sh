#!/bin/sh
# Install this repo's git hooks. Idempotent — run after clone or hook changes:
#   sh scripts/install-git-hooks.sh
#
# Why this exists: `main` and every `release/*` branch move by pull request only
# (docs/BRANCHING.md). A pre-commit guard refuses a direct commit on those
# branches, so every committer in this clone — people, Claude, subagents, Codex —
# is turned back onto a short-lived branch before the server-side ruleset would
# reject the push. Same idea as pre-commit-hooks' `no-commit-to-branch`:
# https://github.com/pre-commit/pre-commit-hooks#no-commit-to-branch
set -e
cd "$(git rev-parse --show-toplevel)"

# Root-cause guard for a known landmine: a stale ABSOLUTE core.hooksPath (left
# behind when a clone is moved on disk) can point at a directory that no longer
# exists, which silently disables ALL hooks. If the resolved hooks dir is
# missing, drop the override so git falls back to the repo's real .git/hooks.
hooks="$(git rev-parse --git-path hooks)"
if [ ! -d "$hooks" ]; then
  git config --unset core.hooksPath 2>/dev/null || true
  hooks="$(git rev-parse --git-path hooks)"
fi
mkdir -p "$hooks"

# The previous guard pinned one "active working branch" through git config. That
# concept is retired (docs/BRANCHING.md, cutover): drop the setting so nothing
# keeps reading it.
git config --unset alis.activeBranch 2>/dev/null || true

cat > "$hooks/pre-commit" <<'HOOK'
#!/bin/sh
# Protected-branch guard — installed by scripts/install-git-hooks.sh (do not
# hand-edit; re-run the installer to change it). `main` and `release/*` move by
# pull request only (docs/BRANCHING.md). Override once, rarely — for example a
# merge commit made deliberately on a release branch:
#   ALIS_ALLOW_PROTECTED=1 git commit ...
current="$(git rev-parse --abbrev-ref HEAD 2>/dev/null)"
case "$current" in
  main|release/*)
    echo "" >&2
    echo "  BRANCH GUARD — refusing a direct commit on '$current'." >&2
    echo "  '$current' moves by pull request only (docs/BRANCHING.md)." >&2
    echo "  Start a branch:  git switch -c feature/<name>   (or a worktree: CLAUDE.md Rule 14)" >&2
    echo "  Override once:   ALIS_ALLOW_PROTECTED=1 git commit ..." >&2
    echo "" >&2
    [ "$ALIS_ALLOW_PROTECTED" = "1" ] || exit 1
    ;;
esac
HOOK
chmod +x "$hooks/pre-commit"

echo "installed: $hooks/pre-commit (refuses direct commits on main and release/*)"
