#if NET10_0_OR_GREATER
using System;

namespace Alis.Reactive.Realtime;

/// <summary>
/// What the app chooses once at startup for every stream its views name: whether the page offers to reconnect one
/// that gave up, and how long the browser waits before it reconnects one that dropped.
/// </summary>
public sealed class RealtimeChoices
{
    private static readonly TimeSpan StatedByNoApp = TimeSpan.FromSeconds(3);

    private OfferChoice _offer = OfferChoice.NotChosen;

    internal RealtimeChoices()
    {
    }

    internal TimeSpan Interval { get; private set; } = StatedByNoApp;

    /// <summary>Shows the page's reconnect component while a connection has given up, so a person can reconnect it.</summary>
    /// <remarks>A page whose scripts provide no reconnect component opens no stream and logs one error.</remarks>
    /// <returns>The choices.</returns>
    public RealtimeChoices ShowReconnect() => Chosen(OfferChoice.Show);

    /// <summary>Offers nothing when a connection gives up: the page's closed steps are what a person sees.</summary>
    /// <returns>The choices.</returns>
    public RealtimeChoices NoReconnectOffer() => Chosen(OfferChoice.None);

    /// <summary>Sets how long the browser waits before it reconnects a stream that dropped.</summary>
    /// <remarks>The endpoint sends it with the stream's first message; an app that sets none waits 3 seconds.</remarks>
    /// <param name="wait">The wait, more than zero.</param>
    /// <returns>The choices.</returns>
    public RealtimeChoices ReconnectionInterval(TimeSpan wait)
    {
        if (wait <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(wait), wait, "A reconnection waits more than zero.");
        Interval = wait;
        return this;
    }

    internal static RealtimeChoices Configured(Action<RealtimeChoices> live)
    {
        if (live is null) throw new ArgumentNullException(nameof(live));
        var choices = new RealtimeChoices();
        live(choices);
        return choices;
    }

    internal static TimeSpan IntervalOf(RealtimeChoices? app) => app?.Interval ?? StatedByNoApp;

    internal static bool Offers(RealtimeChoices? app)
    {
        var offer = app?._offer ?? OfferChoice.NotChosen;
        if (offer == OfferChoice.NotChosen) throw new InvalidOperationException(NoChoice);
        return offer == OfferChoice.Show;
    }

    private const string NoChoice = "A page that names a stream or a hub needs the app's reconnect choice: call ShowReconnect() or NoReconnectOffer() in AddReactiveRealtime.";

    private RealtimeChoices Chosen(OfferChoice offer)
    {
        if (_offer != OfferChoice.NotChosen) throw new InvalidOperationException("The app's reconnect choice is made once: call ShowReconnect() or NoReconnectOffer(), not both.");
        _offer = offer;
        return this;
    }

    private enum OfferChoice
    {
        NotChosen,
        Show,
        None,
    }
}
#endif
