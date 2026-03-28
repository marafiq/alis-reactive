using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Sources;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Pipeline extensions for <see cref="NativeCheckList"/>: set checked values, focus, and read.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via
    /// <see cref="Builders.PipelineBuilder{TModel}.Component{TComponent}(System.Linq.Expressions.Expression{System.Func{TModel, object}})"/>:
    /// <code>p.Component&lt;NativeCheckList&gt;(m =&gt; m.Allergies).SetValue(new[] { "peanuts" })</code>
    /// </remarks>
    public static class NativeCheckListExtensions
    {
        private static readonly NativeCheckList _component = new NativeCheckList();

        /// <summary>
        /// Sets the checked values in the browser.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="self">The check list component reference.</param>
        /// <param name="value">The array of option values to check.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeCheckList, TModel> SetValue<TModel>(
            this ComponentRef<NativeCheckList, TModel> self, string[] value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value"), value: value);
        }

        /// <summary>
        /// Sets the checked values from a source binding (e.g. event payload).
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TSource">The source type containing the value.</typeparam>
        /// <param name="self">The check list component reference.</param>
        /// <param name="source">The source object (e.g. event args).</param>
        /// <param name="path">Expression selecting the property to read (e.g. <c>x => x.Values</c>).</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeCheckList, TModel> SetValue<TModel, TSource>(
            this ComponentRef<NativeCheckList, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.Emit(new SetPropMutation("value"), source: new EventSource(sourcePath));
        }

        /// <summary>
        /// Moves keyboard focus into the check list.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeCheckList, TModel> FocusIn<TModel>(
            this ComponentRef<NativeCheckList, TModel> self)
            where TModel : class
        {
            return self.Emit(new CallMutation("focus"));
        }

        /// <summary>
        /// Reads the currently checked values for use in conditions or gather.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <returns>A typed source representing the check list's selected values as a string array.</returns>
        public static TypedComponentSource<string[]> Value<TModel>(
            this ComponentRef<NativeCheckList, TModel> self)
            where TModel : class
        {
            return new TypedComponentSource<string[]>(self.TargetId, _component.Vendor, _component.ReadExpr);
        }
    }
}
