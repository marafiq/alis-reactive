# SOLID Cleanup — 12 Violations Across TS Runtime + C# Framework

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Fix 12 SOLID violations found in the framework — 6 in the TS runtime, 6 in the C# DSL layer. Each fix restores a specific SOLID principle without changing external behavior.

**Architecture:** The SOLID loop (C# DSL → JSON Plan → JS Runtime) is the invariant. Every fix preserves the plan contract. No schema changes. No DSL syntax changes.

**Tech Stack:** TypeScript (esbuild/vitest), C# (.NET 10, System.Text.Json, FluentValidation, NUnit)

---

## Priority Order

| # | Severity | Layer | Principle | Summary |
|---|----------|-------|-----------|---------|
| 1 | HIGH | TS | O/C + Cardinal Rule | trigger.ts invents native event payload |
| 2 | HIGH | C# | SRP | ReactivePlan does 3 jobs with duplicated tree-walking |
| 3 | MED | TS | S + D | commands.ts has vendor knowledge (ej.base.append) |
| 4 | MED | C# | S + D | RequestDescriptor has mutable internal setters + builder metadata |
| 5 | MED | TS | SRP/DRY | execute.ts duplicates sync/async reaction dispatch |
| 6 | MED | C# | Encapsulation | ReactivePlan mutates descriptors it doesn't own |
| 7 | MED | TS | SRP | http.ts mixes HTTP fetch + validation orchestration |
| 8 | MED | C# | O/C | PipelineBuilder implicit priority-based mode dispatch |
| 9 | LOW | C# | Encapsulation | Command.When has public setter |
| 10 | LOW | C# | DIP | FluentValidationAdapter uses Activator.CreateInstance with swallowed catch |
| 11 | LOW | TS | ISP | resolver.ts dual API surface (string BindExpr + typed BindSource) |
| 12 | LOW | TS | Purity | walk.ts has trace side effect in "pure utility" |

---

## Task 1: trigger.ts — Plan-Driven Event Payload Extraction

**Violation:** O/C + Cardinal Rule ("Runtime NEVER invents")

**Current code** (`trigger.ts:41-43`):
```typescript
const detail = trigger.vendor === "native"
  ? { value: e.target?.value, checked: e.target?.checked, event: e }
  : (e ?? {});
```

The runtime hardcodes what properties matter for native events. If a new native component has a different relevant property (e.g., `selectedIndex`, `files`), this code must change. This violates the cardinal rule: the plan should carry ALL behavior, the runtime should never invent.

**Root cause:** The C# `ComponentEventTrigger` already carries a `readExpr` field (e.g., `"value"`, `"checked"`) that describes how to read the component's value. But the trigger wiring ignores it and hardcodes the extraction.

**Fix:** Use the plan's `readExpr` + `component.ts`'s `resolveRoot()` + `walk.ts` to read the event payload from the component — the same pattern `gather.ts` uses via `evalRead()`.

**Files:**
- Modify: `Scripts/trigger.ts:40-44`

**Before:**
```typescript
(root as EventTarget).addEventListener(trigger.jsEvent, (e: any) => {
  const detail = trigger.vendor === "native"
    ? { value: e.target?.value, checked: e.target?.checked, event: e }
    : (e ?? {});
  executeReaction(reaction, { evt: detail });
});
```

**After:**
```typescript
(root as EventTarget).addEventListener(trigger.jsEvent, (e: any) => {
  const detail = trigger.vendor === "native"
    ? { value: walk(el, trigger.readExpr ?? "value"), event: e }
    : (e ?? {});
  executeReaction(reaction, { evt: detail });
});
```

**Why `walk(el, readExpr)` not `evalRead()`:** `evalRead` does `getElementById` + `resolveRoot` + `walk`. Here we already have the element `el` in scope, and for native vendor `resolveRoot` returns `el` itself, so `walk(el, readExpr)` is equivalent but avoids redundant lookups.

**Note:** The `checked` property is no longer hardcoded — when a NativeCheckBox component triggers, its `readExpr` is `"checked"`, so `walk(el, "checked")` produces the correct value. The `event: e` raw event is still included for advanced use cases.

**What about Fusion?** Fusion events already carry their payload in the event object (e.g., SF's `change` event has `.value` in the event args). The Fusion path passes the raw event through — this is correct because SF decides the payload shape, not the runtime. No change needed.

**Tests:**
- Existing TS tests in `when-triggering-on-component-event.test.ts` cover component events
- Existing Playwright tests in `WhenReactiveExtensionsFireInBrowser.cs` cover native dropdown change events
- Both should continue to pass after this change (readExpr already exists in test plan JSON)

**Verify:** `npm test` + `dotnet test Alis.Reactive.PlaywrightTests --filter "WhenReactiveExtensionsFireInBrowser|WhenConditionsFireInsideReactive"`

---

## Task 2: ReactivePlan — Extract Resolution Into Dedicated Resolvers

**Violation:** SRP — `ReactivePlan<TModel>` has 3 responsibilities:
1. Collecting entries and component registrations (its core job)
2. Resolving AllGather markers into ComponentGather items
3. Resolving validation rules from `IValidationExtractor`

Both resolution passes use nearly identical recursive tree-walking (`ResolveGatherInReaction` lines 77-93 and `ResolveReaction` lines 133-148). Every new "resolve at render time" concern would require another copy.

**Fix:** Extract two static resolvers. The plan calls them as a pipeline in `Render()`.

**Files:**
- Create: `Alis.Reactive/Resolvers/GatherResolver.cs`
- Create: `Alis.Reactive/Resolvers/ValidationResolver.cs`
- Modify: `Alis.Reactive/IReactivePlan.cs` — remove resolution methods, call resolvers

**GatherResolver.cs:**
```csharp
namespace Alis.Reactive.Resolvers
{
    internal static class GatherResolver
    {
        internal static void ResolveAll(
            List<Entry> entries,
            List<ComponentRegistration> components)
        {
            foreach (var entry in entries)
                ResolveReaction(entry.Reaction, components);
        }

        private static void ResolveReaction(Reaction reaction, List<ComponentRegistration> components)
        {
            switch (reaction)
            {
                case HttpReaction hr:
                    ResolveRequest(hr.Request, components);
                    break;
                case ParallelHttpReaction phr:
                    foreach (var req in phr.Requests)
                        ResolveRequest(req, components);
                    break;
                case ConditionalReaction cr:
                    foreach (var branch in cr.Branches)
                        ResolveReaction(branch.Reaction, components);
                    break;
            }
        }

        private static void ResolveRequest(RequestDescriptor req, List<ComponentRegistration> components)
        {
            if (req.Gather != null)
            {
                var expanded = new List<GatherItem>();
                foreach (var item in req.Gather)
                {
                    if (item is AllGather)
                    {
                        foreach (var c in components)
                            expanded.Add(new ComponentGather(c.ComponentId, c.Vendor, c.BindingPath, c.ReadExpr));
                    }
                    else
                        expanded.Add(item);
                }
                req.Gather = expanded;
            }
            if (req.Chained != null)
                ResolveRequest(req.Chained, components);
        }
    }
}
```

**ValidationResolver.cs:**
```csharp
namespace Alis.Reactive.Resolvers
{
    internal static class ValidationResolver
    {
        internal static void ResolveAll(List<Entry> entries, IValidationExtractor extractor)
        {
            foreach (var entry in entries)
                ResolveReaction(entry.Reaction, extractor);
        }

        private static void ResolveReaction(Reaction reaction, IValidationExtractor extractor)
        {
            switch (reaction)
            {
                case HttpReaction hr:
                    ResolveRequest(hr.Request, extractor);
                    break;
                case ParallelHttpReaction phr:
                    foreach (var req in phr.Requests)
                        ResolveRequest(req, extractor);
                    break;
                case ConditionalReaction cr:
                    foreach (var branch in cr.Branches)
                        ResolveReaction(branch.Reaction, extractor);
                    break;
            }
        }

        private static void ResolveRequest(RequestDescriptor req, IValidationExtractor extractor)
        {
            if (req.ValidatorType != null && req.Validation != null)
            {
                var extracted = extractor.ExtractRules(req.ValidatorType, req.Validation.FormId);
                if (extracted != null) req.Validation = extracted;
            }
            if (req.ReadExprOverrides != null && req.Validation != null)
            {
                foreach (var kvp in req.ReadExprOverrides)
                    req.Validation = req.Validation.WithReadExpr(kvp.Key, kvp.Value);
            }
            if (req.Chained != null) ResolveRequest(req.Chained, extractor);
        }
    }
}
```

**ReactivePlan.Render() becomes:**
```csharp
public string Render()
{
    GatherResolver.ResolveAll(_entries, _components);
    if (_extractor != null) ValidationResolver.ResolveAll(_entries, _extractor);
    return JsonSerializer.Serialize(new { entries = _entries }, CompactOptions);
}
```

**Tests:** All existing C# unit tests + Playwright tests — zero behavior change.

**Verify:** `dotnet test tests/Alis.Reactive.UnitTests` + `dotnet test tests/Alis.Reactive.PlaywrightTests`

---

## Task 3: commands.ts — Move ej.base.append to component.ts

**Violation:** S + D — `commands.ts:49-54` contains vendor-specific Syncfusion knowledge (`ej.base.append`). Per architecture rules, `component.ts` is the ONLY module that knows vendor-specific APIs.

**Current code** (`commands.ts:43-55`):
```typescript
case "into": {
  const container = document.getElementById(cmd.target);
  if (container && ctx?.responseBody != null) {
    const temp = document.createElement("div");
    temp.innerHTML = String(ctx.responseBody);
    container.innerHTML = "";
    const ej = (globalThis as any).ej;
    if (ej?.base?.append) {
      ej.base.append(Array.from(temp.childNodes), container, true);
    } else {
      container.append(...Array.from(temp.childNodes));
    }
  }
  break;
}
```

**Fix:** Extract `injectHtml(container, html)` function to `component.ts` — the designated home for vendor awareness.

**Files:**
- Modify: `Scripts/component.ts` — add `export function injectHtml(container: HTMLElement, html: string): void`
- Modify: `Scripts/commands.ts` — replace inline ej.base.append with `injectHtml()` call

**component.ts addition:**
```typescript
/** Inject HTML into a container, using ej.base.append when available (SF component init). */
export function injectHtml(container: HTMLElement, html: string): void {
  const temp = document.createElement("div");
  temp.innerHTML = html;
  container.innerHTML = "";
  const ej = (globalThis as any).ej;
  if (ej?.base?.append) {
    ej.base.append(Array.from(temp.childNodes), container, true);
  } else {
    container.append(...Array.from(temp.childNodes));
  }
}
```

**commands.ts `into` case becomes:**
```typescript
case "into": {
  const container = document.getElementById(cmd.target);
  if (container && ctx?.responseBody != null) {
    injectHtml(container, String(ctx.responseBody));
  }
  break;
}
```

**Tests:** Existing Playwright test `IntoLoadsPartialWithNativeAndFusionComponents` covers this path end-to-end.

**Verify:** `npm test` + `dotnet test Alis.Reactive.PlaywrightTests --filter "ContentType"`

---

## Task 4: RequestDescriptor — Remove Mutable Internal Setters + Builder Metadata

**Violation:** S + D — `RequestDescriptor` has:
- `Gather { get; internal set; }` — mutated by GatherResolver
- `Validation { get; internal set; }` — mutated by ValidationResolver
- `ValidatorType { get; set; }` + `ReadExprOverrides { get; set; }` — builder metadata on a descriptor

**This is tightly coupled to Tasks 2 and 6.** The resolvers in Task 2 currently mutate the descriptors. Task 6 addresses the encapsulation concern.

**Fix strategy:** The cleanest approach is to accept that Gather and Validation are resolved at render time (a build-phase concept) and make the `internal set` pattern explicit and narrow. The `[JsonIgnore]` builder metadata (`ValidatorType`, `ReadExprOverrides`) should move to a separate transport type.

**Files:**
- Create: `Alis.Reactive/Descriptors/Requests/RequestBuildContext.cs`
- Modify: `Alis.Reactive/Descriptors/Requests/RequestDescriptor.cs` — remove ValidatorType + ReadExprOverrides
- Modify: `Alis.Reactive/Builders/Requests/HttpRequestBuilder.cs` — return `(RequestDescriptor, RequestBuildContext)` pair
- Modify: `Alis.Reactive/Descriptors/Reactions/Reaction.cs` — HttpReaction stores optional RequestBuildContext
- Modify: `Alis.Reactive/Resolvers/ValidationResolver.cs` — reads from RequestBuildContext

**RequestBuildContext.cs:**
```csharp
namespace Alis.Reactive.Descriptors.Requests
{
    /// <summary>
    /// Builder metadata that travels alongside a RequestDescriptor during the build phase.
    /// Not serialized to JSON — consumed by resolvers at render time, then discarded.
    /// </summary>
    internal sealed class RequestBuildContext
    {
        public Type? ValidatorType { get; }
        public Dictionary<string, string>? ReadExprOverrides { get; }

        public RequestBuildContext(Type? validatorType, Dictionary<string, string>? readExprOverrides)
        {
            ValidatorType = validatorType;
            ReadExprOverrides = readExprOverrides;
        }
    }
}
```

**RequestDescriptor cleanup:**
```csharp
// REMOVE these two properties:
// [JsonIgnore] internal Type? ValidatorType { get; set; }
// [JsonIgnore] internal Dictionary<string, string>? ReadExprOverrides { get; set; }
```

**Note on `Gather { internal set; }` and `Validation { internal set; }`:** These remain `internal set` because the resolve step needs to write expanded gather items and extracted validation rules. This is a conscious trade-off — the alternative (returning new descriptors) would require rebuilding the entire reaction tree, which is over-engineering for an internal build phase. The `internal` visibility scopes mutation to the assembly.

**Tests:** All existing C# unit tests — zero behavior change.

**Verify:** `dotnet test tests/Alis.Reactive.UnitTests`

---

## Task 5: execute.ts — Unify Sync/Async Reaction Dispatch

**Violation:** DRY — `executeReaction` (sync, lines 9-48) and `executeReactionAsync` (async, lines 54-83) duplicate the entire reaction dispatch logic. Any new reaction kind must be added in both.

**Current design rationale:** The sync path exists to avoid async overhead for 99% of reactions that don't use ConfirmGuard. Valid performance concern.

**Fix:** Single async function with a fast sync check at entry. The runtime detects whether any guard in the reaction tree is a ConfirmGuard and only uses `await` in that case.

**Files:**
- Modify: `Scripts/execute.ts`

**After:**
```typescript
export function executeReaction(reaction: Reaction, ctx?: ExecContext): void {
  if (needsAsync(reaction)) {
    executeReactionImpl(reaction, ctx);
    return;
  }
  executeReactionSync(reaction, ctx);
}

/** Fast sync path — no async overhead. Called when no ConfirmGuard in tree. */
function executeReactionSync(reaction: Reaction, ctx?: ExecContext): void {
  switch (reaction.kind) {
    case "sequential":
      log.debug("sequential", { commands: reaction.commands.length });
      for (const cmd of reaction.commands) executeCommand(cmd, ctx);
      break;
    case "conditional":
      log.debug("conditional", { branches: reaction.branches.length });
      for (const branch of reaction.branches) {
        if (branch.guard == null || evaluateGuard(branch.guard, ctx)) {
          executeReactionSync(branch.reaction, ctx);
          return;
        }
      }
      break;
    case "http":
      executeHttpReaction(reaction, ctx);
      break;
    case "parallel-http":
      executeParallelHttpReaction(reaction, ctx);
      break;
  }
}

/** Async path — only reached when ConfirmGuard detected. */
async function executeReactionImpl(reaction: Reaction, ctx?: ExecContext): Promise<void> {
  switch (reaction.kind) {
    case "sequential":
      for (const cmd of reaction.commands) executeCommand(cmd, ctx);
      return;
    case "conditional":
      for (const branch of reaction.branches) {
        if (branch.guard == null || await evaluateGuardAsync(branch.guard, ctx)) {
          await executeReactionImpl(branch.reaction, ctx);
          return;
        }
      }
      break;
    case "http":
      await executeHttpReaction(reaction, ctx);
      return;
    case "parallel-http":
      await executeParallelHttpReaction(reaction, ctx);
      return;
  }
}

/** Check if any branch in the tree uses ConfirmGuard. */
function needsAsync(reaction: Reaction): boolean {
  if (reaction.kind !== "conditional") return false;
  return reaction.branches.some(b =>
    (b.guard != null && isConfirmGuard(b.guard)) || needsAsync(b.reaction)
  );
}
```

**Key change:** The `needsAsync` check moves from inside `case "conditional"` to the top-level `executeReaction` entry point, and the check is recursive (handles nested conditionals). The sync path no longer references any async code.

**Tests:** All existing TS + Playwright condition tests. The `ConfirmGuard` Playwright test specifically exercises the async path.

**Verify:** `npm test` + `dotnet test Alis.Reactive.PlaywrightTests --filter "WhenConditionsFireInsideReactive|Confirm"`

---

## Task 6: ReactivePlan — Stop Mutating Descriptors It Doesn't Own

**Violation:** Encapsulation — Plan reaches into `RequestDescriptor` instances created by `HttpRequestBuilder` and mutates `Gather` and `Validation` during `Render()`.

**This is addressed by Task 2** (extracting resolvers) + **Task 4** (moving builder metadata to RequestBuildContext). The mutation still happens, but:
1. It happens in dedicated resolver classes (not the plan itself)
2. Builder metadata no longer lives on descriptors
3. The `internal set` visibility scopes mutation to the assembly

**Remaining concern:** Calling `Render()` twice produces undefined behavior because the resolvers mutate in place. The resolvers should be idempotent.

**Fix:** Add idempotency guard to GatherResolver — skip requests where gather has already been expanded (no AllGather markers remain).

**Files:**
- Modify: `Alis.Reactive/Resolvers/GatherResolver.cs` (from Task 2)

**Guard:**
```csharp
private static void ResolveRequest(RequestDescriptor req, List<ComponentRegistration> components)
{
    if (req.Gather != null && req.Gather.Exists(item => item is AllGather))
    {
        // Only expand if AllGather markers still present (idempotent)
        var expanded = new List<GatherItem>();
        // ... same expansion logic
        req.Gather = expanded;
    }
    if (req.Chained != null) ResolveRequest(req.Chained, components);
}
```

**Tests:** Add a test that calls `plan.Render()` twice and verifies identical output.

**Verify:** `dotnet test tests/Alis.Reactive.UnitTests`

---

## Task 7: http.ts — Move Validation Orchestration to Pipeline Layer

**Violation:** SRP — `http.ts` handles HTTP fetching but also orchestrates client-side validation (including wiring live clearing handlers). Validation is a pre-request concern.

**Current code** (`http.ts:11-18`):
```typescript
if (req.validation) {
  wireLiveClearing(req.validation);
  if (!validate(req.validation)) {
    log.debug("validation failed, aborting request");
    return;
  }
}
```

**Fix:** Move validation gating to `pipeline.ts` which already handles the pre-fetch phase. `http.ts` becomes pure HTTP mechanics.

**Files:**
- Modify: `Scripts/pipeline.ts` — add validation check before calling `execRequest()`
- Modify: `Scripts/http.ts` — remove validation imports and check

**pipeline.ts change:**
```typescript
// Before calling execRequest, validate if configured
if (req.validation) {
  wireLiveClearing(req.validation);
  if (!validate(req.validation)) {
    log.debug("validation failed, aborting request");
    return;
  }
}
await execRequest(req, ctx);
```

**http.ts becomes:** Pure fetch — no validation imports.

**Tests:** All existing HTTP + validation Playwright tests.

**Verify:** `npm test` + `dotnet test Alis.Reactive.PlaywrightTests --filter "Validation|Http"`

---

## Task 8: PipelineBuilder — Explicit Mode Enforcement

**Violation:** O/C — `BuildReaction()` uses implicit priority (parallel → HTTP → conditional → sequential). No guard prevents `Post()` after `Get()` (silently overwrites). Mode conflicts are silent.

**Fix:** Track mode explicitly and throw on conflicting calls.

**Files:**
- Modify: `Alis.Reactive/Builders/PipelineBuilder.cs`

**Add enum + enforcement:**
```csharp
private enum PipelineMode { Sequential, Http, Parallel, Conditional }
private PipelineMode _mode = PipelineMode.Sequential;

private void SetMode(PipelineMode mode)
{
    if (_mode != PipelineMode.Sequential && _mode != mode)
        throw new InvalidOperationException(
            $"Cannot switch to {mode} — pipeline is already in {_mode} mode.");
    _mode = mode;
}
```

**Apply in each method:**
- `Get/Post/Put/Delete`: call `SetMode(PipelineMode.Http)` (replaces `EnsureNoConditionals()`)
- `Parallel`: call `SetMode(PipelineMode.Parallel)`
- `When/Confirm`: call `SetMode(PipelineMode.Conditional)` (replaces `Commands.Count > 0` check)

**`BuildReaction()` becomes a clean switch on `_mode`:**
```csharp
public Reaction BuildReaction() => _mode switch
{
    PipelineMode.Parallel => _parallelBuilder!.BuildReaction(Commands.Count > 0 ? Commands : null),
    PipelineMode.Http => new HttpReaction(Commands.Count > 0 ? Commands : null, _httpBuilder!.BuildRequestDescriptor()),
    PipelineMode.Conditional => new ConditionalReaction(ConditionalBranches!.ToArray()),
    _ => new SequentialReaction(Commands),
};
```

**Tests:** Existing tests pass + add negative test for conflicting modes (e.g., `Post()` then `When()` throws).

**Verify:** `dotnet test tests/Alis.Reactive.UnitTests`

---

## Task 9: Command.When — Restrict Setter Visibility

**Violation:** Encapsulation — `public set` on `When` property allows any code to overwrite a descriptor's guard.

**Current code** (`Command.cs:14`):
```csharp
public Guard? When { get; set; }
```

Set by `ElementBuilder.When()` which indexes into the pipeline's command list:
```csharp
_pipeline.Commands[^1].When = gb.Guard;
```

**Fix:** Make `When` have `internal set`. The `ElementBuilder` is in the same assembly so `internal` access works.

**Files:**
- Modify: `Alis.Reactive/Descriptors/Commands/Command.cs:14`

**After:**
```csharp
public Guard? When { get; internal set; }
```

**Tests:** All existing — no external code sets `When` directly.

**Verify:** `dotnet build` + `dotnet test tests/Alis.Reactive.UnitTests`

---

## Task 10: FluentValidationAdapter — Replace Bare Catch + Accept Factory

**Violation:** DIP — `Activator.CreateInstance` with swallowed `catch` blocks means:
1. Validators with constructor dependencies silently fail
2. Instantiation errors are invisible

**Fix (minimal, pragmatic):**
1. Replace bare `catch` with `catch (Exception ex)` + trace logging
2. Accept optional `Func<Type, IValidator?>` factory in constructor, fallback to `Activator.CreateInstance`

**Files:**
- Modify: `Alis.Reactive.FluentValidator/FluentValidationAdapter.cs`

**Constructor:**
```csharp
private readonly Func<Type, IValidator?> _factory;

public FluentValidationAdapter() : this(null) { }

public FluentValidationAdapter(Func<Type, IValidator?>? factory)
{
    _factory = factory ?? (type => Activator.CreateInstance(type) as IValidator);
}
```

**Usage:** Replace all 3 `Activator.CreateInstance` calls with `_factory(type)`.

**Bare catch → specific:**
```csharp
catch (Exception ex)
{
    // Log but don't throw — nested validator can't be instantiated,
    // its rules won't be extracted for client-side validation
    System.Diagnostics.Debug.WriteLine($"[FluentValidationAdapter] Failed to instantiate {adaptor.ValidatorType}: {ex.Message}");
}
```

**Tests:** Existing FluentValidator tests pass. No DI in test validators.

**Verify:** `dotnet test tests/Alis.Reactive.UnitTests --filter "WhenValidating"`

---

## Task 11: resolver.ts — Retire Legacy String API

**Violation:** ISP — Dual API surface: `resolveSource(BindSource)` (typed) alongside `resolve(BindExpr)` (string). `element.ts` uses the string API, bypassing the typed contract.

**Fix:** This is a future cleanup — retire string API when `MutateElementCommand.source` migrates from `string` to `BindSource`. For now, mark legacy functions with `@deprecated` JSDoc.

**Files:**
- Modify: `Scripts/resolver.ts` — add `@deprecated` annotations

**After:**
```typescript
/** @deprecated Use resolveSource() with typed BindSource instead. */
export function resolve(expr: BindExpr, ctx?: ExecContext): unknown { ... }

/** @deprecated Use resolveSourceAs() with typed BindSource instead. */
export function resolveAs(expr: BindExpr, coerceAs: CoercionType, ctx?: ExecContext): unknown { ... }
```

**Tests:** None needed — annotation only.

**Verify:** `npm run typecheck`

---

## Task 12: walk.ts — Remove Trace Side Effect

**Violation:** CLAUDE.md declares walk.ts as "Pure utility. Zero side effects." The `log.trace()` call is a side effect.

**Current code** (`walk.ts:22`):
```typescript
log.trace("walk", { path, result: current });
```

**Fix:** Remove the trace call. Walk is called from hot paths (validation loops, gather loops, every bind resolution). Callers that need trace already log at their own scope (resolver, component, gather).

**Files:**
- Modify: `Scripts/walk.ts` — remove `log` import and trace call

**After:**
```typescript
export function walk(root: unknown, path: string): unknown {
  const parts = path.split(".");
  let current: any = root;
  for (const part of parts) {
    if (current == null) return undefined;
    current = current[part];
  }
  return current;
}
```

**Tests:** All existing walk tests pass (they don't depend on trace output).

**Verify:** `npm test`

---

## Dependency Graph

```
Task 12 (walk.ts trace)        — independent
Task 11 (resolver.ts @deprecated) — independent
Task 9  (Command.When internal set) — independent
Task 10 (FluentValidation factory)  — independent
Task 1  (trigger.ts readExpr)  — independent
Task 3  (commands.ts injectHtml) — independent
Task 5  (execute.ts unify)     — independent
Task 7  (http.ts → pipeline.ts) — independent
Task 8  (PipelineBuilder mode) — independent
Task 2  (ReactivePlan resolvers) — prerequisite for Task 4 and 6
Task 4  (RequestDescriptor cleanup) — depends on Task 2
Task 6  (Idempotent resolution)    — depends on Task 2
```

Tasks 1, 3, 5, 7, 8, 9, 10, 11, 12 are fully independent and can be done in any order. Tasks 4 and 6 must follow Task 2.

---

## Execution Order (Recommended)

**Phase A — Quick wins (LOW severity, independent):**
Tasks 12, 11, 9

**Phase B — TS runtime cleanup (MED severity):**
Tasks 1, 3, 5, 7

**Phase C — C# framework cleanup (MED severity):**
Tasks 2, 4, 6, 8, 10

Each task is one commit. Run all 3 test layers between tasks.
