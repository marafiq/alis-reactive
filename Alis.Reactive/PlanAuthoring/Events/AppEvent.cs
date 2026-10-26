using System;
using System.Runtime.CompilerServices;

namespace Alis.Reactive
{
    /// <summary>
    /// A document custom event, declared once: the name a dispatcher raises and a listener hears, so
    /// the two cannot disagree on it. The name is the declaring member's name camel-cased, as every
    /// member of the object model is named, unless <c>name:</c> gives the one JavaScript uses.
    /// </summary>
    /// <remarks>
    /// <code>
    /// public static class CareEvents
    /// {
    ///     public static readonly AppEvent RosterRefreshed = AppEvent.Declare();
    ///     public static readonly AppEvent&lt;ResidentFlagged&gt; Flagged = AppEvent.Declare&lt;ResidentFlagged&gt;();
    /// }
    ///
    /// Html.On(plan, t =&gt; t.CustomEvent(CareEvents.Flagged, (flagged, p) =&gt; ...));
    /// p.Dispatch(CareEvents.Flagged, e =&gt; e.Set(x =&gt; x.ResidentId, row.Get(r =&gt; r.Id)));
    /// </code>
    /// </remarks>
    public sealed class AppEvent
    {
        private AppEvent(string name) => Name = name;

        /// <summary>The DOM event name.</summary>
        public string Name { get; }

        /// <summary>Declares an event that carries no payload.</summary>
        /// <param name="name">The DOM event name, when it is not the declaring member's name camel-cased.</param>
        /// <param name="declaredBy">The declaring member; the compiler supplies it.</param>
        public static AppEvent Declare(string? name = null, [CallerMemberName] string declaredBy = "") =>
            new AppEvent(DeclaredName.Of(name, declaredBy));

        /// <summary>Declares an event whose detail is a <typeparamref name="TPayload"/>.</summary>
        /// <typeparam name="TPayload">The payload a dispatcher sends and a listener reads.</typeparam>
        /// <param name="name">The DOM event name, when it is not the declaring member's name camel-cased.</param>
        /// <param name="declaredBy">The declaring member; the compiler supplies it.</param>
        public static AppEvent<TPayload> Declare<TPayload>(string? name = null, [CallerMemberName] string declaredBy = "")
            where TPayload : class, new() =>
            new AppEvent<TPayload>(DeclaredName.Of(name, declaredBy));

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
        // The declaring member's name camel-cased, as every member of the object model is named, or the
        // name given where JavaScript names it otherwise.
        internal static string Of(string? name, string declaredBy)
        {
            if (name != null) return Required(name);
            if (string.IsNullOrWhiteSpace(declaredBy))
                throw new ArgumentException(
                    "Declare an event or a hub method as a member of a class, for example public static readonly AppEvent Saved = AppEvent.Declare();",
                    nameof(declaredBy));
            return JsonMemberName.Of(declaredBy);
        }

        internal static string Required(string name) =>
            string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("An event needs a name.", nameof(name)) : name;

        internal static string RequiredUrl(string url) =>
            string.IsNullOrWhiteSpace(url) ? throw new ArgumentException("A stream or hub needs a URL.", nameof(url)) : url;
    }
}
