using Alis.Reactive.Schema;
using PlanExpression = Alis.Reactive.Schema.Expression;

namespace Alis.Reactive.ArrayLambdas;

internal static class ListForms
{
    private const string Filtered = "filter";
    private const string Mapped = "map";
    private const string Some = "some";
    private const string Every = "every";
    private const string First = "find";
    private const string Count = "length";
    private const string Sorted = "toSorted";

    internal static GetExpression Length(PlanExpression list) => new([new MemberStep(Count)], PathMembers.RequiredIfHeld, list);

    internal static CallExpression Filter(PlanExpression list, FnExpression keep) => Called(Filtered, list, keep);

    internal static CallExpression Map(PlanExpression list, FnExpression field) => Called(Mapped, list, field);

    internal static CallExpression AnyOf(PlanExpression list, FnExpression predicate) => Called(Some, list, predicate);

    internal static CallExpression AllOf(PlanExpression list, FnExpression predicate) => Called(Every, list, predicate);

    internal static CallExpression Find(PlanExpression list, FnExpression predicate) => Called(First, list, predicate);

    internal static CallExpression ToSorted(PlanExpression list, FnExpression comparison) => Called(Sorted, list, comparison);

    private static CallExpression Called(string method, PlanExpression list, FnExpression fn) => new([new MemberStep(method)], PathMembers.RequiredIfHeld, list, [fn]);
}
