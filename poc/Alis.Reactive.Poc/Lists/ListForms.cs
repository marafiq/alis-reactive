using Alis.Reactive.Plan;

namespace Alis.Reactive.Lists;

internal static class ListForms
{
    internal static Expression Length(Expression list)
        => new GetExpression(list, [PathStep.Member("length")], ReadPresence.BuiltIn);

    internal static Expression Map(Expression list, FnExpression read) => ArrayMethod(list, "map", read);

    internal static Expression Filter(Expression list, FnExpression keep) => ArrayMethod(list, "filter", keep);

    private static CallExpression ArrayMethod(Expression list, string method, FnExpression item)
        => new(list, [PathStep.Member(method)], ReadPresence.BuiltIn, [item]);
}
