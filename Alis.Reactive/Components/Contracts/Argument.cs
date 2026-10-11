using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// One argument of a method call: a literal, or a value read at runtime. Write either one where
    /// the call takes it; the conversion is implicit.
    /// </summary>
    /// <example>
    /// <code>
    /// s.Component&lt;FusionKanban&gt;("care-kanban").Call(c =&gt; c.AddCard&lt;CareCard&gt;(), json.Read(x =&gt; x.Card), 0)
    /// </code>
    /// </example>
    /// <typeparam name="T">The C# type the method declares for the argument.</typeparam>
    public sealed class Argument<T>
    {
        private Argument(ValueExpression expression)
        {
            Expression = expression;
        }

        internal ValueExpression Expression { get; }

        /// <summary>A literal argument.</summary>
        public static implicit operator Argument<T>(T value) =>
            new Argument<T>(ValueExpression.LiteralFor(value, Shape.FromClrType(typeof(T))));

        /// <summary>An argument read at runtime: a component, a payload, a response, a URL parameter.</summary>
        public static implicit operator Argument<T>(TypedSource<T> source) =>
            new Argument<T>(source.ToValueExpression());
    }
}
