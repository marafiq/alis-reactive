using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.SandboxApp.BroughtComponents
{
    // The whiteboard (Scripts/whiteboard.ts) is a widget the sandbox wrote itself, with no knowledge of
    // Alis.Reactive: the board on a resident's wall. This file is everything the framework needs to
    // drive it, written against the framework's public surface only: where its object lives, each
    // member by its JavaScript name, its event payloads, and a render helper.

    /// <summary>The whiteboard on a resident's wall: who the nurse is, the day's goal, the next check and the notes.</summary>
    /// <remarks>
    /// Each member is declared by the board's own JavaScript name, which is the C# name camel-cased, and
    /// by the kind of member it is. A page reaches the board and uses those members directly:
    /// <code>
    /// var board = p.Component&lt;Whiteboard&gt;(BoardId);
    /// board.Nurse.Set("Nurse Patel");
    /// board.Highlight(Field.Of((WhiteboardRows r) =&gt; r.Goal));
    /// var number = board.NextNoteNumber();
    /// board.AddNote(number, note.Value);
    /// </code>
    /// </remarks>
    public sealed class Whiteboard : Component
    {
        /// <summary>
        /// The board keeps itself on its host element as <c>whiteboard</c>, adds listeners with
        /// <c>on</c>/<c>off</c>, and hands a listener the event's payload.
        /// </summary>
        public static ComponentVendor WhiteboardVendor { get; } = ComponentVendor.Define(
            "whiteboard",
            objectPath: "whiteboard",
            subscribe: "on",
            unsubscribe: "off",
            payload: VendorEventPayload.Argument);

        /// <inheritdoc />
        public override ComponentVendor Vendor => WhiteboardVendor;

        /// <summary>The nurse on the board.</summary>
        public Property<string> Nurse => Property<string>();

        /// <summary>The day's goal on the board.</summary>
        public Property<string> Goal => Property<string>();

        /// <summary>Minutes until the next check; nothing when no check is scheduled.</summary>
        public Property<int?> NextCheckMinutes => Property<int?>();

        /// <summary>How many notes are on the board.</summary>
        public ReadOnly<int> Notes => ReadOnly<int>();

        /// <summary>What the last <see cref="Refresh"/> recorded; empty until one has settled.</summary>
        public ReadOnly<string> LastRefresh => ReadOnly<string>();

        /// <summary>Pages someone from the board; the board shows who.</summary>
        /// <param name="who">Who is paged.</param>
        public void Page(Arg<string> who) => Method(who);

        /// <summary>
        /// Marks a row of the board, which shows the mark as the row's style. The board's JavaScript takes
        /// the row's name and an effect; the page names the row as a member of <see cref="WhiteboardRows"/>,
        /// and the effect is always <c>pulse</c>.
        /// </summary>
        /// <param name="row">The row, for example <c>Field.Of((WhiteboardRows r) =&gt; r.Goal)</c>.</param>
        public void Highlight(Field row) => Method(row, Fixed("pulse"));

        /// <summary>Draws the next note number; each call draws a new one.</summary>
        public TypedSource<int> NextNoteNumber() => Method<int>();

        /// <summary>Adds a numbered note; raises <see cref="NoteAdded"/>.</summary>
        /// <param name="number">The note's number.</param>
        /// <param name="text">The note.</param>
        public void AddNote(Arg<int> number, Arg<string> text) => Method(number, text);

        /// <summary>Clears the notes; raises <see cref="Wiped"/>.</summary>
        public void Wipe() => Method();

        /// <summary>Refreshes the board: a promise the steps after it wait for; <see cref="LastRefresh"/> changes once it settles.</summary>
        public void Refresh() => MethodAsync();

        /// <summary>The wing's motto: a promise the first step that uses it waits for.</summary>
        public TypedSource<string> LoadMotto() => MethodAsync<string>();

        /// <summary>Raised when a note is added, with the note.</summary>
        public Event<WhiteboardNoteArgs> NoteAdded => Event<WhiteboardNoteArgs>();

        /// <summary>Raised when the notes are wiped. It carries nothing.</summary>
        public Event Wiped => Event();
    }

    /// <summary>The rows of a <see cref="Whiteboard"/>, named as the board's JavaScript names them.</summary>
    public sealed class WhiteboardRows
    {
        /// <summary>The nurse row.</summary>
        public string Nurse { get; set; } = "";

        /// <summary>The goal row.</summary>
        public string Goal { get; set; } = "";

        /// <summary>The next-check row.</summary>
        public string NextCheck { get; set; } = "";
    }

    /// <summary>Payload of <see cref="Whiteboard.NoteAdded"/>.</summary>
    public class WhiteboardNoteArgs : JsObject
    {
        /// <summary>The note's number.</summary>
        public ReadOnly<int> Number => ReadOnly<int>();

        /// <summary>The note.</summary>
        public ReadOnly<string> Text => ReadOnly<string>();
    }

    /// <summary>Renders a <see cref="Whiteboard"/> for a plan.</summary>
    public static class WhiteboardHtmlExtensions
    {
        /// <summary>
        /// Renders the board's host with the id the page chose and the nurse it opens with; the app's
        /// bundle constructs the board on the element, and the framework reaches it by id.
        /// </summary>
        /// <typeparam name="TModel">The view's model.</typeparam>
        /// <param name="plan">The plan the board joins.</param>
        /// <param name="elementId">The id the page gives the board.</param>
        /// <param name="nurse">The nurse the board opens with.</param>
        /// <returns>The rendered board; wire its events with <c>.Reactive(evt =&gt; evt.NoteAdded, ...)</c>.</returns>
        public static RenderedComponent<Whiteboard, TModel> Whiteboard<TModel>(
            this ReactivePlan<TModel> plan,
            string elementId,
            string nurse)
            where TModel : class
        {
            var markup = new StringBuilder()
                .Append("<div class=\"whiteboard\" id=\"").Append(Encode(elementId))
                .Append("\" data-nurse=\"").Append(Encode(nurse))
                .Append("\"></div>");
            return plan.Render<Whiteboard>(elementId, new HtmlString(markup.ToString()));
        }

        private static string Encode(string value) => HtmlEncoder.Default.Encode(value);
    }
}
