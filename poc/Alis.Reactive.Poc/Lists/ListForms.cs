using Alis.Reactive.Plan;
using Alis.Reactive.Values;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Lists;

internal static class ListForms
{
    internal static Expression Length(Expression list)
        => new GetExpression(list, [PathStep.Member("length")], ReadPresence.RequiredIfHeld);

    internal static Expression Map(Expression list, FnExpression read) => ArrayMethod(list, "map", read);

    internal static Expression Filter(Expression list, FnExpression keep) => ArrayMethod(list, "filter", keep);

    internal static Expression Some(Expression list, FnExpression test) => ArrayMethod(list, "some", test);

    internal static Expression Every(Expression list, FnExpression test) => ArrayMethod(list, "every", test);

    internal static Expression Find(Expression list, FnExpression keep) => ArrayMethod(list, "find", keep);

    internal static FnExpression EveryItem(PlanBuild build) => new([build.ItemName()], Literal.Of(true, build));

    private static CallExpression ArrayMethod(Expression list, string method, FnExpression item)
        => new(list, [PathStep.Member(method)], ReadPresence.RequiredIfHeld, [item]);
}
