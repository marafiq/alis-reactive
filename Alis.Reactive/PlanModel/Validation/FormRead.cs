using System;
using System.Collections.Generic;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel.Values;
using Alis.Reactive.PlanModel.Writing;

namespace Alis.Reactive.PlanModel.Validation
{
    internal sealed class FormRead : ValueExpression
    {
        private readonly string _name;
        private readonly Shape _shape;
        private readonly Expr _indexes;
        private readonly Unsent _unsent;

        internal FormRead(string name, Shape shape, Expr indexes, Unsent unsent)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
            _indexes = indexes ?? throw new ArgumentNullException(nameof(indexes));
            _unsent = unsent ?? throw new ArgumentNullException(nameof(unsent));
        }

        internal override Shape OutputShape => _shape;

        private protected override IEnumerable<ValueExpression> Operands => Array.Empty<ValueExpression>();

        internal override Expr Write(Scope scope)
        {
            var model = G.Obj(scope.Objects.Model());
            var sent = G.Ref(FormFieldValues.Sent);
            Expr Sends(string pattern)
                => G.Call(model, new[] { "sends" }, new[] { sent, G.Lit(pattern), _indexes }, Shape.Boolean, PathMembers.Required);

            var unsent = _unsent.Write(scope, _indexes, Sends);
            return G.Call(model, new[] { "sentValueOf" }, new[] { sent, G.Lit(_name), _indexes, unsent }, Shape.Any, PathMembers.Required);
        }
    }
}
