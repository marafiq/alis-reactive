using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>A member of the validator's model: its path, the class that declares it, and the shape of its value.</summary>
    internal sealed class FieldReference
    {
        private FieldReference(string path, Type declarer, Type type, Shape shape)
        {
            Path = path;
            Declarer = declarer;
            Type = type;
            Shape = shape;
        }

        internal string Path { get; }

        /// <summary>The class that declares the member, whose scope its input's id takes wherever the member is placed.</summary>
        internal Type Declarer { get; }

        internal Type Type { get; }

        internal Shape Shape { get; }

        // x => x.Address.City is "Address.City", typed string, declared by Address. Anything but a chain of
        // members from the model's parameter names no member.
        internal static FieldReference Of(LambdaExpression member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            var body = Expressions.StripConvert(member.Body);
            if (member.Parameters.Count != 1 || !ExpressionPathHelper.TryMemberName(body, member.Parameters[0], out var name))
                throw new ArgumentException(
                    $"A browser rule names a member of the model, like x => x.Address.City; {member} does not.", nameof(member));
            return new FieldReference(name, ExpressionPathHelper.ToDeclarer(member), body.Type, Shape.FromClrType(body.Type));
        }

        /// <summary>
        /// The item of a collection member itself, with no path of its own: the member's element, whose input
        /// (<c>Tags[0]</c>) binds the collection's member.
        /// </summary>
        internal static FieldReference ItemOf(FieldReference collection)
        {
            var itemType = ItemTypeOf(collection.Type)
                ?? throw new ArgumentException($"'{collection.Path}' is not a collection.", nameof(collection));
            return new FieldReference("", collection.Declarer, itemType, Shape.FromClrType(itemType));
        }

        internal FieldReference PrefixedBy(string prefix) =>
            prefix.Length == 0 ? this
            : Path.Length == 0 ? new FieldReference(prefix, Declarer, Type, Shape)
            : new FieldReference(prefix + "." + Path, Declarer, Type, Shape);

        private static Type? ItemTypeOf(Type type)
        {
            if (type.IsArray) return type.GetElementType();
            foreach (var candidate in new[] { type }.Concat(type.GetInterfaces()))
                if (candidate.IsGenericType && candidate.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    return candidate.GetGenericArguments()[0];
            return null;
        }
    }
}
