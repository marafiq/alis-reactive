using System.Collections.Generic;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload delivered after the context menu opens or closes.
    /// </summary>
    public sealed class FusionContextMenuOpenCloseArgs
    {
        public List<FusionContextMenuItem> Items { get; set; } = [];

        public FusionContextMenuItem? ParentItem { get; set; }
    }
}
