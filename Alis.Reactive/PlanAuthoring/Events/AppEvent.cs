using System;

namespace Alis.Reactive
{
    /// <summary>
    /// A document custom event, declared once: the name a dispatcher raises and a listener hears, so
    /// the two cannot disagree on it.
    /// </summary>
    /// <remarks>
    /// <code>
    /// public static class CareEvents
    /// {
    ///     public static readonly AppEvent RosterRefreshed = AppEvent.Named("roster-refreshed");
    ///     public static readonly AppEvent&lt;ResidentFlagged&gt; Flagged = AppEvent.Named&lt;ResidentFlagged&gt;("resident-flagged");
    /// }
    ///
    /// Html.On(plan, t =&gt; t.CustomEvent(CareEvents.Flagged, (flagged, p) =&gt; ...));
    /// p.DispatchWith(CareEvents.Flagged, e =&gt; e.Set(x =&gt; x.ResidentId, row.Get(r =&gt; r.Id)));
    /// </code>
    /// </remarks>
    public sealed class AppEvent
    {
        private AppEvent(string name) => Name = name;

        /// <summary>The DOM event name.</summary>
        public string Name { get; }

        /// <summary>Declares an event that carries no payload.</summary>
        /// <param name="name">The DOM event name, as raised on the document.</param>
        public static AppEvent Named(string name) => new AppEvent(DeclaredName.Required(name));

        /// <summary>Declares an event whose detail is a <typeparamref name="TPayload"/>.</summary>
        /// <typeparam name="TPayload">The payload a dispatcher sends and a listener reads.</typeparam>
        /// <param name="name">The DOM event name, as raised on the document.</param>
        public static AppEvent<TPayload> Named<TPayload>(string name)
            where TPayload : class, new() =>
            new AppEvent<TPayload>(DeclaredName.Required(name));

        /// <inheritdoc />
        public override string ToString() => Name;
    }

    /// <summary>A document custom event whose detail is a <typeparamref name="TPayload"/>; see <see cref="AppEvent"/>.</summary>
    /// <typeparam name="TPayload">The payload a dispatcher sends and a listener reads.</typeparam>
    public sealed class AppEvent<TPayload> where TPayload : class, new()
    {
        internal AppEvent(string name) => Name = name;

        /// <summary>The DOM event name.</summary>
        public string Name { get; }

        /// <inheritdoc />
        public override string ToString() => Name;
    }

    internal static class DeclaredName
    {
        internal static string Required(string name) =>
            string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("An event needs a name.", nameof(name)) : name;

        internal static string RequiredUrl(string url) =>
            string.IsNullOrWhiteSpace(url) ? throw new ArgumentException("A stream or hub needs a URL.", nameof(url)) : url;
    }
}
