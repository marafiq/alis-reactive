using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// A member of the model a rule reads, from the expression that names it: <c>x =&gt; x.Address.City</c> is
    /// the path <c>Address.City</c>. An expression that computes a value (<c>x =&gt; DateTime.Today</c>) has
    /// no path and no input on the page: a comparison may read one on the server, and a rule refuses one.
    /// </summary>
    internal sealed class ModelMember<T>
    {
        // A member path reads the same way in every validator of T, so it is compiled once; a computed
        // value may hold a closure, so it is compiled where it is written.
        private static readonly ConcurrentDictionary<string, Func<T, object?>> PathReaders =
            new ConcurrentDictionary<string, Func<T, object?>>(StringComparer.Ordinal);

        private readonly Func<T, object?> _read;

        private ModelMember(LambdaExpression expression, string? path, string name, Type type, Func<T, object?> read, string written)
        {
            Expression = expression;
            Path = path;
            Name = name;
            Type = type;
            _read = read;
            Written = written;
        }

        /// <summary>The member as the validator wrote it: the path a browser rule names it by.</summary>
        internal LambdaExpression Expression { get; }

        /// <summary>The member's path from the model, as model binding names it; null for a computed value.</summary>
        internal string? Path { get; }

        /// <summary>The member's own name: the last step of its path.</summary>
        internal string Name { get; }

        /// <summary>The member's type.</summary>
        internal Type Type { get; }

        /// <summary>The expression as written: <c>x =&gt; x.Address.City</c>.</summary>
        internal string Written { get; }

        // A member read through a null (x => x.Pharmacy.Phone with no pharmacy) is a rule written without a
        // guard; the error names the rule's expression.
        internal object? ValueIn(T instance)
        {
            try
            {
                return _read(instance);
            }
            catch (NullReferenceException missing)
            {
                throw new NullReferenceException(
                    $"A rule read {Written} through a null. If a member on the way can be missing, guard the rule with When(x => x.Member != null, ...).",
                    missing);
            }
        }

        internal static ModelMember<T> Of<TValue>(Expression<Func<T, TValue>> expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            var body = Expressions.StripConvert(expression.Body);
            var path = PathOf(body, expression.Parameters[0]);
            var name = body is MemberExpression member ? member.Member.Name : "";
            var read = path == null ? Reader(expression) : PathReaders.GetOrAdd(path, _ => Reader(expression));
            return new ModelMember<T>(expression, path, name, body.Type, read, expression.ToString());
        }

        private static Func<T, object?> Reader<TValue>(Expression<Func<T, TValue>> expression)
        {
            var compiled = expression.Compile();
            return instance => compiled(instance);
        }

        // x.A.B is "A.B"; anything but a chain of members from the model parameter has no path.
        private static string? PathOf(Expression body, ParameterExpression model)
        {
            var names = new List<string>();
            var current = body;
            while (current is MemberExpression member && member.Expression != null)
            {
                names.Insert(0, member.Member.Name);
                current = Expressions.StripConvert(member.Expression);
            }
            return current == model && names.Count > 0 ? string.Join(".", names) : null;
        }
    }
}
