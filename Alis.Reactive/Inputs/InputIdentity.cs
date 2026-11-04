using System;
using System.Linq.Expressions;
using Alis.Reactive.Grammar;
using Alis.Reactive.WireTerms;
using Alis.Reactive.Members;

namespace Alis.Reactive.Inputs
{
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

            var id = IdGenerator.ForMember(ExpressionPathHelper.ToDeclarer(expression), bindingName);
            var bodyPath = HtmlFieldPrefix.Join(htmlFieldPrefix, ExpressionPathHelper.ToBodyPath(expression));
            return new InputIdentity(
                ComponentId.Of(id),
                BindingPath.ForMember(bindingName, bodyPath),
                ClrShape.Of(typeof(TValue)));
        }
    }
}
