using System;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// The native HTML <c>&lt;input&gt;</c>, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.InputField(plan, m =&gt; m.PreferredName).NativeTextBox(b =&gt; ...)</c>; target it with
    /// <c>p.Component&lt;NativeTextBox&gt;(m =&gt; m.PreferredName)</c> and its members work with <c>Set</c>, <c>Call</c>
    /// and <c>Get</c>.
    /// </remarks>
    public sealed class NativeTextBox : NativeComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ValueMember => "value";

        /// <summary>The input value; empty text when nothing is entered.</summary>
        public ComponentProperty<string?> Value => Property<string?>(ValueMember);

        /// <summary>Moves focus to the element.</summary>
        public ComponentMethod Focus => Method("focus");

        /// <summary>Raised when the value changes and the element loses focus.</summary>
        public TypedEvent<NativeTextBoxChangeArgs> Change =>
            Event<NativeTextBoxChangeArgs>("change");
    }

    /// <summary>Payload of <see cref="NativeTextBox.Change"/>, read from the element.</summary>
    public class NativeTextBoxChangeArgs
    {
        /// <summary>The input value after the change.</summary>
        public string? Value { get; set; }
    }

    /// <summary>Renders a <see cref="NativeTextBox"/> for a model-bound field.</summary>
    public static class NativeTextBoxHtmlExtensions
    {
        /// <summary>Renders a NativeTextBox with its defaults; see the overload that takes <c>build</c>.</summary>
        public static RenderedComponent<global::Alis.Reactive.Native.Components.NativeTextBox, TModel> NativeTextBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.NativeTextBox(_ => { });

        /// <summary>
        /// Renders a NativeTextBox bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Native.Components.NativeTextBox, TModel> NativeTextBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<NativeTextBoxBuilder<TModel, TProp>> build)
            where TModel : class
        {
            var builder = new NativeTextBoxBuilder<TModel, TProp>(field.Helper, field.Expression, field.ElementId, field.BindingPath);
            build(builder);
            return field.Render<global::Alis.Reactive.Native.Components.NativeTextBox>(builder);
        }
    }
}
