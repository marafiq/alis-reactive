using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.SandboxApp.BroughtComponents
{
    public sealed class Whiteboard : Component
    {
        public static ComponentVendor WhiteboardVendor { get; } = ComponentVendor.Define(
            "whiteboard",
            objectPath: "whiteboard",
            subscribe: "on",
            unsubscribe: "off",
            payload: VendorEventPayload.Argument);

        public override ComponentVendor Vendor => WhiteboardVendor;

        public Property<string> Nurse => Property<string>();

        public Property<string> Goal => Property<string>();

        public Property<int?> NextCheckMinutes => Property<int?>();

        public ReadOnly<int> Notes => ReadOnly<int>();

        public ReadOnly<string> LastRefresh => ReadOnly<string>();

        public void Page(Arg<string> who) => Method(who);

        public void Highlight(Field row) => Method(row, Fixed("pulse"));

        public TypedSource<int> NextNoteNumber() => Method<int>();

        public void AddNote(Arg<int> number, Arg<string> text) => Method(number, text);

        public void Wipe() => Method();

        public void Refresh() => MethodAsync();

        public TypedSource<string> LoadMotto() => MethodAsync<string>();

        public Event<WhiteboardNoteArgs> NoteAdded => Event<WhiteboardNoteArgs>();

        public Event Wiped => Event();
    }

    public sealed class WhiteboardRows
    {
        public string Nurse { get; set; } = "";

        public string Goal { get; set; } = "";

        public string NextCheck { get; set; } = "";
    }

    public sealed class WhiteboardNoteArgs : JsObject
    {
        public ReadOnly<int> Number => ReadOnly<int>();

        public ReadOnly<string> Text => ReadOnly<string>();
    }

    public static class WhiteboardHtmlExtensions
    {
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
