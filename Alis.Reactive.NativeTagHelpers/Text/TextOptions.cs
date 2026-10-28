namespace Alis.Reactive.NativeTagHelpers.Text;

/// <summary>
/// Type size of a <c>native-text</c>.
/// </summary>
public enum TextSize
{
    /// <summary>0.75rem.</summary>
    Xs,

    /// <summary>0.875rem.</summary>
    Sm,

    /// <summary>1rem.</summary>
    Base,

    /// <summary>1.125rem.</summary>
    Lg,

    /// <summary>1.25rem.</summary>
    Xl
}

/// <summary>
/// Color of a <c>native-text</c>.
/// </summary>
public enum TextColor
{
    /// <summary>The primary text color.</summary>
    Primary,

    /// <summary>The secondary text color.</summary>
    Secondary,

    /// <summary>The muted text color.</summary>
    Muted,

    /// <summary>White, for text on a dark surface.</summary>
    Inverse,

    /// <summary>The brand accent.</summary>
    Accent,

    /// <summary>The success color.</summary>
    Success,

    /// <summary>The warning color.</summary>
    Warning,

    /// <summary>The error color.</summary>
    Error,

    /// <summary>The surrounding text's color.</summary>
    Inherit
}
