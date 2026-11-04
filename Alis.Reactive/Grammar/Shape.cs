using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Grammar
{
    [JsonConverter(typeof(PlanNodeDiscriminator<Shape>))]
    internal abstract class Shape : IEquatable<Shape>
    {
        internal static readonly Shape String = new StringShape();
        internal static readonly Shape Number = new NumberShape();
        internal static readonly Shape Boolean = new BooleanShape();
        internal static readonly Shape Date = new DateShape();
        internal static readonly Shape Instant = new InstantShape();
        internal static readonly Shape Any = new AnyShape();

        private protected Shape() { }

        public abstract string Kind { get; }

        internal bool DescribesArray => this is NullableShape nullable ? nullable.Inner.DescribesArray : this is ArrayShape;

        internal bool IsScalar => this switch
        {
            NullableShape nullable => nullable.Inner.IsScalar,
            StringShape or NumberShape or BooleanShape or DateShape or InstantShape => true,
            _ => false,
        };

        internal static Shape ArrayOf(Shape item) => new ArrayShape(Wrappable(item, nameof(item)));

        internal static Shape Nullable(Shape inner) => new NullableShape(Wrappable(inner, nameof(inner)));

        internal static Shape ObjectOf(Dictionary<string, Shape> fields)
        {
            if (fields is null) throw new ArgumentNullException(nameof(fields));
            return new ObjectShape(new ReadOnlyDictionary<string, Shape>(fields));
        }

        private static Shape Wrappable(Shape shape, string parameter) =>
            shape ?? throw new ArgumentNullException(parameter);

        internal bool TryGetArrayItemShape(out Shape item)
        {
            item = this is ArrayShape array ? array.Item : Any;
            return this is ArrayShape;
        }

        internal bool TryGetNullableInnerShape(out Shape inner)
        {
            inner = this is NullableShape nullable ? nullable.Inner : Any;
            return this is NullableShape;
        }

        internal string DescribeContract() => this switch
        {
            ArrayShape array => "array<" + array.Item.DescribeContract() + ">",
            NullableShape nullable => "nullable<" + nullable.Inner.DescribeContract() + ">",
            ObjectShape record => "object{" + string.Join(", ", record.Fields.Select(field => field.Key + ":" + field.Value.DescribeContract())) + "}",
            _ => Kind,
        };

        public bool Equals(Shape? other) => other switch
        {
            null => false,
            ArrayShape array => this is ArrayShape mine && mine.Item.Equals(array.Item),
            NullableShape nullable => this is NullableShape mine && mine.Inner.Equals(nullable.Inner),
            ObjectShape record => this is ObjectShape mine && SameFields(mine.Fields, record.Fields),
            _ => Kind == other.Kind,
        };

        private static bool SameFields(IReadOnlyDictionary<string, Shape> mine, IReadOnlyDictionary<string, Shape> theirs) =>
            mine.Count == theirs.Count
            && mine.All(field => theirs.TryGetValue(field.Key, out var shape) && field.Value.Equals(shape));

        public override bool Equals(object? obj) => Equals(obj as Shape);

        public override int GetHashCode() => this switch
        {
            ArrayShape array => unchecked(StringComparer.Ordinal.GetHashCode(Kind) * 31 + array.Item.GetHashCode()),
            NullableShape nullable => unchecked(StringComparer.Ordinal.GetHashCode(Kind) * 31 + nullable.Inner.GetHashCode()),
            _ => StringComparer.Ordinal.GetHashCode(Kind),
        };

        public static bool operator ==(Shape? left, Shape? right) => Equals(left, right);
        public static bool operator !=(Shape? left, Shape? right) => !Equals(left, right);
    }

    internal sealed class StringShape : Shape
    {
        public override string Kind => "string";
    }

    internal sealed class NumberShape : Shape
    {
        public override string Kind => "number";
    }

    internal sealed class BooleanShape : Shape
    {
        public override string Kind => "boolean";
    }

    internal sealed class DateShape : Shape
    {
        public override string Kind => "date";
    }

    internal sealed class InstantShape : Shape
    {
        public override string Kind => "instant";
    }

    internal sealed class AnyShape : Shape
    {
        public override string Kind => "any";
    }

    internal sealed class ArrayShape : Shape
    {
        internal ArrayShape(Shape item)
        {
            Item = item;
        }

        public override string Kind => "array";
        public Shape Item { get; }
    }

    internal sealed class NullableShape : Shape
    {
        internal NullableShape(Shape inner)
        {
            Inner = inner;
        }

        public override string Kind => "nullable";
        public Shape Inner { get; }
    }

    internal sealed class ObjectShape : Shape
    {
        internal ObjectShape(IReadOnlyDictionary<string, Shape> fields)
        {
            Fields = fields;
        }

        public override string Kind => "object";
        public IReadOnlyDictionary<string, Shape> Fields { get; }
    }
}
