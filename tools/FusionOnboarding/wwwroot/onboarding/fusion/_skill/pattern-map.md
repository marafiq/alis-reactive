# Fusion Skill Pattern Map

Status: active. This file exists to make audits improve the onboarding skill itself.

Audit does not mean checking one component and moving on. Audit means extracting the reusable pattern that prevents the same mistake across every existing and future Fusion component. A component row cannot close unless its audit finding links to a pattern here or proves an existing pattern already covered it.

## Pattern Row Rules

Every pattern row must name:

- trigger condition that makes the pattern apply;
- raw EJ2 evidence required;
- shipped source/docs evidence required;
- primitive mapping decision;
- C# naming rule;
- vertical slice rule;
- typed DSL Playwright proof required;
- common false-positive that the skill must reject.

## Patterns

### P000: Discovery Is Exhaustive But Public C# DSL Is Selective

Trigger condition: raw EJ2 discovery exposes payload fields, object branches, method return details, or nested members that may not all belong in public typed Fusion DSL.

Raw EJ2 evidence required:

- record every observed field/member in discovery, including fields that will be excluded;
- record missing/undefined fields by trigger variant.

Shipped source/docs evidence required:

- use source/docs to classify stability, ownership, and intent where available.

Primitive mapping decision:

- a member is accepted only when existing primitives can consume it deterministically;
- do not add primitives only to avoid excluding a discovered member.

C# naming rule:

- public C# names require a clear typed use case and predictable contract;
- exclude broad DOM/browser/internal objects rather than exposing `object`, `dynamic`, or stringly paths.
- a discovered field such as an event `target` may be accepted only if the row
  proves a stable typed scalar or typed object use case; deep DOM access up to
  document/window paths stays browser-owned and out of the public DSL.

Vertical slice rule:

- every accepted/excluded judgment must be recorded in the component artifact tree.

Typed DSL Playwright proof required:

- accepted members need behavior proof;
- excluded members need an artifact reason and must not appear in public typed C# API.

Common false-positive to reject:

- treating exhaustive discovery as a requirement to expose every payload branch in C#.

First evidence: Grid `dataStateChange.action.target` is discovered but excluded from C# because it can be a DOM `TH` element and has no safe typed use case in this row.

### P001: Event Variants Are Separate Rows Until Proven Equivalent

Trigger condition: one Syncfusion event name can fire from multiple gestures, methods, lifecycle paths, or data modes.

Raw EJ2 evidence required:

- one focused probe per trigger variant;
- one deterministic trace per trigger variant;
- explicit present keys, absent keys, nested payloads, writable fields, payload methods, arrays, and lifecycle timing for each variant.

Shipped source/docs evidence required:

- event `EmitType<TArgs>` or equivalent source declaration;
- trigger path source showing where the event is fired;
- docs only as supporting evidence, never as sole proof.

Primitive mapping decision:

- map each variant only after its payload shape is known;
- reuse event payload read, nested payload read, payload mutation, payload method call, and proper array primitives;
- do not add primitives during component onboarding.

C# naming rule:

- use exact EJ2 payload names when they are developer-meaningful;
- hide or exclude metadata only after reviewer proof explains why it should not be public typed DSL.

Vertical slice rule:

- keep event payload contracts under the component `Events/` slice;
- split only by coherent event/use-case when file size harms reviewability.

Typed DSL Playwright proof required:

- trigger the exact variant through visible user behavior or typed DSL command;
- prove every accepted field for that variant through visible output, request payload, or visible runtime state;
- do not let a different trigger variant satisfy the row.

Common false-positive to reject:

- method state changed, but the event did not fire.

First evidence: Grid `dataStateChange` sorting rejected a plain array `dataSource` probe because it updated `sortSettings.columns` without emitting `dataStateChange`; custom binding `{ result, count }` produced the actual event trace.

### P015: Proof References Must Be Current After Any Row-Affecting Change

Trigger condition: a public C# contract, probe, trace, mapping row, vertical
slice row, sandbox proof surface, or Playwright assertion changes after a proof
artifact was recorded.

Raw EJ2 evidence required:

- preserve the raw trace that justified the row;
- regenerate the trace when the probe or trigger path changes.

Shipped source/docs evidence required:

- re-check source/docs only when the changed row depends on vendor naming,
  ownership, or trigger timing evidence.

Primitive mapping decision:

- keep the primitive mapping unchanged only when the current trace still proves
  the exact same source path, target path, mutation, or method call.

C# naming rule:

- do not let a stale proof keep an old public member alive;
- if the row removes or renames a public member, the matrix and proof artifacts
  must no longer cite behavior from before that change.

Vertical slice rule:

- update every artifact that cites the proof result in the same row pass:
  event row, vertical slice plan, Playwright proof, audit report, and generated
  matrix when applicable.

Typed DSL Playwright proof required:

- rerun the focused Playwright behavior proof after the row-affecting change;
- cited TRX/log paths must point to the post-change run.

Common false-positive to reject:

- a test passed earlier, but the implementation or artifact row changed after
  that run.

First evidence: Grid `actionBegin` save/edit removed
`FusionGridEditActionArgs.Index`; reviewers rejected the row until the proof
artifacts were updated from the pre-removal TRX to the post-removal
`playwright-20260606-214650.trx`.

### P016: Shared Payload Types Do Not Prove Shared Event Rows

Trigger condition: two public typed events reuse the same C# event args type or
Syncfusion source type family, such as `actionBegin` and `actionComplete`
sharing `FusionGridEditActionArgs<TRow>`.

Raw EJ2 evidence required:

- one focused probe and trace per event name and trigger variant;
- record event identity fields such as `name`, `type`, `requestType`, and
  `action` separately for each event;
- record extra present, absent, or undefined keys even when C# currently excludes
  them.

Shipped source/docs evidence required:

- use vendor event type declarations and trigger source only as naming/supporting
  evidence.

Primitive mapping decision:

- accepted shared members may reuse the same primitive only after each event row
  proves the payload key exists with compatible behavior;
- excluded members remain event-row-specific until proven equivalent.

C# naming rule:

- a shared C# args type may stay shared when accepted member names are identical
  and useful, but the coverage matrix must keep the shared contract unproven
  until every event and trigger variant using it is proven or excluded.

Vertical slice rule:

- keep each event row as a separate artifact and link it from the component
  master index.

Typed DSL Playwright proof required:

- trigger the exact event name through realistic behavior and assert visible
  output from that event, not from a sibling event using the same args type.

Common false-positive to reject:

- `ActionBegin` proof also proves `ActionComplete` because both use
  `FusionGridEditActionArgs<TRow>`.

First evidence: Grid `actionComplete` save/edit emits `name/type` as
`actionComplete` and includes an undefined `promise` own key; it requires a
separate row from `actionBegin` save/edit even though both use
`FusionGridEditActionArgs<TRow>`.

### P017: Focused Proof Views Preserve Vertical Slice Accountability

Trigger condition: a component sandbox page has accumulated multiple unrelated
behaviors and a new row proof could pass because adjacent workflows already
exercise part of the same component.

Raw EJ2 evidence required:

- the focused view still requires its own raw EJ2 probe and trace for the exact
  row being proven;
- the raw probe remains vendor-only and does not use the Alis wrapper.

Shipped source/docs evidence required:

- source/docs stay row-specific and are cited only for the relevant event,
  property, method, or overload.

Primitive mapping decision:

- the focused view must use the same primitive rows that the artifact maps;
- do not add helper-only public APIs or stringly escape hatches to make the view
  easier to drive.

C# naming rule:

- names are still decided from raw trace plus useful typed C# behavior, not from
  the convenience of the proof view.

Vertical slice rule:

- create a route, model/data setup, Razor view, reactive behavior, and
  Playwright test that all belong to the same component vertical slice;
- update the component's sandbox index/listing so the view is discoverable;
- do not leave focused proof views as orphaned routes.

Typed DSL Playwright proof required:

- prove realistic behavior through the focused view;
- assert the visible component state and visible event-derived output from the
  exact typed API row;
- include explicit exclusion proof when the row removes or rejects public typed
  members.

Common false-positive to reject:

- adding assertions to a large existing sandbox page where another reaction,
  default Syncfusion behavior, or previous test setup can satisfy the visible
  state without proving the target row.

First evidence: Grid `actionComplete` save/edit moved to
`/Sandbox/Components/Grid/ActionCompleteSaveEdit`, a focused vertical-slice view
with its own row loading, edit commands, typed `ActionComplete` reaction, and
Playwright proof.

### P019: Clear And Reset Methods Must Not Be Masked By Manual Reloads

Trigger condition: a component method clears or resets state that also affects
remote/custom-bound data, such as Grid filtering, sorting, grouping, or search.

Raw EJ2 evidence required:

- trace the clear/reset method after the state is actively applied;
- record whether the cleared state key is absent, empty, or replaced by a
  different request type such as `refresh`;
- record visible/runtime effect after the method so the probe proves the state
  actually cleared.

Shipped source/docs evidence required:

- use vendor source/docs only to classify the method name and lifecycle timing;
- do not infer that a clear method uses the same payload shape as the apply
  method.

Primitive mapping decision:

- use the existing component method-call primitive for the clear/reset method;
- use existing event payload reads only for fields the clear/reset event emits;
- do not add a primitive because the clear/reset row uses `refresh` or omits the
  applied-state payload key.

C# naming rule:

- keep the public C# clear/reset method when it maps directly to the vendor
  method and gives a real typed use case;
- do not expose duplicate internal action/settings fields only to identify that
  a clear happened.

Vertical slice rule:

- remove direct manual reloads that can make the page refresh even when the
  clear/reset event pipeline is broken;
- let the component method's actual event/remote-data lane own the refresh;
- if a guard excludes generic refresh events, prove whether the clear/reset
  method needs a row-specific path before keeping the guard.

Typed DSL Playwright proof required:

- start from active non-default state;
- trigger the typed clear/reset method;
- capture the request produced by the method's actual event/remote-data lane;
- assert the cleared payload key is absent or empty exactly as raw EJ2 proved;
- assert visible component state returns to the unfiltered/unsearched/ungrouped
  or unsorted state;
- assert no second manual reload is what made the visible state pass.

Common false-positive to reject:

- a button calls the typed clear/reset method and then separately posts a manual
  reload, allowing the test to pass without proving the method's emitted event
  payload or remote-data behavior.

First evidence: Grid `ClearFiltering()` emits `dataStateChange` with
`action.requestType=refresh` and no top-level `where`; the Directory sandbox
previously called `LoadDirectory(...)` after `ClearFiltering()`, which could
mask a broken clear-filter event lane. Grid `ClearSorting()` emits
`dataStateChange` with `action.requestType=sorting` and no top-level `sorted`
after an active `SortBy` state; it must be proven as absence, not as
`Sorted=[]` or as a reused sorting-apply payload. Grid `ClearGrouping()` emits
one final `dataStateChange` with `action.requestType=ungrouping`,
`action.columnName=wing` after two active group columns, and no top-level
`group`, `groups`, or `sorted`; it must be proven as its own clear/reset row,
not coalesced with `UngroupBy(...)`.

### P014: Gesture Commit Semantics Are Part Of Event Proof

Trigger condition: a Syncfusion event row is fired by user input whose event
timing depends on component settings, such as FilterBar `Immediate` versus
`OnEnter` behavior.

Raw EJ2 evidence required:

- trace the real input element and the event path that fires the payload;
- record the gesture/settings used by the probe;
- if the probe changes timing settings for determinism, state that explicitly.

Shipped source/docs evidence required:

- source or docs that identify the user gesture path and timing condition.

Primitive mapping decision:

- gesture timing must not create new primitives;
- map the emitted payload only after the gesture produces the target event.

C# naming rule:

- do not expose timing/settings-only fields unless a separate typed use case is
  proven.

Vertical slice rule:

- Playwright proof must use the actual application gesture, not the probe-only
  shortcut.

Typed DSL Playwright proof required:

- trigger the event through the real user/component gesture used by the
  application, such as typing and pressing Enter when the sandbox FilterBar is
  not in immediate mode.

Common false-positive to reject:

- assuming text entry alone proves the event when the component waits for a
  commit gesture.

First evidence: Grid FilterBar typing did not fire a sandbox request from typing
one character alone; the behavior proof passed only after typing and pressing
Enter.

### P002: Custom-Binding Data Events Require Custom-Binding Data Shape

Trigger condition: Syncfusion event docs describe server/custom/remote data binding, or source emits the event through a data module state path.

Raw EJ2 evidence required:

- probe must use the binding shape that activates the event path, such as `{ result, count }` for Grid custom binding;
- trace must include the event row, not only method/property state after a call.

Shipped source/docs evidence required:

- source path that builds state and triggers the event;
- docs or source statement that the event is tied to data operations/custom binding.

Primitive mapping decision:

- if the valid binding mode changes emitted keys, the binding mode is part of the row evidence;
- map the emitted payload, not the method state.

C# naming rule:

- do not infer payload fields from method arguments;
- name from emitted payload keys and typed source contracts.

Vertical slice rule:

- the sandbox proof page must use the same binding mode as the raw trace for that row.

Typed DSL Playwright proof required:

- request/response path must prove the event-fed server data refresh, not only local client sorting.

Common false-positive to reject:

- direct array data source with local sort is treated as proof for `dataStateChange`.

First evidence: Grid `dataStateChange` sorting.

### P003: Proper Array Primitive Means Whole Typed Array Source Or Array Operation

Trigger condition: payload or component member is an array, collection, or list.

Raw EJ2 evidence required:

- trace must show the array key, array item keys, item scalar/null types, and empty/missing behavior for the row variant.

Shipped source/docs evidence required:

- d.ts or source declaration for the array element shape when available.

Primitive mapping decision:

- gather whole arrays through typed event payload reads when the server needs the array;
- use typed array operations when client behavior needs element-level filtering/counting/projection;
- never model this as indexed paths.

C# naming rule:

- preserve the array property as `List<T>` or typed array-compatible shape;
- item properties must map to observed item keys.

Vertical slice rule:

- do not add untyped element accessors to component slices.

Typed DSL Playwright proof required:

- whole-array gather proof for server requests or typed array operation proof for client behavior.

Common false-positive to reject:

- proving only the first item or using an index-like path as if it covered the array contract.

First evidence: Grid `dataStateChange.sorted[]`.

### P004: DOM Payload Objects Are Browser-Owned Until A DOM Source Row Proves Them

Trigger condition: an event payload key is a DOM `Element`, `HTMLElement`, browser event object, or another cyclic/browser-owned object.

Raw EJ2 evidence required:

- trace must identify whether the value is absent, null, or a concrete DOM/browser object for each trigger variant;
- if one trigger emits null and another emits an object, they remain separate rows until proven safe.

Shipped source/docs evidence required:

- d.ts/source declaration of the DOM/browser object where available;
- if d.ts omits the property but runtime emits it, record the source path that attaches it.

Primitive mapping decision:

- do not expose browser-owned objects as broad `object`, `dynamic`, or stringly payload fields;
- exclude the member from public typed Fusion payload unless an existing DOM/component/plugin source primitive can consume it deterministically;
- if behavior needs it, create a separate row that maps a safe scalar derived from the object through an existing primitive.

C# naming rule:

- no public `object Target` or `dynamic Target` on component event args;
- prefer explicit exclusion until a typed scalar/source contract is proven.

Vertical slice rule:

- record the excluded DOM payload member in the component audit report and row artifact.

Typed DSL Playwright proof required:

- if excluded, prove accepted fields without relying on the DOM object;
- if later accepted as a derived scalar/source, prove the exact derivation through typed DSL behavior.

Common false-positive to reject:

- serializing a DOM element or treating its debug text as a stable payload contract.

First evidence: Grid `dataStateChange.action.target` was `null` for method-fired sorting and a `TH` element for header-click sorting.

### P005: Event Payload Coverage Is Property-Level

Trigger condition: a typed Fusion event payload class exposes one or more public
properties.

Raw EJ2 evidence required:

- one trace row for each accepted public payload property;
- for same-named events, the trace row must name the trigger variant that proves
  the property;
- absent, undefined, null, duplicate, and excluded fields must stay documented.

Shipped source/docs evidence required:

- source declaration for the event type and any imported payload type;
- if d.ts names collide across Syncfusion packages, resolve through the
  component import graph or fail as ambiguous.

Primitive mapping decision:

- map each property independently to an existing payload read, nested payload
  read, whole typed array source, array operation, payload mutation, or payload
  method primitive;
- do not mark a payload class complete while one public property is unproven.

C# naming rule:

- public payload class members require clear C# names and focused use cases;
- shared payload classes with variant-only members remain open until each public
  member has its own row or is removed.

Vertical slice rule:

- generated coverage matrices must include `Class.Property` rows, not only
  class rows.

Typed DSL Playwright proof required:

- each accepted property must be consumed by behavior through the typed DSL,
  visible output, request payload, or visible runtime state.

Common false-positive to reject:

- a green payload class row whose public properties were never individually
  traced and asserted.

First evidence: Grid `FusionGridEditActionArgs.SelectedRow` needed its own
ActionBegin save/edit proof, while `FusionGridEditActionArgs.Index` stayed
unproven for that variant.

### P006: Static Type Discovery Must Follow The Component Import Graph

Trigger condition: TypeScript event payload discovery finds a type name that
exists in more than one Syncfusion package or file.

Raw EJ2 evidence required:

- runtime trace still decides the accepted payload keys for a row;
- static source only narrows candidate payload types and inherited members.

Shipped source/docs evidence required:

- resolve the type through the component d.ts import statement when present;
- prefer the component package only when no explicit import is available;
- if multiple candidates remain, record `ambiguous` and fail the artifact gate.

Primitive mapping decision:

- no primitive mapping may use an ambiguously resolved payload type.

C# naming rule:

- do not name C# payloads from an unrelated package's type with the same name.

Vertical slice rule:

- discovery tooling must reject polluted `event-payload-surface.json` output
  before C# implementation starts.

Typed DSL Playwright proof required:

- Playwright proof cannot rescue a polluted static payload contract; rerun
  discovery with scoped type resolution first.

Common false-positive to reject:

- using the first declaration found under `node_modules/@syncfusion` for a
  common name such as `SearchEventArgs`, `DeleteEventArgs`, or `ClickEventArgs`.

First evidence: Grid static discovery initially resolved `SearchEventArgs` from
FileManager, `DeleteEventArgs` from Buttons, and toolbar `ClickEventArgs` from
Chips instead of the Grid import graph.

### P007: Remote Data Is A Primary Behavior Lane

Trigger condition: a component binds, queries, refreshes, or replaces data after
render, especially through custom binding, DataManager, adaptor, response-body
binding, or nested data-source members. Treat remote data as mandatory for
data-capable Syncfusion components because realistic web apps cannot assume all
data is loaded locally.

Raw EJ2 evidence required:

- probe must use the data mode that activates the realistic behavior, such as
  Grid custom binding with `{ result, count }`;
- trace must show request/data-state payloads, response assignment shape,
  visible row/item refresh, and relevant `dataSource`, `refresh`, DataManager,
  adaptor, or nested data-source behavior.

Shipped source/docs evidence required:

- source/docs for the data module, remote/custom-binding event path, and
  adaptor/data-source shape.

Primitive mapping decision:

- use existing component property set/read, response-body read, event payload
  read, whole typed array source, and method-call primitives;
- do not add primitives during onboarding to compensate for unread discovery.

C# naming rule:

- remote-data APIs must be named around the real typed behavior:
  `SetDataSource`, `Data`, `Refresh`, data-state events, or component-specific
  nested data-source paths.

Vertical slice rule:

- remote/custom/data-source rows belong in the component data-source slice and
  must stay linked to the event/method rows that cause refresh.

Typed DSL Playwright proof required:

- prove a real user/component workflow that sends data-state input or consumes a
  response, then visibly updates the component through the typed Fusion DSL.

Common false-positive to reject:

- proving only local static arrays or plan JSON and calling remote/custom
  binding complete;
- closing a data-capable component without identifying its remote binding,
  filtering, lookup, paging, virtualization, lazy-load, or server-query lane.

First evidence: Grid `dataStateChange` rows require custom binding and
whole-response `SetDataSource(ResponseBody<TResponse>)` behavior for
`{ result, count }`. The whole-response row is separate from response-path,
event-payload-path, DataManager/adaptor, nested data-source, typed-array
`SetDataSource`, `Data`, and `Refresh` rows.

### P008: Public API Matrix Rows Need Stable Behavior Identity

Trigger condition: public C# APIs share the same method/event name through
overloads, extension receivers, generic arguments, or lifecycle-specific helper
methods.

Raw EJ2 evidence required:

- one row per distinct Syncfusion behavior shape, argument shape, lifecycle
  event, payload mutation, or return source.

Shipped source/docs evidence required:

- source signature and receiver type for every overload/helper;
- source event/lifecycle path for payload mutation helpers.

Primitive mapping decision:

- do not reuse one proof across same-name overloads unless source and raw traces
  prove equivalent behavior.

C# naming rule:

- matrix display names must include a stable lane, signature, or owner:
  `SetDataSource [whole response body]`, `SetDataSource [event payload path]`,
  `FusionGridCellSaveArgs.Cancel()`.

Vertical slice rule:

- keep the row identity stable across artifact regeneration so reviewers can
  diff proof status.

Typed DSL Playwright proof required:

- each overload/helper row gets its own realistic behavior proof or remains
  unproven.

Common false-positive to reject:

- one green `SetDataSource` or `Cancel` row silently closing multiple different
  overloads/lifecycle behaviors.

First evidence: Grid has four distinct `SetDataSource` overloads and four
payload `Cancel` helpers that initially generated indistinguishable matrix rows.

### P009: Variant-Sensitive Payload Rows Must Not Be Coalesced

Trigger condition: one event payload class is shared by variants where a member
can be present, absent, duplicated, excluded, or deferred depending on trigger.

Raw EJ2 evidence required:

- one variant row per trigger, including present keys, absent keys, undefined
  keys, and excluded keys.

Shipped source/docs evidence required:

- source event declaration and source action path for each trigger variant when
  available.

Primitive mapping decision:

- mark each property as accepted, absent, excluded, or deferred per trigger
  variant before class-level status can become green.

C# naming rule:

- do not let a property proven in one variant imply that it is safe for all
  variants.

Vertical slice rule:

- generated matrices must keep shared payload classes open until variant status
  is explicit.

Typed DSL Playwright proof required:

- the proof must trigger the exact variant that accepts the property.

Common false-positive to reject:

- `FusionGridAction.Cancel` marked proven by sorting while searching/grouping
  artifacts explicitly say it is absent.

First evidence: Grid `dataStateChange` shares `FusionGridAction` across
sorting, paging, filtering, searching, and grouping, but not every property is
accepted in every variant.

### P010: Open Audit Lanes Must Be Matrix Rows, Not Prose

Trigger condition: reviewers identify a major behavior lane, overload family,
variant family, event-payload judgment, or applicability question that is not
closed yet.

Raw EJ2 evidence required:

- one pending or proven matrix row for every behavior lane that must eventually
  get a raw trace;
- one pending or proven matrix row for every accepted or excluded event-payload
  member once a judgment-call artifact names that decision;
- no prose-only open lanes.

Shipped source/docs evidence required:

- source file or shipped docs path for the open lane;
- source member/behavior name stable enough to survive regeneration.

Primitive mapping decision:

- unresolved lanes stay `unproven`; they do not justify new primitives.

C# naming rule:

- row names must be concrete enough to identify the future C# API surface, for
  example `remote-data: DataManager adaptor dataSource` and
  `remote-data: SetDataSource [response path] response refresh`.

Vertical slice rule:

- the generated matrix is the source of truth for what remains open; audit
  prose may explain the gap but cannot be the only place it exists.

Typed DSL Playwright proof required:

- each row gets realistic behavior proof or remains fail-closed.

Common false-positive to reject:

- mentioning remote data, DataManager/adaptor, nested data source paths, or
  variant payload gaps in the audit report while the generated matrix has no
  row that can fail the gate.

First evidence: Grid remote/custom-binding rows and dataStateChange variant
rows are now generated as typed API coverage matrix rows. The gate counts them
with the rest of the matrix.

Second evidence: Grid `beginEdit` normal-edit judgment originally named
accepted fields and exclusions only in prose. The generator now emits 13
`beginEdit/normal-edit` matrix rows immediately, all `unproven` until focused
typed DSL behavior proves reads, cancel mutation, and explicit exclusions.

### P018: Writable Payload Fields Need Lifecycle Effect Proof

Trigger condition: raw EJ2 event payload exposes a writable field or a public
C# payload helper would mutate the event payload, especially lifecycle flags
such as `cancel`.

Raw EJ2 evidence required:

- record the payload before mutation;
- mutate the exact payload field inside the live event handler;
- record the payload after mutation;
- record the visible or object-state effect that proves whether the mutation
  changed Syncfusion behavior at that lifecycle point.

Shipped source/docs evidence required:

- use vendor source/docs only to identify event timing and payload names;
- do not treat a vendor type declaration with a writable property as behavior
  proof.

Primitive mapping decision:

- map to the existing event payload set primitive only when raw trace proves the
  mutation changes the intended behavior for that event lifecycle;
- if mutation is writable but too late or behavior-neutral, keep it in
  discovery and exclude it from public typed C# for that row.

C# naming rule:

- cancellable lifecycle events may expose a public helper such as `Cancel()`;
- post-action lifecycle events must use a separate payload type or omit the
  helper when the trace proves mutation does not affect behavior.

Vertical slice rule:

- the event row, judgment calls, primitive map, vertical slice plan, audit
  report, and generated matrix must all name whether mutation is accepted or
  excluded for that exact event lifecycle.

Typed DSL Playwright proof required:

- accepted mutation helpers require a behavior proof that the real component
  state changes because the helper ran;
- excluded mutation helpers require public contract absence proof and raw trace
  proof that mutation would be misleading for that lifecycle.

Common false-positive to reject:

- `args.cancel` became `true` in the handler, therefore the event is
  cancellable.

First evidence: Grid `cellSaved` batch edit exposes writable `cancel`, but the
raw trace records `cancelPreventedSavedValue=false` and batch changes still
carry `openTasks=8`. The public C# contract was split to
`FusionGridCellSavedArgs<TRow, TValue>` with no `Cancel()` helper, while
`cellSave` keeps `Cancel()` because its raw and typed proofs show the blocked
value is not accepted.

### P011: Generated Coverage Rows Must Be Artifact-Derived

Trigger condition: a generated proof matrix adds supplemental rows for variants,
remote lanes, overload lanes, exclusions, or audit gaps.

Raw EJ2 evidence required:

- the generated row must trace back to a row artifact, judgment-call table,
  static discovery artifact, or shipped source path.

Shipped source/docs evidence required:

- the generator must read the artifact that owns the decision whenever practical;
- hardcoded proof rows are allowed only for proof status, not for the accepted
  or excluded decision itself.
- a newly written judgment-call table must make the generated matrix stale until
  its accepted/excluded rows appear in the matrix.

Primitive mapping decision:

- accepted/excluded status and proof status are different facts. A field can be
  accepted by judgment and still remain `unproven` until typed DSL behavior
  proves it.

C# naming rule:

- supplemental rows must use the same typed member names that the judgment row
  maps to; unknown or conceptual members may appear only as unproven audit rows.

Vertical slice rule:

- if judgment artifacts and generated rows disagree, the gate must expose the
  row as open or the audit report must mark the artifact drift.
- missing judgment artifacts or unmapped judgment payload names must fail
  generation; silently returning zero rows or skipping unknown names is a false
  pass.

Typed DSL Playwright proof required:

- a generated row becomes `row-proven` only when the proof exercises that
  member's behavior for that exact variant/lane.

Common false-positive to reject:

- hardcoding a subset of accepted fields in the generator and calling the
  variant matrix complete while judgment-call tables accept additional fields.

First evidence: Grid filtering/searching/grouping judgment-call artifacts
accepted common `dataStateChange` fields that the first supplemental generator
omitted. The generator now reads judgment-call tables and marks unproved
accepted fields as unproven rows.

Second evidence: Grid `beginEdit` normal-edit generation now fails when
`judgment-calls-begin-edit-normal.md` is missing or when a judgment row names an
unmapped payload such as `newVendorPayload`.

Third evidence: `_inventory/onboarding-status.*` became stale after BeginEdit
added supplemental rows. `report-fusion-onboarding-status.mjs --check` and the
component artifact gate now fail stale dashboard artifacts instead of relying on
manual count comparison.

### P012: Exclusion Rows Require Explicit Exclusion Proof

Trigger condition: a discovered member is excluded from public typed Fusion DSL
or omitted from a typed request/body.

Raw EJ2 evidence required:

- trace the exact variant that emits, omits, nulls, or changes the member;
- record whether absence is runtime absence, request-body absence, source-level
  absence, duplicated information, DOM/browser ownership, or a builder-owned
  configuration concern.

Shipped source/docs evidence required:

- source or documentation that proves the public contract shape, ownership, or
  non-use of the member for the exact variant when available.

Primitive mapping decision:

- exclusion is a decision, not proof;
- request-body absence and judgment-call text are supporting evidence only;
- the row stays `unproven` until an explicit artifact proves why the public
  typed DSL must not expose or consume that member.

C# naming rule:

- do not close an exclusion row by removing or not naming the C# member; the
  matrix row remains open until the exclusion proof names the exact public
  member and reason.

Vertical slice rule:

- keep excluded members visible in the matrix so future audits can revisit the
  decision without rediscovering the payload from scratch.
- when a generated matrix marks an exclusion row `row-proven`, the vertical
  slice proof narrative must name the exact excluded public member list derived
  from the matrix. A broad phrase such as "all listed exclusions" is not enough
  if the adjacent proof steps name only a subset.

Typed DSL Playwright proof required:

- accepted fields must be proven without relying on the excluded member;
- exclusion rows require their own focused proof or source-backed impossibility
  proof and cannot inherit another row's green status.

Common false-positive to reject:

- treating a passed behavior test, request payload omission, or vertical-slice
  removal as proof that the excluded payload member is fully audited.
- letting the matrix and audit report claim every exclusion is proven while the
  vertical slice proof section names only the members that were convenient to
  document.

First evidence: Grid `dataStateChange/sorting: FusionGridAction.Target` and
filtering `FusionGridTextFilterCriterion.MatchCase`/`Predicate` were previously
green from a hardcoded generator allowlist. They are now fail-closed until an
explicit exclusion proof exists.

Second evidence: Grid `actionComplete/save-edit` row-proven exclusions included
`Row`, `Form`, `Target`, `ForeignKeyData`, `IsScroll`, `PrimaryKey`,
`PrimaryKeyValue`, `RowData`, `Index`, and `Promise`, but the vertical-slice
proof narrative named only `Index` and `Promise`. The artifact gate now derives
row-proven edit-action exclusions from the generated matrix and fails when the
vertical-slice proof section omits any member.

### P013: Data-Source Rows Split By Value Scope And Refresh Behavior

Trigger condition: a component exposes a data-source property, remote/custom
binding shape, data-source read API, data-source write API, or refresh method.

Raw EJ2 evidence required:

- focused probe for the exact value scope being onboarded: local array,
  `{ result, count }` custom-binding object, DataManager/adaptor, nested
  data-source path, response body path, event payload path, or component
  property read;
- visible before/after rows when a write or refresh is claimed;
- method return shape when a method is called.

Shipped source/docs evidence required:

- public d.ts/source member for `dataSource`, `refresh`, or the relevant data
  adaptor path;
- MVC builder ownership for initial render configuration when applicable.

Primitive mapping decision:

- `SetDataSource(ResponseBody<T>)`, `SetDataSource(ResponseBody<T>, path)`,
  `SetDataSource(eventPayload, path)`, `SetDataSource(TypedSource<T[]>)`,
  `Data<T>()`, and `Refresh()` are separate rows;
- proving one source scope does not prove another overload.

C# naming rule:

- overload row names must include the source lane, such as `typed array source`,
  `whole response body`, `response path`, or `event payload path`.

Vertical slice rule:

- keep data-source read/write/method APIs in the component data-source slice;
  do not hide source-scope differences behind one broad row.

Typed DSL Playwright proof required:

- write rows must visibly change the component;
- read rows must feed a realistic consumer, such as an array transform or
  request payload;
- refresh rows must prove visible post-render effect.

Common false-positive to reject:

- a passing remote/custom-binding event row closes every `SetDataSource` or
  `Data()` overload without proving the exact value scope.

First evidence: Grid `SetDataSource(ResponseBody<TResponse>)` is proven by the
remote whole-response `{ result, count }` row. `SetDataSource(TypedSource<T[]>)`,
`Data<TRow>()`, and `Refresh()` are proven by the ArrayGrid typed array row,
while response-path, event-payload-path, DataManager/adaptor, builder-owned
initial dataSource, and nested path rows remain fail-closed.

### P020: Public Methods Need Their Own Row Proof

Trigger condition: a public typed component method changes component state and
then causes an event row that already has payload proof, such as Grid
`SortBy(...)` causing `dataStateChange`.

Raw EJ2 evidence required:

- trace the underlying EJ2 method call and resulting component/event behavior;
- record method arguments, state change, emitted event payload, and absent
  foreign payload fields for that trigger.

Shipped source/docs evidence required:

- use vendor source or declarations to confirm the public method name, argument
  meaning, and whether the method is expected to preserve existing state.

Primitive mapping decision:

- reuse the existing component method-call primitive when the method maps to a
  deterministic EJ2 call;
- do not let the event row alone close the method row.

C# naming rule:

- public C# may wrap stringly EJ2 arguments in typed expressions, enums, or
  clear option names when that improves real use without changing vendor
  semantics;
- the method row must document the wrapper-to-EJ2 mapping.

Vertical slice rule:

- the sandbox control must call the public C# method directly and the vertical
  slice proof must name the method row separately from the event row it emits.

Typed DSL Playwright proof required:

- trigger the public C# method through the vertical slice;
- prove the exact request/event payload produced by that method;
- prove visible behavior changes because the method ran;
- prove excluded/foreign fields are absent for that method trigger.

Common false-positive to reject:

- the event row passed, or the method was used as setup for another test, so the
  public method row is proven.

First evidence: Grid `SortBy((ResidentDirectoryGridItem x) => x.RiskLevel,
Descending)` was previously only used as setup for `ClearSorting()`. It now has
its own focused proof that `sortColumn("riskLevel", "Descending", false)` emits
`sorted[0].name=riskLevel`, `sorted[0].direction=descending`, and visibly
refreshes resident-directory rows.

### P020: Status Next Actions Must Not Pretend Aggregate Rows Are Closable

Trigger condition: an automation report, dashboard, hook, or generated status
artifact selects the next unproven matrix row for a component.

Raw EJ2 evidence required:

- when the first unproven row is an aggregate payload contract, shared event
  selector, broad public property row, or variant-scoped placeholder, the next
  action must point to variant discovery or lane decomposition rather than
  direct row closure;
- the concrete follow-up row still needs its own raw EJ2 probe and trace.

Shipped source/docs evidence required:

- use source/docs only to identify possible trigger variants or lanes;
- do not treat source declarations such as a shared event args type as proof
  that the aggregate row can close.

Primitive mapping decision:

- aggregate rows do not map directly to a primitive;
- only concrete variant, overload, value-scope, event-payload, method, or
  exclusion rows can name an authoritative primitive.

C# naming rule:

- broad C# contracts may remain fail-closed while concrete variant rows prove
  accepted and excluded members;
- status text must preserve that distinction so the next pass does not promote
  broad public API claims without variant evidence.

Vertical slice rule:

- status artifacts should report aggregate gaps as variant/lane discovery work;
- do not send agents to implement or test a broad row that the matrix itself
  says requires variant-scoped proof.

Typed DSL Playwright proof required:

- the proof must target the concrete variant/lane produced by decomposition;
- passing behavior for one concrete row does not close the aggregate row unless
  the matrix proves no other variants or lanes remain.

Common false-positive to reject:

- reporting `close matrix row FusionGridAction` when the generated row says
  `dataStateChange payload requires variant-scoped property rows`.

First evidence: the Fusion onboarding status report selected
`FusionGridAction` as the next Grid row because it was the first unproven matrix
row. `report-fusion-onboarding-status.mjs` now reports
`variant-discovery` and asks to decompose that aggregate row into the next
missing variant or lane row.

### P021: Read-Only Aggregate Payload Rows Resolve Only Through Complete Variant Coverage

Trigger condition: a generated matrix carries an aggregate
`event-payload-contract` or `event-payload-property` row for a payload class
that is also decomposed into per-trigger-variant rows (P020), and every variant
row that emits that member is already proven.

Judgment recorded (read-only aggregate -> covered-by-variant). The aggregate row
resolves to `row-proven`, citing its variant rows, only when ALL listed reasons
hold. Reasons:

- the member is **read-only**: its only behavior is being read, so there is no
  separate mutation/method behavior left to prove at the aggregate level;
- **every** trigger variant that emits the member has a proven row, accepted
  (read proven) or excluded/absent (proven absent) per that variant;
- a read-only property proven across all its trigger variants is fully proven;
  the union of the variant rows already covers every observable behavior;
- a standalone aggregate proof is redundant and unauthorable: the property only
  exists inside a trigger payload, so there is no aggregate gesture to drive in a
  per-scenario Playwright slice;
- resolving it surfaces existing variant proof; it does not weaken the gate,
  because the gate still fails if any cited variant is unproven.

Raw EJ2 evidence required:

- no new raw probe: the proof is the already-committed variant traces the member
  is cited against;
- the member must appear in the variant judgment-call artifacts as accepted or
  excluded for each trigger.

Shipped source/docs evidence required:

- the payload class declaration proving the member is a read-only property, not a
  writable field or a payload method.

Primitive mapping decision:

- reuse the variant rows' primitive mappings; add no new primitive;
- the aggregate row maps to no primitive of its own.

C# naming rule:

- the aggregate keeps the public payload member name; resolution changes status
  only, never the contract.

Vertical slice rule:

- no new slice: the aggregate cites the per-variant focused proof slices already
  recorded.

Typed DSL Playwright proof required:

- the existing per-variant Playwright proofs; the aggregate adds none.

Common false-positive to reject:

- resolving an aggregate while any emitting variant is still unproven;
- resolving a **writable** member (e.g. a `cancel` mutation / `Cancel()` helper)
  through read-variant coverage. Writable members never resolve here; they remain
  their own open mutation rows until P018 mutation proof closes them.

First evidence: Grid `dataStateChange` read-only aggregates
(`FusionGridDataStateChangeArgs.*`, `FusionGridAction.*` read props,
`FusionGridSortColumn/SearchDescriptor/TextFilterCriterion.*`) are covered by the
proven sorting/paging/filtering/searching/grouping variant rows, while the four
writable `Cancel()` mutation helpers stay open as their own method rows requiring
P018 proof.

### P022: Equivalent-Shape Event Variants Collapse to One Selector Row When Raw EJ2 Proves Typed-Surface Equivalence

Trigger condition: one Syncfusion event fires from multiple triggers (gesture,
method, or mode) whose ONLY difference is the surrounding UI, not the typed
payload the C# event class exposes. This is the P001 "Until Proven Equivalent"
exit: it applies only after raw EJ2 evidence, never as an assumption.

Senior-living workflow: care-ops supervisor starts editing a resident record.
The facility uses two editors for the same gesture - inline edit on the roster
table and a dialog editor on the resident profile - and both must report the
same audited row (which resident, which row position, what edit type).

Judgment recorded (P001 variant -> proven equivalent -> one resolved selector +
payload-read rows). The broad event-selector row and its read-only payload-read
rows resolve to `row-proven` only when ALL reasons hold. Reasons:

- the typed C# payload surface is byte-identical across every trigger variant -
  proven by comparing committed raw EJ2 traces field-by-field, not inferred;
- each compared field is **read-only** (writable mutation flags stay open under
  P018 and never collapse here);
- one focused per-scenario slice triggers the event through **every** equivalent
  variant and asserts the same typed reads from each, so the proof is not a
  single-variant shortcut satisfying the row;
- the variants differ only in vendor UI chrome (inline editor vs dialog), which
  is excluded DOM detail (P012), not typed DSL behavior.

Raw EJ2 evidence required:

- one committed probe + trace per trigger variant (e.g.
  `raw-ej2-begin-edit-normal`, `raw-ej2-begin-edit-dialog`);
- a field-by-field equivalence check over the typed surface proving every typed
  member equal across variants; any differing typed field forbids collapse and
  keeps separate rows per P001/P009.

Shipped source/docs evidence required:

- the event payload class declaration showing the exact typed surface that must
  match (e.g. `FusionGridBeginEditArgs<TRow>` = RowData, RowIndex, Type, Cancel).

Primitive mapping decision:

- reuse event payload read and payload mutation primitives; add no new primitive.

C# naming rule:

- equivalence changes status only; the selector and payload member names are
  unchanged.

Vertical slice rule:

- one focused slice proving all equivalent variants in the same scenario; do not
  spread equivalent variants across unrelated views.

Typed DSL Playwright proof required:

- the slice drives each variant through real user interaction and asserts the
  identical typed reads from each variant's own payload.

Common false-positive to reject:

- collapsing variants from docs or assumption without a field-by-field trace
  comparison;
- collapsing when any typed field differs across variants (stays separate per
  P001/P009);
- collapsing a writable mutation member through read equivalence (stays open per
  P018/P021).

First evidence: Grid `beginEdit` normal-mode vs dialog-mode raw EJ2 traces are
byte-identical across the typed surface (RowData, RowIndex, Type, Cancel), so the
`BeginEdit` selector and its read rows resolve through one focused
resident-edit-audit slice that triggers both editors, while
`FusionGridBeginEditArgs.Cancel()` stays its own P018 mutation row.

### P023: A Read-Only Object-Valued Payload Property Is Covered When Its Value-Type Class Is Fully Proven

Trigger condition: an event payload class exposes a read-only property whose type
is another typed payload class (a nested object), and the matrix decomposes the
nested object's own members into per-variant rows rather than the property.

Judgment recorded (read-only object property -> covered-by-value-type). The
property row resolves to `row-proven` only when ALL reasons hold. Reasons:

- the property is **read-only**: its only behavior is returning the nested
  object so the plan can read the object's members; it has no mutation of its
  own (a writable member living *inside* the object, e.g. `FusionGridAction.Cancel`,
  is its own member row under P018 and is unaffected);
- the nested value-type class is fully proven: the class contract row and every
  one of its member rows are `row-proven` across all emitting variants
  (`allVariantsProvenForClass`);
- reading any `object.member` necessarily exercises the property accessor, so a
  fully-proven value type means the accessor was read in every variant that
  emits it; a standalone property proof would be redundant and unauthorable;
- resolving it surfaces existing variant proof; the gate still fails if the
  value-type class has any unproven member.

Raw EJ2 evidence required:

- no new probe: the proof is the committed variant traces in which the nested
  object's members are read.

Shipped source/docs evidence required:

- the payload class declaration proving the property returns a typed payload
  class and is not itself a writable scalar or a payload method.

Primitive mapping decision:

- reuse the nested object's variant rows' primitive mappings; add no new primitive.

C# naming rule:

- the property keeps its public name; resolution changes status only.

Vertical slice rule:

- no new slice: the property cites the nested object's per-variant proof slices.

Typed DSL Playwright proof required:

- the existing per-variant proofs that read the nested object's members.

Common false-positive to reject:

- resolving the property while any member of its value-type class is unproven;
- treating a scalar property as object-valued to dodge its own variant rows;
- resolving a writable member that merely lives inside the object - that member
  keeps its own P018 row.

First evidence: Grid `FusionGridDataStateChangeArgs.Action` returns the
fully-proven `FusionGridAction` class (contract plus all nine members
row-proven across sorting/paging/filtering/searching/grouping variants), so the
read-only `Action` property resolves through that value-type coverage while the
object's writable `FusionGridAction.Cancel` stays its own member row.

### P024: Edit-Action Variants Split Into Invariant Members And Variant-Sensitive Members By Raw EJ2 Population

Trigger condition: a shared edit-lifecycle payload (`actionBegin`/`actionComplete`
with one C# args class) fires for multiple edit requestTypes (save-edit, save-add,
delete, cancel) and a probe shows the typed members populate differently per
requestType.

Raw EJ2 evidence required (committed): one probe per add/delete requestType beside
the save-edit probe, recording each typed member's value and presence.

First evidence: Grid `raw-ej2-action-add-delete` proves the populations are NOT
equivalent across requestTypes, so P022 collapse is forbidden:

- `RequestType`, `Type`, `Name`, `Cancel`, `RowIndex`, `SelectedRow` are
  **invariant** - present with the same type in every requestType (only the
  requestType string value changes). These resolve once read for save-edit AND a
  second requestType, then P021 covers the aggregate.
- `Action` is **variant-sensitive**: `edit` for save-edit, `add` for save-add,
  `undefined` for delete/add-begin.
- `Data` is **variant-sensitive in shape**: the single edited/new row for
  save-edit/save-add, but an array for delete - the typed `TRow Data` targets the
  single-row variants.
- `PreviousData` is **variant-sensitive in presence**: the full original row for
  save-edit, an empty object for save-add, absent for delete/add-begin - the typed
  `TRow PreviousData` is meaningful only for save-edit.

Resolution rule:

- invariant members resolve through P021 after the add/delete variant proves them
  present;
- `Action`/`Data`/`PreviousData` resolve only by recording, per requestType, an
  accepted read (where the typed shape applies) or a proven-absent judgment (P012)
  with the probe as evidence; they never coalesce under one save-edit proof.

Common false-positive to reject:

- treating the save-edit proof as covering add/delete because the C# class is
  shared - the runtime payload differs and the probe proves it.
