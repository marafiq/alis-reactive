namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>The community's branding: the colors its resident portal and printed notices use.</summary>
    public class ColorPickerModel
    {
        /// <summary>The brand color headers and buttons use; text on it is white.</summary>
        public string? ThemeColor { get; set; }

        /// <summary>An optional accent color for highlights; the community may choose none.</summary>
        public string? AccentColor { get; set; }
    }
}
