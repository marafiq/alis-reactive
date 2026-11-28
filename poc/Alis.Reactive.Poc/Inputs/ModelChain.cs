using System.Linq.Expressions;

namespace Alis.Reactive.Inputs;

internal static class ModelChain
{
    internal static bool NamedByMvc(LambdaExpression lambda, string mvcName)
    {
        var steps = 0;
        var node = lambda.Body;
        while (node != lambda.Parameters[0])
        {
            var holder = node switch
            {
                MemberExpression { Expression: { } member } => member,
                MethodCallExpression { Object: { } instance, Arguments.Count: 1 } => instance,
                BinaryExpression { NodeType: ExpressionType.ArrayIndex } position => position.Left,
                _ => null,
            };
            if (holder is null) return false;
            node = holder;
            steps++;
        }

        return NamesEachStep(mvcName, steps);
    }

    internal static MemberExpression? NearestMember(LambdaExpression lambda)
    {
        var node = lambda.Body;
        while (node is not MemberExpression)
        {
            switch (node)
            {
                case MethodCallExpression { Object: { } instance }:
                    node = instance;
                    break;
                case BinaryExpression { NodeType: ExpressionType.ArrayIndex } position:
                    node = position.Left;
                    break;
                default:
                    return null;
            }
        }

        return (MemberExpression)node;
    }

    internal static bool ReadsItsModel(LambdaExpression lambda)
    {
        var parameterUse = new ParameterUse(lambda.Parameters[0]);
        parameterUse.Visit(lambda.Body);
        return parameterUse.Found;
    }

    private static bool NamesEachStep(string mvcName, int steps)
    {
        var named = 0;
        var at = 0;
        while (at < mvcName.Length)
        {
            var position = mvcName[at] == '[';
            if (position || named > 0) at++;
            while (at < mvcName.Length && !EndsStep(mvcName[at], position)) at++;
            if (position) at++;
            named++;
        }

        return named == steps;
    }

    private static bool EndsStep(char character, bool position)
        => position ? character == ']' : character is '.' or '[';
}

internal sealed class ParameterUse(ParameterExpression parameter) : ExpressionVisitor
{
    internal bool Found { get; private set; }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        Found |= node == parameter;
        return node;
    }
}
