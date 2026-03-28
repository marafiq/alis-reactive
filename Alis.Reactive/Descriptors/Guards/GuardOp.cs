namespace Alis.Reactive.Descriptors.Guards
{
    /// <summary>
    /// String constants for the <c>op</c> field in <see cref="ValueGuard"/>. Each constant
    /// maps to a comparison operator evaluated by the JavaScript runtime conditions engine.
    /// </summary>
    /// <remarks>
    /// Values are serialized directly into the JSON plan as the <c>op</c> property of a
    /// <c>value</c> guard. The runtime resolves the left-hand source, coerces it, then
    /// applies the operator against the operand (or right-hand source for source-vs-source guards).
    /// </remarks>
    public static class GuardOp
    {
        // -- Comparison --

        /// <summary>Equals: source value must equal the operand.</summary>
        public const string Eq = "eq";

        /// <summary>Not equals: source value must differ from the operand.</summary>
        public const string Neq = "neq";

        /// <summary>Greater than: source value must be strictly greater than the operand.</summary>
        public const string Gt = "gt";

        /// <summary>Greater than or equal: source value must be at least the operand.</summary>
        public const string Gte = "gte";

        /// <summary>Less than: source value must be strictly less than the operand.</summary>
        public const string Lt = "lt";

        /// <summary>Less than or equal: source value must be at most the operand.</summary>
        public const string Lte = "lte";

        // -- Presence --

        /// <summary>Truthy: source value is truthy (non-null, non-zero, non-empty string).</summary>
        public const string Truthy = "truthy";

        /// <summary>Falsy: source value is falsy (null, zero, or empty string).</summary>
        public const string Falsy = "falsy";

        /// <summary>Is null: source value is <see langword="null"/> or <c>undefined</c>.</summary>
        public const string IsNull = "is-null";

        /// <summary>Not null: source value is neither <see langword="null"/> nor <c>undefined</c>.</summary>
        public const string NotNull = "not-null";

        /// <summary>Is empty: source value is an empty string or empty array.</summary>
        public const string IsEmpty = "is-empty";

        /// <summary>Not empty: source value is a non-empty string or non-empty array.</summary>
        public const string NotEmpty = "not-empty";

        // -- Membership --

        /// <summary>In: source value is contained within the operand array.</summary>
        public const string In = "in";

        /// <summary>Not in: source value is not contained within the operand array.</summary>
        public const string NotIn = "not-in";

        // -- Range --

        /// <summary>Between: source value falls within the inclusive range defined by the operand pair.</summary>
        public const string Between = "between";

        // -- Array --

        /// <summary>Array contains: the source array contains the operand value.</summary>
        public const string ArrayContains = "array-contains";

        // -- Text --

        /// <summary>Contains: source string contains the operand substring.</summary>
        public const string Contains = "contains";

        /// <summary>Starts with: source string begins with the operand prefix.</summary>
        public const string StartsWith = "starts-with";

        /// <summary>Ends with: source string ends with the operand suffix.</summary>
        public const string EndsWith = "ends-with";

        /// <summary>Matches: source string matches the operand regular expression.</summary>
        public const string Matches = "matches";

        /// <summary>Min length: source string length is at least the operand value.</summary>
        public const string MinLength = "min-length";
    }
}
