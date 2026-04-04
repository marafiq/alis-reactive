namespace Alis.Reactive.PlanModel
{
    internal readonly struct AuthoredValue
    {
        internal AuthoredValue(ValueExpr expression, ValueShape shape)
        {
            Expression = expression;
            Shape = shape;
        }

        internal ValueExpr Expression { get; }
        internal ValueShape Shape { get; }
    }
}
