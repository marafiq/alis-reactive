#if NET10_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Alis.Reactive.Realtime;

/// <summary>
/// The endpoint's writer of a stream's messages, each under the name of the event its declaration gives it.
/// </summary>
public static class ServerEvents
{
    /// <summary>Writes a stream's messages as server-sent events, each message as the app's JSON writes it.</summary>
    /// <remarks>The first message carries the app's reconnection interval, so a browser that drops the stream waits what the app chose before it reconnects.</remarks>
    /// <param name="context">The request the stream answers.</param>
    /// <param name="messages">The messages, in the order the page receives them.</param>
    /// <returns>The stream's response.</returns>
    public static ServerSentEventsResult<string> Write(HttpContext context, IAsyncEnumerable<ServerMessage> messages)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));
        if (messages is null) throw new ArgumentNullException(nameof(messages));
        var json = context.RequestServices.GetRequiredService<IOptions<Microsoft.AspNetCore.Mvc.JsonOptions>>().Value.JsonSerializerOptions;
        var interval = RealtimeChoices.IntervalOf(context.RequestServices.GetService<RealtimeChoices>());
        return TypedResults.ServerSentEvents(Items(messages, json, interval, context.RequestAborted));
    }

    private static async IAsyncEnumerable<SseItem<string>> Items(IAsyncEnumerable<ServerMessage> messages, JsonSerializerOptions json, TimeSpan interval, [EnumeratorCancellation] CancellationToken stopped)
    {
        var first = true;
        await foreach (var message in messages.WithCancellation(stopped))
        {
            var item = new SseItem<string>(message.DataUnder(json), message.Name);
            yield return first ? item with { ReconnectionInterval = interval } : item;
            first = false;
        }
    }
}
#endif
