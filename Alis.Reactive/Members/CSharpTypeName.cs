using System;
using System.Linq;

namespace Alis.Reactive.Members
{
    internal static class CSharpTypeName
    {
        internal static string Of(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type);
            if (underlying != null) return Of(underlying) + "?";
            if (type.IsArray) return Of(type.GetElementType()!) + "[" + new string(',', type.GetArrayRank() - 1) + "]";
            if (!type.IsGenericType) return type.Name;
            // A class nested in a generic one is generic without a backtick of its own.
            var arity = type.Name.IndexOf('`');
            var name = arity < 0 ? type.Name : type.Name.Substring(0, arity);
            return $"{name}<{string.Join(", ", type.GetGenericArguments().Select(Of))}>";
        }
    }
}
