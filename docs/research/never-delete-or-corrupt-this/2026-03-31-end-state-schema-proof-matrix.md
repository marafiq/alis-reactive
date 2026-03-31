# Source Of Truth

- Companion proof:
  - [2026-03-31-end-state-schema-proof.md](./2026-03-31-end-state-schema-proof.md)
- Final contract:
  - [2026-03-31-end-state-reactive-plan.schema.json](./2026-03-31-end-state-reactive-plan.schema.json)
- Archived context:
  - [2026-03-31-architecture-understanding.md](./2026-03-31-architecture-understanding.md)
  - [2026-03-31-architecture-understanding-continuation-02.md](./2026-03-31-architecture-understanding-continuation-02.md)

# 2026-03-31 End-State Schema Proof Matrix

## Exhaustive Semantic Families

| Family | Real evidence | End-state schema objects | Self-sufficient at wire time? | Lazy resolution allowed? | Verdict |
|---|---|---|---|---|---|
| `dom-ready` | current `dom-ready` runtime/tests | `DomReadyTrigger` | yes | no | passes cleanly |
| custom event with payload | current custom-event runtime/tests | `CustomEventTrigger`, `payload.trigger`, `DispatchCommand.payload` | yes | no | passes cleanly |
| native readable component event | native input-like components, TestWidgetNative | `ComponentEventTrigger.target`, `ComponentEventTrigger.payload.object`, `ComponentReadValue` | yes | no | passes cleanly |
| native non-readable component event | `NativeButton` | `ComponentEventTrigger.payload.none` | yes | no | passes cleanly |
| fusion callback event | fusion inputs, TestWidgetSyncFusion, Tab, Accordion | `ComponentEventTrigger.payload.callback` | yes | no | passes cleanly |
| payload mutation helpers | filtering helpers / `PreventDefault` / `UpdateData` | `MutatePayloadCommand`, `Mutation`, `PlanValue` | yes | no | passes cleanly |
| component reads outside events | current component sources in commands/guards/requests | `ValueAccess.source.component` | no | no | passes cleanly |
| request gather from literal | static gather | `GatherField.value = LiteralValue` | no | no | passes cleanly |
| request gather from trigger payload | event gather | `GatherField.value = ReadValue(payload.trigger)` | no | no | passes cleanly |
| request gather from response payload | chained-request continuity | `GatherField.value = ReadValue(payload.response)` | no | no | passes cleanly |
| request gather from component | component gather | `GatherField.value = ReadValue(component)` | no | no | passes cleanly |
| `IncludeAll` | current gather include-all | `AllGather`, `components` map | no | yes | passes cleanly |
| GET query sink | current request runtime/tests | `RequestDescriptor.verb = GET` + `GatherField[]` | no | no | passes cleanly |
| JSON body sink | current request runtime/tests | `RequestDescriptor` + `GatherField[]` | no | no | passes cleanly |
| form-data sink | current request runtime/tests | `RequestDescriptor.contentType = form-data` + `GatherField[]` | no | no | passes cleanly |
| success/error handlers | current `StatusHandler` routes | `StatusHandler.reaction` | no | no | passes cleanly |
| chained requests | current chained runtime/tests | `RequestDescriptor.chained` + `payload.response` | no | no | passes cleanly |
| parallel requests | current parallel runtime/tests | `ParallelHttpReaction` | no | no | passes cleanly |
| validation request input | current public validation DSL | `ValidationDescriptor`, `ValidationField`, `ValidationRule` | no | yes | passes cleanly |
| partial validation lifecycle | ajax partial playwright/tests | `ValidationField.modelPath`, `components` map | no | yes | passes cleanly |
| dispatch across scope | component event -> custom event | `DispatchCommand`, `CustomEventTrigger` | no | no | passes cleanly |
| SSE | server-push runtime/tests | `ServerPushTrigger`, `payload.trigger` | yes | no | passes cleanly |
| SignalR | signalr runtime/tests | `SignalRTrigger`, `payload.trigger` | yes | no | passes cleanly |

## Exhaustive Vertical-Slice Mapping

### Native component-event families

**Readable current-value payload**

- `NativeCheckBox/NativeCheckBoxReactiveExtensions.cs`
- `NativeCheckList/NativeCheckListReactiveExtensions.cs`
- `NativeDropDown/NativeDropDownReactiveExtensions.cs`
- `NativeHiddenField/NativeHiddenFieldReactiveExtensions.cs`
- `NativeRadioGroup/NativeRadioGroupReactiveExtensions.cs`
- `NativeTextArea/NativeTextAreaReactiveExtensions.cs`
- `NativeTextBox/NativeTextBoxReactiveExtensions.cs`
- `TestWidgetNative/TestWidgetNativeReactiveExtensions.cs`

End-state mapping:
- `ComponentEventTrigger.payload.kind = object`
- fields project from component current value reads

**Non-readable / marker-only payload**

- `NativeButton/NativeButtonReactiveExtensions.cs`

End-state mapping:
- `ComponentEventTrigger.payload.kind = none`

### Fusion component-event families

**Callback payload passed through**

- `FusionAutoComplete/`
- `FusionColorPicker/`
- `FusionDatePicker/`
- `FusionDateRangePicker/`
- `FusionDateTimePicker/`
- `FusionDropDownList/`
- `FusionFileUpload/`
- `FusionInputMask/`
- `FusionMultiColumnComboBox/`
- `FusionMultiSelect/`
- `FusionNumericTextBox/`
- `FusionRichTextEditor/`
- `FusionSwitch/`
- `FusionTimePicker/`
- `TestWidgetSyncFusion/`

End-state mapping:
- `ComponentEventTrigger.payload.kind = callback`

**Callback payload on non-input interactive widgets**

- `FusionAccordion/`
- `FusionTab/`

End-state mapping:
- `ComponentEventTrigger.payload.kind = callback`

Note:
- `bindingPath` is not needed on the trigger contract in the end state
- component correlation belongs in `components` map and request/validation lookup, not trigger wiring

## Partial-Merge Rule Matrix

| Concern | End-state rule |
|---|---|
| component-event trigger wiring | trigger JSON must be complete at emit time |
| validation field resolution | may resolve against merged `components` map later |
| `IncludeAll` | resolves against the latest `components` map |
| partial unload / reload | components map changes; validation and include-all re-resolve from keys, not copied fields |

## Public DSL Alignment

Compile-time public placeholders remain public DSL only:

- typed event args via `TypedEventDescriptor<TArgs>`
- `ResponseBody<T>`
- public validation input objects
- public raw `BindSource` input where already reachable today

They do **not** need one-to-one emitted schema types.

The emitted schema only carries what the runtime needs:

- trigger attachment
- payload/read/value flow
- request sinks
- validation rules keyed by model path

## Why This Matrix Matters

This file is the kill-switch for the stacked refactor.

If any new proposed end-state contract cannot satisfy every row in this matrix without:

- adding hidden runtime branching
- inventing lazy trigger enrichment
- widening the public DSL

then the stacked refactor plan is dead and must be rewritten.
