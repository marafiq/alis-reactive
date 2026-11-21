using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A breadcrumb: EJ2's Breadcrumb, a trail of links to the pages above the one a person is on.
/// </summary>
/// <remarks>EJ2 raises <see cref="ItemClick"/> when a person clicks an item of the trail.</remarks>
[Mirrors("@syncfusion/ej2-navigations", "Breadcrumb")]
public sealed class FusionBreadcrumb : FusionComponent
{
    /// <summary>Gets the address of the trail's active item.</summary>
    public Property<string> ActiveItem => Property<string>();

    /// <summary>Applies the changes written to the breadcrumb's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Gets the event raised when a person clicks an item of the trail.</summary>
    public Event<BreadcrumbClickEventArgs> ItemClick => Event<BreadcrumbClickEventArgs>();

    /// <summary>
    /// What a listener of <see cref="ItemClick"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-navigations", "BreadcrumbClickEventArgs")]
    public sealed class BreadcrumbClickEventArgs : JsObject
    {
        /// <summary>Gets the item the person clicked, read with <c>Read</c>.</summary>
        public ReadOnly<BreadcrumbItemModel> Item => ReadOnly<BreadcrumbItemModel>();
    }

    /// <summary>
    /// An item of the trail, as EJ2 holds it.
    /// </summary>
    [Mirrors("@syncfusion/ej2-navigations", "BreadcrumbItemModel")]
    public sealed class BreadcrumbItemModel
    {
        /// <summary>Gets or sets the text the item shows.</summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>Gets or sets the item's id.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>Gets or sets the address the item links to.</summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        /// <summary>Gets or sets the CSS classes of the item's icon.</summary>
        [JsonPropertyName("iconCss")]
        public string? IconCss { get; set; }

        /// <summary>Gets or sets whether the item is disabled.</summary>
        [JsonPropertyName("disabled")]
        public bool? Disabled { get; set; }
    }
}
