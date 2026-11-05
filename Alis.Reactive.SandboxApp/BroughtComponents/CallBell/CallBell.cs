using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.SandboxApp.BroughtComponents
{
    public sealed class CallBell : Component
    {
        public static ComponentVendor CallBellVendor { get; } = ComponentVendor.DefineCallbackProperties(
            "callbell",
            objectPath: "callBell",
            payload: VendorEventPayload.Argument);

        public override ComponentVendor Vendor => CallBellVendor;

        public Property<string> Room => Property<string>();

        public ReadOnly<bool> Ringing => ReadOnly<bool>();

        public ReadOnly<int> Presses => ReadOnly<int>();

        public void Clear() => Method();

        public Event<CallBellCallArgs> OnPressed => Event<CallBellCallArgs>();

        public Event<CallBellCallArgs> OnCleared => Event<CallBellCallArgs>();
    }

    public sealed class CallBellCallArgs : JsObject
    {
        public ReadOnly<string> Room => ReadOnly<string>();

        public ReadOnly<int> Presses => ReadOnly<int>();
    }

    public static class CallBellHtmlExtensions
    {
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
