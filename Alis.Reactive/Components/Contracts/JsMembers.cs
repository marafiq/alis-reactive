using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// A property of a JavaScript object that a page reads: a source wherever the DSL takes a value,
    /// in conditions, writes, request members, arguments and arrays.
    /// </summary>
    /// <remarks>
    /// Declared on a <see cref="JsObject"/> with <c>ReadOnly&lt;T&gt;()</c>. A property the page also
    /// writes is a <see cref="Prop{TValue}"/>.
    /// <code>
    /// p.When(args.IsInteracted).Truthy()
    /// p.When(rate.Value).Gt(100m)
    /// </code>
    /// </remarks>
    /// <typeparam name="TValue">The C# type of the property's value.</typeparam>
    public class ReadOnly<TValue> : TypedSource<TValue>
    {
        internal ReadOnly(JsObject owner, ComponentProperty<TValue> member)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Member = member ?? throw new ArgumentNullException(nameof(member));
        }

        internal JsObject Owner { get; }

        internal ComponentProperty<TValue> Member { get; }

        internal override ValueExpression ToValueExpression() => Owner.Binding.Read(Member, Shape);

        /// <summary>Reads into the property's value, which is data: a row, a slot, a record.</summary>
        /// <example><c>p.When(args.Slot.Get(s =&gt; s.Quiet)).Truthy()</c></example>
        /// <typeparam name="TResult">The type of the member read.</typeparam>
        /// <param name="path">The member, by the property's own type, for example <c>s =&gt; s.Label</c>.</param>
        public TypedSource<TResult> Get<TResult>(Expression<Func<TValue, TResult>> path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            var dataPath = ExpressionPathHelper.ToEventPath(path);
            if (dataPath.Length == 0)
                throw new ArgumentException("Select a member of the value, for example s => s.Label.", nameof(path));
            return new MemberPathRead<TResult>(this, dataPath);
        }

        private sealed class MemberPathRead<TResult> : TypedSource<TResult>
        {
            private readonly ReadOnly<TValue> _property;
            private readonly string _path;

            internal MemberPathRead(ReadOnly<TValue> property, string path)
            {
                _property = property;
                _path = path;
            }

            internal override ValueExpression ToValueExpression() =>
                _property.Owner.Binding.ReadPath(_property.Member, _path, Shape);
        }
    }

    /// <summary>
    /// A property of a JavaScript object that a page reads and writes. <see cref="Set(TValue)"/> writes
    /// it in the callback the line is written in.
    /// </summary>
    /// <remarks>
    /// Declared on a <see cref="JsObject"/> with <c>Prop&lt;T&gt;()</c>.
    /// <code>
    /// rate.Min.Set(0m);
    /// rate.Value.Set(json.Get(x =&gt; x.Rate));
    /// args.Cancel.Set(true);
    /// </code>
    /// </remarks>
    /// <typeparam name="TValue">The C# type of the property's value.</typeparam>
    public sealed class Prop<TValue> : ReadOnly<TValue>
    {
        internal Prop(JsObject owner, ComponentProperty<TValue> member)
            : base(owner, member)
        {
        }

        /// <summary>Writes a value fixed when the view renders.</summary>
        /// <param name="value">The value, written into the plan.</param>
        public void Set(TValue value) =>
            Owner.Binding.Set(Member, value is IValueSource source
                ? source.ToValueExpression()
                : ValueExpression.LiteralFor(value, Member.Shape));

        /// <summary>
        /// Writes a value read when the step runs: another object's property, a method's result, an
        /// event payload, a response, the URL or an array.
        /// </summary>
        /// <remarks>
        /// The value's type must be the property's type or derive from it, so a value that may be missing
        /// (<c>int?</c>) cannot fill a property that may not (<c>int</c>).
        /// </remarks>
        /// <typeparam name="TSource">The value's type.</typeparam>
        /// <param name="source">The value.</param>
        public void Set<TSource>(TypedSource<TSource> source) where TSource : TValue
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            Owner.Binding.Set(Member, source.ToValueExpression());
        }

        internal void SetExpression(ValueExpression value) => Owner.Binding.Set(Member, value);
    }

    /// <summary>Writes to properties whose value may be missing.</summary>
    public static class PropExtensions
    {
        /// <summary>Writes a value read when the step runs to a property that may hold no value.</summary>
        /// <typeparam name="TValue">The property's underlying type.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="source">A value of the property's underlying type.</param>
        public static void Set<TValue>(this Prop<TValue?> property, TypedSource<TValue> source)
            where TValue : struct
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (source == null) throw new ArgumentNullException(nameof(source));
            property.SetExpression(source.ToValueExpression());
        }
    }

    /// <summary>
    /// One argument of a method a declaration declares: a literal, or a value read when the step runs.
    /// </summary>
    public abstract class Arg
    {
        private protected Arg(ValueExpression expression, Shape shape)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal ValueExpression Expression { get; }

        /// <summary>The shape the method declares for this argument.</summary>
        internal Shape Shape { get; }

        internal static Arg Fixed(object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var shape = Shape.FromValue(value);
            return new ExpressionArg(ValueExpression.LiteralFor(value, shape), shape);
        }

        internal static Arg Declared<TDeclared, TSource>(TypedSource<TSource> source)
            where TSource : TDeclared
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return new ExpressionArg(source.ToValueExpression(), Shape.FromClrType(typeof(TDeclared)));
        }

        private sealed class ExpressionArg : Arg
        {
            internal ExpressionArg(ValueExpression expression, Shape shape)
                : base(expression, shape)
            {
            }
        }
    }

    /// <summary>
    /// One argument of a method: a literal, or a value read when the step runs. Write either one where
    /// the method takes it; the conversion is implicit.
    /// </summary>
    /// <example>
    /// <code>
    /// planner.Book(args.Added, visitors.Value, "Bring the photo album");
    /// kanban.AddCard(json.Get(x =&gt; x.Card), 0);
    /// </code>
    /// </example>
    /// <typeparam name="T">The C# type the method declares for the argument.</typeparam>
    public sealed class Arg<T> : Arg
    {
        private Arg(ValueExpression expression)
            : base(expression, Shape.FromClrType(typeof(T)))
        {
        }

        /// <summary>A literal argument, written into the plan.</summary>
        /// <remarks>A parameter typed <c>object</c> takes any value read when the step runs, too.</remarks>
        public static implicit operator Arg<T>(T value) =>
            new Arg<T>(value is IValueSource source
                ? source.ToValueExpression()
                : ValueExpression.LiteralFor(value, Shape.FromClrType(typeof(T))));

        /// <summary>An argument read when the step runs.</summary>
        public static implicit operator Arg<T>(TypedSource<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return new Arg<T>(source.ToValueExpression());
        }
    }
}
