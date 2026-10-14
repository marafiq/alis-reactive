using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Alis.Reactive.Builders.Requests
{
    // A form input is sent by the view model's binding name. When the endpoint binds a class other than the
    // view model, that name must be a member of the class too, of a type the input's value binds to, or the
    // server drops or refuses the value. MVC matches member names ignoring case; an index steps into the
    // collection's item type.
    internal static class RequestMembers
    {
        internal static void Require(Type request, string bindingName, Type inputType)
        {
            var member = MemberType(request, bindingName);
            if (member == null || Binds(inputType, member)) return;

            throw new InvalidOperationException(
                $"Form sends the input '{bindingName}' as {inputType.Name}, but {request.Name}.{bindingName}, which " +
                $"the endpoint binds, is {member.Name}. Give the request member the input's type.");
        }

        // The member's type, or null when the path reaches an open member (object, a dictionary) that takes any name.
        private static Type? MemberType(Type request, string bindingName)
        {
            var current = request;
            foreach (var segment in bindingName.Split('.'))
            {
                var bracket = segment.IndexOf('[');
                var name = bracket < 0 ? segment : segment.Substring(0, bracket);
                var property = current.GetProperty(
                    name,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property == null)
                    throw new InvalidOperationException(
                        $"Form sends the input '{bindingName}', but {request.Name}, the request the endpoint binds, " +
                        $"has no member '{name}'{(current == request ? "" : " on " + current.Name)}.");

                current = property.PropertyType;
                var indexes = bracket < 0 ? 0 : segment.Count(character => character == '[');
                for (var index = 0; index < indexes; index++)
                    current = ItemType(current);

                if (IsOpen(current)) return null;
            }

            return current;
        }

        // The same type, a nullable form of it, a base type, or a collection of the same items. A value that
        // may be missing (int?) does not bind to a member that may not (int).
        private static bool Binds(Type input, Type member)
        {
            var missingIntoRequired = Nullable.GetUnderlyingType(input) != null && Nullable.GetUnderlyingType(member) == null;
            if (missingIntoRequired) return false;

            var inputCore = Nullable.GetUnderlyingType(input) ?? input;
            var memberCore = Nullable.GetUnderlyingType(member) ?? member;
            if (memberCore.IsAssignableFrom(inputCore)) return true;

            var bothCollections = IsCollection(inputCore) && IsCollection(memberCore);
            return bothCollections && ItemType(inputCore) == ItemType(memberCore);
        }

        private static bool IsCollection(Type type) =>
            type != typeof(string) && (type.IsArray || type.GetInterfaces().Any(candidate =>
                candidate.IsGenericType && candidate.GetGenericTypeDefinition() == typeof(IEnumerable<>)));

        // A member typed as object or a dictionary takes any name below it.
        private static bool IsOpen(Type type) =>
            type == typeof(object) || DictionaryValueType(type) != null;

        private static Type ItemType(Type collection)
        {
            if (collection.IsArray) return collection.GetElementType()!;
            var dictionaryValue = DictionaryValueType(collection);
            if (dictionaryValue != null) return dictionaryValue;

            var enumerable = collection.IsGenericType && collection.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                ? collection
                : collection.GetInterfaces().FirstOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            return enumerable?.GetGenericArguments()[0] ?? typeof(object);
        }

        private static Type? DictionaryValueType(Type type)
        {
            var dictionary = new[] { type }.Concat(type.GetInterfaces()).FirstOrDefault(candidate =>
                candidate.IsGenericType
                && (candidate.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                    || candidate.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)));
            return dictionary?.GetGenericArguments()[1];
        }
    }
}
