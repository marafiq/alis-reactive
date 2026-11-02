using System;
using System.Collections.Generic;
using Alis.Reactive.Grammar;
using Alis.Reactive.Members;

namespace Alis.Reactive.Components
{
    /// <summary>
    /// The shape a declaration gives a property or a method argument of a CLR type: as <see cref="ClrShape.Of(Type)"/>,
    /// except that a class of the app's own data (a server's class, a row, an event) is an object of its members by
    /// their JSON names, so a date member reaches the method as a <c>Date</c> (grammar.md 1.4, 2.1: a call argument
    /// converts to the shape the member declares). A live JavaScript object (a declaration), a framework or library
    /// type and a dictionary are <c>any</c>; a class that contains itself is <c>any</c> where it recurs.
    /// </summary>
    internal static class ArgumentShape
    {
        internal static Shape Of(Type type) => Of(type, new HashSet<Type>());

        private static Shape Of(Type type, HashSet<Type> enclosingRecords) =>
            ClrShape.Of(type, candidate =>
                IsDataClass(candidate) && enclosingRecords.Add(candidate) ? RecordOf(candidate, enclosingRecords) : Shape.Any);

        // A declaration is a live JavaScript object, not data.
        private static bool IsDataClass(Type type) => ClrShape.IsDataClass(type) && !typeof(JsObject).IsAssignableFrom(type);

        private static Shape RecordOf(Type type, HashSet<Type> enclosingRecords)
        {
            var fields = new Dictionary<string, Shape>(StringComparer.Ordinal);
            foreach (var property in ClrShape.SerializedProperties(type))
                fields[JsonMemberName.Of(property.Name)] = Of(property.PropertyType, enclosingRecords);
            enclosingRecords.Remove(type);
            return Shape.ObjectOf(fields);
        }
    }
}
