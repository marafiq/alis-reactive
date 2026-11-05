using System.Text.Json.Serialization;

namespace Alis.Reactive.Grammar
{
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
