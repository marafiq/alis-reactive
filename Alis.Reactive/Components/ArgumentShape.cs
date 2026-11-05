using System;
using System.Collections.Generic;
using Alis.Reactive.Grammar;
using Alis.Reactive.Members;

namespace Alis.Reactive.Components
{
    internal static class ArgumentShape
    {
        internal static Shape Of(Type type) => Of(type, new HashSet<Type>());

        private static Shape Of(Type type, HashSet<Type> enclosingRecords)
            => ClrShape.Of(type, candidate =>
                IsDataClass(candidate) && enclosingRecords.Add(candidate) ? RecordOf(candidate, enclosingRecords) : Shape.Any);

        private static bool IsDataClass(Type type) => ClrShape.IsDataClass(type) && !typeof(JsObject).IsAssignableFrom(type);

        private static Shape RecordOf(Type type, HashSet<Type> enclosingRecords)
        {
            var fields = new Dictionary<string, Shape>(StringComparer.Ordinal);
            foreach (var property in ClrShape.SerializedProperties(type))
                fields[JsonMemberName.Of(property)] = Of(property.PropertyType, enclosingRecords);
            enclosingRecords.Remove(type);
            return Shape.ObjectOf(fields);
        }
    }
}
