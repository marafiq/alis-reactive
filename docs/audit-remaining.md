# Audit & Quality — Remaining Items

## Done: Component Audit
- [x] `components/native/drawer.ts` — PASS. App-level singleton, IDs match C# NativeDrawerExtensions renderer
- [x] `components/native/loader.ts` — PASS. App-level singleton, IDs match C# NativeLoader. Page-lifetime observer/timeout
- [x] `components/native/checklist.ts` — PASS. data-reactive-checklist is internal contract with NativeCheckListBuilder
- [x] `components/native/native-action-link.ts` — PASS. assertNever added. Event delegation correct for dynamic links
- [x] `components/fusion/confirm.ts` — PASS. App-level singleton, promise queue correct for serialized dialogs
- [x] `root.ts` — PASS. One justified wide selector for plan discovery

## Remaining
- [ ] Flaky SF ComboBox Playwright test — appears every run, needs root cause
- [ ] Sequence diagrams outdated — directory structure changed (http/ merged into execution/, inject moved)
- [ ] Memory files stale — validation-multiselect-bug.md (fixed), test counts, next-session file
- [ ] CLAUDE.md test counts outdated (still says 907)
- [ ] Update SOLID skill with session learnings (tree walker pattern, isHidden lesson, no rubber-stamping)

## Done This Session
- [x] assertNever on all switches
- [x] types.ts split into 9 domain files
- [x] Screaming directory structure (11 dirs → 10 after http/ merge)
- [x] core/coerce.ts (64 tests, 2 bugs found)
- [x] eslint + typescript-eslint (0 errors)
- [x] Fail-fast Into, gather assertNever
- [x] Sequence diagrams (51) — created, needs update
- [x] SF live-clear fix (per-field via resolveRoot)
- [x] ID-only error span + summary lookups
- [x] isHidden fix (check error span parent)
- [x] data-alis → data-reactive rename
- [x] inject.ts moved to execution/
- [x] http/ merged into execution/ (circular dep eliminated)
- [x] assertNever in native-action-link
- [x] walkValidationDescriptors extracted (12 tests)
- [x] Array component validation (17 tests)
- [x] Real ID validation (6 tests)
- [x] Live-clearing BDD (17 tests)
- [x] Coerce BDD (64 tests)
- [x] CLAUDE.md cross-layer changes section
