using System;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// A native group of radio buttons whose value is the chosen option, declared by the names its element uses.
    /// </summary>
    /// <remarks>
    /// A hidden input carries the element ID and the chosen value; it raises <c>change</c> when the user
    /// chooses an option. Render it with <c>Html.InputField(plan, m =&gt; m.CareLevel).NativeRadioGroup(b =&gt; ...)</c>.
    /// </remarks>
    public sealed class NativeRadioGroup : NativeComponent, IInputComponent
    {
        /// <summary>The chosen option value; empty text when none is chosen.</summary>
        public ComponentProperty<string?> Value => Property<string?>("value");

        /// <summary>Moves focus to the element.</summary>
        public ComponentMethod Focus => Method("focus");

        /// <summary>Raised when the user chooses an option.</summary>
        public TypedEvent<NativeRadioGroupChangeArgs> Change =>
            Event<NativeRadioGroupChangeArgs>("change");

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
    }

    /// <summary>Payload of <see cref="NativeRadioGroup.Change"/>, read from the hidden input.</summary>
    public class NativeRadioGroupChangeArgs
    {
        /// <summary>The chosen option value after the change.</summary>
        public string? Value { get; set; }
    }

    /// <summary>Renders a <see cref="NativeRadioGroup"/> for a model-bound field.</summary>
    public static class NativeRadioGroupHtmlExtensions
    {
        /// <summary>Renders a NativeRadioGroup with its defaults; see the overload that takes <c>build</c>.</summary>
        public static RenderedComponent<global::Alis.Reactive.Native.Components.NativeRadioGroup, TModel> NativeRadioGroup<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.NativeRadioGroup(_ => { });

        /// <summary>
        /// Renders a NativeRadioGroup bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Native.Components.NativeRadioGroup, TModel> NativeRadioGroup<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<NativeRadioGroupBuilder<TModel, TProp>> build)
            where TModel : class
        {
            var builder = new NativeRadioGroupBuilder<TModel, TProp>(field.Helper, field.Expression, field.ElementId, field.BindingPath);
            build(builder);
            return field.Render<global::Alis.Reactive.Native.Components.NativeRadioGroup>(builder);
        }
    }
}
