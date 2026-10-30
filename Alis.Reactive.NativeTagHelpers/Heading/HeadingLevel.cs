namespace Alis.Reactive.NativeTagHelpers.Heading;

/// <summary>
/// The heading tag (<c>h1</c> through <c>h6</c>) a <c>native-heading</c> renders, and its type scale.
/// </summary>
public enum HeadingLevel
{
    /// <summary>A page title: <c>h1</c>.</summary>
    H1 = 1,

    /// <summary>A section title: <c>h2</c>.</summary>
    H2 = 2,

    /// <summary>A subsection title: <c>h3</c>.</summary>
    H3 = 3,

    /// <summary><c>h4</c>.</summary>
    H4 = 4,

    /// <summary><c>h5</c>.</summary>
    H5 = 5,

    /// <summary>A small uppercase label: <c>h6</c>.</summary>
    H6 = 6
}
