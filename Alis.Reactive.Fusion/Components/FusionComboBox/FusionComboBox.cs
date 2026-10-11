using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion ComboBox, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Use as the type in <c>p.Component&lt;FusionComboBox&gt;(m =&gt; m.Physician)</c>; every
    /// declared member works with <c>Set</c>, <c>Call</c> and <c>Get</c>, for example
    /// <c>.Set(c =&gt; c.Value, "alice")</c>, <c>.Call(c =&gt; c.ShowPopup)</c>, <c>.Get(c =&gt; c.Text)</c>.
    /// </remarks>
    public sealed class FusionComboBox : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ValueMember => "value";

        /// <summary>The selected value.</summary>
        public ComponentProperty<string?> Value => Property<string?>(ValueMember);

        /// <summary>The text shown in the input.</summary>
        public ComponentProperty<string?> Text => Property<string?>("text");

        /// <summary>The zero-based index of the selected item.</summary>
        public ComponentProperty<int?> Index => Property<int?>("index");

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public ComponentMethod DataBind => Method("dataBind");

        /// <summary>Moves focus into the input.</summary>
        public ComponentMethod FocusIn => Method("focusIn");

        /// <summary>Removes focus from the input.</summary>
        public ComponentMethod FocusOut => Method("focusOut");

        /// <summary>Opens the popup.</summary>
        public ComponentMethod ShowPopup => Method("showPopup");

        /// <summary>Closes the popup.</summary>
        public ComponentMethod HidePopup => Method("hidePopup");

        /// <summary>Clears the selection and the text.</summary>
        public ComponentMethod Clear => Method("clear");

        /// <summary>Raised when the selected value changes.</summary>
        public TypedEvent<FusionComboBoxChangeArgs> Change =>
            Event<FusionComboBoxChangeArgs>("change");

        /// <summary>Raised when the input receives focus.</summary>
        public TypedEvent<FusionComboBoxFocusArgs> Focus =>
            Event<FusionComboBoxFocusArgs>("focus");

        /// <summary>Raised when the input loses focus.</summary>
        public TypedEvent<FusionComboBoxBlurArgs> Blur =>
            Event<FusionComboBoxBlurArgs>("blur");
    }

    /// <summary>Payload of <see cref="FusionComboBox.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args, x =&gt; x.Value).Eq("alice")</c>.</remarks>
    public class FusionComboBoxChangeArgs
    {
        /// <summary>Selected value after the change.</summary>
        public string? Value { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>Payload of <see cref="FusionComboBox.Focus"/>; carries no data.</summary>
    public class FusionComboBoxFocusArgs
    {
    }

    /// <summary>Payload of <see cref="FusionComboBox.Blur"/>; carries no data.</summary>
    public class FusionComboBoxBlurArgs
    {
    }

    /// <summary>Renders a <see cref="FusionComboBox"/> for a model-bound field.</summary>
    public static class FusionComboBoxHtmlExtensions
    {
        /// <summary>Renders a FusionComboBox with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionComboBox, TModel> FusionComboBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionComboBox(_ => { });

        /// <summary>
        /// Renders a FusionComboBox bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionComboBox, TModel> FusionComboBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<ComboBoxBuilder> build)
            where TModel : class
        {
            var builder = field.Helper.EJS().ComboBoxFor(field.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath });
            build(builder);
            return field.Render<global::Alis.Reactive.Fusion.Components.FusionComboBox>(builder.Render());
        }
    }
}
