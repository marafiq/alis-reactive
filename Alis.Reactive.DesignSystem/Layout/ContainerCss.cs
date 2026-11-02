using Alis.Reactive.DesignSystem;

namespace Alis.Reactive.DesignSystem
{
    public static class ContainerCss
    {
        public static string Classes(string? cssClass = null)
        {
            return CssUtils.MergeClasses("max-w-7xl mx-auto px-4 sm:px-6 lg:px-8", cssClass);
        }
    }
}
