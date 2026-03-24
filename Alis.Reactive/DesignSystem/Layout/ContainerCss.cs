using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    public static class ContainerCss
    {
        public static string Classes(string? userClass = null)
        {
            return CssUtils.MergeClasses("max-w-7xl mx-auto px-4 sm:px-6 lg:px-8", userClass);
        }
    }
}
