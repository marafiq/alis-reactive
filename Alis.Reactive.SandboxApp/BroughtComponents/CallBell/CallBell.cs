using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.SandboxApp.BroughtComponents
{
    // The call bell (Scripts/call-bell.ts) is a widget the sandbox wrote itself, with no knowledge of
    // Alis.Reactive. It takes a listener the way a chart takes options.onClick: a function assigned to a
    // property named for the event. This file is everything the framework needs to drive it, written
    // against the framework's public surface only: where its object lives and how it listens, each member
    // by its JavaScript name, its callback payload, and a render helper.

    /// <summary>A resident's call bell: pressed in the room, heard at the desk while someone is on duty.</summary>
    /// <remarks>
    /// Each callback is declared as an event by the property's JavaScript name, so a page handles it as it
    /// handles any event, and the other members are used directly, one statement each:
    /// <code>
    /// plan.CallBell(BellId, room: "12").Reactive(evt =&gt; evt.OnPressed, (call, p) =&gt; ..);
    /// var bell = p.Component&lt;CallBell&gt;(BellId);
    /// p.When(bell.Ringing).Truthy().Then(ringing =&gt; bell.Clear());
    /// bell.Room.Set("Respite 3");
    /// </code>
    /// </remarks>
    public sealed class CallBell : Component
    {
        /// <summary>
        /// The bell keeps itself on its host element as <c>callBell</c> and takes each listener as a property
        /// named for the event: <c>callBell.onPressed = fn</c>. A listener receives the bell's payload.
        /// </summary>
        public static ComponentVendor CallBellVendor { get; } = ComponentVendor.DefineCallbackProperties(
            "callbell",
            objectPath: "callBell",
            payload: VendorEventPayload.Argument);

        /// <inheritdoc />
        public override ComponentVendor Vendor => CallBellVendor;

        /// <summary>The room the bell is in; the bell shows it and names it in each call.</summary>
        public Property<string> Room => Property<string>();

        /// <summary>Whether the bell is ringing: pressed and not yet cleared.</summary>
        public ReadOnly<bool> Ringing => ReadOnly<bool>();

        /// <summary>How many times the bell has been pressed.</summary>
        public ReadOnly<int> Presses => ReadOnly<int>();

        /// <summary>Clears the ring, as answering the call does; the bell goes quiet and calls <see cref="OnCleared"/>.</summary>
        public void Clear() => Method();

        /// <summary>The bell's <c>onPressed</c> callback: called with each press.</summary>
        public Event<CallBellCallArgs> OnPressed => Event<CallBellCallArgs>();

        /// <summary>The bell's <c>onCleared</c> callback: called when the ring is cleared.</summary>
        public Event<CallBellCallArgs> OnCleared => Event<CallBellCallArgs>();
    }

    /// <summary>What the bell hands <see cref="CallBell.OnPressed"/> and <see cref="CallBell.OnCleared"/>.</summary>
    public class CallBellCallArgs : JsObject
    {
        /// <summary>The room the bell is in.</summary>
        public ReadOnly<string> Room => ReadOnly<string>();

        /// <summary>How many times the bell has been pressed tonight, this press included.</summary>
        public ReadOnly<int> Presses => ReadOnly<int>();
    }

    /// <summary>Renders a <see cref="CallBell"/> for a plan.</summary>
    public static class CallBellHtmlExtensions
    {
        /// <summary>
        /// Renders the bell's host with the id the page chose and the room it is in; the app's bundle
        /// constructs the bell on the element, and the framework reaches it by id.
        /// </summary>
        /// <typeparam name="TModel">The view's model.</typeparam>
        /// <param name="plan">The plan the bell joins.</param>
        /// <param name="elementId">The id the page gives the bell.</param>
        /// <param name="room">The room the bell is in.</param>
        /// <returns>The rendered bell; wire its callbacks with <c>.Reactive(evt =&gt; evt.OnPressed, ...)</c>.</returns>
        public static RenderedComponent<CallBell, TModel> CallBell<TModel>(
            this ReactivePlan<TModel> plan,
            string elementId,
            string room)
            where TModel : class
        {
            var markup = new StringBuilder()
                .Append("<div class=\"call-bell\" id=\"").Append(Encode(elementId))
                .Append("\" data-room=\"").Append(Encode(room))
                .Append("\"></div>");
            return plan.Render<CallBell>(elementId, new HtmlString(markup.ToString()));
        }

        private static string Encode(string value) => HtmlEncoder.Default.Encode(value);
    }
}
