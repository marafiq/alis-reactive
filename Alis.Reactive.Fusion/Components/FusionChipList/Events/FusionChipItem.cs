namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A selected chip's bound data, as returned in <c>getSelectedChips().data</c> (proven in
    /// <c>@syncfusion/ej2-buttons</c> <c>chip-list.js</c>: <c>selectedItems.data.push(this.chips[index])</c>).
    /// Each element is the chip model the developer bound, so the array DSL can operate on the
    /// selection by member — e.g. <c>p.From(payload, x =&gt; x.Selection.Data).Where(c =&gt; c.Value == "memory")</c>.
    /// </summary>
    public sealed class FusionChipItem
    {
        /// <summary>The chip's display text (<c>data[i].text</c>).</summary>
        public string Text { get; set; } = "";

        /// <summary>The chip's bound value (<c>data[i].value</c>).</summary>
        public string Value { get; set; } = "";
    }
}
