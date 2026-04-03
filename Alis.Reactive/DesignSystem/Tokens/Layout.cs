namespace Alis.Reactive.DesignSystem.Tokens
{
    /// <summary>
    /// Defines the supported grid column counts.
    /// </summary>
    public enum GridCols
    {
        /// <summary>Renders a single-column grid.</summary>
        C1 = 1,
        /// <summary>Renders a two-column grid.</summary>
        C2 = 2,
        /// <summary>Renders a three-column grid.</summary>
        C3 = 3,
        /// <summary>Renders a four-column grid.</summary>
        C4 = 4,
        /// <summary>Renders a five-column grid.</summary>
        C5 = 5,
        /// <summary>Renders a six-column grid.</summary>
        C6 = 6
    }

    /// <summary>
    /// Defines the supported cross-axis alignment options for flex layouts.
    /// </summary>
    public enum AlignItems
    {
        /// <summary>Aligns items to the start edge.</summary>
        Start,
        /// <summary>Centers items on the cross axis.</summary>
        Center,
        /// <summary>Aligns items to the end edge.</summary>
        End,
        /// <summary>Stretches items to fill the cross axis.</summary>
        Stretch,
        /// <summary>Aligns items using their text baselines.</summary>
        Baseline
    }

    /// <summary>
    /// Defines the supported main-axis distribution options for flex layouts.
    /// </summary>
    public enum JustifyContent
    {
        /// <summary>Packs items toward the start edge.</summary>
        Start,
        /// <summary>Centers items on the main axis.</summary>
        Center,
        /// <summary>Packs items toward the end edge.</summary>
        End,
        /// <summary>Places equal space between items.</summary>
        Between,
        /// <summary>Places space around items.</summary>
        Around,
        /// <summary>Places equal space around and between items.</summary>
        Evenly
    }
}
