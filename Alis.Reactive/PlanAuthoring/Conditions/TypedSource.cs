using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// A value read when a step runs, typed by C#: an object's property, what a method returns, an event's
    /// payload, a response, the URL or an array. It goes wherever the DSL takes a value.
    /// </summary>
    /// <typeparam name="TProp">The value's type.</typeparam>
    public abstract class TypedSource<TProp> : IValueSource
    {
        internal abstract ValueExpression ToValueExpression();

        ValueExpression IValueSource.ToValueExpression() => ToValueExpression();

        internal Shape Shape => Shape.FromClrType(typeof(TProp));

        internal Shape ElementShape =>
            Shape.CollectionItemShapeOrNone(typeof(TProp));
    }

    /// <summary>A value read when a step runs, whatever its type: a literal of a type that takes anything is read as the value it is.</summary>
    internal interface IValueSource
    {
        ValueExpression ToValueExpression();
    }
}
