using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Preserves the property type through condition and reaction authoring
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
