## What and why

<!-- One paragraph: the behavior this PR closes and the DSL source that required it. -->

## Definition of done

Playwright is the proof (`CLAUDE.md`). Each box carries its evidence (the command and one line of
its output), or the gap is named instead of ticked.

- [ ] Behavior proven by a Playwright test that fails when it breaks (`scripts/playwright.sh` output line), or N/A
- [ ] `runtime/types/plan.ts` regenerated and `npm run typecheck` clean, or plan shape unchanged
- [ ] `git diff --stat` inspected: no dead code, drift markers, or new null markers
- [ ] Labelled for the release notes (`.github/release.yml`): `enhancement`, `bug`, `refactor`, `documentation`, `breaking-change`, or `chore` / `skip-changelog` to leave it out

VERIFIED / ASSUMED / UNCHECKED:
