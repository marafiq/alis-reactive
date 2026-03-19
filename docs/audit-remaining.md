# Audit & Quality — Remaining Items

## Done: Component Audit
- [x] `components/native/drawer.ts` — PASS. App-level singleton, IDs match C# NativeDrawerExtensions renderer
- [x] `components/native/loader.ts` — PASS. App-level singleton, IDs match C# NativeLoader. Page-lifetime observer/timeout
- [x] `components/native/checklist.ts` — PASS. data-reactive-checklist is internal contract with NativeCheckListBuilder
- [x] `components/native/native-action-link.ts` — PASS. assertNever added. Event delegation correct for dynamic links
- [x] `components/fusion/confirm.ts` — PASS. App-level singleton, promise queue correct for serialized dialogs
- [x] `root.ts` — PASS. One justified wide selector for plan discovery

## Remaining
- [x] Memory files cleaned
- [x] CLAUDE.md test counts updated
- [x] SOLID skill updated
- [x] Sequence diagrams regenerated
- [x] Async fire-and-forget — .catch() added to all 3 async call sites

## Next: HTTP Pipeline Hardening (HIGH PRIORITY)
- [ ] Pre-fetch errors (gather throws, validation throws) must route through developer's onError handlers, not swallow into .catch() log
- [ ] ExecContext immutability — HTTP spreads ctx for responseBody but original is mutable. Must be immutable for retry support
- [ ] Retry support — plan must be able to specify retry policy, pipeline must support it
- [ ] Error status handlers must ALWAYS fire when registered — the .catch() safety net must not skip them

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
