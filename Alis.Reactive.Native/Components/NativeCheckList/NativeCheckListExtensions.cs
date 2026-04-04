using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;

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
            return self.Set(NativeCheckList.Value, value);
        }

        /// <summary>
        /// Sets the checked values from a source binding (e.g. event payload).
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TSource">The source type containing the value.</typeparam>
        /// <param name="self">The check list component reference.</param>
        /// <param name="source">The source object (e.g. event args).</param>
        /// <param name="path">Expression selecting the property to read (e.g. <c>x => x.Value</c>).</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeCheckList, TModel> SetValue<TModel, TSource>(
            this ComponentRef<NativeCheckList, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
            => self.SetFromEvent(NativeCheckList.Value, path);

        /// <summary>
        /// Moves keyboard focus into the check list.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeCheckList, TModel> FocusIn<TModel>(
            this ComponentRef<NativeCheckList, TModel> self)
            where TModel : class
        {
            return self.Call(NativeCheckList.Focus);
        }

        /// <summary>
        /// Reads the currently checked values for use in conditions or gather.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>A typed value expression representing the check list's selected values as a string array.</returns>
        public static ReactiveValue<string[]> Value<TModel>(
            this ComponentRef<NativeCheckList, TModel> self)
            where TModel : class
        {
            return self.CreateValue<string[]>();
        }
    }
}
