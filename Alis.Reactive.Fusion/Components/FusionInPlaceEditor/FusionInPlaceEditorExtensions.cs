using System.Collections.Generic;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations and value reading for <see cref="FusionInPlaceEditor"/> in a reactive pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionInPlaceEditor&gt;(m =&gt; m.DateOfBirth).Save()</c>.
    /// </remarks>
    public static class FusionInPlaceEditorExtensions
    {
        private static readonly FusionInPlaceEditor Component = new FusionInPlaceEditor();

        /// <summary>Sets the committed value.</summary>
        /// <remarks>
        /// Writes to Syncfusion's <c>value</c> property. Updates the displayed text immediately
        /// without firing <c>change</c>.
        /// </remarks>
        /// <param name="value">The value to commit, or <see langword="null"/> to clear.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionInPlaceEditor, TModel> SetValue<TModel>(
            this ComponentRef<FusionInPlaceEditor, TModel> self, string? value)
            where TModel : class
            => self.EmitSet("value", ValueProducer.Literal(value));

        /// <summary>Enables the editor, restoring edit-mode entry.</summary>
        /// <remarks>
        /// Calls Syncfusion's <c>disable(false)</c> method. A plain write to <c>disabled</c> does
        /// not apply the <c>.e-disable</c> CSS class that suppresses clicks.
        /// </remarks>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionInPlaceEditor, TModel> Enable<TModel>(
            this ComponentRef<FusionInPlaceEditor, TModel> self)
            where TModel : class
            => self.EmitCall("disable", new List<ValueProducer> { ValueProducer.Literal(false) });

        /// <summary>Disables the editor, blocking edit-mode entry.</summary>
        /// <remarks>Calls Syncfusion's <c>disable(true)</c> method. Applies the <c>.e-disable</c> CSS class.</remarks>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionInPlaceEditor, TModel> Disable<TModel>(
            this ComponentRef<FusionInPlaceEditor, TModel> self)
            where TModel : class
            => self.EmitCall("disable", new List<ValueProducer> { ValueProducer.Literal(true) });

        /// <summary>Programmatically commits the current edit.</summary>
        /// <remarks>
        /// Calls Syncfusion's <c>save()</c> method. Fires <c>beginEdit → change → endEdit → actionBegin → actionSuccess</c>.
        /// Does not fire <c>submitClick</c>, which is user-gesture only.
        /// </remarks>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionInPlaceEditor, TModel> Save<TModel>(
            this ComponentRef<FusionInPlaceEditor, TModel> self)
            where TModel : class
            => self.EmitCall("save");

        /// <summary>Moves focus into the inner editor input.</summary>
        /// <remarks>Calls Syncfusion's <c>setFocus()</c> method.</remarks>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionInPlaceEditor, TModel> Focus<TModel>(
            this ComponentRef<FusionInPlaceEditor, TModel> self)
            where TModel : class
            => self.EmitCall("setFocus");

        /// <summary>Triggers on-demand validation against the configured validationRules.</summary>
        /// <remarks>
        /// Calls Syncfusion's <c>validate()</c> method. Fires the <c>validating</c> event without firing
        /// <c>actionBegin</c>. Only meaningful when the builder configures <c>validationRules</c>.
        /// </remarks>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionInPlaceEditor, TModel> Validate<TModel>(
            this ComponentRef<FusionInPlaceEditor, TModel> self)
            where TModel : class
            => self.EmitCall("validate");

        /// <summary>Reads the current committed value for use in conditions or gather.</summary>
        /// <remarks>
        /// Reads Syncfusion's outer <c>value</c> property using the shape registered at render time
        /// by <see cref="FusionInPlaceEditorHtmlExtensions"/> (i.e. <c>Shape.FromClrType(typeof(TProp))</c>).
        /// A <c>DateTime?</c>-bound editor reads as date, a <c>decimal</c>-bound editor reads as number,
        /// a <c>string</c>-bound editor reads as string — no hardcoded <see cref="Shape.String"/> that
        /// would conflict with the registered shape and fail <c>EnsureProperty</c> at plan-build time.
        /// </remarks>
        /// <returns>A typed source representing the editor's current committed value.</returns>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionInPlaceEditor, TModel> self)
            where TModel : class
        {
            self.Pipeline.Context.EnsureComponent(self.TargetId, Component.Vendor);

            // Honor the shape the HtmlExtensions registered. EnsureProperty would otherwise throw on
            // a shape mismatch when the component was registered with a non-string shape (Date, Number).
            var shape = self.Pipeline.Context.TryFindRegistrationById(self.TargetId, out var reg) && reg != null
                ? reg.Shape
                : Shape.String;

            self.Pipeline.Context.EnsureProperty(self.TargetId, Component.ValueMember, Component.ValueMember, shape, "read");
            return new TypedComponentSource<string>(self.TargetId, Component.Vendor, Component.ValueMember);
        }

    }
}
