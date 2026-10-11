using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.InPlaceEditor;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion InPlaceEditor, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.InputField(plan, m =&gt; m.Value).FusionInPlaceEditor(b =&gt; ...)</c>; target it with
    /// <c>p.Component&lt;FusionInPlaceEditor&gt;(m =&gt; m.Value)</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>, for example <c>.Call(c =&gt; c.Disable, true)</c> or <c>.Call(c =&gt; c.Save)</c>.
    /// </remarks>
    public sealed class FusionInPlaceEditor : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ValueMember => "value";

        /// <summary>The edited value.</summary>
        public ComponentProperty<string?> Value => Property<string?>(ValueMember);

        /// <summary>Disables (<see langword="true"/>) or enables the editor.</summary>
        public ComponentMethod<bool> Disable => Method<bool>("disable");

        /// <summary>Saves the edited value.</summary>
        public ComponentMethod Save => Method("save");

        /// <summary>Moves focus into the editor.</summary>
        public ComponentMethod SetFocus => Method("setFocus");

        /// <summary>Adds a CSS class to the editor element.</summary>
        public ComponentMethod<string> AddClass => Method<string>("element.classList.add");

        /// <summary>Removes a CSS class from the editor element.</summary>
        public ComponentMethod<string> RemoveClass => Method<string>("element.classList.remove");

        /// <summary>Raised before the editor opens.</summary>
        public TypedEvent<FusionInPlaceEditorBeginEditArgs> BeginEdit =>
            Event<FusionInPlaceEditorBeginEditArgs>("beginEdit");

        /// <summary>Raised before the editor closes.</summary>
        public TypedEvent<FusionInPlaceEditorEndEditArgs> EndEdit =>
            Event<FusionInPlaceEditorEndEditArgs>("endEdit");

        /// <summary>Raised when the value changes.</summary>
        public TypedEvent<FusionInPlaceEditorChangeArgs> Change =>
            Event<FusionInPlaceEditorChangeArgs>("change");

        /// <summary>Raised before the value is submitted.</summary>
        public TypedEvent<FusionInPlaceEditorActionBeginArgs> ActionBegin =>
            Event<FusionInPlaceEditorActionBeginArgs>("actionBegin");

        /// <summary>Raised after the value is submitted.</summary>
        public TypedEvent<FusionInPlaceEditorActionSuccessArgs> ActionSuccess =>
            Event<FusionInPlaceEditorActionSuccessArgs>("actionSuccess");

        /// <summary>Raised when the user clicks save.</summary>
        public TypedEvent<FusionInPlaceEditorSubmitClickArgs> SubmitClick =>
            Event<FusionInPlaceEditorSubmitClickArgs>("submitClick");

        /// <summary>Raised when the user clicks cancel.</summary>
        public TypedEvent<FusionInPlaceEditorCancelClickArgs> CancelClick =>
            Event<FusionInPlaceEditorCancelClickArgs>("cancelClick");
    }

    /// <summary>Payload of <see cref="FusionInPlaceEditor.BeginEdit"/>.</summary>
    public class FusionInPlaceEditorBeginEditArgs
    {
        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public bool Cancel { get; set; }

        /// <summary>Set to <see langword="true"/> to keep focus off the editor.</summary>
        public bool CancelFocus { get; set; }

        /// <summary>The edit mode.</summary>
        public string? Mode { get; set; }

        /// <summary>The field name.</summary>
        public string? Name { get; set; }
    }

    /// <summary>Payload of <see cref="FusionInPlaceEditor.EndEdit"/>.</summary>
    public class FusionInPlaceEditorEndEditArgs
    {
        /// <summary>Why the editor is closing.</summary>
        public string? Action { get; set; }

        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public bool Cancel { get; set; }

        /// <summary>The edit mode.</summary>
        public string? Mode { get; set; }

        /// <summary>The field name.</summary>
        public string? Name { get; set; }
    }

    /// <summary>Payload of <see cref="FusionInPlaceEditor.Change"/>.</summary>
    public class FusionInPlaceEditorChangeArgs
    {
        /// <summary>The value after the change.</summary>
        public string? Value { get; set; }

        /// <summary>The value before the change.</summary>
        public string? PreviousValue { get; set; }

        /// <summary>The field name.</summary>
        public string? Name { get; set; }
    }

    /// <summary>Payload of <see cref="FusionInPlaceEditor.ActionBegin"/>.</summary>
    public class FusionInPlaceEditorActionBeginArgs
    {
        /// <summary>The data about to be submitted.</summary>
        public IDictionary<string, object>? Data { get; set; }

        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public bool Cancel { get; set; }
    }

    /// <summary>Payload of <see cref="FusionInPlaceEditor.ActionSuccess"/>.</summary>
    public class FusionInPlaceEditorActionSuccessArgs
    {
        /// <summary>The server's answer.</summary>
        public object? Data { get; set; }

        /// <summary>The submitted value.</summary>
        public string? Value { get; set; }

        /// <summary>The field name.</summary>
        public string? Name { get; set; }
    }

    /// <summary>Payload of <see cref="FusionInPlaceEditor.SubmitClick"/>.</summary>
    public class FusionInPlaceEditorSubmitClickArgs
    {
        /// <summary>The field name.</summary>
        public string? Name { get; set; }
    }

    /// <summary>Payload of <see cref="FusionInPlaceEditor.CancelClick"/>.</summary>
    public class FusionInPlaceEditorCancelClickArgs
    {
        /// <summary>The field name.</summary>
        public string? Name { get; set; }
    }

    /// <summary>
    /// Renders the in-place editor component inside a bound input field.
    /// </summary>
    /// <remarks>
    /// Start the chain with <c>Html.InputField(plan, m =&gt; m.DateOfBirth)</c>, then call
    /// <c>.FusionInPlaceEditor(b =&gt; { b.Type(InputType.Date).Mode(RenderMode.Inline); })</c>.
    /// Use the vendor builder directly for configuration (<c>Type</c>, <c>Mode</c>, <c>EditableOn</c>,
    /// <c>ShowButtons</c>, etc.) and <c>.Reactive(plan, evt =&gt; evt.ActionBegin, …)</c> to own the commit flow.
    /// </remarks>
    public static class FusionInPlaceEditorHtmlExtensions
    {
        /// <summary>Renders a FusionInPlaceEditor with its defaults; see the overload that takes the configuration.</summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionInPlaceEditor, TModel> FusionInPlaceEditor<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup)
            where TModel : class
            => setup.FusionInPlaceEditor(_ => { });

        /// <summary>
        /// Renders the in-place editor component bound to the field's model property.
        /// </summary>
        /// <typeparam name="TProp">Model value type rendered by the in-place editor.</typeparam>
        /// <param name="setup">Field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Callback to configure the editor (type, mode, inner model, reactive events).</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionInPlaceEditor, TModel> FusionInPlaceEditor<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<InPlaceEditorBuilder> build)
            where TModel : class
        {
            var initialValue = InitialEditorValue.FromModel(
                setup.Expression,
                setup.Helper.ViewData.Model);

            var builder = setup.Helper.EJS().InPlaceEditor(setup.ElementId)
                .Name(setup.BindingPath)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId });

            builder = initialValue.ApplyTo(builder);

            build(builder);
            return setup.Render<global::Alis.Reactive.Fusion.Components.FusionInPlaceEditor>(builder.Render());
        }
    }

    internal abstract class InitialEditorValue
    {
        private protected InitialEditorValue() { }

        internal static InitialEditorValue FromModel<TModel, TProp>(
            System.Linq.Expressions.Expression<Func<TModel, TProp>> expression,
            TModel model)
            where TModel : class
        {
            try
            {
                var compiled = expression.Compile();
                var value = compiled(model);
                return value == null
                    ? Missing
                    : new PresentInitialEditorValue(value);
            }
            catch
            {
                return Missing;
            }
        }

        private static InitialEditorValue Missing { get; } =
            new MissingInitialEditorValue();

        internal abstract InPlaceEditorBuilder ApplyTo(InPlaceEditorBuilder builder);
    }

    internal sealed class MissingInitialEditorValue : InitialEditorValue
    {
        internal override InPlaceEditorBuilder ApplyTo(InPlaceEditorBuilder builder) => builder;
    }

    internal sealed class PresentInitialEditorValue : InitialEditorValue
    {
        private readonly object _value;

        internal PresentInitialEditorValue(object value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal override InPlaceEditorBuilder ApplyTo(InPlaceEditorBuilder builder) =>
            builder.Value(_value);
    }
}
