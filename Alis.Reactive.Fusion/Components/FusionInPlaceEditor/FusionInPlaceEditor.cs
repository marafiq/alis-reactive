using System;
using System.Collections.Generic;
using Syncfusion.EJ2;
using Syncfusion.EJ2.InPlaceEditor;

namespace Alis.Reactive.Fusion.Components
{
#nullable disable annotations
    /// <summary>
    /// The Syncfusion InPlaceEditor, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.InputField(plan, m =&gt; m.Value).FusionInPlaceEditor(b =&gt; ...)</c>; target it with
    /// <c>var editor = p.Component&lt;FusionInPlaceEditor&gt;(m =&gt; m.Value)</c>, then write or call a member as its own
    /// statement, for example <c>editor.Disable(true)</c> or <c>editor.Save()</c>.
    /// </remarks>
    public sealed class FusionInPlaceEditor : FusionComponent, IInputComponent
    {
        /// <summary>The edited value.</summary>
        public Prop<string> Value => Prop<string>();

        /// <summary>Disables (<see langword="true"/>) or enables the editor.</summary>
        /// <param name="value"><see langword="true"/> to disable the editor; <see langword="false"/> to enable it.</param>
        public void Disable(Arg<bool> value) => Call(value);

        /// <summary>Saves the edited value.</summary>
        public void Save() => Call();

        /// <summary>Moves focus into the editor.</summary>
        public void SetFocus() => Call();

        /// <summary>Adds a CSS class to the editor element.</summary>
        /// <param name="token">The CSS class to add.</param>
        public void AddClass(Arg<string> token) => Call(token, name: "element.classList.add");

        /// <summary>Removes a CSS class from the editor element.</summary>
        /// <param name="token">The CSS class to remove.</param>
        public void RemoveClass(Arg<string> token) => Call(token, name: "element.classList.remove");

        /// <summary>Raised before the editor opens.</summary>
        public TypedEvent<FusionInPlaceEditorBeginEditArgs> BeginEdit =>
            Event<FusionInPlaceEditorBeginEditArgs>();

        /// <summary>Raised before the editor closes.</summary>
        public TypedEvent<FusionInPlaceEditorEndEditArgs> EndEdit =>
            Event<FusionInPlaceEditorEndEditArgs>();

        /// <summary>Raised when the value changes.</summary>
        public TypedEvent<FusionInPlaceEditorChangeArgs> Change =>
            Event<FusionInPlaceEditorChangeArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

        /// <summary>Raised before the value is submitted.</summary>
        public TypedEvent<FusionInPlaceEditorActionBeginArgs> ActionBegin =>
            Event<FusionInPlaceEditorActionBeginArgs>();

        /// <summary>Raised after the value is submitted.</summary>
        public TypedEvent<FusionInPlaceEditorActionSuccessArgs> ActionSuccess =>
            Event<FusionInPlaceEditorActionSuccessArgs>();

        /// <summary>Raised when the user clicks save.</summary>
        public TypedEvent<FusionInPlaceEditorSubmitClickArgs> SubmitClick =>
            Event<FusionInPlaceEditorSubmitClickArgs>();

        /// <summary>Raised when the user clicks cancel.</summary>
        public TypedEvent<FusionInPlaceEditorCancelClickArgs> CancelClick =>
            Event<FusionInPlaceEditorCancelClickArgs>();
    }

    /// <summary>Payload of <see cref="FusionInPlaceEditor.BeginEdit"/>.</summary>
    public class FusionInPlaceEditorBeginEditArgs : JsObject
    {
        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();

        /// <summary>Whether the editor opens without taking focus.</summary>
        public ReadOnly<bool> CancelFocus => ReadOnly<bool>();

        /// <summary>The edit mode.</summary>
        public ReadOnly<string> Mode => ReadOnly<string>();

        /// <summary>The field name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();
    }

    /// <summary>Payload of <see cref="FusionInPlaceEditor.EndEdit"/>.</summary>
    public class FusionInPlaceEditorEndEditArgs : JsObject
    {
        /// <summary>Why the editor is closing.</summary>
        public ReadOnly<string> Action => ReadOnly<string>();

        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();

        /// <summary>The edit mode.</summary>
        public ReadOnly<string> Mode => ReadOnly<string>();

        /// <summary>The field name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();
    }

    /// <summary>Payload of <see cref="FusionInPlaceEditor.Change"/>.</summary>
    public class FusionInPlaceEditorChangeArgs : JsObject
    {
        /// <summary>The value after the change.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>The value before the change.</summary>
        public ReadOnly<string> PreviousValue => ReadOnly<string>();

        /// <summary>The field name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();
    }

    /// <summary>Payload of <see cref="FusionInPlaceEditor.ActionBegin"/>.</summary>
    public class FusionInPlaceEditorActionBeginArgs : JsObject
    {
        /// <summary>The data about to be submitted.</summary>
        public ReadOnly<IDictionary<string, object>> Data => ReadOnly<IDictionary<string, object>>();

        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();
    }

    /// <summary>Payload of <see cref="FusionInPlaceEditor.ActionSuccess"/>.</summary>
    public class FusionInPlaceEditorActionSuccessArgs : JsObject
    {
        /// <summary>The server's answer.</summary>
        public ReadOnly<object> Data => ReadOnly<object>();

        /// <summary>The submitted value.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>The field name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();
    }

    /// <summary>Payload of <see cref="FusionInPlaceEditor.SubmitClick"/>.</summary>
    public class FusionInPlaceEditorSubmitClickArgs : JsObject
    {
        /// <summary>The field name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();
    }

    /// <summary>Payload of <see cref="FusionInPlaceEditor.CancelClick"/>.</summary>
    public class FusionInPlaceEditorCancelClickArgs : JsObject
    {
        /// <summary>The field name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();
    }
#nullable restore annotations

    /// <summary>
    /// Renders the in-place editor component inside a bound input field.
    /// </summary>
    /// <remarks>
    /// Start the chain with <c>Html.InputField(plan, m =&gt; m.DateOfBirth)</c>, then call
    /// <c>.FusionInPlaceEditor(b =&gt; { b.Type(InputType.Date).Mode(RenderMode.Inline); })</c>.
    /// Use the vendor builder directly for configuration (<c>Type</c>, <c>Mode</c>, <c>EditableOn</c>,
    /// <c>ShowButtons</c>, etc.) and <c>.Reactive(evt =&gt; evt.ActionBegin, ...)</c> to own the commit flow.
    /// </remarks>
    public static class FusionInPlaceEditorHtmlExtensions
    {
        /// <summary>Renders a FusionInPlaceEditor with its defaults; see the overload that takes the configuration.</summary>
        /// <typeparam name="TModel">The view model the plan is authored for.</typeparam>
        /// <typeparam name="TProp">Model value type rendered by the in-place editor.</typeparam>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionInPlaceEditor, TModel> FusionInPlaceEditor<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup)
            where TModel : class
            => setup.FusionInPlaceEditor(_ => { });

        /// <summary>
        /// Renders the in-place editor component bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view model the plan is authored for.</typeparam>
        /// <typeparam name="TProp">Model value type rendered by the in-place editor.</typeparam>
        /// <param name="setup">Field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Configures the editor on the Syncfusion builder: type, mode, editable-on and the rest.</param>
        /// <returns>The rendered editor; wire its events with <c>.Reactive(evt =&gt; evt.ActionBegin, ...)</c>.</returns>
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
