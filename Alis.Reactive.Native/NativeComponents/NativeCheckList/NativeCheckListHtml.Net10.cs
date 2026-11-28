#if NET10_0_OR_GREATER
using System.Collections.Generic;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.NativeComponents;

public static partial class NativeCheckListHtml
{
    private static partial Rendered<NativeCheckList, TModel> MvcCheckList<TModel, TValue>(InputField<TModel, TValue> field, CheckListMarkup list)
    {
        var holder = new TagBuilder("div");
        Merges(holder, list.HolderAttributes);
        foreach (var box in list.Boxes) holder.InnerHtml.AppendHtml(OptionOf(box, list));
        if (list.SendsJoinedText) holder.InnerHtml.AppendHtml(InputOf(list.JoinedAttributes));
        return field.Render<NativeCheckList>(holder);
    }

    private static TagBuilder OptionOf(ListBox box, CheckListMarkup list)
    {
        var option = new TagBuilder("div");
        Merges(option, list.OptionAttributes);
        option.InnerHtml.AppendHtml(InputOf(box.Attributes));
        var label = new TagBuilder("label");
        label.MergeAttribute("for", box.Id);
        label.InnerHtml.Append(box.Option.Text);
        option.InnerHtml.AppendHtml(label);
        if (box.Option.Description is not string description) return option;
        var described = new TagBuilder("span");
        described.MergeAttribute("id", box.DescriptionId);
        described.InnerHtml.Append(description);
        option.InnerHtml.AppendHtml(described);
        return option;
    }

    private static TagBuilder InputOf(Dictionary<string, string> attributes)
    {
        var input = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
        Merges(input, attributes);
        return input;
    }

    private static void Merges(TagBuilder element, Dictionary<string, string> attributes)
    {
        foreach (var attribute in attributes) element.MergeAttribute(attribute.Key, attribute.Value);
    }
}
#endif
