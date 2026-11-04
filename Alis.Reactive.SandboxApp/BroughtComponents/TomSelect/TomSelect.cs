using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.SandboxApp.BroughtComponents
{
    public sealed class TomSelect : Component, IInputComponent
    {
        public static ComponentVendor TomSelectVendor { get; } = ComponentVendor.Define(
            "tomselect",
            objectPath: "tomselect",
            subscribe: "on",
            unsubscribe: "off",
            payload: VendorEventPayload.Argument);

        public override ComponentVendor Vendor => TomSelectVendor;

        private TomSelectInput Input => Object<TomSelectInput>();

        public Property<string> Value => Input.Value;

        public void SetValue(Arg<string> value) => Method(value);

        public void Clear() => Method();

        public Event<TomSelectChangeArgs> Change => Event<TomSelectChangeArgs>();

        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
    }

    internal sealed class TomSelectInput : JsObject
    {
        public Property<string> Value => Property<string>();
    }

    public sealed class TomSelectChangeArgs : JsObject
    {
    }

    public sealed class TomSelectOption
    {
        public TomSelectOption(string value, string text)
        {
            Value = value;
            Text = text;
        }

        public string Value { get; }

        public string Text { get; }
    }

    public static class TomSelectHtmlExtensions
    {
        public static RenderedComponent<global::Alis.Reactive.SandboxApp.BroughtComponents.TomSelect, TModel> TomSelect<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            IEnumerable<TomSelectOption> options,
            string placeholder)
            where TModel : class
        {
            var selected = CurrentValue(field);
            var html = new StringBuilder()
                .Append("<select class=\"tom-select\" id=\"").Append(Encode(field.ElementId))
                .Append("\" name=\"").Append(Encode(field.BindingPath))
                .Append("\" placeholder=\"").Append(Encode(placeholder)).Append("\">")
                .Append("<option value=\"\"></option>");

            foreach (var option in options)
            {
                html.Append("<option value=\"").Append(Encode(option.Value)).Append('"');
                if (option.Value == selected) html.Append(" selected");
                html.Append('>').Append(Encode(option.Text)).Append("</option>");
            }

            html.Append("</select>");
            return field.Render<global::Alis.Reactive.SandboxApp.BroughtComponents.TomSelect>(new HtmlString(html.ToString()));
        }

        private static string? CurrentValue<TModel, TProp>(InputBoundField<TModel, TProp> field)
            where TModel : class
        {
            var model = field.Helper.ViewData.Model;
            if (model == null) return null;

            var value = field.Expression.Compile()(model);
            return value == null ? null : value.ToString();
        }

        private static string Encode(string value) => HtmlEncoder.Default.Encode(value);
    }
}
