using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Provides component-source overloads for adding request body fields.
    /// </summary>
    public static class GatherExtensions
    {
        /// <summary>
        /// Adds a model-bound input component value to the request body.
        /// </summary>
        /// <remarks>
        /// The model expression supplies both the generated component ID and the request
        /// body field name. The component contract supplies the value member read at runtime.
        /// </remarks>
        /// <typeparam name="TComponent">The input component contract to read.</typeparam>
        /// <typeparam name="TModel">The view model used to derive the generated component ID.</typeparam>
        /// <param name="self">The request-input gather builder.</param>
        /// <param name="expr">The model property expression for the component ID and body field.</param>
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
        /// Adds a component value to the request body by explicit component ID.
        /// </summary>
        /// <remarks>
        /// Input components read their configured value member. Components without an
        /// input-value contract read the member named by <paramref name="name"/>.
        /// </remarks>
        /// <typeparam name="TComponent">The component contract to read.</typeparam>
        /// <typeparam name="TModel">The view model that owns the request pipeline.</typeparam>
        /// <param name="self">The request-input gather builder.</param>
        /// <param name="refId">The explicit controlled component ID rendered in markup.</param>
        /// <param name="name">The request body field name; also the member name for non-input components.</param>
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
        /// Adds a typed component member read to the request body.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the request pipeline.</typeparam>
        /// <typeparam name="TProp">The component member value type.</typeparam>
        /// <param name="self">The request-input gather builder.</param>
        /// <param name="source">The component property or method source; its default body field name is used.</param>
        public static GatherBuilder<TModel> Include<TModel, TProp>(
            this GatherBuilder<TModel> self,
            TypedComponentSource<TProp> source)
            where TModel : class
        {
            self.Include(source, source.DefaultPayloadName);
            return self;
        }

        /// <summary>
        /// Adds a typed component member read to an explicit request body field.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the request pipeline.</typeparam>
        /// <typeparam name="TProp">The component member value type.</typeparam>
        /// <param name="self">The request-input gather builder.</param>
        /// <param name="source">The component property or method source evaluated before the request is sent.</param>
        /// <param name="paramName">The request body field name.</param>
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
