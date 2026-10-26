using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// A member of an object, read when the step runs: the object is located then, and the member is the
    /// path into it. An empty path reads the object itself, such as a response body that is the array a
    /// grid binds.
    /// </summary>
    /// <typeparam name="TProp">The C# type of the member.</typeparam>
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
