using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    // A form input is sent by its binding name, under the view's field prefix. When the endpoint binds a class
    // other than the view model, that name must be a member of the class too, of a type the input's value binds
    // to, or the server drops or refuses the value. MVC matches member names ignoring case; a position steps
    // into the collection's item type.
    internal static class RequestMembers
    {
        internal static void Require(Type request, BindingPath binding, Type inputType)
        {
            var member = MemberType(request, binding);
            if (member == null || Binds(inputType, member)) return;

            throw new InvalidOperationException(
                $"Form sends the input '{binding.Value}' as {Spelled(inputType)}, but {Spelled(request)}.{binding.Value}, which " +
                $"the endpoint binds, is {Spelled(member)}. Give the request member the input's type.");
        }

        // A type as C# writes it, so a message names Int32? and List<IFormFile> rather than Nullable`1 and List`1.
        private static string Spelled(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type);
            if (underlying != null) return Spelled(underlying) + "?";
            if (!type.IsGenericType) return type.Name;
            // A class nested in a generic one is generic without a backtick of its own.
            var arity = type.Name.IndexOf('`');
            var name = arity < 0 ? type.Name : type.Name.Substring(0, arity);
            return $"{name}<{string.Join(", ", type.GetGenericArguments().Select(Spelled))}>";
        }

        // The member's type, or null when the path reaches an open member (object, a dictionary) that takes any name.
        private static Type? MemberType(Type request, BindingPath binding)
        {
            var current = request;
            foreach (var segment in binding.Path.Segments)
            {
                if (segment.IsIndex || segment.IsEachItem)
                {
                    current = ItemType(current);
                }
                else
                {
                    var name = segment.ToPathText();
                    var property = current.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                        ?? throw new InvalidOperationException(
                            $"Form sends the input '{binding.Value}', but {Spelled(request)}, the request the endpoint binds, " +
                            $"has no member '{name}'{(current == request ? "" : " on " + Spelled(current))}.");
                    current = property.PropertyType;
                }

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
            type != typeof(string) && (type.IsArray || SequenceOf(type) != null);

        // A member typed as object or a dictionary takes any name below it.
        private static bool IsOpen(Type type) =>
            type == typeof(object) || DictionaryValueType(type) != null;

        private static Type ItemType(Type collection)
        {
            if (collection.IsArray) return collection.GetElementType()!;
            var dictionaryValue = DictionaryValueType(collection);
            if (dictionaryValue != null) return dictionaryValue;

            return SequenceOf(collection)?.GetGenericArguments()[0] ?? typeof(object);
        }

        // The IEnumerable<T> a type is or implements: an interface does not list itself among its interfaces.
        private static Type? SequenceOf(Type type) =>
            new[] { type }.Concat(type.GetInterfaces()).FirstOrDefault(candidate =>
                candidate.IsGenericType && candidate.GetGenericTypeDefinition() == typeof(IEnumerable<>));

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
