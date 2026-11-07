using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

public sealed class CareBoard : Component
{
    public static ComponentVendor CareBoardVendor { get; } = ComponentVendor.Define("careboard", objectPath: "careBoard", subscribe: "on", unsubscribe: "off");

    public override ComponentVendor Vendor => CareBoardVendor;

    public Property<string> Title => Property<string>();

    public ReadOnly<string> Status => ReadOnly<string>();

    public WriteOnly<string> Pin => WriteOnly<string>();
}

public static class CareBoardHtml
{
    public static Rendered<CareBoard, TModel> CareBoard<TModel>(this ReactivePlan<TModel> plan, string id, string title, string unlockCode)
        => plan.Render<CareBoard>(id, Board(id, "1", title, unlockCode));

    public static Rendered<CareBoard, TModel> CareBoardVersion2<TModel>(this ReactivePlan<TModel> plan, string id, string title)
        => plan.Render<CareBoard>(id, Board(id, "2", title, ""));

    private static TagBuilder Board(string id, string version, string title, string unlockCode)
    {
        var board = new TagBuilder("section");
        board.Attributes["id"] = id;
        board.Attributes["data-careboard"] = version;
        board.Attributes["data-title"] = title;
        board.Attributes["data-unlock-code"] = unlockCode;
        return board;
    }
}
