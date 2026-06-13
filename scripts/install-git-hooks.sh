#!/bin/sh
# Install this repo's git hooks. Idempotent — run after clone or hook changes:
#   sh scripts/install-git-hooks.sh
#
# Why this exists: a pre-commit "branch guard" refuses commits on any branch
# other than the configured active branch, so EVERY committer in this clone —
# Claude, subagents, and Codex alike — stays on the one latest-and-greatest
# branch. Documentation alone never stopped the drift; a hook does, because git
# runs it no matter who or what invokes `git commit`.
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

# Single source of truth for the active branch. Change it later with:
#   git config alis.activeBranch <name>
if ! git config alis.activeBranch >/dev/null 2>&1; then
  git config alis.activeBranch "tiny-safe-but-important-refactorings"
fi

cat > "$hooks/pre-commit" <<'HOOK'
#!/bin/sh
# Branch guard — installed by scripts/install-git-hooks.sh (do not hand-edit;
# re-run the installer to change it). All work on this clone belongs on the
# configured active branch; every committer is covered (Claude, subagents,
# Codex). Override once, rarely: ALIS_ALLOW_BRANCH=1 git commit ...
expected="$(git config alis.activeBranch 2>/dev/null || echo tiny-safe-but-important-refactorings)"
current="$(git rev-parse --abbrev-ref HEAD 2>/dev/null)"
if [ "$current" != "$expected" ]; then
  echo "" >&2
  echo "  BRANCH GUARD — refusing commit on '$current'." >&2
  echo "  All work belongs on '$expected' (latest-and-greatest)." >&2
  echo "  Switch:        git switch $expected" >&2
  echo "  Override once: ALIS_ALLOW_BRANCH=1 git commit ..." >&2
  echo "" >&2
  [ "$ALIS_ALLOW_BRANCH" = "1" ] || exit 1
fi
HOOK
chmod +x "$hooks/pre-commit"

echo "installed: $hooks/pre-commit"
echo "active branch (git config alis.activeBranch): $(git config alis.activeBranch)"
