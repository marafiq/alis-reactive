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
    /// Use on a component reference resolved from a pipeline.
    /// </remarks>
    public static class NativeHiddenFieldExtensions
    {
        private static readonly ComponentProperty<string> ValueProperty =
            ComponentProperty<string>.Named("value");

        /// <summary>
        /// Writes the hidden input value through the component contract.
        /// </summary>
        /// <param name="value">The value to write to the hidden input.</param>
        public static ComponentRef<NativeHiddenField, TModel> SetValue<TModel>(
            this ComponentRef<NativeHiddenField, TModel> self, string value)
            where TModel : class
        {
            return self.EmitSet(ValueProperty, ValueExpression.Literal(value));
        }

        /// <summary>
        /// Writes the hidden input value from another component value source.
        /// </summary>
        /// <param name="source">The component value source to write from.</param>
        public static ComponentRef<NativeHiddenField, TModel> SetValue<TModel>(
            this ComponentRef<NativeHiddenField, TModel> self, TypedComponentSource<string> source)
            where TModel : class
        {
            return self.EmitSet(ValueProperty, source.ToValueExpression());
        }

        /// <summary>
        /// Writes the hidden input value from an HTTP response body path.
        /// </summary>
        /// <typeparam name="TResponse">The response DTO type used by the response body source.</typeparam>
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
        /// <returns>A typed source representing the hidden input's current value.</returns>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<NativeHiddenField, TModel> self)
            where TModel : class
        {
            return self.Read(ValueProperty);
        }
    }
}
