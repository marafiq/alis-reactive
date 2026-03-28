using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Sources;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Pipeline extensions for <see cref="NativeRadioGroup"/>: set selected value, focus, and read.
    /// </summary>
    public static class NativeRadioGroupExtensions
    {
        private static readonly NativeRadioGroup _component = new NativeRadioGroup();

        /// <summary>
        /// Sets the selected radio button value in the browser.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="self">The radio group component reference.</param>
        /// <param name="value">The radio option value to select.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeRadioGroup, TModel> SetValue<TModel>(
            this ComponentRef<NativeRadioGroup, TModel> self, string value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value"), value: value);
        }

        /// <summary>
        /// Sets the selected radio button value from a source binding (e.g. event payload).
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TSource">The source type containing the value.</typeparam>
        /// <param name="self">The radio group component reference.</param>
        /// <param name="source">The source object (e.g. event args).</param>
        /// <param name="path">Expression selecting the property to read (e.g. <c>x => x.Value</c>).</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeRadioGroup, TModel> SetValue<TModel, TSource>(
            this ComponentRef<NativeRadioGroup, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.Emit(new SetPropMutation("value"), source: new EventSource(sourcePath));
        }

        /// <summary>
        /// Moves keyboard focus into the radio group.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeRadioGroup, TModel> FocusIn<TModel>(
            this ComponentRef<NativeRadioGroup, TModel> self)
            where TModel : class
        {
            return self.Emit(new CallMutation("focus"));
        }

        /// <summary>
        /// Reads the currently selected value for use in conditions or gather.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <returns>A typed source representing the radio group's selected value.</returns>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<NativeRadioGroup, TModel> self)
            where TModel : class
        {
            return new TypedComponentSource<string>(self.TargetId, _component.Vendor, _component.ReadExpr);
        }
    }
}
