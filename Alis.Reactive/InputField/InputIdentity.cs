using System;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.InputField
{
    /// <summary>
    /// What the framework dictates for an input before any component renders it: the element id
    /// derived from the model type and the MVC binding name, the MVC binding path the value posts
    /// under, and the shape of the member's value. The binding name carries the HTML field prefix, so a
    /// partial rendered under two prefixes renders two inputs with two ids.
    /// </summary>
    internal sealed class InputIdentity
    {
        private InputIdentity(ComponentId id, BindingPath binding, Shape valueShape)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Binding = binding ?? throw new ArgumentNullException(nameof(binding));
            ValueShape = valueShape ?? throw new ArgumentNullException(nameof(valueShape));
        }

        internal ComponentId Id { get; }

        internal BindingPath Binding { get; }

        internal Shape ValueShape { get; }

        internal string ElementId => Id.Value;

        internal string BindingName => Binding.Value;

        internal static InputIdentity For<TModel, TValue>(
            Expression<Func<TModel, TValue>> expression,
            string bindingName,
            string htmlFieldPrefix)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var bodyPath = Path.Parse(htmlFieldPrefix).Append(ExpressionPathHelper.ToBodyPath(expression));
            return new InputIdentity(
                ComponentId.Of(IdGenerator.For(typeof(TModel), bindingName)),
                BindingPath.ForMember(bindingName, bodyPath),
                Shape.FromClrType(typeof(TValue)));
        }
    }
}
