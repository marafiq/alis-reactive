using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.Descriptors.Requests;

namespace Alis.Reactive.Fusion.Extensions
{
    /// <summary>
    /// Adds Include&lt;TComponent&gt;() to GatherBuilder for Syncfusion components.
    /// Vendor = "fusion", readExpr resolved from IInputComponent.ReadExpr on TComponent.
    /// Vendor determines root (ej2_instances[0]), readExpr walks from that root.
    /// </summary>
    public static class FusionGatherExtensions
    {
        /// <summary>
        /// Gathers the value of a Fusion component bound to a model property.
        /// The component is identified by the model expression (m => m.FacilityId).
        /// ReadExpr is resolved from the component type's IInputComponent.ReadExpr property.
        /// </summary>
        public static GatherBuilder<TModel> Include<TComponent, TModel>(
            this GatherBuilder<TModel> self,
            Expression<Func<TModel, object?>> expr)
            where TComponent : FusionComponent, IInputComponent, new()
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
        /// Gathers the value of a Fusion component identified by string ref.
        /// Used for non-model-bound components (grids, string-id controls).
        /// </summary>
        public static GatherBuilder<TModel> Include<TComponent, TModel>(
            this GatherBuilder<TModel> self,
            string refId,
            string name)
            where TComponent : FusionComponent, IInputComponent, new()
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
