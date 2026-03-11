using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.Descriptors.Requests;

namespace Alis.Reactive.Native.Extensions
{
    /// <summary>
    /// Adds Include&lt;TComponent&gt;() to GatherBuilder for native DOM components.
    /// Vendor = "native", readExpr resolved from [ReadExpr] attribute on TComponent.
    /// </summary>
    public static class NativeGatherExtensions
    {
        /// <summary>
        /// Gathers the value of a native DOM component bound to a model property.
        /// The component is identified by the model expression (m => m.FirstName).
        /// ReadExpr is resolved from the component type's [ReadExpr] attribute.
        /// </summary>
        public static GatherBuilder<TModel> Include<TComponent, TModel>(
            this GatherBuilder<TModel> self,
            Expression<Func<TModel, object?>> expr)
            where TComponent : NativeComponent, IReadableComponent
            where TModel : class
        {
            var elementId = ExpressionPathHelper.ToElementId(expr);
            var propertyName = ExpressionPathHelper.ToPropertyName(expr);
            self.AddItem(new ComponentGather(
                elementId,
                "native",
                propertyName,
                ComponentHelper.GetReadExpr<TComponent>()));
            return self;
        }

        /// <summary>
        /// Gathers the value of a native DOM component identified by string ref.
        /// </summary>
        public static GatherBuilder<TModel> Include<TComponent, TModel>(
            this GatherBuilder<TModel> self,
            string refId,
            string name)
            where TComponent : NativeComponent, IReadableComponent
            where TModel : class
        {
            self.AddItem(new ComponentGather(
                refId,
                "native",
                name,
                ComponentHelper.GetReadExpr<TComponent>()));
            return self;
        }
    }
}
