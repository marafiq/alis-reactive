## What and why

<!-- One paragraph: the behavior this PR closes and the DSL source that required it. -->

## Definition of done

The bar is `.claude/rules/definition-of-done.md`. Each box carries its evidence (the command and one
line of its output), or the gap is named instead of ticked.

- [ ] Focused behavior tests pass:
- [ ] `runtime/types/plan.ts` regenerated and `npm run typecheck` clean, or plan shape unchanged
- [ ] Page-visible change seen in a real browser (gesture and what the page showed), then pinned by a Playwright slice, or N/A
- [ ] `git diff --stat` inspected: no dead code, drift markers, or new null markers
- [ ] Labelled for the release notes (`.github/release.yml`): `enhancement`, `bug`, `refactor`, `documentation`, `breaking-change`, or `chore` / `skip-changelog` to leave it out

VERIFIED / ASSUMED / UNCHECKED:
