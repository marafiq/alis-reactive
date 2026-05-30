using System.Collections.Generic;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload delivered after the menu opens or closes.
    /// </summary>
    public sealed class FusionMenuOpenCloseArgs
    {
        public List<FusionMenuItem> Items { get; set; } = [];

        public FusionMenuItem? ParentItem { get; set; }
    }
}
