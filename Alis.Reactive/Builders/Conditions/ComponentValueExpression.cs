using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// A typed value expression that reads the current value of a component in the browser.
    /// Returned by each component's <c>Value()</c> extension method.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Pass to <c>When()</c> to build guard conditions:
    /// </para>
    /// <code>
    /// var country = p.Component&lt;FusionDropDownList&gt;(m =&gt; m.Country);
    /// p.When(country.Value()).NotEmpty()
    ///  .Then(t =&gt; t.Element("country-selected").Show());
    /// </code>
    /// <para>
    /// Pass to <c>SetText()</c> or <c>SetHtml()</c> to display the component's current value:
    /// </para>
    /// <code>
    /// p.Element("echo").SetText(country.Value());
    /// </code>
    /// <para>
    /// Pass to value-vs-value operators to compare two component values:
    /// </para>
    /// <code>
    /// var rate = p.Component&lt;FusionNumericTextBox&gt;(m =&gt; m.Rate);
    /// var budget = p.Component&lt;FusionNumericTextBox&gt;(m =&gt; m.Budget);
    /// p.When(rate.Value()).Gt(budget.Value())
    ///  .Then(t =&gt; t.Element("warning").Show());
    /// </code>
    /// </remarks>
    /// <typeparam name="TProp">The property type of the component value (e.g. <see cref="string"/>, <see cref="decimal"/>).</typeparam>
    public sealed class ComponentValueExpression<TProp> : ValueExpression<TProp>
    {
        private readonly string _componentId;
        private readonly string _vendor;
        private readonly string _memberPath;

        /// <summary>
        /// NEVER make public. Constructed by each component's <c>Value()</c> extension
        /// method, which supplies the correct component ID, vendor, and member path.
        /// </summary>
        public ComponentValueExpression(string componentId, string vendor, string valueMemberPath)
        {
            _componentId = componentId;
            _vendor = vendor;
            _memberPath = valueMemberPath;
        }

        /// <inheritdoc/>
        internal override ValueExpr ToValueExpr(PlanAuthoringContext authoring) =>
            authoring.CreateComponentMemberValue(_componentId, _vendor, _memberPath);
    }
}
