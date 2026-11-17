#if NET10_0_OR_GREATER
using System;
using System.Runtime.CompilerServices;
using Alis.Reactive.Declarations;
using Alis.Reactive.Realtime;
using Alis.Reactive.Schema;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.TriggersAndPipelines;

public sealed partial class Trigger<TModel>
{
    /// <summary>Runs a pipeline each time a stream's endpoint sends one of its declared events, with the event's message.</summary>
    /// <remarks>
    /// Messages run in the order they arrive. A message whose data is not JSON runs nothing and logs one error, and the
    /// next message runs. A message's member the data lacks reads nothing.
    /// </remarks>
    /// <typeparam name="TStream">The stream's declaration.</typeparam>
    /// <typeparam name="TMessage">The class of the event's message.</typeparam>
    /// <param name="stream">The stream, as <c>ServerPushStream.At&lt;TStream&gt;(url)</c> names it.</param>
    /// <param name="message">The event, as a member of the stream's declaration.</param>
    /// <param name="pipeline">The steps to run on each message, with the message's data.</param>
    /// <param name="callerFile">The file of the call, which the compiler supplies.</param>
    /// <param name="callerLine">The line of the call, which the compiler supplies.</param>
    /// <returns>The trigger, to handle another.</returns>
    public Trigger<TModel> ServerPush<TStream, TMessage>(ServerPushSource<TStream> stream, Func<TStream, ServerEvent<TMessage>> message, Action<Data<TMessage>, Pipeline<TModel>> pipeline, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        where TStream : ServerPushStream, new()
        => ServerPush(stream, message, HandlerPolicy.Overlap, pipeline, callerFile, callerLine);

    /// <summary>Runs a pipeline each time a stream's endpoint sends one of its declared events, under a policy for a message that comes while its last run is in flight.</summary>
    /// <remarks>
    /// Messages run in the order they arrive. A message whose data is not JSON runs nothing and logs one error, and the
    /// next message runs. A message's member the data lacks reads nothing.
    /// </remarks>
    /// <typeparam name="TStream">The stream's declaration.</typeparam>
    /// <typeparam name="TMessage">The class of the event's message.</typeparam>
    /// <param name="stream">The stream, as <c>ServerPushStream.At&lt;TStream&gt;(url)</c> names it.</param>
    /// <param name="message">The event, as a member of the stream's declaration.</param>
    /// <param name="policy">What a message does while the run the last message started is in flight.</param>
    /// <param name="pipeline">The steps to run on each message, with the message's data.</param>
    /// <param name="callerFile">The file of the call, which the compiler supplies.</param>
    /// <param name="callerLine">The line of the call, which the compiler supplies.</param>
    /// <returns>The trigger, to handle another.</returns>
    public Trigger<TModel> ServerPush<TStream, TMessage>(ServerPushSource<TStream> stream, Func<TStream, ServerEvent<TMessage>> message, HandlerPolicy policy, Action<Data<TMessage>, Pipeline<TModel>> pipeline, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        where TStream : ServerPushStream, new()
    {
        var (declared, objectId) = StreamPlugin.Reached(_writer, stream);
        var heard = message(declared);
        var data = new Data<TMessage>(_writer, HeldForm.App(_writer.Content.Json, _writer.Content.RendersUnder));
        var steps = _writer.Handler(() => pipeline(data, new Pipeline<TModel>(_writer)));
        _writer.Content.Handles(new Handler(objectId, [], heard.Name, StreamPlugin.MessageData, PathMembers.Required, policy.Grammar, steps), callerFile, callerLine);
        return this;
    }

    /// <summary>Runs a pipeline each time a stream's connection drops, opens again or is given up: <c>s =&gt; s.Reconnecting</c>, <c>s =&gt; s.Reconnected</c> or <c>s =&gt; s.Closed</c>.</summary>
    /// <typeparam name="TStream">The stream's declaration.</typeparam>
    /// <param name="stream">The stream, as <c>ServerPushStream.At&lt;TStream&gt;(url)</c> names it.</param>
    /// <param name="lifecycle">The event, as a member of the stream's declaration.</param>
    /// <param name="pipeline">The steps to run each time.</param>
    /// <param name="callerFile">The file of the call, which the compiler supplies.</param>
    /// <param name="callerLine">The line of the call, which the compiler supplies.</param>
    /// <returns>The trigger, to handle another.</returns>
    public Trigger<TModel> ServerPush<TStream>(ServerPushSource<TStream> stream, Func<TStream, Event> lifecycle, Action<Pipeline<TModel>> pipeline, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        where TStream : ServerPushStream, new()
        => ServerPush(stream, lifecycle, HandlerPolicy.Overlap, pipeline, callerFile, callerLine);

    /// <summary>Runs a pipeline each time a stream's connection drops, opens again or is given up, under a policy for an event that comes while its last run is in flight.</summary>
    /// <typeparam name="TStream">The stream's declaration.</typeparam>
    /// <param name="stream">The stream, as <c>ServerPushStream.At&lt;TStream&gt;(url)</c> names it.</param>
    /// <param name="lifecycle">The event, as a member of the stream's declaration.</param>
    /// <param name="policy">What an event does while the run the last one started is in flight.</param>
    /// <param name="pipeline">The steps to run each time.</param>
    /// <param name="callerFile">The file of the call, which the compiler supplies.</param>
    /// <param name="callerLine">The line of the call, which the compiler supplies.</param>
    /// <returns>The trigger, to handle another.</returns>
    public Trigger<TModel> ServerPush<TStream>(ServerPushSource<TStream> stream, Func<TStream, Event> lifecycle, HandlerPolicy policy, Action<Pipeline<TModel>> pipeline, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        where TStream : ServerPushStream, new()
    {
        var (declared, objectId) = StreamPlugin.Reached(_writer, stream);
        var heard = lifecycle(declared);
        var steps = _writer.Handler(() => pipeline(new Pipeline<TModel>(_writer)));
        _writer.Content.Handles(new Handler(objectId, [], heard.Name, DocumentPlugin.NoPayload, PathMembers.Required, policy.Grammar, steps), callerFile, callerLine);
        return this;
    }

    /// <summary>Runs a pipeline each time a hub sends one of its declared pushes, with the push's message.</summary>
    /// <remarks>
    /// Pushes run in the order they arrive. A push that holds more than one argument, or one that is not an object,
    /// runs nothing and logs one error, and the next push runs. A message's member the data lacks reads nothing.
    /// </remarks>
    /// <typeparam name="THub">The hub's declaration.</typeparam>
    /// <typeparam name="TMessage">The class of the push's message.</typeparam>
    /// <param name="hub">The hub, as <c>SignalRHub.At&lt;THub&gt;(url)</c> names it.</param>
    /// <param name="push">The push, as a member of the hub's declaration.</param>
    /// <param name="pipeline">The steps to run on each push, with the message's data.</param>
    /// <param name="callerFile">The file of the call, which the compiler supplies.</param>
    /// <param name="callerLine">The line of the call, which the compiler supplies.</param>
    /// <returns>The trigger, to handle another.</returns>
    public Trigger<TModel> SignalR<THub, TMessage>(SignalRSource<THub> hub, Func<THub, HubPush<TMessage>> push, Action<Data<TMessage>, Pipeline<TModel>> pipeline, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        where THub : SignalRHub, new()
        => SignalR(hub, push, HandlerPolicy.Overlap, pipeline, callerFile, callerLine);

    /// <summary>Runs a pipeline each time a hub sends one of its declared pushes, under a policy for a push that comes while its last run is in flight.</summary>
    /// <remarks>
    /// Pushes run in the order they arrive. A push that holds more than one argument, or one that is not an object,
    /// runs nothing and logs one error, and the next push runs. A message's member the data lacks reads nothing.
    /// </remarks>
    /// <typeparam name="THub">The hub's declaration.</typeparam>
    /// <typeparam name="TMessage">The class of the push's message.</typeparam>
    /// <param name="hub">The hub, as <c>SignalRHub.At&lt;THub&gt;(url)</c> names it.</param>
    /// <param name="push">The push, as a member of the hub's declaration.</param>
    /// <param name="policy">What a push does while the run the last push started is in flight.</param>
    /// <param name="pipeline">The steps to run on each push, with the message's data.</param>
    /// <param name="callerFile">The file of the call, which the compiler supplies.</param>
    /// <param name="callerLine">The line of the call, which the compiler supplies.</param>
    /// <returns>The trigger, to handle another.</returns>
    public Trigger<TModel> SignalR<THub, TMessage>(SignalRSource<THub> hub, Func<THub, HubPush<TMessage>> push, HandlerPolicy policy, Action<Data<TMessage>, Pipeline<TModel>> pipeline, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        where THub : SignalRHub, new()
    {
        var (declared, objectId) = HubPlugin.Reached(_writer, hub);
        var heard = push(declared);
        var data = new Data<TMessage>(_writer, HeldForm.App(_writer.Content.Json, _writer.Content.RendersUnder));
        var steps = _writer.Handler(() => pipeline(data, new Pipeline<TModel>(_writer)));
        _writer.Content.Handles(new Handler(objectId, [], heard.Name, HubPlugin.MessageData, PathMembers.Required, policy.Grammar, steps), callerFile, callerLine);
        return this;
    }

    /// <summary>Runs a pipeline each time a hub's connection drops or is given up: <c>h =&gt; h.Reconnecting</c> or <c>h =&gt; h.Closed</c>.</summary>
    /// <typeparam name="THub">The hub's declaration.</typeparam>
    /// <param name="hub">The hub, as <c>SignalRHub.At&lt;THub&gt;(url)</c> names it.</param>
    /// <param name="lifecycle">The event, as a member of the hub's declaration.</param>
    /// <param name="pipeline">The steps to run each time.</param>
    /// <param name="callerFile">The file of the call, which the compiler supplies.</param>
    /// <param name="callerLine">The line of the call, which the compiler supplies.</param>
    /// <returns>The trigger, to handle another.</returns>
    public Trigger<TModel> SignalR<THub>(SignalRSource<THub> hub, Func<THub, Event> lifecycle, Action<Pipeline<TModel>> pipeline, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        where THub : SignalRHub, new()
        => SignalR(hub, lifecycle, HandlerPolicy.Overlap, pipeline, callerFile, callerLine);

    /// <summary>Runs a pipeline each time a hub's connection drops or is given up, under a policy for an event that comes while its last run is in flight.</summary>
    /// <typeparam name="THub">The hub's declaration.</typeparam>
    /// <param name="hub">The hub, as <c>SignalRHub.At&lt;THub&gt;(url)</c> names it.</param>
    /// <param name="lifecycle">The event, as a member of the hub's declaration.</param>
    /// <param name="policy">What an event does while the run the last one started is in flight.</param>
    /// <param name="pipeline">The steps to run each time.</param>
    /// <param name="callerFile">The file of the call, which the compiler supplies.</param>
    /// <param name="callerLine">The line of the call, which the compiler supplies.</param>
    /// <returns>The trigger, to handle another.</returns>
    public Trigger<TModel> SignalR<THub>(SignalRSource<THub> hub, Func<THub, Event> lifecycle, HandlerPolicy policy, Action<Pipeline<TModel>> pipeline, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        where THub : SignalRHub, new()
    {
        var (declared, objectId) = HubPlugin.Reached(_writer, hub);
        var heard = lifecycle(declared);
        var steps = _writer.Handler(() => pipeline(new Pipeline<TModel>(_writer)));
        _writer.Content.Handles(new Handler(objectId, [], heard.Name, DocumentPlugin.NoPayload, PathMembers.Required, policy.Grammar, steps), callerFile, callerLine);
        return this;
    }

    /// <summary>Runs a pipeline each time a hub's connection opens again after it was lost, with what the event hands: <c>h =&gt; h.Reconnected</c>.</summary>
    /// <typeparam name="THub">The hub's declaration.</typeparam>
    /// <typeparam name="TArgs">The declaration of what the event hands.</typeparam>
    /// <param name="hub">The hub, as <c>SignalRHub.At&lt;THub&gt;(url)</c> names it.</param>
    /// <param name="lifecycle">The event, as a member of the hub's declaration.</param>
    /// <param name="pipeline">The steps to run each time, with what the event hands.</param>
    /// <param name="callerFile">The file of the call, which the compiler supplies.</param>
    /// <param name="callerLine">The line of the call, which the compiler supplies.</param>
    /// <returns>The trigger, to handle another.</returns>
    public Trigger<TModel> SignalR<THub, TArgs>(SignalRSource<THub> hub, Func<THub, Event<TArgs>> lifecycle, Action<TArgs, Pipeline<TModel>> pipeline, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        where THub : SignalRHub, new()
        where TArgs : JsObject, new()
        => SignalR(hub, lifecycle, HandlerPolicy.Overlap, pipeline, callerFile, callerLine);

    /// <summary>Runs a pipeline each time a hub's connection opens again after it was lost, with what the event hands, under a policy for an event that comes while its last run is in flight.</summary>
    /// <typeparam name="THub">The hub's declaration.</typeparam>
    /// <typeparam name="TArgs">The declaration of what the event hands.</typeparam>
    /// <param name="hub">The hub, as <c>SignalRHub.At&lt;THub&gt;(url)</c> names it.</param>
    /// <param name="lifecycle">The event, as a member of the hub's declaration.</param>
    /// <param name="policy">What an event does while the run the last one started is in flight.</param>
    /// <param name="pipeline">The steps to run each time, with what the event hands.</param>
    /// <param name="callerFile">The file of the call, which the compiler supplies.</param>
    /// <param name="callerLine">The line of the call, which the compiler supplies.</param>
    /// <returns>The trigger, to handle another.</returns>
    public Trigger<TModel> SignalR<THub, TArgs>(SignalRSource<THub> hub, Func<THub, Event<TArgs>> lifecycle, HandlerPolicy policy, Action<TArgs, Pipeline<TModel>> pipeline, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        where THub : SignalRHub, new()
        where TArgs : JsObject, new()
    {
        var (declared, objectId) = HubPlugin.Reached(_writer, hub);
        var heard = lifecycle(declared);
        var steps = _writer.Handler(() => pipeline(JsObject.Reaching<TArgs>(new Received(_writer)), new Pipeline<TModel>(_writer)));
        _writer.Content.Handles(new Handler(objectId, [], heard.Name, heard.Payload, PathMembers.Required, policy.Grammar, steps), callerFile, callerLine);
        return this;
    }
}
#endif
