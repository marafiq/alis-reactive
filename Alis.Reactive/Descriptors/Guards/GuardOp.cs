namespace Alis.Reactive.Descriptors.Guards
{
    public static class GuardOp
    {
        // Comparison
        public const string Eq = "eq";
        public const string Neq = "neq";
        public const string Gt = "gt";
        public const string Gte = "gte";
        public const string Lt = "lt";
        public const string Lte = "lte";

        // Presence
        public const string Truthy = "truthy";
        public const string Falsy = "falsy";
        public const string IsNull = "is-null";
        public const string NotNull = "not-null";
        public const string IsEmpty = "is-empty";
        public const string NotEmpty = "not-empty";

        // Membership
        public const string In = "in";
        public const string NotIn = "not-in";

        // Range
        public const string Between = "between";

        // Text
        public const string Contains = "contains";
        public const string StartsWith = "starts-with";
        public const string EndsWith = "ends-with";
        public const string Matches = "matches";
        public const string MinLength = "min-length";
    }
}
