namespace Alis.Reactive.DesignSystem.Tokens
{
    public static class CssUtils
    {
        public static string MergeClasses(string generated, string userClass)
        {
            if (string.IsNullOrWhiteSpace(userClass))
                return generated;

            return generated + " " + userClass.Trim();
        }
    }
}
