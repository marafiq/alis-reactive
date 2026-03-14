# DESIGN-030: PlanExtensions.ResolvePlan() Is Identical to ReactivePlan()

## Status: Design Issue — Misleading API

## File
`Alis.Reactive.Native/Extensions/PlanExtensions.cs:18-32`

## How to Reproduce

1. Read the two methods:
   ```csharp
   public static IReactivePlan<TModel> ReactivePlan<TModel>(this IHtmlHelper<TModel> html)
       where TModel : class
   {
       return new ReactivePlan<TModel>();
   }

   public static IReactivePlan<TModel> ResolvePlan<TModel>(this IHtmlHelper<TModel> html)
       where TModel : class
   {
       return new ReactivePlan<TModel>();
   }
   ```
2. Both methods have identical implementations: `return new ReactivePlan<TModel>();`.
3. The comment on `ResolvePlan` says: "Creates a ReactivePlan for a partial that belongs to the parent's plan. Same planId — runtime merges by planId. Same code as ReactivePlan."

## Deep Reasoning: Why This Is a Design Issue

The method name `ResolvePlan` implies "find and return an existing plan" — as if it looks up a parent view's plan and returns a reference to it. But it creates a brand-new `ReactivePlan<TModel>`. The name is a **semantic lie**.

The comment "Same code as ReactivePlan" is honest, but raises the question: why does this method exist? If it does exactly the same thing, it adds API surface area with no behavioral value. A developer must now choose between two methods that do the same thing, creating unnecessary cognitive load.

The intent (documented in the comment) is to communicate that partials should use `ResolvePlan` and parent views should use `ReactivePlan`. This is a **naming convention for developer guidance**, not a functional distinction. The guidance should be in documentation, not in a duplicate method.

## How Fixing This Improves the Codebase

Option A: **Remove `ResolvePlan()`** — partials and parent views both use `ReactivePlan()`. The comment on `ReactivePlan` can note that partials create separate plans that the runtime merges by planId.

Option B: **Make `ResolvePlan()` actually resolve** — accept a plan instance as parameter and return it, or look up the parent plan from `ViewContext`. This would give the method a real purpose.

Option A is simpler and eliminates confusion. The runtime already merges plans by planId (auto-boot.ts lines 18-24) — the C# API does not need to distinguish between parent and partial plan creation.

## How This Fix Will Not Break Existing Features

- If `ResolvePlan()` is removed, any view that calls it will get a compile error pointing to `ReactivePlan()` as the replacement. The behavior is identical.
- The runtime merge-by-planId logic is unaffected — it operates on the serialized JSON, not on how the plan was created.
- All Playwright tests work because the plan JSON output is identical regardless of which factory method was used.
