using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// A property the page reads. It is a source wherever the DSL takes one: a reaction value, a condition,
    /// a gather value, a route parameter, a header, a dispatch payload or a plugin argument.
    /// </summary>
    /// <typeparam name="TValue">The C# type of the value.</typeparam>
    public class ReadOnly<TValue> : TypedSource<TValue>
    {
        internal ReadOnly(JsObject owner, ComponentProperty<TValue> member)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Member = member ?? throw new ArgumentNullException(nameof(member));
        }

        internal JsObject Owner { get; }

        internal ComponentProperty<TValue> Member { get; }

        internal override ValueExpression Expression => Owner.Binding.Read(Member.Path, Shape);

        /// <summary>Reads a member of the value, for a value that is data: <c>args.Slot.Get(s =&gt; s.Label)</c>.</summary>
        /// <typeparam name="TResult">The C# type of the member.</typeparam>
        /// <param name="path">The member to read, for example <c>s =&gt; s.Label</c>.</param>
        public TypedSource<TResult> Get<TResult>(Expression<Func<TValue, TResult>> path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            var dataPath = ExpressionPathHelper.ToJsonPath(path);
            if (dataPath.IsNone)
                throw new ArgumentException("Select a member of the value, for example s => s.Label.", nameof(path));
            return new MemberRead<TResult>(Owner.Binding.Locate, Member.Path.Append(dataPath));
        }
    }

    /// <summary>A property the page reads and writes: <c>rate.Min.Set(0m)</c>, <c>p.When(rate.Min).Gt(0m)</c>.</summary>
    /// <typeparam name="TValue">The C# type of the value.</typeparam>
    public sealed class Property<TValue> : ReadOnly<TValue>
    {
        internal Property(JsObject owner, ComponentProperty<TValue> member)
            : base(owner, member)
        {
        }

        /// <summary>Writes a literal.</summary>
        /// <param name="value">The value to write.</param>
        public void Set(TValue value) =>
            Owner.Binding.Set(Member, value is Arg source
                ? source.Expression
                : ValueExpression.LiteralFor(value, Member.Shape));

        /// <summary>Writes a value read when the step runs.</summary>
        /// <typeparam name="TSource">The type of the value read, assignable to the property.</typeparam>
        /// <param name="source">The value read when the step runs.</param>
        public void Set<TSource>(TypedSource<TSource> source) where TSource : TValue
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            Owner.Binding.Set(Member, source.Expression);
        }

        internal void SetExpression(ValueExpression value) => Owner.Binding.Set(Member, value);
    }

    /// <summary>
    /// A property the page writes and never reads: the JavaScript object has a setter and no getter, such
    /// as a code that unlocks it. It is not a source, so a read of it does not compile.
    /// </summary>
    /// <typeparam name="TValue">The C# type of the value.</typeparam>
    public sealed class WriteOnly<TValue>
    {
        private readonly JsObject _owner;
        private readonly ComponentProperty<TValue> _member;

        internal WriteOnly(JsObject owner, ComponentProperty<TValue> member)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _member = member ?? throw new ArgumentNullException(nameof(member));
        }

        /// <summary>Writes a literal.</summary>
        /// <param name="value">The value to write.</param>
        public void Set(TValue value) =>
            _owner.Binding.Set(_member, value is Arg source
                ? source.Expression
                : ValueExpression.LiteralFor(value, _member.Shape));

        /// <summary>Writes a value read when the step runs.</summary>
        /// <typeparam name="TSource">The type of the value read, assignable to the property.</typeparam>
        /// <param name="source">The value read when the step runs.</param>
        public void Set<TSource>(TypedSource<TSource> source) where TSource : TValue
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            _owner.Binding.Set(_member, source.Expression);
        }
    }

    /// <summary>
    /// A list of objects with members of their own, held by a property or returned by a method:
    /// <c>thermostat.Zones.At(1).Boost()</c>, <c>p.When(thermostat.Zones.Count).Gt(1)</c>,
    /// <c>p.When(thermostat.Zones.Each(zone =&gt; zone.Name)).ArrayContains("Bath")</c>.
    /// </summary>
    /// <typeparam name="TItem">The declaration of each item.</typeparam>
    public sealed class Objects<TItem>
        where TItem : JsObject, new()
    {
        private readonly JsObject _owner;
        private readonly Path _path;

        internal Objects(JsObject owner, Path path)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        /// <summary>The item at a position, used as the object: <c>thermostat.Zones.At(0).Target.Set(70)</c>.</summary>
        /// <param name="index">The item's zero-based position.</param>
        public TItem At(int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), index, "An item's position is zero or more.");
            return JsObject.Nested<TItem>(_owner, _path.AtIndex(index));
        }

        /// <summary>How many items the list has: its <c>length</c>.</summary>
        public ReadOnly<int> Count => new ReadOnly<int>(_owner, ComponentProperty<int>.At(_path.Then("length")));

        /// <summary>A property of each item, as a list: <c>thermostat.Zones.Each(zone =&gt; zone.Name)</c>.</summary>
        /// <typeparam name="TField">The C# type of the property.</typeparam>
        /// <param name="property">The property to read of each item, for example <c>zone =&gt; zone.Name</c>.</param>
        public ReadOnly<IReadOnlyList<TField>> Each<TField>(Func<TItem, ReadOnly<TField>> property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            var eachItem = JsObject.Nested<TItem>(_owner, _path.Each());
            var member = property(eachItem)
                ?? throw new ArgumentException("Select a property of the item, for example zone => zone.Name.", nameof(property));
            return new ReadOnly<IReadOnlyList<TField>>(_owner, ComponentProperty<IReadOnlyList<TField>>.At(member.Member.Path));
        }
    }

    /// <summary>Reads a field of each item of a list property.</summary>
    public static class ListPropertyExtensions
    {
        /// <summary>A field of each item, as a list: <c>planner.Slots.Each(slot =&gt; slot.Day)</c> reads every slot's <c>day</c>.</summary>
        /// <typeparam name="TItem">The C# type of an item.</typeparam>
        /// <typeparam name="TField">The C# type of the field.</typeparam>
        /// <param name="list">The list property.</param>
        /// <param name="field">The field to read of each item.</param>
        public static ReadOnly<IReadOnlyList<TField>> Each<TItem, TField>(
            this ReadOnly<IReadOnlyList<TItem>> list,
            Expression<Func<TItem, TField>> field)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (field == null) throw new ArgumentNullException(nameof(field));
            var fieldPath = ExpressionPathHelper.ToJsonPath(field);
            if (fieldPath.IsNone)
                throw new ArgumentException("Select a field of the item, for example slot => slot.Day.", nameof(field));
            return new ReadOnly<IReadOnlyList<TField>>(
                list.Owner,
                ComponentProperty<IReadOnlyList<TField>>.At(list.Member.Path.Each().Append(fieldPath)));
        }
    }

    /// <summary>Writes a nullable property from a value that is never null.</summary>
    public static class PropertyExtensions
    {
        /// <summary>Writes a value read when the step runs into a nullable property.</summary>
        /// <typeparam name="TValue">The C# type of the value.</typeparam>
        /// <param name="property">The nullable property.</param>
        /// <param name="source">The value read when the step runs.</param>
        public static void Set<TValue>(this Property<TValue?> property, TypedSource<TValue> source)
            where TValue : struct
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (source == null) throw new ArgumentNullException(nameof(source));
            property.SetExpression(source.Expression);
        }
    }

    /// <summary>
    /// An argument of a declared method: a literal, a value read when the step runs, or a field.
    /// </summary>
    public abstract class Arg
    {
        private protected Arg() { }

        internal abstract ValueExpression Expression { get; }

        internal abstract Shape Shape { get; }

        internal static Arg Fixed(object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return new DeclaredArg(value, Shape.FromValue(value));
        }

        internal static Arg Declared<TDeclared, TSource>(TypedSource<TSource> source)
            where TSource : TDeclared
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return new DeclaredArg(source, Shape.OfArgument(typeof(TDeclared)));
        }

        /// <summary>A field is passed as its JavaScript name.</summary>
        /// <param name="field">The field.</param>
        public static implicit operator Arg(Field field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            return new DeclaredArg(field, Shape.FromValue(field));
        }

        // A literal, or a value read when the step runs, in the shape the declaration gives it.
        private protected static ValueExpression ExpressionOf(object? value, Shape shape) =>
            value is Arg source ? source.Expression : ValueExpression.LiteralFor(value, shape);

        private sealed class DeclaredArg : Arg
        {
            private readonly object _value;

            internal DeclaredArg(object value, Shape shape)
            {
                _value = value;
                Shape = shape;
            }

            internal override ValueExpression Expression => ExpressionOf(_value, Shape);

            internal override Shape Shape { get; }
        }
    }

    /// <summary>
    /// A typed argument of a declared method. A literal converts to it, and a <see cref="TypedSource{T}"/>
    /// is one, so literals and runtime values mix: <c>grid.SelectRow(0)</c>, <c>grid.SelectRow(args.RowIndex)</c>.
    /// </summary>
    /// <typeparam name="T">The C# type the JavaScript method takes.</typeparam>
    public abstract class Arg<T> : Arg
    {
        private protected Arg() { }

        internal override Shape Shape => Shape.OfArgument(typeof(T));

        /// <summary>A literal is passed as it is.</summary>
        /// <param name="value">The literal.</param>
        public static implicit operator Arg<T>(T value) => new Literal(value);

        private sealed class Literal : Arg<T>
        {
            private readonly T _value;

            internal Literal(T value)
            {
                _value = value;
            }

            internal override ValueExpression Expression => ExpressionOf(_value, Shape);
        }
    }
}
