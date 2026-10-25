using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.SandboxApp.BroughtComponents
{
    // Tom Select (https://tom-select.js.org), brought the way any app brings a component: the app
    // installs the npm package, bundles it (Scripts/sandbox-components.ts) and constructs it on
    // alis:mount. This file is everything the framework needs, written against its public surface
    // only: the vendor, the members by Tom Select's own names, and a render helper.

    /// <summary>A Tom Select dropdown, declared by the names its JavaScript API uses.</summary>
    /// <remarks>
    /// A page reaches the dropdown and uses its members directly, one statement each:
    /// <code>
    /// var suite = p.Component&lt;TomSelect&gt;(m =&gt; m.SuiteType);
    /// p.When(suite.Value).NotEmpty().Then(..);
    /// suite.SetValue(recommendation.Get(x =&gt; x.SuiteType));
    /// suite.Clear();
    /// </code>
    /// </remarks>
    public sealed class TomSelect : Component, IInputComponent
    {
        /// <summary>
        /// Tom Select stores itself on the original select as <c>tomselect</c>, adds listeners with
        /// <c>on</c>/<c>off</c>, and hands a listener the event's first argument.
        /// </summary>
        public static ComponentVendor TomSelectVendor { get; } = ComponentVendor.Define(
            "tomselect",
            objectPath: "tomselect",
            subscribe: "on",
            unsubscribe: "off",
            payload: VendorEventPayload.Argument);

        /// <inheritdoc />
        public override ComponentVendor Vendor => TomSelectVendor;

        // The original select, which Tom Select keeps in sync with the selection. A page reads the
        // selection as Value, the one way to it.
        private TomSelectInput Input => Object<TomSelectInput>();

        /// <summary>The selected value, read from the original select. Change it with <see cref="SetValue"/>.</summary>
        /// <remarks>Tom Select keeps the original select in sync; its value is what a form posts.</remarks>
        public Property<string> Value => Input.Value;

        /// <summary>Selects an option by its value and raises <see cref="Change"/>.</summary>
        /// <param name="value">The option's value.</param>
        public void SetValue(Arg<string> value) => Method(value);

        /// <summary>Clears the selection and raises <see cref="Change"/>.</summary>
        public void Clear() => Method();

        /// <summary>Raised when the selection changes, by the user or by <see cref="SetValue"/>.</summary>
        public Event<TomSelectChangeArgs> Change => Event<TomSelectChangeArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
    }

    /// <summary>
    /// The original select a <see cref="TomSelect"/> enhances (Tom Select's <c>input</c>); the component
    /// reads its value as <see cref="TomSelect.Value"/>.
    /// </summary>
    internal sealed class TomSelectInput : JsObject
    {
        /// <summary>The select's value: what a form posts.</summary>
        public Property<string> Value => Property<string>();
    }

    /// <summary>
    /// Payload of <see cref="TomSelect.Change"/>. Tom Select hands the listener the new value itself,
    /// which has no members to declare; read it from the component instead:
    /// <c>p.When(suite.Value).NotEmpty()</c>.
    /// </summary>
    public class TomSelectChangeArgs : JsObject
    {
    }

    /// <summary>One option a <see cref="TomSelect"/> offers.</summary>
    public sealed class TomSelectOption
    {
        /// <summary>Creates an option.</summary>
        /// <param name="value">The value the form posts when the option is selected.</param>
        /// <param name="text">What the dropdown shows for the option.</param>
        public TomSelectOption(string value, string text)
        {
            Value = value;
            Text = text;
        }

        /// <summary>The value the form posts when the option is selected.</summary>
        public string Value { get; }

        /// <summary>What the dropdown shows for the option.</summary>
        public string Text { get; }
    }

    /// <summary>Renders a <see cref="TomSelect"/> for a model-bound field.</summary>
    public static class TomSelectHtmlExtensions
    {
        /// <summary>
        /// Renders the select Tom Select enhances, with the framework's element ID and binding name,
        /// the field's current value selected.
        /// </summary>
        /// <typeparam name="TModel">The view's model.</typeparam>
        /// <typeparam name="TProp">The type of the bound model property.</typeparam>
        /// <param name="field">The model-bound field the dropdown renders into.</param>
        /// <param name="options">The options the dropdown offers.</param>
        /// <param name="placeholder">What the dropdown shows before an option is selected.</param>
        /// <returns>The rendered dropdown; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
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
