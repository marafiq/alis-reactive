using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion MaskedTextBox, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Use as the type in <c>p.Component&lt;FusionInputMask&gt;(m =&gt; m.PhoneNumber)</c>; every
    /// declared member works with <c>Set</c>, <c>Call</c> and <c>Get</c>, for example
    /// <c>.Set(c =&gt; c.Value, "(555) 123-4567")</c> or <c>p.When(phone.Get(c =&gt; c.Value)).NotEmpty()</c>.
    /// </remarks>
    public sealed class FusionInputMask : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ValueMember => "value";

        /// <summary>The value without the mask's literal characters; <see langword="null"/> when empty.</summary>
        public ComponentProperty<string?> Value => Property<string?>(ValueMember);

        /// <summary>Moves focus into the input.</summary>
        public ComponentMethod FocusIn => Method("focusIn");

        /// <summary>Raised when the value changes.</summary>
        public TypedEvent<FusionInputMaskChangeArgs> Change =>
            Event<FusionInputMaskChangeArgs>("change");
    }

    /// <summary>Payload of <see cref="FusionInputMask.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args, x =&gt; x.Value).NotNull()</c>.</remarks>
    public class FusionInputMaskChangeArgs
    {
        /// <summary>Value after the change.</summary>
        public string? Value { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>Renders a <see cref="FusionInputMask"/> for a model-bound field.</summary>
    public static class FusionInputMaskHtmlExtensions
    {
        /// <summary>Renders a FusionInputMask with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionInputMask, TModel> FusionInputMask<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionInputMask(_ => { });

        /// <summary>
        /// Renders a FusionInputMask bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionInputMask, TModel> FusionInputMask<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<MaskedTextBoxBuilder> build)
            where TModel : class
        {
            var builder = field.Helper.EJS().MaskedTextBoxFor(field.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath });
            build(builder);
            return field.Render<global::Alis.Reactive.Fusion.Components.FusionInputMask>(builder.Render());
        }
    }
}
