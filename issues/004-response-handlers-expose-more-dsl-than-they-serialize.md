# Response Handlers Expose More DSL Than They Serialize

## Verdict

This is a legitimate issue.

`ResponseBuilder.OnSuccess(...)` and `OnError(...)` accept `Action<PipelineBuilder<TModel>>`, which suggests the full pipeline DSL is supported inside response handlers. But the descriptor layer only stores `List<Command>`, so non-sequential response-handler logic is lost or becomes invalid.

## Why This Is Legit

This is a public API contract mismatch.

The API surface invites developers to believe they can use the same pipeline constructs inside response handlers that they can use elsewhere:

- conditional branches
- nested HTTP
- parallel HTTP

But the serialization path only preserves raw `Commands`.

That means the framework currently exposes a richer DSL than it can actually represent for response handlers.

## Evidence

`ResponseBuilder` accepts the full `PipelineBuilder<TModel>`:

```csharp
public ResponseBuilder<TModel> OnSuccess(Action<PipelineBuilder<TModel>> configure)
{
    var builder = new PipelineBuilder<TModel>();
    configure(builder);
    SuccessHandlers.Add(new StatusHandler(builder.Commands));
    return this;
}
```

The same is true for `OnError(...)`:

```csharp
public ResponseBuilder<TModel> OnError(int statusCode, Action<PipelineBuilder<TModel>> configure)
{
    var builder = new PipelineBuilder<TModel>();
    configure(builder);
    ErrorHandlers.Add(new StatusHandler(statusCode, builder.Commands));
    return this;
}
```

But `StatusHandler` stores only commands:

```csharp
public class StatusHandler
{
    public int? StatusCode { get; }
    public List<Command> Commands { get; }
}
```

There is no `Reaction` here, only `Command[]`.

## Why This Matters

This creates a DSL boundary that looks wider than it really is.

A developer can reasonably assume this is valid:

```csharp
.OnSuccess(s => s.When(payload, x => x.Flag).Eq(true).Then(t => t.Dispatch("ok")))
```

But response handlers do not serialize branch reactions. They only serialize `builder.Commands`.

Even if some misuse is now blocked incidentally by other builder checks, the API shape itself is still misleading and too permissive.

## Why This Is Strong

- It is a direct mismatch between public builder API and descriptor representation.
- It is local and easy to explain.
- Fixing it improves SOLID boundaries: response handlers are command handlers, not arbitrary reaction builders.
- The fix can preserve the current developer experience for valid use cases.

## Minimal Fix Direction

Choose one of these:

1. Restrict response handlers to a command-only builder type.
2. Keep `PipelineBuilder<TModel>` but explicitly reject any non-sequential mode before serialization.

Option 2 is the minimal change. It keeps the DSL call shape while enforcing the true contract.

## Suggested Test

Add a unit test that attempts to use conditional or HTTP behavior inside `OnSuccess(...)` and asserts a clear `InvalidOperationException` explaining that response handlers support only sequential commands.

That would turn a leaky API surface into an explicit, accurate contract.

---

## Response — Claude

### Verdict: Fixed (both parts)

This issue had two parts. Both are resolved.

#### Part 1: OnSuccess / OnError — now fully support conditions (issue 000)

The original problem — `OnSuccess`/`OnError` calling `new StatusHandler(builder.Commands)` and silently dropping conditional branches — was fixed in commit `1139d8f` as part of issue 000.

`ResponseBuilder.BuildHandler()` now calls `builder.BuildReaction()`. `StatusHandler` carries either `Commands` (sequential) or `Reaction` (conditional, HTTP, etc). The DSL surface matches serialization. 7 C# + 7 TS tests prove it.

#### Part 2: WhileLoading / OnAllSettled — now fail fast

These two surfaces are command-list by design (no ExecContext exists at those pipeline stages). They now call `BuildReaction()` and throw `InvalidOperationException` if the result is not `SequentialReaction`.

**WhileLoading** (`HttpRequestBuilder.cs:67`):
```csharp
var reaction = builder.BuildReaction();
if (!(reaction is SequentialReaction sr))
    throw new InvalidOperationException(
        "WhileLoading only supports plain commands (sequential). " +
        "Conditions, HTTP, and parallel pipelines are not valid here.");
_whileLoading = sr.Commands;
```

**OnAllSettled** (`ParallelBuilder.cs:32`):
```csharp
var reaction = pb.BuildReaction();
if (!(reaction is SequentialReaction))
    throw new InvalidOperationException(
        "OnAllSettled only supports plain commands (sequential). " +
        "Conditions, HTTP, and parallel pipelines are not valid here.");
```

#### Tests

6 tests in `WhenRejectingNonSequentialInCommandListSurfaces.cs`:

| Test | Surface | Pipeline | Expected |
|------|---------|----------|----------|
| `WhileLoading_rejects_conditional_pipeline` | WhileLoading | `When().Then()` | Throws |
| `WhileLoading_rejects_http_pipeline` | WhileLoading | `Get(url)` | Throws |
| `WhileLoading_allows_plain_commands` | WhileLoading | `Element().Show()` | OK |
| `OnAllSettled_rejects_conditional_pipeline` | OnAllSettled | `When().Then()` | Throws |
| `OnAllSettled_rejects_http_pipeline` | OnAllSettled | `Get(url)` | Throws |
| `OnAllSettled_allows_plain_commands` | OnAllSettled | `Element().SetText()` | OK |

#### Summary

Every `PipelineBuilder`-accepting surface now either:
- Fully serializes the reaction (`BuildReaction()`) — 13 reaction surfaces
- Fails fast on non-sequential pipelines — 2 command-list surfaces

No surface silently drops DSL constructs. The frozen DSL is honored.
