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

        // SetValue&lt;TProp&gt;(TProp value) is provided by the ComponentRef base class.
        // Vendor-specific write overloads (TypedComponentSource source, ResponseBody source+path,
        // event payload source+path) remain on the vendor's Extensions class because their
        // signatures need TResponse/TSource type parameters alongside TProp.

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
        {
            var key = self.Pipeline.Context.EnsureComponent(self.TargetId, self.Vendor);
            self.Pipeline.Context.EnsureMethod(key, "classAdd", "element.classList.add");
            self.Pipeline.Steps.Add(Reaction.Call(
                ComponentSource.Of(key), "classAdd",
                new List<ValueProducer> { ValueProducer.Literal(className) }));
            return self;
        }

        /// <summary>Removes a CSS class from the editor's outer wrapper.</summary>
        /// <remarks>Emits a call on Syncfusion's <c>element.classList.remove</c>.</remarks>
        /// <param name="self">The component reference for the target editor.</param>
        /// <param name="className">The class name to remove.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionInPlaceEditor, TModel> RemoveClass<TModel>(
            this ComponentRef<FusionInPlaceEditor, TModel> self, string className)
            where TModel : class
        {
            var key = self.Pipeline.Context.EnsureComponent(self.TargetId, self.Vendor);
            self.Pipeline.Context.EnsureMethod(key, "classRemove", "element.classList.remove");
            self.Pipeline.Steps.Add(Reaction.Call(
                ComponentSource.Of(key), "classRemove",
                new List<ValueProducer> { ValueProducer.Literal(className) }));
            return self;
        }

        // Value&lt;TProp&gt;() is provided by the ComponentRef base class — see ComponentRef.cs.
        // FusionInPlaceEditor is IInputComponent, so the base method reads ValueMember="value"
        // and cross-checks TProp against the registered model property shape.

    }
}
