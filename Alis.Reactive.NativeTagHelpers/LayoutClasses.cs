namespace Alis.Reactive.NativeTagHelpers;

internal static class LayoutClasses
{
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
