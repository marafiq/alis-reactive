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
    /// <c>p.When(comp.Value()).NotEmpty()</c>.
    /// </para>
    /// <para>
    /// Pass to <c>SetText()</c> or <c>SetHtml()</c> to display the component's current value:
    /// <c>p.Element("echo").SetText(comp.Value())</c>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TProp">The property type of the component value (e.g. <see cref="string"/>, <see cref="decimal"/>).</typeparam>
    public sealed class TypedComponentSource<TProp> : TypedSource<TProp>
    {
        private readonly string _componentId;
        private readonly string _vendor;
        private readonly string _readExpr;

        public TypedComponentSource(string componentId, string vendor, string readExpr)
        {
            _componentId = componentId;
            _vendor = vendor;
            _readExpr = readExpr;
        }

        public override BindSource ToBindSource()
            => new ComponentSource(_componentId, _vendor, _readExpr);
    }
}
