using System.Collections.Generic;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Narrow typed menu item payload projected from Syncfusion Menu item objects.
    /// </summary>
    public sealed class FusionMenuItem
    {
        public string Id { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string IconCss { get; set; } = string.Empty;

        public bool Separator { get; set; }

        public List<FusionMenuItem> Items { get; set; } = [];
    }
}
