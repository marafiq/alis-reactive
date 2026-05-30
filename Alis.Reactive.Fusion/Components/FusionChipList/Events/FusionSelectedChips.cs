using System;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The result of <c>ChipList.getSelectedChips()</c> for a multiple-selection chip list. Mirrors
    /// the shipped shape <c>{ texts, Indexes, data, elements }</c> (chip-list.js). <see cref="Data"/>
    /// carries the selected chip objects, so the array DSL can filter/aggregate the selection by
    /// member; <see cref="Texts"/> carries the selected display strings.
    /// </summary>
    public sealed class FusionSelectedChips
    {
        /// <summary>The selected chips' display text (<c>texts</c>).</summary>
        public string[] Texts { get; set; } = Array.Empty<string>();

        /// <summary>
        /// The selected chips' bound data objects (<c>data</c>) — the array DSL source for
        /// operating on the selection by member (text/value).
        /// </summary>
        public FusionChipItem[] Data { get; set; } = Array.Empty<FusionChipItem>();

        /// <summary>
        /// The selected chips' indexes. NOTE: Syncfusion emits this key as <c>Indexes</c> (capital I,
        /// chip-list.js), so the camelCased read path resolves to empty. Reading the selected indexes
        /// requires a read-path name override (a framework capability, recorded as follow-up). Use
        /// <see cref="Data"/>/<see cref="Texts"/> until then.
        /// </summary>
        public int[] Indexes { get; set; } = Array.Empty<int>();
    }
}
