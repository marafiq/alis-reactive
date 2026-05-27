using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Vendor-agnostic gather extensions for including component values in HTTP requests.
    /// </summary>
    public static class GatherExtensions
    {
        /// <summary>
        /// Includes an input component's value, identified by model expression.
        /// The property name from the expression becomes the HTTP parameter name.
        /// </summary>
        public static GatherBuilder<TModel> Include<TComponent, TModel>(
            this GatherBuilder<TModel> self,
            Expression<Func<TModel, object>> expr)
            where TComponent : IComponent, IInputComponent, new()
            where TModel : class
        {
            var component = new TComponent();
            var elementId = IdGenerator.For<TModel, object>(expr);
            var propertyName = ExpressionPathHelper.ToPropertyName<TModel, object>(expr);
            var shape = Shape.FromClrType(ExpressionPathHelper.ToPropertyType(expr));
            self.Include(elementId, component.Vendor, propertyName, component.ValueMember, shape);
            return self;
        }

        /// <summary>
        /// Includes a component's value by explicit element ID and property name.
        /// Works for both input and display components. For input components, reads
        /// the ValueMember; for display components, reads the named property.
        /// </summary>
        public static GatherBuilder<TModel> Include<TComponent, TModel>(
            this GatherBuilder<TModel> self,
            string refId,
            string name)
            where TComponent : IComponent, new()
            where TModel : class
        {
            var component = new TComponent();
            var valueMember = name;
            if (component is IInputComponent input)
                valueMember = input.ValueMember;

            self.Include(refId, component.Vendor, name, valueMember);
            return self;
        }

        /// <summary>
        /// Includes a typed component member value in the gather.
        /// The member name becomes the HTTP parameter name.
        /// Use with component value sources like <c>schedule.CurrentView()</c>,
        /// <c>schedule.SelectedDate()</c>, or method-return sources such as <c>schedule.GetEvents()</c>.
        /// </summary>
        public static GatherBuilder<TModel> Include<TModel, TProp>(
            this GatherBuilder<TModel> self,
            TypedComponentSource<TProp> source)
            where TModel : class
        {
            self.Include(source, source.ReadMember);
            return self;
        }

        /// <summary>
        /// Includes a typed component member value with an explicit HTTP parameter name.
        /// Use when the parameter name differs from the component property name.
        /// </summary>
        public static GatherBuilder<TModel> Include<TModel, TProp>(
            this GatherBuilder<TModel> self,
            TypedComponentSource<TProp> source,
            string paramName)
            where TModel : class
        {
            self.Include(source, paramName);
            return self;
        }
    }
}
