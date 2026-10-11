using System;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// The native HTML <c>&lt;textarea&gt;</c>, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.InputField(plan, m =&gt; m.CareNote).NativeTextArea(b =&gt; ...)</c>; target it with
    /// <c>p.Component&lt;NativeTextArea&gt;(m =&gt; m.CareNote)</c> and its members work with <c>Set</c>, <c>Call</c>
    /// and <c>Get</c>.
    /// </remarks>
    public sealed class NativeTextArea : NativeComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ValueMember => "value";

        /// <summary>The textarea value; empty text when nothing is entered.</summary>
        public ComponentProperty<string?> Value => Property<string?>(ValueMember);

        /// <summary>Moves focus to the element.</summary>
        public ComponentMethod Focus => Method("focus");

        /// <summary>Raised when the value changes and the element loses focus.</summary>
        public TypedEvent<NativeTextAreaChangeArgs> Change =>
            Event<NativeTextAreaChangeArgs>("change");
    }

    /// <summary>Payload of <see cref="NativeTextArea.Change"/>, read from the element.</summary>
    public class NativeTextAreaChangeArgs
    {
        /// <summary>The textarea value after the change.</summary>
        public string? Value { get; set; }
    }

    /// <summary>Renders a <see cref="NativeTextArea"/> for a model-bound field.</summary>
    public static class NativeTextAreaHtmlExtensions
    {
        /// <summary>Renders a NativeTextArea with its defaults; see the overload that takes <c>build</c>.</summary>
        public static RenderedComponent<global::Alis.Reactive.Native.Components.NativeTextArea, TModel> NativeTextArea<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.NativeTextArea(_ => { });

        /// <summary>
        /// Renders a NativeTextArea bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Native.Components.NativeTextArea, TModel> NativeTextArea<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<NativeTextAreaBuilder<TModel, TProp>> build)
            where TModel : class
        {
            var builder = new NativeTextAreaBuilder<TModel, TProp>(field.Helper, field.Expression, field.ElementId, field.BindingPath);
            build(builder);
            return field.Render<global::Alis.Reactive.Native.Components.NativeTextArea>(builder);
        }
    }
}
