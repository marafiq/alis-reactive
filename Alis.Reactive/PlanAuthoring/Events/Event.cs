using System;
using System.Collections.Generic;

namespace Alis.Reactive
{
    /// <summary>
    /// An event a declaration has, whose listener receives a payload declared as its own class:
    /// <c>public Event&lt;FusionGridRowSelectedArgs&gt; RowSelected =&gt; Event&lt;FusionGridRowSelectedArgs&gt;();</c>.
    /// A page handles it with <c>.Reactive(evt =&gt; evt.RowSelected, (args, p) =&gt; ..)</c>.
    /// </summary>
    /// <typeparam name="TArgs">The declaration of the payload.</typeparam>
    public class Event<TArgs>
        where TArgs : JsObject
    {
        /// <summary>The event's JavaScript name.</summary>
        public string ObjectEvent { get; }

        internal TArgs Args { get; }

        // The path from the component's object to the object that raises the event: empty for the component.
        internal IReadOnlyList<string> ObjectPath { get; }

        internal Event(string jsEvent, TArgs args, IReadOnlyList<string> objectPath)
        {
            ObjectEvent = jsEvent ?? throw new ArgumentNullException(nameof(jsEvent));
            Args = args ?? throw new ArgumentNullException(nameof(args));
            ObjectPath = objectPath ?? throw new ArgumentNullException(nameof(objectPath));
        }
    }

    /// <summary>
    /// An event a declaration has, whose listener receives nothing: <c>public Event Unlocked =&gt; Event();</c>.
    /// A page handles it with <c>.Reactive(evt =&gt; evt.Unlocked, p =&gt; ..)</c>.
    /// </summary>
    public sealed class Event : Event<NoPayload>
    {
        internal Event(string jsEvent, IReadOnlyList<string> objectPath)
            : base(jsEvent, new NoPayload(), objectPath)
        {
        }
    }

    /// <summary>What a listener of an <see cref="Event"/> receives: nothing.</summary>
    public sealed class NoPayload : JsObject
    {
    }
}
