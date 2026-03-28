using System;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors.Guards;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Entry point for building inner guards inside <c>And</c>/<c>Or</c> lambdas.
    /// </summary>
    /// <remarks>
    /// Received as the <c>cs</c> parameter in lambda guard composition:
    /// <code>
    /// p.When(args, x =&gt; x.Score).Gte(90)
    ///  .And(cs =&gt; cs.When(args, x =&gt; x.Role).Eq("admin")
    ///               .Or(args, x =&gt; x.Role).Eq("nurse"))
    ///  .Then(t =&gt; t.Element("badge").Show());
    /// </code>
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    public sealed class ConditionStart<TModel> where TModel : class
    {
        /// <summary>
        /// NEVER make public. Constructed by <see cref="GuardBuilder{TModel}.And(Func{ConditionStart{TModel}, GuardBuilder{TModel}})"/>
        /// and <see cref="GuardBuilder{TModel}.Or(Func{ConditionStart{TModel}, GuardBuilder{TModel}})"/>.
        /// </summary>
        internal ConditionStart() { }

        /// <summary>
        /// Begins a condition on an event payload property inside a guard lambda.
        /// </summary>
        /// <typeparam name="TPayload">The event args type.</typeparam>
        /// <typeparam name="TProp">The property type, inferred from the expression.</typeparam>
        /// <param name="payload">The event args instance (type inference only).</param>
        /// <param name="path">Expression selecting the property to test.</param>
        /// <returns>A <see cref="ConditionSourceBuilder{TModel, TProp}"/> for applying an operator.</returns>
        public ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(
            TPayload payload,
            Expression<Func<TPayload, TProp>> path)
        {
            var source = new EventArgSource<TPayload, TProp>(path);
            return new ConditionSourceBuilder<TModel, TProp>(source);
        }

        /// <summary>
        /// Begins a condition on a component's current value inside a guard lambda.
        /// </summary>
        /// <typeparam name="TProp">The component's value type.</typeparam>
        /// <param name="source">A typed source from a component's <c>Value()</c> extension.</param>
        /// <returns>A <see cref="ConditionSourceBuilder{TModel, TProp}"/> for applying an operator.</returns>
        public ConditionSourceBuilder<TModel, TProp> When<TProp>(TypedSource<TProp> source)
        {
            return new ConditionSourceBuilder<TModel, TProp>(source);
        }

        /// <summary>
        /// Shows a browser confirmation dialog inside a guard lambda.
        /// </summary>
        /// <param name="message">The message shown in the browser confirmation dialog.</param>
        /// <returns>
        /// A <see cref="GuardBuilder{TModel}"/> that must be terminated with <c>Then</c>.
        /// </returns>
        public GuardBuilder<TModel> Confirm(string message)
        {
            return new GuardBuilder<TModel>(new ConfirmGuard(message));
        }
    }
}
