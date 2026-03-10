using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.Descriptors.Requests;

namespace Alis.Reactive.Fusion.Extensions
{
    /// <summary>
    /// Adds Include&lt;TComponent&gt;() to GatherBuilder for Syncfusion components.
    /// Vendor = "fusion", readExpr = "comp.value" (resolved by evalRead via ej2_instances[0]).
    /// </summary>
    public static class FusionGatherExtensions
    {
        /// <summary>
        /// Gathers the value of a Fusion component bound to a model property.
        /// The component is identified by the model expression (m => m.FacilityId)
        /// and the value is read via evalRead: comp.value → ej2_instances[0].value.
        /// </summary>
        public static GatherBuilder<TModel> Include<TComponent, TModel>(
            this GatherBuilder<TModel> self,
            Expression<Func<TModel, object?>> expr)
            where TComponent : FusionComponent
            where TModel : class
        {
            var elementId = ExpressionPathHelper.ToElementId(expr);
            var propertyName = ExpressionPathHelper.ToPropertyName(expr);
            self.AddItem(new ComponentGather(
                elementId,
                "fusion",
                propertyName,
                "comp.value"));
            return self;
        }

        /// <summary>
        /// Gathers the value of a Fusion component identified by string ref.
        /// Used for non-model-bound components (grids, string-id controls).
        /// </summary>
        public static GatherBuilder<TModel> Include<TComponent, TModel>(
            this GatherBuilder<TModel> self,
            string refId,
            string name)
            where TComponent : FusionComponent
            where TModel : class
        {
            self.AddItem(new ComponentGather(
                refId,
                "fusion",
                name,
                "comp.value"));
            return self;
        }
    }
}
