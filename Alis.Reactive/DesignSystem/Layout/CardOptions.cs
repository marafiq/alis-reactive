namespace Alis.Reactive.DesignSystem.Layout
{
    /// <summary>
    /// Defines the supported card shadow treatments.
    /// </summary>
    public enum CardElevation
    {
        /// <summary>Renders the card without a shadow.</summary>
        Flat,
        /// <summary>Renders the card with a subtle shadow.</summary>
        Low,
        /// <summary>Renders the card with a medium shadow.</summary>
        Medium,
        /// <summary>Renders the card with a pronounced shadow.</summary>
        High
    }

    /// <summary>
    /// Defines where card section dividers should appear.
    /// </summary>
    public enum CardDivider
    {
        /// <summary>Renders no divider.</summary>
        None,
        /// <summary>Renders a divider below the header.</summary>
        Header,
        /// <summary>Renders a divider above the footer.</summary>
        Footer,
        /// <summary>Renders both header and footer dividers.</summary>
        Both
    }

    /// <summary>
    /// Defines the supported padding presets for card bodies.
    /// </summary>
    public enum CardPadding
    {
        /// <summary>Renders no body padding.</summary>
        None,
        /// <summary>Renders compact body padding.</summary>
        Compact,
        /// <summary>Renders the standard body padding.</summary>
        Standard
    }
}
