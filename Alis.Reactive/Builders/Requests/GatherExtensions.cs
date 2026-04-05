using System;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Vendor-agnostic gather extensions for any <see cref="IComponent"/> + <see cref="IInputComponent"/>.
    /// </summary>
    public static class GatherExtensions
    {
        public static GatherBuilder<TModel> Include<TComponent, TModel>(
            this GatherBuilder<TModel> self,
            Expression<Func<TModel, object>> expr)
            where TComponent : IComponent, IInputComponent, new()
            where TModel : class
        {
            var component = new TComponent();
            var elementId = IdGenerator.For<TModel>(expr);
            var propertyName = ExpressionPathHelper.ToPropertyName(expr);
            self.Include(elementId, component.Vendor, propertyName, component.ValueMember);
            return self;
        }

        public static GatherBuilder<TModel> Include<TComponent, TModel>(
            this GatherBuilder<TModel> self,
            string refId,
            string name)
            where TComponent : IComponent, IInputComponent, new()
            where TModel : class
        {
            var component = new TComponent();
            self.Include(refId, component.Vendor, name, component.ValueMember);
            return self;
        }
    }
}
