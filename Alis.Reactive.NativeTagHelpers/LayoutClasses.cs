namespace Alis.Reactive.NativeTagHelpers;

/// <summary>
/// Composes the class names the layout tag helpers write. Each names a part of the package's own markup
/// and is styled by its stylesheet, <c>_content/AlisReactive.NativeTagHelpers/css/native-tag-helpers.css</c>.
/// </summary>
internal static class LayoutClasses
{
    /// <summary>Joins the non-empty classes in order; the page's own classes go last.</summary>
    internal static string Join(params string?[] classes) =>
        string.Join(" ", classes.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c!.Trim()));

    internal static string Gap(SpacingScale gap) => gap switch
    {
        SpacingScale.None => "alis-gap-none",
        SpacingScale.Xs => "alis-gap-xs",
        SpacingScale.Sm => "alis-gap-sm",
        SpacingScale.Base => "alis-gap-base",
        SpacingScale.Md => "alis-gap-md",
        SpacingScale.Lg => "alis-gap-lg",
        SpacingScale.Xl => "alis-gap-xl",
        SpacingScale.Xxl => "alis-gap-xxl",
        SpacingScale.Max => "alis-gap-max",
        _ => throw new ArgumentOutOfRangeException(nameof(gap), gap, "Not a spacing scale step.")
    };

    /// <summary>The space-below class; empty for <see cref="ElementSpacing.None"/>.</summary>
    internal static string SpaceBelow(ElementSpacing spacing) => spacing switch
    {
        ElementSpacing.None => "",
        ElementSpacing.Xs => "alis-space-xs",
        ElementSpacing.Sm => "alis-space-sm",
        ElementSpacing.Base => "alis-space-base",
        ElementSpacing.Md => "alis-space-md",
        ElementSpacing.Lg => "alis-space-lg",
        _ => throw new ArgumentOutOfRangeException(nameof(spacing), spacing, "Not an element spacing step.")
    };
}
