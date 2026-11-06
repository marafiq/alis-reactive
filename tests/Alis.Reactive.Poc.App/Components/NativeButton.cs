using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

public sealed class NativeButton : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Event Click => Event();
}

public static class NativeButtonHtml
{
    public static Rendered<NativeButton> Button<TModel>(this ReactivePlan<TModel> plan, string id, string text)
    {
        var button = new TagBuilder("button");
        button.Attributes["id"] = id;
        button.Attributes["type"] = "button";
        button.InnerHtml.Append(text);
        return plan.Render<NativeButton>(id, button);
    }
}
