using Alis.Reactive.Descriptors.Sources;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// A typed source that reads the current value of a component in the browser.
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
    /// Pass to source-vs-source operators to compare two component values:
    /// </para>
    /// <code>
    /// var rate = p.Component&lt;FusionNumericTextBox&gt;(m =&gt; m.Rate);
    /// var budget = p.Component&lt;FusionNumericTextBox&gt;(m =&gt; m.Budget);
    /// p.When(rate.Value()).Gt(budget.Value())
    ///  .Then(t =&gt; t.Element("warning").Show());
    /// </code>
    /// </remarks>
    /// <typeparam name="TProp">The property type of the component value (e.g. <see cref="string"/>, <see cref="decimal"/>).</typeparam>
    public sealed class TypedComponentSource<TProp> : TypedSource<TProp>
    {
        private readonly string _componentId;
        private readonly string _vendor;
        private readonly string _readExpr;

        /// <summary>
        /// NEVER make public. Constructed by each component's <c>Value()</c> extension
        /// method, which supplies the correct component ID, vendor, and read expression.
        /// </summary>
        public TypedComponentSource(string componentId, string vendor, string readExpr)
        {
            _componentId = componentId;
            _vendor = vendor;
            _readExpr = readExpr;
        }

        /// <inheritdoc/>
        public override BindSource ToBindSource()
            => new ComponentSource(_componentId, _vendor, _readExpr);
    }
}
