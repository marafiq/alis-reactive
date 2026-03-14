# DESIGN-023: EventSource Type Name Used for Response Body Paths (Misleading)

## Status: Design Issue — Naming Inconsistency

## File
`Alis.Reactive/Builders/ElementBuilder.cs:62`

## How to Reproduce

1. Use a response body path in a SetText call:
   ```csharp
   p.Get("/api/data")
    .Response(r => r.OnSuccess(s =>
        s.Element("output").SetText(ResponseBody.Of<DataResponse>(), x => x.Name)
    ));
   ```
2. Examine the plan JSON:
   ```json
   {
     "kind": "mutate-element",
     "target": "output",
     "jsEmit": "el.textContent = val",
     "source": { "kind": "event", "path": "responseBody.name" }
   }
   ```
3. The source is `EventSource` with kind `"event"`, but the path starts with `"responseBody."` — this is not an event.

## Deep Reasoning: Why This Is a Design Issue

The `BindSource` polymorphic type has two variants:
- `EventSource` (kind: `"event"`) — resolves paths against `ExecContext`
- `ComponentSource` (kind: `"component"`) — reads from DOM component

The name `EventSource` implies "data from an event payload" (`evt.address.city`). But `ExpressionPathHelper.ToResponsePath()` generates paths like `"responseBody.name"`, which are response body data, not event payloads. Both are resolved the same way (walking the `ExecContext`), but the semantic meaning is different.

This naming lie has consequences:
1. **Developer confusion**: When reading plan JSON, `"kind": "event"` for a response body path is misleading.
2. **Schema documentation**: The schema says EventSource path is "Dot-notation path into execution context" — technically correct but hides the dual usage.
3. **TypeScript naming**: `types.ts` exports `EventSource` (line 116) which conflicts with the browser's built-in `EventSource` global (Server-Sent Events).

The root cause: the `ExecContext` is a flat bag that holds both `evt` (event payload) and `responseBody` (HTTP response). The resolution mechanism is the same (`walk(ctx, path)`), so the C# DSL reuses `EventSource` for both. Functionally correct, semantically misleading.

## How Fixing This Improves the Codebase

Option A: Rename `EventSource` to `ContextSource` (or `PathSource`) — accurately describes that it resolves a path against the execution context, regardless of whether the data came from an event or a response.

Option B: Add a third `BindSource` variant: `ResponseSource` (kind: `"response"`) — makes the plan self-documenting. The resolver dispatches `"event"` → `walk(ctx, path)` and `"response"` → `walk(ctx, path)` — same implementation, different semantic marker.

Option A is simpler and sufficient.

## How This Fix Will Not Break Existing Features

- The JSON schema `kind` discriminator would change from `"event"` to the new name. This requires:
  1. Update `BindSource.cs` class name and `[JsonDerivedType]` discriminator
  2. Update `reactive-plan.schema.json` definition
  3. Update `types.ts` interface name and kind literal
  4. Update `resolver.ts` switch case
  5. Update all `.verified.txt` snapshot files
- This is a rename refactor — all layers change consistently, no behavioral difference.
- Alternatively, the rename can be schema-backward-compatible by keeping `kind: "event"` as the discriminator but renaming only the C# class and TS type. This is less clean but avoids snapshot churn.
