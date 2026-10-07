# Remaining framework bugs after PR 175

Work began from `ca6f70d154d94c7cdce2266e525ae4c4fb638d69` (main). The existing
`fix/framework-bugs` checkout was clean at `be11d298958e122177ca8d3f502868a285196ce4`;
the two trees are identical. Unrelated edits in other checkouts were left intact.

## Nullable resident dropdown

Close matrix row: `InputField(... int? ...).FusionDropDownList(...)` plus
`Component<FusionDropDownList>(...).Value<TModel, int?>()` / `SetValue((int?)...)`
→ typed input value contract → nullable-number read/set in the synchronous lane.

Source requirements: `FusionDropDownListHtmlExtensions`,
`FusionDropDownListExtensions`, `NativeDropDownExtensions`, `ComponentRef.Read/EmitSet`,
`ModelBoundInputComponentSlot.For` and `TypedComponentSource<TProp>`.
No shared plan shape or runtime change is needed. No helpers or core validation are
deleted. The commit boundary is the two dropdown slices and their resident-assignment
browser proof.

Before the fix, the real Razor page returned HTTP 500 for unset and selected `int?`
bindings in each of these cases: `Value()`, `SetValue("1042")`, `SetValue(null)` and
NativeDropDown `Value()`. Every failure named `Property 'value' registered with shape
'nullable<number>' but re-registered with conflicting shape 'string'`.

In the browser, selection and `IncludeAll()` without those calls already worked:
unset → refused assignment; selected Helen → saved Helen; cleared Helen → refused
assignment. Registration was correct. The typed method declaration was the defect.

The added typed overloads carry the same `TValue` in the component property contract,
literal/read expression, and returned `TypedComponentSource<TValue>`. Use
`Value<ResidentAssignmentModel, int?>()` for a nullable integer read, and
`SetValue((int?)1042)` / `SetValue((int?)null)` for writes and clearing.
Existing no-type-argument `Value()` and `SetValue(string)` retain their string contract.
The old string calls on numeric bindings remain errors: strict shape merging is unchanged.
The local implementation commit is `2736a235` on `fix/remaining-framework-bugs`.

Null is required in this journey: it means no resident has been assigned. Zero is a
numeric value and cannot safely replace missing selection; an empty string would
contradict the registered numeric shape. The new nullable declaration is in the
journey model, matching the reported input type, rather than a framework fallback.

Focused evidence:

- Ten before-fix render attempts: two plain registration/gather pages returned 200;
  eight read/write/clear attempts returned the numeric-versus-string contract error.
- Fourteen after-fix render attempts: all eight correctly typed attempts returned
  200; all six incompatible string attempts still returned the same contract error.
- `scripts/playwright.sh --filter 'FullyQualifiedName~WhenCoordinatorAssignsResident|FullyQualifiedName~WhenDropdownItemSelected'`:
  22/22 passed (the new numeric journey and existing string dropdown suite).
- A final `scripts/playwright.sh --no-build --filter 'FullyQualifiedName~WhenCoordinatorAssignsResident'`:
  8/8 passed, including the saved NativeDropDown selection on initial load.
- Real browser: Assign Helen, assign Arthur as backup, move to West, save →
  Helen/Arthur selected; current values 1042/1043; "Helen Park assigned to West wing."

Full gate: `scripts/test.sh --parallel` exited 0. All 1,320 Playwright tests,
247 runtime tests and two architecture tests passed. Type checking, asset builds,
script self-tests, CI shard/Syncfusion checks and the configured behavioral coverage
gate passed. The .NET build covered net48 and net10.0 with zero warnings or errors.
The coverage gate audits 15 other component slices; it does not establish a complete
behavioral audit of DropDownList.

The separate blind text reviewer passed all eight tests after checking the BDD
principles and fixture. It withdrew an initial folder-depth flag against the skill's
Grid/Billing exemplar. Independent headed review remains incomplete: the reviewer's
browser inventory was empty and `iab` was unavailable. The repository's
`.claude/memory/bdd-principles.md` requires the reviewer to "open[] the page in headed
browser"; the author's real browser observation above supplies functional UI evidence,
but does not fulfill that independent observation requirement.

## Pending HTTP after slot unload

Close matrix row being investigated:
slot `Into(...)` → `loadPartialSlot`/`unloadPartialSlot` ownership → pending
`Get(...).Response(OnSuccess/OnError/Chained).Finally(...)` effects in the async lane.
Sources: `PipelineBuilder.Into`, `HttpRequestBuilder.Finally`,
`ResponseBuilder.Chained`, `applied-plans.ts`, `trigger.ts`, `execution-context.ts`,
`http.ts` and `http-fetch.ts`. This row remains **Not done**. No production lifecycle
or HTTP execution code was changed.

Reproduce the current behavior without timing sleeps or a real remote service:

```sh
node_modules/.bin/vitest run --config docs/diagnostics/slot-http-lifecycle.config.ts
```

The opt-in probe uses the real boot, slot composition, trigger, HTTP and reaction
executors with a deferred fetch boundary in jsdom. It is a characterization of the
current defect, excluded from the normal regression suite. Its passing assertions
mean the observations were reproduced; they do **not** mean slot cancellation works.
For each scenario it starts a retired-slot request and a sibling-slot request on the
same plan, retires one slot before settlement, and settles the two requests separately.
For replacement it first writes "replacement is current" from the new slot, then
shows that the old response overwrites it. It also dispatches the old trigger after
retirement to prove listener cleanup works and starts no new request.

Eight scenarios reproduced these observations identically on two runs. The final
report also counts exactly one request from the retired trigger in every scenario:

| Pending result | Unload | Replacement | Chained request | Sibling slot |
| --- | --- | --- | --- | --- |
| 200 success | late success and Finally run | old success overwrites replacement; Finally runs | starts after retirement | completes normally |
| 500 error | late error and Finally run | old error overwrites replacement; Finally runs | does not start | completes normally |
| network failure | catch-all error and Finally run | old error overwrites replacement; Finally runs | does not start | completes normally |
| headers received, body still pending | late success and Finally run | old success overwrites replacement; Finally runs | starts after retirement | completes normally |

No request carried an AbortSignal. The slot abort controller reaches event-listener
options and validation wiring. `wireTrigger` creates an `ExecutionContext` without
that signal; `http-fetch.ts` builds a fetch init without it. Removing the listener
stops future events but cannot stop work already awaiting a response. Active Plan
recomposition keeps the root/sibling objects mounted, so late effects can still
resolve and update them. This is **not** failure to remove the original listener.

### Finally semantics to resolve before changing behavior

The public `Finally` contract says it runs after success, error or network failure,
and the root guidance says slot unload aborts slot-owned behavior. Neither explicitly
defines whether unload is request settlement, whether cleanup survives its originating
slot, or which targets it may change after replacement.

- Suppressing every old-slot effect, including Finally, can leave an app-level loader
  shown by WhileLoading. App-level objects remain mounted after unload.
- Running arbitrary old-slot Finally reactions can hide a replacement request's loader
  or mutate its current UI. If they target a component removed during unload, component
  lookup can fail instead of cleaning up. Shared identifiers are not proof of ownership.
- Aborting fetch alone is insufficient: the current client-failure path runs the
  catch-all error route and Finally. It would turn cancellation into another stale effect.
- Ownership must follow the slot load instance, not the shared PlanId or reused SlotId;
  retirement of one slot must preserve the sibling and replacement instances.

Before implementing cancellation, choose whether Finally is skipped on retirement or
is allowed a defined cleanup scope. Then prove the selected contract with a loader
shown by the retired request, a loader used by its replacement, removed slot targets,
chained requests and the sibling slot. Ordinary success/error/network/build-failure
Finally behavior must remain intact. No such semantic choice was made by this change.
