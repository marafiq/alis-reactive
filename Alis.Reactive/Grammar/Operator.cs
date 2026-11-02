using System.Text.Json.Serialization;

namespace Alis.Reactive.Grammar
{
    /// <summary>
    /// The closed operator table (grammar.md 2.2), each written by its camelCase name. Each operator gives
    /// the answer C# gives for the developer's expression over the converted values.
    /// </summary>
    [JsonConverter(typeof(CamelCaseEnum<Operator>))]
    internal enum Operator
    {
        Eq,
        Neq,
        Lt,
        Lte,
        Gt,
        Gte,
        And,
        Or,
        Not,
        Add,
        Sub,
        Mul,
        Mod,
        Neg,
        Coalesce,
        Cond,
        Truthy,
        Empty,
        Matches,
        ToNumber,
        ToDate,
        ToBoolean,
        ToText,
        ToArray,
        Finite,
        Order,
        Merge,
    }
}
