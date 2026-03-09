using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.Descriptors.Requests;

namespace Alis.Reactive.Native.Extensions
{
    /// <summary>
    /// Adds Include&lt;TComponent&gt;() to GatherBuilder for native DOM components.
    /// Vendor = "native", no readExpr (runtime reads .value directly from DOM element).
    /// </summary>
    public static class NativeGatherExtensions
    {
        /// <summary>
        /// Gathers the value of a native DOM component bound to a model property.
        /// The component is identified by the model expression (m => m.FirstName).
        /// Native elements are read directly via el.value — no readExpr needed.
        /// </summary>
        public static GatherBuilder<TModel> Include<TComponent, TModel>(
            this GatherBuilder<TModel> self,
            Expression<Func<TModel, object?>> expr)
            where TComponent : NativeComponent
            where TModel : class
        {
            var elementId = ExpressionPathHelper.ToElementId(expr);
            var propertyName = ExpressionPathHelper.ToPropertyName(expr);
            self.AddItem(new ComponentGather(
                elementId,
                "native",
                propertyName));
            return self;
        }

        /// <summary>
        /// Gathers the value of a native DOM component identified by string ref.
        /// </summary>
        public static GatherBuilder<TModel> Include<TComponent, TModel>(
            this GatherBuilder<TModel> self,
            string refId,
            string name)
            where TComponent : NativeComponent
            where TModel : class
        {
            self.AddItem(new ComponentGather(
                refId,
                "native",
                name));
            return self;
        }
    }
}
