using System;
using System.Collections.Generic;
using Syncfusion.EJ2;
using Syncfusion.EJ2.InPlaceEditor;

namespace Alis.Reactive.Fusion.Components
{
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
        public Prop<string?> Value => Prop<string?>();

        /// <summary>Disables (<see langword="true"/>) or enables the editor.</summary>
        /// <param name="value"><see langword="true"/> to disable the editor; <see langword="false"/> to enable it.</param>
        public void Disable(Arg<bool> value) => Call(value);

        /// <summary>Saves the edited value.</summary>
        public void Save() => Call();

        /// <summary>Moves focus into the editor.</summary>
        public void SetFocus() => Call();

        /// <summary>The editor's element: <c>editor.Element.ClassList.Add("alis-editor-saved")</c>.</summary>
        public FusionInPlaceEditorElement Element => Object<FusionInPlaceEditorElement>();

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
        /// <remarks>
        /// Committed by <see cref="EndEdit"/>, not <see cref="Change"/>: EJ2's save moves focus off the inner
        /// input, which raises the editor's change, before it stores the new value (inplace-editor.js:1196-1201);
        /// endEdit follows the store (1203-1204), so the form checks the value the person entered.
        /// </remarks>
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: EndEdit);

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

    /// <summary>The element the in-place editor renders into.</summary>
    public sealed class FusionInPlaceEditorElement : JsObject
    {
        /// <summary>The element's CSS classes.</summary>
        public FusionInPlaceEditorClassList ClassList => Object<FusionInPlaceEditorClassList>();
    }

    /// <summary>The CSS classes of the in-place editor's element (the element's <c>classList</c>).</summary>
    public sealed class FusionInPlaceEditorClassList : JsObject
    {
        /// <summary>Adds a CSS class.</summary>
        /// <param name="tokens">The CSS class to add.</param>
        public void Add(Arg<string> tokens) => Call(tokens);

        /// <summary>Removes a CSS class.</summary>
        /// <param name="tokens">The CSS class to remove.</param>
        public void Remove(Arg<string> tokens) => Call(tokens);
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

        /// <summary>The event's own name, which EJ2 sets on every payload; the edited field's name is not sent.</summary>
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

        /// <summary>The event's own name, which EJ2 sets on every payload; the edited field's name is not sent.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();
    }

    /// <summary>Payload of <see cref="FusionInPlaceEditor.Change"/>.</summary>
    public class FusionInPlaceEditorChangeArgs : JsObject
    {
        /// <summary>The value after the change.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>The value before the change.</summary>
        public ReadOnly<string> PreviousValue => ReadOnly<string>();

        /// <summary>The event's own name, which EJ2 sets on every payload; the edited field's name is not sent.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();
    }

    /// <summary>Payload of <see cref="FusionInPlaceEditor.ActionBegin"/>.</summary>
    public class FusionInPlaceEditorActionBeginArgs : JsObject
    {
        /// <summary>What is about to be submitted: the field's name, the record's key and the value.</summary>
        public ReadOnly<FusionInPlaceEditorSubmission> Data => ReadOnly<FusionInPlaceEditorSubmission>();

        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();
    }

    /// <summary>What the editor submits for a field: as EJ2 names its members.</summary>
    public sealed class FusionInPlaceEditorSubmission
    {
        /// <summary>The field's name, as the editor was rendered with.</summary>
        public string Name { get; set; } = "";

        /// <summary>The record's key, as the editor was rendered with.</summary>
        public string PrimaryKey { get; set; } = "";

        /// <summary>The committed value, as text.</summary>
        public string Value { get; set; } = "";
    }

    /// <summary>Payload of <see cref="FusionInPlaceEditor.ActionSuccess"/>.</summary>
    public class FusionInPlaceEditorActionSuccessArgs : JsObject
    {
        /// <summary>The server's answer, typed by the class the editor's own request returns.</summary>
        /// <typeparam name="TResponse">The answer's class.</typeparam>
        public ReadOnly<TResponse> Data<TResponse>() where TResponse : class => ReadOnly<TResponse>();

        /// <summary>The submitted value.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>The event's own name, which EJ2 sets on every payload; the edited field's name is not sent.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();
    }

    /// <summary>Payload of <see cref="FusionInPlaceEditor.SubmitClick"/>.</summary>
    public class FusionInPlaceEditorSubmitClickArgs : JsObject
    {
        /// <summary>The event's own name, which EJ2 sets on every payload; the edited field's name is not sent.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();
    }

    /// <summary>Payload of <see cref="FusionInPlaceEditor.CancelClick"/>.</summary>
    public class FusionInPlaceEditorCancelClickArgs : JsObject
    {
        /// <summary>The event's own name, which EJ2 sets on every payload; the edited field's name is not sent.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();
    }

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
