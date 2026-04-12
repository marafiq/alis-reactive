namespace Alis.Reactive.DesignSystem.Tokens
{
    public static class CssUtils
    {
        public static string MergeClasses(string generated, string? cssClass)
        {
            if (string.IsNullOrWhiteSpace(cssClass))
                return generated;

            return generated + " " + cssClass.Trim();
        }
    }
}
