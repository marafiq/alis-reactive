using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<Condition>))]
    internal abstract class Condition
    {
        private protected Condition() { }

        internal static Condition Compare(ValueProducer left, string op, ValueProducer right = null, Shape shape = null, Shape itemShape = null) =>
            new CompareCondition(left, op, right, shape, itemShape);

        internal static Condition All(params Condition[] terms) =>
            new AllCondition(new List<Condition>(terms));

        internal static Condition Any(params Condition[] terms) =>
            new AnyCondition(new List<Condition>(terms));

        internal static Condition Not(Condition term) =>
            new NotCondition(term);

        internal static Condition Confirm(string message) =>
            new ConfirmCondition(message);
    }

    internal sealed class CompareCondition : Condition
    {
        public string Kind => "compare";
        public ValueProducer Left { get; }
        public string Op { get; }
        public ValueProducer Right { get; }
        public Shape Shape { get; }
        public Shape ItemShape { get; }

        internal CompareCondition(ValueProducer left, string op, ValueProducer right, Shape shape, Shape itemShape)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Op = op ?? throw new ArgumentNullException(nameof(op));
            Right = right;
            Shape = shape == null || shape.IsNone ? null : shape;
            ItemShape = itemShape == null || itemShape.IsNone ? null : itemShape;
        }
    }

    internal sealed class AllCondition : Condition
    {
        public string Kind => "all";
        public List<Condition> Terms { get; }

        internal AllCondition(List<Condition> terms)
        {
            Terms = terms ?? throw new ArgumentNullException(nameof(terms));
        }
    }

    internal sealed class AnyCondition : Condition
    {
        public string Kind => "any";
        public List<Condition> Terms { get; }

        internal AnyCondition(List<Condition> terms)
        {
            Terms = terms ?? throw new ArgumentNullException(nameof(terms));
        }
    }

    internal sealed class NotCondition : Condition
    {
        public string Kind => "not";
        public Condition Term { get; }

        internal NotCondition(Condition term)
        {
            Term = term ?? throw new ArgumentNullException(nameof(term));
        }
    }

    internal sealed class ConfirmCondition : Condition
    {
        public string Kind => "confirm";
        public string Message { get; }

        internal ConfirmCondition(string message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
}
