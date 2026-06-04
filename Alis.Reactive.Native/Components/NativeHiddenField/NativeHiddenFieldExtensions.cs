using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Reactive Plan operations for <see cref="NativeHiddenField"/> values.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via
    /// <see cref="Builders.PipelineBuilder{TModel}.Component{TComponent}(System.Linq.Expressions.Expression{System.Func{TModel, object}})"/>.
    /// </remarks>
    public static class NativeHiddenFieldExtensions
    {
        private static readonly ComponentProperty<string> ValueProperty =
            ComponentProperty<string>.Named("value");

        /// <summary>
        /// Sets the hidden input value through the component contract.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the component reference.</typeparam>
        /// <param name="self">The hidden-field component reference.</param>
        /// <param name="value">The value to write to the hidden input.</param>
        public static ComponentRef<NativeHiddenField, TModel> SetValue<TModel>(
            this ComponentRef<NativeHiddenField, TModel> self, string value)
            where TModel : class
        {
            return self.EmitSet(ValueProperty, ValueExpression.Literal(value));
        }

        /// <summary>
        /// Sets the hidden input value from another component value source.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the component reference.</typeparam>
        /// <param name="self">The hidden-field component reference.</param>
        /// <param name="source">The component value source to write from.</param>
        public static ComponentRef<NativeHiddenField, TModel> SetValue<TModel>(
            this ComponentRef<NativeHiddenField, TModel> self, TypedComponentSource<string> source)
            where TModel : class
        {
            return self.EmitSet(ValueProperty, source.ToValueExpression());
        }

        /// <summary>
        /// Sets the hidden input value from an HTTP response body path.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the component reference.</typeparam>
        /// <typeparam name="TResponse">The response DTO type used by the response body source.</typeparam>
        /// <param name="self">The hidden-field component reference.</param>
        /// <param name="source">The response body scope to read from.</param>
        /// <param name="path">The response DTO member path to write into the hidden input.</param>
        public static ComponentRef<NativeHiddenField, TModel> SetValue<TModel, TResponse>(
            this ComponentRef<NativeHiddenField, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.EmitSet(ValueProperty, ValueExpression.Read(source.Scope, "body", Path.Parse(sourcePath)));
        }

        /// <summary>
        /// Reads the current hidden input value for use in conditions or gather.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the component reference.</typeparam>
        /// <returns>A typed source representing the hidden input's current value.</returns>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<NativeHiddenField, TModel> self)
            where TModel : class
        {
            return self.Read(ValueProperty);
        }
    }
}
