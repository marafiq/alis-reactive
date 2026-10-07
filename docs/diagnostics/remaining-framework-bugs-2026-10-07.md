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
`ModelBoundInputComponentSlot.For`, `TypedComponentSource<TProp>` and the native
component driver. No shared plan shape change is needed. No helpers or core validation
are deleted. The typed-overload commit boundary is the two dropdown slices and their
resident-assignment browser proof. A subsequent independent headed review exposed a
native DOM delivery defect, addressed at the native driver's select boundary.

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

Independent headed review then caught an incomplete native clearing result: the
action removed the numeric value and gathered null, but the closed select displayed
no placeholder. The browser's select value setter coerces null to the option value
`"null"`, which this journey does not have. The native component driver now delivers
model null as the empty DOM option value, while numeric/string values and the declared
nullable-number shape remain intact. WebIDL accessors and methods continue to receive
the actual DOM element. Fusion delivery and generic RuntimeObject/RuntimeShape behavior
are unchanged. The native fixture now checks the selected option's user-visible label
as well as the numeric/null POST body.

The strengthened action-removal test failed before the correction because no selected
option had the expected placeholder label. Four focused native-boundary checks pass
for nullable clear, zero/the literal string `"null"`, real DOM focus/listeners, and
clearing a select without an empty option.
The local native-clear correction is `66412b86`. The corrected journey plus existing
Fusion and Native string dropdown suites pass together: 41/41 through
`scripts/playwright.sh --no-build --filter 'FullyQualifiedName~WhenCoordinatorAssignsResident|FullyQualifiedName~WhenDropdownItemSelected|FullyQualifiedName~WhenDropdownSelectionChanges'`.

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
- An additional `scripts/playwright.sh --no-build --filter 'FullyQualifiedName~WhenCoordinatorAssignsResident'`:
  8/8 passed, including the saved NativeDropDown selection on initial load.
- Real browser: Assign Helen, assign Arthur as backup, move to West, save →
  Helen/Arthur selected; current values 1042/1043; "Helen Park assigned to West wing."

Final full gate after the independent review correction:
`scripts/test.sh --parallel` exited 0. All 1,320 Playwright tests,
251 runtime tests and two architecture tests passed. Type checking, asset builds,
script self-tests, CI shard/Syncfusion checks and the configured behavioral coverage
gate passed. The .NET build covered net48 and net10.0 with zero warnings or errors.
The final log is preserved at
`task-8/evidence/final-full-gate-after-headed-review.log`; its five shard TRX files
are named `playwright-20261006-234136.<shard>.trx` under the test project's observable
results directory. The independent review observer is not part of those 1,320 tests.
The coverage gate audits 15 other component slices; it does not establish a complete
behavioral audit of DropDownList.

Independent blind BDD review, including headed observation, is complete. A fresh
reviewer read only the principles, fixture and required tooling, used the sanctioned
`scripts/playwright.sh` workflow with a temporary observer subclass, and inspected all
eight full-page screenshots. The first run exposed the native placeholder gap above;
the corrected run passed 8/8 and the reviewer found that issue addressed with no new
concern. The visible form passes the product-screen check. Source/model/data exclusivity
and a full seven-behavior component audit were not established by this blind review.

The headed configuration used Playwright's documented `.runsettings`
`LaunchOptions.Headless=false`, passed through the installed MSBuild `VSTestSetting`
property. [Official Playwright launch settings](https://playwright.dev/dotnet/docs/test-runners#customizing-browserlaunch-options)
describe that configuration. DEBUG launch logs show the existing cached Chromium
binary starting without `--headless`; no browser was installed. Empty CUA browser
inventory did not prevent this repository-supported route, and no policy denial
occurred. Both observer runs used the real app, original eight gesture/assertion
methods, and screenshot capture only. The temporary observer was removed; cleanup
builds passed with zero warnings/errors and the rebuilt DLL excludes its type.

Before/after evidence is preserved beside the checkout in
`task-8/evidence/independent-headed-review/` and
`task-8/evidence/independent-headed-review-after/`: observer/configuration copies,
launch/test logs, TRX/diagnostics, all screenshots, review results and cleanup checks.

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

### Recommended public cancellation contract (proposal, not implemented)

Treat retirement of the originating slot load as cancellation of that behavior's
remaining work, rather than an HTTP error or ordinary settlement. This follows the
root contract that unload aborts slot-owned behavior. The request, its nested routes,
follow-up requests, parallel branches and post-await pipeline continuation inherit
that exact load instance. A new load with the same SlotId has a new lifetime. PlanId,
target component ID and the presence of a mounted target do not establish ownership.
Requests started by boot-owned or sibling-owned triggers retain their own lifetimes;
the destination of an `Into(...)` does not transfer ownership of its initiating request.

The recommended user-observable rules are:

| Situation | Response/user effects | Finally | Managed loading resources |
| --- | --- | --- | --- |
| Live request succeeds | success route; then existing successful chain order | runs with existing context/order | released exactly once |
| Live request returns an HTTP error or has a network/client failure | existing matching error route | runs | released exactly once |
| Live request cannot be built after WhileLoading completes | no response route; report the build failure | runs | released exactly once |
| Client validation blocks, or WhileLoading fails | request does not start; existing error reporting applies | does not run, as documented today | release any resources actually acquired |
| Originating load unloads or is replaced, including during a pending body read | no remaining route, mutation, dispatch, injection, child request or continuation | no remaining user-authored Finally reactions | release only that load's request leases |

Cancellation is a distinct internal outcome, not a fabricated status code, catch-all
OnError call or noisy network failure. Abort the fetch/body read where supported and
also gate effect execution: cancellation may win after the transport has completed,
or a transport may ignore its signal. Check the originating lifetime before sending,
after every async boundary and before each effect/continuation. Keep immediate live
reactions synchronous. Retiring within a route (including an injection replacing its
own slot) stops subsequent effects and chaining; completed effects are not rolled back.
This does not reverse a request the server has already processed. An AbortError while
the owner is still live remains a client failure, rather than being mistaken for unload.
Retirement completes the framework-owned pending work promptly as cancelled without
rejecting solely for cancellation or waiting for an uncooperative transport. Any late
transport settlement is consumed without user effects. Genuine resource-disposal
failures remain reportable; cancellation does not erase failures already reported.

Finally is **user-authored behavior**, not an enforceable resource destructor. The
source takes `Action<PipelineBuilder<TModel>>`, builds a general `ReactionGraph`, and
permits conditions, mutations and calls. Cleanup is its common use, not a restricted
effect type. Running it against root targets, old component IDs or an old plan snapshot
cannot prove that it is safe after retirement. A callback may itself dispatch or send
a request, and the singleton loader may already serve a replacement or sibling.
Do not infer inverses of arbitrary WhileLoading reactions or silently reinterpret
selected Finally commands as safe cleanup.

Provide an explicit managed loading-resource option on the request DSL as a separate
capability before adopting this policy for applications with persistent spinners.
Its public promise should be acquisition after validation, before user WhileLoading,
and idempotent release on ordinary completion, build/WhileLoading failure, unload or
replacement. It accepts a declared loader/indicator configuration, not a general
PipelineBuilder. The API spelling still needs design; this is a proposed addition,
not an existing method or automatic property of WhileLoading/Finally.
For a live request, release occurs after its user Finally completes or throws, before
the current successful follow-up request begins; response/Finally/chain ordering stays
the same. WhileLoading/build failures release any acquired lease even when user Finally
is ineligible. On retirement, release occurs immediately instead of waiting for the
transport, and subsequent settlement cannot release the lease twice.

The managed indicator must give each request an opaque lease, tied to its originating
load and the concrete indicator instance captured at acquisition. Releasing a lease
removes only its own contribution: the indicator hides when no live lease remains,
and releasing an old lease cannot resolve a reused ID to a new object, remove another
request's lease, or clear another request's message/target/timer. A shared loader's
configuration is per lease; the newest active acquisition supplies its presentation,
with the previous still-active configuration restored when that lease is released.
Lifecycle disposal releases leases synchronously with retirement, before replacement
behaviors can acquire theirs. Actual removed instances need no plan lookup or new UI
mutation; their remaining resource handles are disposed. Release is bounded resource
work, never an arbitrary reaction graph. Cleanup failures must be reported while
other leases are still released.

NativeLoader currently has one global auto-hide timer and writes one DOM object's
class, target and timeout. It has no request lease capability. Its removal observer
hides a targeted loader when its container disappears, but does not manage a viewport
loader or concurrent ownership. Implementing and proving the lease capability is
required; merely passing AbortSignal or checking `isConnected` cannot supply it.
Timeouts and target removal for a managed lease must retire that lease's presentation
without clearing another live request's state. Existing unmanaged Show/Hide calls
retain their imperative meaning and must not share control of a managed indicator;
the migration must establish one ownership model for each affected indicator.

Migration impact is explicit: Finally on a live request keeps today's success,
HTTP-error, network/client-error and build-failure behavior. After load retirement its
user effects stop. Code using WhileLoading/Finally as a Show/Hide pair for a persistent
indicator must migrate to the managed request resource, or give the indicator a truly
slot-local lifetime. Arbitrary effects and notifications expected after unload must
move to a deliberately longer-lived workflow/boot-owned trigger. Do not offer a
general "run Finally after unload" switch as the loader solution. Moving request
ownership to boot scope also moves its response effects and must be an intentional
application choice, not an automatic promotion based on target visibility.
An existing arbitrary viewport Show/Hide pair has no recorded acquisition to release;
skipping its Finally alone can leave it visible. Do not claim that cancellation alone
fixes that application. Managed ownership and migration of the affected persistent
indicator are part of delivering the proposed no-stuck-loader contract.

This is the recommendation for parent review. The public XML documentation's
"regardless of ... outcome" wording needs an explicit lifetime qualification, and the
new resource declaration needs matching C# plan/generated TS/runtime proof. No HTTP,
loader, plan contract or documentation defining production behavior was changed here.
Do not mark the lifecycle row Done until the policy is adopted and the following proof
passes: unload/replacement before headers and during body reading; live success/error/
network/build failures; retirement within success/Finally; removed targets; queued
continuations, nested/parallel/chained requests; persistent and slot-targeted loaders;
replacement and sibling leases sharing an indicator; and exactly-once release with
no stale mutation or leftover busy state.
