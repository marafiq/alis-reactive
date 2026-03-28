using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.Descriptors.Requests;
using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.Extensions
{
    /// <summary>
    /// Gather shorthand that defaults to <see cref="NativeTextBox"/> when no component
    /// type is specified.
    /// </summary>
    /// <remarks>
    /// <para>
    /// "Gather" collects the current values of form components and includes them in the
    /// HTTP request payload before sending. Each <c>Include</c>
    /// call adds one component's value to the request body.
    /// </para>
    /// <para>
    /// This class provides a convenience overload that assumes <see cref="NativeTextBox"/>.
    /// For other component types, use the typed
    /// <see cref="Builders.Requests.GatherExtensions"/> overload:
    /// <c>g.Include&lt;NativeDropDown, MyModel&gt;(m =&gt; m.Status)</c>.
    /// </para>
    /// </remarks>
    public static class NativeGatherExtensions
    {
        private static readonly NativeTextBox DefaultComponent = new NativeTextBox();

        /// <summary>
        /// Gathers the value of a native text input bound to <paramref name="expr"/>.
        /// </summary>
        /// <remarks>
        /// Shorthand for <see cref="NativeTextBox"/>. Use the typed overload
        /// when gathering from a different component type.
        /// </remarks>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="self">The gather builder.</param>
        /// <param name="expr">The model property bound to the text input.</param>
        /// <returns>The builder for chaining.</returns>
        public static GatherBuilder<TModel> Include<TModel>(
            this GatherBuilder<TModel> self,
            Expression<Func<TModel, object?>> expr)
            where TModel : class
        {
            var elementId = IdGenerator.For(expr);
            var propertyName = ExpressionPathHelper.ToPropertyName(expr);
            self.AddItem(new ComponentGather(
                elementId,
                DefaultComponent.Vendor,
                propertyName,
                DefaultComponent.ReadExpr));
            return self;
        }
    }
}
