using System.Collections.Generic;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Narrow typed context menu item payload projected from Syncfusion ContextMenu item objects.
    /// </summary>
    public sealed class FusionContextMenuItem
    {
        public string Id { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string IconCss { get; set; } = string.Empty;

        public bool Separator { get; set; }

        public List<FusionContextMenuItem> Items { get; set; } = [];
    }
}
