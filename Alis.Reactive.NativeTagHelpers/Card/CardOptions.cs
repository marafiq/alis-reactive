namespace Alis.Reactive.NativeTagHelpers.Card;

/// <summary>
/// How far a <c>native-card</c> lifts off the page.
/// </summary>
public enum CardElevation
{
    /// <summary>No shadow.</summary>
    Flat,

    /// <summary>A small shadow.</summary>
    Low,

    /// <summary>A medium shadow.</summary>
    Medium,

    /// <summary>A large shadow.</summary>
    High
}

/// <summary>
/// Which card sections draw a separating border.
/// </summary>
public enum CardDivider
{
    /// <summary>No separating border.</summary>
    None,

    /// <summary>A border below the header.</summary>
    Header,

    /// <summary>A border above the footer.</summary>
    Footer,

    /// <summary>A border below the header and above the footer.</summary>
    Both
}

/// <summary>
/// Inner padding of a <c>native-card-body</c>.
/// </summary>
public enum CardPadding
{
    /// <summary>No padding.</summary>
    None,

    /// <summary>Tight padding for dense content.</summary>
    Compact,

    /// <summary>Roomy padding that grows on wider screens.</summary>
    Standard
}

/// <summary>
/// Color of the accent border drawn on the left of a <c>native-card</c>.
/// </summary>
public enum AccentColor
{
    /// <summary>The brand accent.</summary>
    Primary,

    /// <summary>The secondary text color.</summary>
    Secondary,

    /// <summary>The success color.</summary>
    Success,

    /// <summary>The warning color.</summary>
    Warning,

    /// <summary>The error color.</summary>
    Error,

    /// <summary>The informational color.</summary>
    Info,

    /// <summary>The muted text color.</summary>
    Muted
}
