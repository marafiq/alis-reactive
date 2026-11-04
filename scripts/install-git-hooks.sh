#!/bin/sh
set -e
cd "$(git rev-parse --show-toplevel)"

hooks="$(git rev-parse --git-path hooks)"
if [ ! -d "$hooks" ]; then
  git config --unset core.hooksPath 2>/dev/null || true
  hooks="$(git rev-parse --git-path hooks)"
fi
mkdir -p "$hooks"

cat > "$hooks/pre-commit" <<'HOOK'
#!/bin/sh
current="$(git rev-parse --abbrev-ref HEAD 2>/dev/null)"
case "$current" in
  main|release/*)
    echo "" >&2
    echo "  BRANCH GUARD — refusing a direct commit on '$current'." >&2
    echo "  '$current' moves by pull request only (docs/BRANCHING.md)." >&2
    echo "  Start a branch:  git switch -c feature/<name>   (or a worktree: CLAUDE.md, Workflow)" >&2
    echo "  Override once:   ALIS_ALLOW_PROTECTED=1 git commit ..." >&2
    echo "" >&2
    [ "$ALIS_ALLOW_PROTECTED" = "1" ] || exit 1
    ;;
esac
HOOK
chmod +x "$hooks/pre-commit"

echo "installed: $hooks/pre-commit (refuses direct commits on main and release/*)"
