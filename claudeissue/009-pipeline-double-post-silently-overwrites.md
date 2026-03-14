# FAIL-FAST-009: PipelineBuilder Double Post() Silently Overwrites First HTTP Builder

## Status: Fail-Fast Violation (Rule 8)

## File
`Alis.Reactive/Builders/PipelineBuilder.cs:91-97`

## How to Reproduce

1. Call `Post()` twice in the same pipeline:
   ```csharp
   Html.On(plan, t => t.CustomEvent("submit", p => {
       p.Post("/api/save", g => g.IncludeAll())
        .Response(r => r.OnSuccess(s => s.Into("result")));

       // Developer accidentally adds another POST (copy-paste error)
       p.Post("/api/other", g => g.IncludeAll());
   }));
   ```
2. `SetMode(PipelineMode.Http)` on line 93 succeeds because `_mode` is already `Http` — the guard `_mode != PipelineMode.Sequential && _mode != mode` passes when re-entering the same mode.
3. Line 94: `_httpBuilder = new HttpRequestBuilder<TModel>()` replaces the previous builder. The entire first POST configuration (URL, gather, response handlers) is lost.
4. The plan contains only the second POST. The first POST with its response handler is silently dropped.

## Deep Reasoning: Why This Is a Real Bug

The `SetMode` method (line 140-146) correctly prevents switching between different modes (e.g., Http to Parallel), but it allows **re-entering the same mode**. The intent was to allow calling the same mode initializer once, but the code doesn't distinguish between "first call" and "subsequent calls."

Every HTTP verb method (`Get`, `Post`, `Put`, `Delete`) unconditionally creates `_httpBuilder = new HttpRequestBuilder<TModel>()`. There is no check for "was an HTTP builder already configured?"

This is a copy-paste error trap. When a developer builds a complex pipeline with gather and response handling, they might accidentally paste a second `p.Post(...)` call. The first configuration — which may include validation, gather, response routing, chained requests — is silently replaced.

The framework's design says one pipeline = one reaction. A pipeline cannot have two HTTP requests (that is what `Parallel()` is for). The builder should enforce this invariant at build time.

## How Fixing This Improves the Codebase

1. **Fail-fast on double entry**: After `SetMode(Http)`, if `_httpBuilder` is already non-null, throw: `"Cannot configure a second HTTP request in the same pipeline. Use Parallel() for concurrent requests."`.
2. **Guide the developer**: The error message points to the correct API (`Parallel()`) for the scenario they likely intended.
3. **Same pattern for Parallel**: If `_parallelBuilder` is already set, throw on second `Parallel()` call.

## How This Fix Will Not Break Existing Features

- No legitimate code path calls `Post()` twice in the same pipeline. The second call is always a bug.
- The fix only adds a check before the assignment — existing single-call patterns are unaffected.
- The `Parallel()` API exists for the multi-request use case and is unaffected.
