using System;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors.Requests;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Vendor-agnostic gather extensions for any <see cref="IComponent"/> + <see cref="IInputComponent"/>.
    /// Works for both Native and Fusion components: vendor and readExpr
    /// are resolved from the component instance at build time.
    /// </summary>
    /// <remarks>
    /// Gather collects current component values from the browser and includes them
    /// in the HTTP request payload before sending. Use inside
    /// <see cref="HttpRequestBuilder{TModel}.Gather"/> to specify which components contribute values:
    /// <code>
    /// p.Post("/api/save", g => g.Include&lt;NativeTextBox, MyModel&gt;(m => m.Name));
    /// </code>
    /// </remarks>
    public static class GatherExtensions
    {
        /// <summary>
        /// Gathers the value of a component bound to a model property.
        /// The component is identified by the model expression (m => m.FacilityId).
        /// </summary>
        public static GatherBuilder<TModel> Include<TComponent, TModel>(
            this GatherBuilder<TModel> self,
            Expression<Func<TModel, object?>> expr)
            where TComponent : IComponent, IInputComponent, new()
            where TModel : class
        {
            var component = new TComponent();
            var elementId = IdGenerator.For<TModel>(expr);
            var propertyName = ExpressionPathHelper.ToPropertyName(expr);
            self.AddItem(new ComponentGather(
                elementId,
                component.Vendor,
                propertyName,
                component.ReadExpr));
            return self;
        }

        /// <summary>
        /// Escape hatch: gathers a component value by raw element ID instead of model expression.
        /// Use for non-model-bound components (grids, string-id controls) that cannot use
        /// the expression-based <see cref="Include{TComponent,TModel}(GatherBuilder{TModel}, System.Linq.Expressions.Expression{System.Func{TModel, object}})"/> overload.
        /// </summary>
        /// <param name="refId">The element ID of the component on the page.</param>
        /// <param name="name">The key name in the request payload.</param>
        public static GatherBuilder<TModel> Include<TComponent, TModel>(
            this GatherBuilder<TModel> self,
            string refId,
            string name)
            where TComponent : IComponent, IInputComponent, new()
            where TModel : class
        {
            var component = new TComponent();
            self.AddItem(new ComponentGather(
                refId,
                component.Vendor,
                name,
                component.ReadExpr));
            return self;
        }
    }
}
