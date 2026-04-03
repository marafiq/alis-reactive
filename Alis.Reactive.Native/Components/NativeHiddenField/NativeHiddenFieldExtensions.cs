using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Provides hidden-field mutations and reads for reactive pipelines.
    /// </summary>
    public static class NativeHiddenFieldExtensions
    {
        private static readonly NativeHiddenField _component = new NativeHiddenField();

        /// <summary>Sets the hidden field value from a literal string.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="value">The value to assign.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<NativeHiddenField, TModel> SetValue<TModel>(
            this ComponentRef<NativeHiddenField, TModel> self, string value)
            where TModel : class
        {
            return self.Set("value", value);
        }

        /// <summary>Sets the hidden field value from another component read.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="source">The component value expression to copy from.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<NativeHiddenField, TModel> SetValue<TModel>(
            this ComponentRef<NativeHiddenField, TModel> self, ComponentValueExpression<string> source)
            where TModel : class
        {
            return self.Set("value", source);
        }

        /// <summary>Sets the hidden field value from an HTTP response path.</summary>
        /// <typeparam name="TModel">The page model type that owns the reactive plan.</typeparam>
        /// <typeparam name="TResponse">The response body type.</typeparam>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="source">The response body placeholder.</param>
        /// <param name="path">The response path selecting the value.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<NativeHiddenField, TModel> SetValue<TModel, TResponse>(
            this ComponentRef<NativeHiddenField, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.SetFromPath("value", sourcePath);
        }

        /// <summary>Reads the current hidden field value for conditions and gather.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>A typed value expression representing the hidden field value.</returns>
        public static ComponentValueExpression<string> Value<TModel>(
            this ComponentRef<NativeHiddenField, TModel> self)
            where TModel : class
        {
            return new ComponentValueExpression<string>(self.TargetId, _component.Vendor, _component.ValueMemberPath);
        }
    }
}
