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
}
#endif
