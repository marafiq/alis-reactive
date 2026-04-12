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
            var elementId = IdGenerator.For<TModel>(expr);
            var propertyName = ExpressionPathHelper.ToPropertyName(expr);
            self.Include(elementId, component.Vendor, propertyName, component.ValueMember);
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
            var valueMember = (component is IInputComponent input) ? input.ValueMember : name;
            self.Include(refId, component.Vendor, name, valueMember);
            return self;
        }

        /// <summary>
        /// Includes a typed component property read in the gather.
        /// The member name becomes the HTTP parameter name.
        /// Use with display component readable properties like
        /// <c>schedule.CurrentView()</c> or <c>schedule.SelectedDate()</c>.
        /// </summary>
        public static GatherBuilder<TModel> Include<TModel, TProp>(
            this GatherBuilder<TModel> self,
            TypedComponentSource<TProp> source)
            where TModel : class
        {
            self.Include(source.ComponentId, source.Vendor, source.ReadMember, source.ReadMember);
            return self;
        }

        /// <summary>
        /// Includes a typed component property read with an explicit HTTP parameter name.
        /// Use when the parameter name differs from the component property name.
        /// </summary>
        public static GatherBuilder<TModel> Include<TModel, TProp>(
            this GatherBuilder<TModel> self,
            TypedComponentSource<TProp> source,
            string paramName)
            where TModel : class
        {
            self.Include(source.ComponentId, source.Vendor, paramName, source.ReadMember);
            return self;
        }
    }
}
