using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Adds component value sources to HTTP request payloads.
    /// </summary>
    public static class GatherExtensions
    {
        /// <summary>
        /// Adds a model-bound input component's value to the request payload.
        /// The model property name becomes the payload field name.
        /// </summary>
        /// <typeparam name="TComponent">The input component contract type.</typeparam>
        /// <typeparam name="TModel">The view model that owns the component ID.</typeparam>
        /// <param name="self">The gather builder being configured.</param>
        /// <param name="expr">The model property expression used to generate the component ID and payload field name.</param>
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
        /// Adds a component value to the request payload by explicit element ID and field name.
        /// </summary>
        /// <remarks>
        /// Input components read their configured value member. Display components read the
        /// named member directly.
        /// </remarks>
        /// <typeparam name="TComponent">The component contract type.</typeparam>
        /// <typeparam name="TModel">The view model for the gather builder.</typeparam>
        /// <param name="self">The gather builder being configured.</param>
        /// <param name="refId">The explicit controlled component ID rendered in markup.</param>
        /// <param name="name">The payload field name and, for display components, the component member name.</param>
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
        /// Adds a typed component member value to the request payload.
        /// </summary>
        /// <typeparam name="TModel">The view model for the gather builder.</typeparam>
        /// <typeparam name="TProp">The component member value type.</typeparam>
        /// <param name="self">The gather builder being configured.</param>
        /// <param name="source">The typed component value source. Its default payload name is used.</param>
        public static GatherBuilder<TModel> Include<TModel, TProp>(
            this GatherBuilder<TModel> self,
            TypedComponentSource<TProp> source)
            where TModel : class
        {
            self.Include(source, source.DefaultPayloadName);
            return self;
        }

        /// <summary>
        /// Adds a typed component member value to the request payload with an explicit field name.
        /// </summary>
        /// <typeparam name="TModel">The view model for the gather builder.</typeparam>
        /// <typeparam name="TProp">The component member value type.</typeparam>
        /// <param name="self">The gather builder being configured.</param>
        /// <param name="source">The typed component value source to evaluate before the request is sent.</param>
        /// <param name="paramName">The HTTP payload field name.</param>
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
