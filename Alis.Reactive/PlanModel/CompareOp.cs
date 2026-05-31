namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// String constants for all compare-condition operators.
    /// Prevents typos when building conditions — callers use
    /// <c>CompareOperator.Eq</c> where a domain value object is available.
    /// The constants remain the JSON tokens shared with generated TypeScript.
    /// </summary>
    internal static class CompareOp
    {
        // Equality
        internal const string Eq = "eq";
        internal const string Neq = "neq";

        // Ordering
        internal const string Gt = "gt";
        internal const string Gte = "gte";
        internal const string Lt = "lt";
        internal const string Lte = "lte";

        // Presence
        internal const string Truthy = "truthy";
        internal const string Falsy = "falsy";
        internal const string IsNull = "is-null";
        internal const string NotNull = "not-null";
        internal const string IsEmpty = "is-empty";
        internal const string NotEmpty = "not-empty";

        // Membership
        internal const string In = "in";
        internal const string NotIn = "not-in";
        internal const string Between = "between";

        // Text
        internal const string Contains = "contains";
        internal const string StartsWith = "starts-with";
        internal const string EndsWith = "ends-with";
        internal const string Matches = "matches";
        internal const string MinLength = "min-length";

        // Array
        internal const string ArrayContains = "array-contains";
    }
}
