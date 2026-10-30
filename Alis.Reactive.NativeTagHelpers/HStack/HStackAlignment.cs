namespace Alis.Reactive.NativeTagHelpers.HStack;

/// <summary>
/// How the children of a <c>native-hstack</c> align across the row.
/// </summary>
public enum AlignItems
{
    /// <summary>Along the top of the row.</summary>
    Start,

    /// <summary>Centered in the row.</summary>
    Center,

    /// <summary>Along the bottom of the row.</summary>
    End,

    /// <summary>Stretched to the row's height.</summary>
    Stretch,

    /// <summary>On a shared text baseline.</summary>
    Baseline
}

/// <summary>
/// How the children of a <c>native-hstack</c> are distributed along the row.
/// </summary>
public enum JustifyContent
{
    /// <summary>Packed at the start.</summary>
    Start,

    /// <summary>Packed in the middle.</summary>
    Center,

    /// <summary>Packed at the end.</summary>
    End,

    /// <summary>The first at the start, the last at the end, the rest spread between.</summary>
    Between,

    /// <summary>Equal space around each child.</summary>
    Around,

    /// <summary>Equal space between and around the children.</summary>
    Evenly
}
