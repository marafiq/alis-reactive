using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Members;

namespace Alis.Reactive.Validation
{
    internal sealed class ModelMember<T>
    {
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

        internal LambdaExpression Expression { get; }

        internal string? Path { get; }

        internal string Name { get; }

        internal Type Type { get; }

        internal string Written { get; }

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
