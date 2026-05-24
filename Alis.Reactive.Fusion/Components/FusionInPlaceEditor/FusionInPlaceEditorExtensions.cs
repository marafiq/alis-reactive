using System;
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
        private static readonly ComponentProperty<string> ValueProperty =
            ComponentProperty<string>.Named("value");
        private static readonly ComponentMethod DisableMethod =
            ComponentMethod.Named("disable").WithArgs<bool>();
        private static readonly ComponentMethod SaveMethod =
            ComponentMethod.Named("save");
        private static readonly ComponentMethod FocusMethod =
            ComponentMethod.Named("setFocus");
        private static readonly ComponentMethod ClassAddMethod =
            ComponentMethod.Mapped("classAdd", "element.classList.add").WithArgs<string>();
        private static readonly ComponentMethod ClassRemoveMethod =
            ComponentMethod.Mapped("classRemove", "element.classList.remove").WithArgs<string>();

        /// <summary>Sets the committed value.</summary>
        /// <remarks>
        /// Writes to Syncfusion's <c>value</c> property. Updates the displayed text immediately
        /// without firing <c>change</c>.
        /// </remarks>
        /// <param name="self">The component reference for the target editor.</param>
        /// <param name="value">The value to commit, or <see langword="null"/> to clear.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionInPlaceEditor, TModel> SetValue<TModel>(
            this ComponentRef<FusionInPlaceEditor, TModel> self, string? value)
            where TModel : class
            => self.EmitSet(ValueProperty, value != null ? ValueProducer.Literal(value) : ValueProducer.Null());

        /// <summary>Enables the editor, restoring edit-mode entry.</summary>
        /// <remarks>
        /// Calls Syncfusion's <c>disable(false)</c> method. A plain write to <c>disabled</c> does
        /// not apply the <c>.e-disable</c> CSS class that suppresses clicks.
        /// </remarks>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionInPlaceEditor, TModel> Enable<TModel>(
            this ComponentRef<FusionInPlaceEditor, TModel> self)
            where TModel : class
            => self.EmitCall(DisableMethod, new List<ValueProducer> { ValueProducer.Literal(false) });

        /// <summary>Disables the editor, blocking edit-mode entry.</summary>
        /// <remarks>Calls Syncfusion's <c>disable(true)</c> method. Applies the <c>.e-disable</c> CSS class.</remarks>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionInPlaceEditor, TModel> Disable<TModel>(
            this ComponentRef<FusionInPlaceEditor, TModel> self)
            where TModel : class
            => self.EmitCall(DisableMethod, new List<ValueProducer> { ValueProducer.Literal(true) });

        /// <summary>Programmatically commits the current edit.</summary>
        /// <remarks>
        /// Calls Syncfusion's <c>save()</c> method. Fires <c>beginEdit → change → endEdit → actionBegin → actionSuccess</c>.
        /// Does not fire <c>submitClick</c>, which is user-gesture only.
        /// </remarks>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionInPlaceEditor, TModel> Save<TModel>(
            this ComponentRef<FusionInPlaceEditor, TModel> self)
            where TModel : class
            => self.EmitCall(SaveMethod);

        /// <summary>Moves focus into the inner editor input.</summary>
        /// <remarks>Calls Syncfusion's <c>setFocus()</c> method.</remarks>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionInPlaceEditor, TModel> Focus<TModel>(
            this ComponentRef<FusionInPlaceEditor, TModel> self)
            where TModel : class
            => self.EmitCall(FocusMethod);

        /// <summary>Adds a CSS class to the editor's outer wrapper.</summary>
        /// <remarks>
        /// Emits a call on Syncfusion's <c>element.classList.add</c>. The Fusion vendor resolver
        /// returns the ej2 instance, so the path reaches through <c>ej.element</c> (SF's reference
        /// back to the editor's outer DOM element). The class persists across SF's edit/close
        /// cycles; typical use is a visual commit signal (e.g. a CSS <c>::after</c> check mark)
        /// wired on <c>ActionSuccess</c> and removed on <c>BeginEdit</c>.
        /// </remarks>
        /// <param name="self">The component reference for the target editor.</param>
        /// <param name="className">The class name to add.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionInPlaceEditor, TModel> AddClass<TModel>(
            this ComponentRef<FusionInPlaceEditor, TModel> self, string className)
            where TModel : class
            => self.EmitCall(ClassAddMethod, new List<ValueProducer> { ValueProducer.Literal(className) });

        /// <summary>Removes a CSS class from the editor's outer wrapper.</summary>
        /// <remarks>Emits a call on Syncfusion's <c>element.classList.remove</c>.</remarks>
        /// <param name="self">The component reference for the target editor.</param>
        /// <param name="className">The class name to remove.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionInPlaceEditor, TModel> RemoveClass<TModel>(
            this ComponentRef<FusionInPlaceEditor, TModel> self, string className)
            where TModel : class
            => self.EmitCall(ClassRemoveMethod, new List<ValueProducer> { ValueProducer.Literal(className) });

        /// <summary>Reads the current committed value for use in conditions or gather.</summary>
        /// <remarks>
        /// Reads Syncfusion's outer <c>value</c> property using the shape registered at render time
        /// by <see cref="FusionInPlaceEditorHtmlExtensions"/> (i.e. <c>Shape.FromClrType(typeof(TProp))</c>).
        /// A <c>DateTime?</c>-bound editor reads as date, a <c>decimal</c>-bound editor reads as number,
        /// a <c>string</c>-bound editor reads as string. The component must be registered via
        /// <c>Html.InputField(plan, m => m.X).FusionInPlaceEditor(...)</c> before this read is built
        /// into the plan: no hardcoded shape, no fallback.
        /// </remarks>
        /// <returns>A typed source representing the editor's current committed value.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no <c>FusionInPlaceEditor</c> registration exists for <paramref name="self"/>'s
        /// target id. Call <c>Html.InputField(plan, m =&gt; m.X).FusionInPlaceEditor(...)</c> first.
        /// </exception>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionInPlaceEditor, TModel> self)
            where TModel : class
        {
            self.Pipeline.Context.EnsureComponent(self.TargetId, Component.Vendor);

            var registration = self.Pipeline.Context.FindRegistrationById(self.TargetId);
            var registeredShape = registration.RequireShape(
                ComponentRegistrationRequirement.ForFusionInPlaceEditorValueRead(self.TargetId));

            return self.Read(ValueProperty.WithShape(registeredShape));
        }

    }
}
