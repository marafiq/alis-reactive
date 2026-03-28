using System.Linq.Expressions;
using System;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Guards;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Conditional logic: <c>When</c>/<c>Then</c>/<c>ElseIf</c>/<c>Else</c> branching and
    /// <c>Confirm</c> halting guards.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Conditions test a value from an event payload or a component and branch
    /// into one of several pipelines. The full chain reads naturally:
    /// </para>
    /// <code>
    /// p.When(args, x =&gt; x.Score).Gte(90)
    ///  .Then(t =&gt; t.Element("grade").SetText("A"))
    ///  .ElseIf(args, x =&gt; x.Score).Gte(80)
    ///  .Then(t =&gt; t.Element("grade").SetText("B"))
    ///  .Else(e =&gt; e.Element("grade").SetText("F"));
    /// </code>
    /// <para>
    /// Multiple <c>When</c> blocks in the same pipeline are independent: each evaluates
    /// on its own and does not interfere with the others.
    /// </para>
    /// </remarks>
    public partial class PipelineBuilder<TModel> where TModel : class
    {
        /// <summary>
        /// Starts a conditional branch that tests a property from the event payload.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <typeparamref name="TProp"/> is inferred from the expression, so all subsequent
        /// operators (<c>Eq</c>, <c>Gt</c>, <c>Contains</c>, etc.) enforce compile-time type safety.
        /// </para>
        /// <para>
        /// Multiple <c>When</c> calls in the same pipeline produce independent condition blocks,
        /// each evaluated separately:
        /// </para>
        /// <code>
        /// // Two independent conditions in one pipeline:
        /// p.When(args, x =&gt; x.Country).Eq("US")
        ///  .Then(t =&gt; t.Element("flag").Show());
        ///
        /// p.When(args, x =&gt; x.Score).Gt(90)
        ///  .Then(t =&gt; t.Element("badge").Show());
        /// </code>
        /// </remarks>
        /// <typeparam name="TPayload">The event args type carrying the property to test.</typeparam>
        /// <typeparam name="TProp">The property type, inferred from the expression. Operators enforce this type.</typeparam>
        /// <param name="payload">The event args instance (used only for type inference, not evaluated at build time).</param>
        /// <param name="path">Expression selecting the property to test (e.g. <c>x =&gt; x.Country</c>).</param>
        /// <returns>
        /// A <see cref="ConditionSourceBuilder{TModel, TProp}"/> exposing typed operators
        /// such as <c>Eq</c>, <c>Gt</c>, <c>Contains</c>, and <c>NotNull</c>.
        /// </returns>
        public ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(
            TPayload payload,
            Expression<Func<TPayload, TProp>> path)
        {
            // Flush previous segment if there are already branches (second+ When call)
            if (ConditionalBranches != null && ConditionalBranches.Count > 0)
                FlushSegment();

            SetMode(PipelineMode.Conditional);

            var source = new EventArgSource<TPayload, TProp>(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>
        /// Starts a conditional branch that tests a component's current value in the browser.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Obtain a <see cref="TypedSource{TProp}"/> by calling a component's <c>Value()</c>
        /// extension. The property type flows through the operator chain for compile-time safety.
        /// </para>
        /// <code>
        /// var country = p.Component&lt;FusionDropDownList&gt;(m =&gt; m.Country);
        /// p.When(country.Value()).NotEmpty()
        ///  .Then(t =&gt; t.Element("country-selected").Show())
        ///  .Else(e =&gt; e.Element("country-selected").Hide());
        /// </code>
        /// </remarks>
        /// <typeparam name="TProp">The component's value type, inferred from the <see cref="TypedSource{TProp}"/>.</typeparam>
        /// <param name="source">A typed source from a component's <c>Value()</c> extension.</param>
        /// <returns>
        /// A <see cref="ConditionSourceBuilder{TModel, TProp}"/> exposing typed operators.
        /// </returns>
        public ConditionSourceBuilder<TModel, TProp> When<TProp>(TypedSource<TProp> source)
        {
            if (ConditionalBranches != null && ConditionalBranches.Count > 0)
                FlushSegment();

            SetMode(PipelineMode.Conditional);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>
        /// Shows a browser confirmation dialog that halts the pipeline until the user responds.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the user clicks OK, the <c>Then</c> branch executes. If the user clicks Cancel,
        /// the branch is skipped entirely.
        /// </para>
        /// <code>
        /// p.Confirm("Delete this record?")
        ///  .Then(t =&gt; t.Post("/api/records/delete").Gather(...));
        /// </code>
        /// <para>
        /// Use only in user-initiated pipelines (e.g. button clicks). Never use in <c>DomReady</c>.
        /// </para>
        /// </remarks>
        /// <param name="message">The message shown in the browser confirmation dialog.</param>
        /// <returns>
        /// A <see cref="GuardBuilder{TModel}"/> that must be terminated with <c>Then</c> to
        /// define the commands that execute when the user confirms.
        /// </returns>
        public GuardBuilder<TModel> Confirm(string message)
        {
            if (ConditionalBranches != null && ConditionalBranches.Count > 0)
                FlushSegment();

            SetMode(PipelineMode.Conditional);

            return new GuardBuilder<TModel>(new ConfirmGuard(message), this);
        }
    }
}
