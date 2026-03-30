---
name: XML docs developer perspective audit results
description: Gaps found by .NET MVC devs reading only XML docs — blocking issues, confusion points, best-in-class patterns
type: project
---

## Dev Perspective Audit (2026-03-28)

Three agents role-played as .NET MVC developers reading only XML docs. Results below.

**Why:** Tests whether docs alone let a developer use the API. Not "are docs well-formed?" but "can I build a feature from these docs?"

### Blocking Gaps (dev would get stuck)

1. **"Gather" is unexplained jargon** — referenced in NativeGatherExtensions, all component Value() methods. Never defined. Fix: one-sentence explanation on GatherExtensions or NativeGatherExtensions.
2. **ComponentRef entry point missing** — extensions exist on ComponentRef<T> but no docs say how to obtain one. Fix: remarks on each Extensions class pointing to PipelineBuilder.Component<T>().
3. **InputBoundField/Html.InputField() cross-reference missing** — Fusion HtmlExtensions say "created by Html.InputField()" but never link where that's documented. Fix: seealso to InputFieldExtensions.
4. **TriggerBuilder constructor is public but should be internal** — code bug per CLAUDE.md rules. Separate fix.

### Confusion Gaps (dev would need to experiment)

5. "Phantom" pattern is jargon — repeated across triggers and element builders without centralized explanation.
6. TypedComponentSource return value is opaque — Value() returns it but no "what to do next" guidance.
7. BindSource/TypedSource overloads lack usage examples.
8. BuildReaction()/BuildReactions() audience unclear — public but feels internal.
9. ComponentsMap public but feels internal — docs should state who it's for.
10. "Producer" in HtmlExtensions.On() is undefined jargon.
11. ResolvePlan merge mechanism vague — does partial also call RenderPlan?
12. DrawerPosition enum orphaned — exists but no method accepts it.
13. ResponseBody<T> unexplained — used in SetDataSource overloads.
14. Where .Reactive() goes in builder chain unclear.

### Best-in-Class Patterns (preserve these)

- FusionAutoCompleteOnFiltering.cs — explains full server-side filtering workflow + anti-patterns
- PlanExtensions class-level remarks — full lifecycle in one place
- AddToComponentsMap exception docs — tells what broke AND how to fix
- NativeTextBoxChangeArgs condition example — one line shows the pattern
- NativeDrawer/NativeLoader code examples — one-line usage in remarks

### Orphaned/Dead Code Found During Audit

- `DrawerPosition` enum (`Alis.Reactive.Native/AppLevel/NativeDrawer/DrawerPosition.cs`) — defined but never referenced anywhere. No extension method accepts it. Either delete or add a SetPosition extension.
- `DrawerSize` enum values lack concrete meaning — "Small", "Medium", "Large" without pixel/percentage indication.
- Drawer title/content manipulation undocumented — rendered HTML has `#alis-drawer-title` and `#alis-drawer-content` but no extension methods or docs explain how to populate them.

### Closed Gaps

- #4 TriggerBuilder constructor — made internal (2026-03-28)
- #8 BuildReaction(s) — made internal (2026-03-28)
- #9 ComponentsMap — made internal (2026-03-28)

### Round 2 Dev Perspective Findings (2026-03-28)

Rating: 7/10 (up from first round). Feature-building test: text input + dropdown + change handler + gather + POST.

New gaps discovered:
- **BLOCKING: Button/form submit trigger undocumented** — NativeButton exists but the Dispatch→CustomEvent indirection pattern is not explained anywhere in the docs a dev would read. This blocks the feature entirely.
- **MINOR: Dropdown data source not mentioned** — Syncfusion `.DataSource(items)` never referenced in DropDownListHtmlExtensions docs.
- **MINOR: Response handling shape** — OnSuccess/OnError API on ResponseBuilder not in the discovery path.

### How to apply

Fix blocking gaps first. Then confusion gaps. Preserve best-in-class patterns as templates for new docs.
