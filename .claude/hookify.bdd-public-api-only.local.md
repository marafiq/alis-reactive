---
name: bdd-public-api-only
enabled: true
event: file
action: warn
conditions:
  - field: file_path
    operator: regex_match
    pattern: (tests/|Tests/).*\.cs$
  - field: new_text
    operator: regex_match
    pattern: new\s+(ReactivePlanV2Document|Workflow|DomReadySubscription|DocumentEventSubscription|ObjectEventSubscription|ServerPushSubscription|SignalRSubscription|SequenceAction|BranchAction|ParallelAction|SetAction|CallAction|DispatchAction|RequestAction|InjectAction|ShowValidationErrorsAction|ComparePredicate|AllPredicate|AnyPredicate|NotPredicate|ConfirmPredicate|RequestPlan|CapabilityContract|RuntimeObject|FieldBinding)\s*\(
---

**Internal constructor used in test code.**

Tests should arrange using the public DSL only. Internal constructors bypass builders
and create fragile tests that break on refactors even when behavior is unchanged.

**Public DSL alternatives:**

| Internal Type | Use Instead |
|---------------|-------------|
| `Workflow` | `Trigger(plan)...` plus `Then(...)` / `.Reactive(...)` |
| subscription classes | `Trigger(plan).DomReady(...)`, `.OnCustom(...)`, `.OnServerPush(...)`, `.OnSignalR(...)` |
| action classes | `pipeline.Set(...)`, `.Call(...)`, `.Dispatch(...)`, `.Request(...)`, `.Inject(...)` |
| predicate classes | `When(source).Eq(value).And(...)` via condition builders |
| plan document / object / binding / contract classes | Public plan DSL and component registration APIs |

Write tests using the public API only. See `memory/feedback_bdd_no_internals.md`.
