# API Surface Code Review — `refactor/api-surface-xml-docs`

**Reviewed:** 2026-03-28
**Branch:** `refactor/api-surface-xml-docs` vs `main`
**Scope:** 263 C# files changed, ~11,700 lines of diff

## Summary Table

| Category | Files Checked | Issues Found | Verdict |
|---|---|---|---|
| Parameter renames (`configure` to semantic names) | 59 removals across builders, extensions, triggers | 0 | PASS |
| Event args property accessors | 26 event args files (20 Fusion, 6 Native) | 0 | PASS |
| Event args XML docs voice | 26 event args files | 0 | PASS |
| "Syncfusion" prefix removal in comments/docs | All 263 changed files + full codebase grep | 0 in scope; 4 pre-existing gaps | PASS (with notes) |
| Constructor visibility (descriptors internal, args public) | All descriptor + reaction + guard + source types | 5 | FAIL |
| Bonus: Grammar issues in XML docs | All `*ReactiveExtensions.cs` | 2 | MINOR |
| Bonus: Method rename consistency (Fusion prefix) | All `*HtmlExtensions.cs` | 0 | PASS |
| Bonus: `IReactivePlan` removal | Full codebase | 0 | PASS |
| Bonus: `InputFieldSetup` to `InputBoundField` rename | Full codebase | 0 | PASS |

---

## Category 1: Parameter Renames

**What changed:** All `Action<...> configure` parameters were renamed to semantic names:
- `configure` on trigger callbacks -> `trigger`
- `configure` on pipeline callbacks -> `pipeline`
- `configure` on SF/Native builder callbacks -> `build`
- `configure` on gather callbacks -> `gather`
- `configure` on response callbacks -> `response`
- `configure` on request callbacks -> `request`
- `configure` on `Then`/`Else`/`ElseIf` -> `pipeline`

**Verification:** Grepped all `+` lines for `configure` -- zero matches. Grepped all `+` lines for `Action<...> configure` -- zero matches. All 59 removals of `configure` have corresponding semantic replacements. Every call site (`configure(builder)` -> `build(builder)`, `configure(pb)` -> `pipeline(pb)`) was updated.

**Verdict:** PASS. Clean, complete rename.

---

## Category 2: Event Args Property Accessors

**What changed:** XML docs were added/updated on all event args properties across 20 Fusion and 6 Native event args files.

**Expected pattern per review instructions:**
- Fusion: `{ get; private set; }`
- Native: varies

**Actual state (both main and HEAD):** All event args in both Fusion and Native use `{ get; set; }`. This was NOT changed by this branch -- it matches main. These are phantom marker types used for expression-based path resolution (`ExpressionPathHelper` resolves `x => x.Value` to `"evt.value"`). The properties are never actually set by framework code. The `{ get; set; }` pattern is consistent across the entire codebase.

**Decision point:** If `{ get; private set; }` is truly required for Fusion event args, that would be a separate change. The branch did not attempt or claim to change accessors -- only XML docs.

**Verdict:** PASS -- no accessor changes were attempted, and the existing pattern is internally consistent.

---

## Category 3: Event Args XML Docs Voice

**What changed:** All Fusion event args properties updated to "Gets or sets" voice. All Native event args properties also use "Gets or sets" voice.

**Verified pattern:**
- All properties with `{ get; set; }` use "Gets or sets" -- correct per C# convention
- All constructors have `/// <summary>Creates a new instance. Framework-internal: ...</summary>` (Fusion) or `/// <summary>Initializes a new instance. Framework use only.</summary>` (Native)
- All class summaries use "Event payload delivered when..." (Fusion) or "Event args for..." (Native)

**Voice consistency:** Slight style difference between Fusion ("Creates a new instance. Framework-internal:") and Native ("Initializes a new instance. Framework use only.") constructor docs. Functionally equivalent but not identical.

**Verdict:** PASS.

---

## Category 4: "Syncfusion" Prefix Removal

**What changed:** All `/// Syncfusion XxxComponent` summaries changed to `/// FusionXxx` or `/// A FusionXxx`. Example:
- `/// Syncfusion AutoComplete component` -> `/// A FusionAutoComplete for typing and filtering suggestions`
- `/// Syncfusion DatePicker component` -> `/// A FusionDatePicker for selecting a single date`

**What was preserved (correctly):**
- All `using Syncfusion.*` imports -- untouched (correct, these are package references)
- `FusionComponent.cs` line 4: `/// Base type for all Syncfusion-backed components.` -- factual vendor description, not a naming prefix
- `IdGenerator.cs` line 16: `/// All component vendors (Syncfusion, native) produce the same ID` -- factual reference

**Pre-existing gaps NOT in this branch's scope:**
1. `FusionConfirm.cs` line 4: `/// App-level confirm dialog backed by Syncfusion Dialog.`
2. `FusionToast.cs` line 4: `/// App-level toast notification backed by Syncfusion Toast.`
3. `ToastPosition.cs` line 4: `/// Maps to Syncfusion Toast position { X, Y } values.`
4. `ToastType.cs` line 4: `/// Maps to Syncfusion Toast cssClass values.`

These are factual references ("backed by Syncfusion") and arguably correct, but they weren't touched by the branch.

**Verdict:** PASS. The in-scope changes are correct. Pre-existing references are factual, not naming prefixes.

---

## Category 5: Constructor Visibility

**What changed:** Descriptor constructors were changed from `public` to `internal` with a standard XML doc: `"NEVER make public. Constructed exclusively by framework builders."`

**ISSUES FOUND:**

### Issue 1: `AllGuard` constructor is `public` but doc says "NEVER make public"
- **File:** `/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/Alis.Reactive/Alis.Reactive/Descriptors/Guards/AllGuard.cs`
- **Line:** 30
- **Code:** `public AllGuard(IReadOnlyList<Guard> guards)`
- **Expected:** `internal AllGuard(IReadOnlyList<Guard> guards)`
- **Severity:** Bug -- doc contradicts visibility

### Issue 2: `AnyGuard` constructor is `public` but doc says "NEVER make public"
- **File:** `/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/Alis.Reactive/Alis.Reactive/Descriptors/Guards/AnyGuard.cs`
- **Line:** 30
- **Code:** `public AnyGuard(IReadOnlyList<Guard> guards)`
- **Expected:** `internal AnyGuard(IReadOnlyList<Guard> guards)`
- **Severity:** Bug -- doc contradicts visibility

### Issue 3: `ConditionalReaction` constructor is `public` but doc says "NEVER make public"
- **File:** `/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/Alis.Reactive/Alis.Reactive/Descriptors/Reactions/ConditionalReaction.cs`
- **Line:** 44
- **Code:** `public ConditionalReaction(List<Command>? commands, IReadOnlyList<Branch> branches)`
- **Expected:** `internal ConditionalReaction(List<Command>? commands, IReadOnlyList<Branch> branches)`
- **Severity:** Bug -- doc contradicts visibility

### Issue 4: `SequentialReaction` constructor is `public` -- no XML doc, no `internal`
- **File:** `/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/Alis.Reactive/Alis.Reactive/Descriptors/Reactions/SequentialReaction.cs`
- **Line:** 14
- **Code:** `public SequentialReaction(List<Command> commands)`
- **Expected:** `internal` with "NEVER make public" XML doc
- **Severity:** Missed -- not updated at all

### Issue 5: `HttpReaction` constructor is `public` -- no XML doc, no `internal`
- **File:** `/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/Alis.Reactive/Alis.Reactive/Descriptors/Reactions/HttpReaction.cs`
- **Line:** 18
- **Code:** `public HttpReaction(List<Command>? preFetch, RequestDescriptor request)`
- **Expected:** `internal` with "NEVER make public" XML doc
- **Severity:** Missed -- not updated at all

**Correctly updated (for reference):**
All Command types (`DispatchCommand`, `IntoCommand`, `MutateElementCommand`, `MutateEventCommand`, `ValidationErrorsCommand`), all Trigger types (`ComponentEventTrigger`, `CustomEventTrigger`, `ServerPushTrigger`, `SignalRTrigger`), `Entry`, `Branch`, `ParallelHttpReaction`, `ConfirmGuard`, `InvertGuard`, `ValueGuard`, all Mutation types (`SetPropMutation`, `CallMutation`), all MethodArg types (`LiteralArg`, `SourceArg`), all Source types (`EventSource`, `ComponentSource`), all Gather types (`ComponentGather`, `EventGather`, `StaticGather`), `StatusHandler`, `RequestDescriptor`, `ComponentRegistration`.

**Verdict:** FAIL -- 5 descriptor constructors remain `public` when they should be `internal`.

---

## Bonus Findings

### Grammar: "an" vs "a" before "Fusion"

Two XML docs use "an FusionXxx" instead of "a FusionXxx":

1. **File:** `/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/Alis.Reactive/Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoCompleteReactiveExtensions.cs`
   - **Line:** 32
   - **Text:** `/// Wires an FusionAutoComplete event to a reactive pipeline`
   - **Fix:** `/// Wires a FusionAutoComplete event to a reactive pipeline`

2. **File:** `/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/Alis.Reactive/Alis.Reactive.Fusion/Components/FusionInputMask/FusionInputMaskReactiveExtensions.cs`
   - **Line:** 29
   - **Text:** `/// Wires an FusionInputMask event to a reactive pipeline`
   - **Fix:** `/// Wires a FusionInputMask event to a reactive pipeline`

All other 12 Reactive extensions correctly use "a Fusion...".

### Pre-existing gaps not in this branch's scope

These were NOT changed and are NOT blocking, but noted for completeness:

1. **FusionAccordionOnExpanded.cs** -- old-style XML docs (not updated to new "Event payload delivered when..." pattern)
2. **FusionTabOnSelected.cs** -- old-style XML docs (still says "Event args for the SF Tab 'selected' event")
3. **FusionConfirmExtensions.cs** / **FusionToastExtensions.cs** -- no XML docs on methods at all
4. **FusionAccordionBuilder.cs** line 8 -- old-style comment: `/// Wraps SF AccordionBuilder.Render() output`

---

## Conclusion

The branch is **substantially correct** -- the parameter renames, "Syncfusion" prefix removal, XML doc voice, and method name prefixing are all clean and complete. The `IReactivePlan` removal and `InputFieldSetup` rename are thorough with zero leftover references.

**Blocking issues (5):** Five descriptor constructor visibility mismatches need fixing before merge:
- `AllGuard`, `AnyGuard`, `ConditionalReaction` -- have "NEVER make public" doc but are still `public`
- `SequentialReaction`, `HttpReaction` -- completely missed (no doc, still `public`)

**Non-blocking (2):** Grammar fixes ("an Fusion" -> "a Fusion") in two `*ReactiveExtensions.cs` files.
