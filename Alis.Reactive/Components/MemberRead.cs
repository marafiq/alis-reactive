using System;
using Alis.Reactive.PlanModel.Values;
using Alis.Reactive.WireTerms;

namespace Alis.Reactive.Components
{
    internal sealed class MemberRead<TProp> : TypedSource<TProp>
    {
        private readonly Func<Target> _locate;
        private readonly Path _path;

        internal MemberRead(Func<Target> locate, Path path)
        {
            _locate = locate ?? throw new ArgumentNullException(nameof(locate));
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        internal override ValueExpression Expression =>
            _path.IsNone ? ValueExpression.Get(_locate(), Shape) : ValueExpression.Get(_locate(), _path.Text, Shape);
    }
}
