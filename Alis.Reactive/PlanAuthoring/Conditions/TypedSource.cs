using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Preserves the property type through the condition and mutation pipeline
    /// for compile-time type safety.
    /// </summary>
    public abstract class TypedSource<TProp>
    {
        internal abstract ValueExpression ToValueExpression();

        internal Shape Shape => Shape.FromClrType(typeof(TProp));

        internal Shape ElementShape =>
            Shape.CollectionItemShapeOrNone(typeof(TProp));
    }
}
