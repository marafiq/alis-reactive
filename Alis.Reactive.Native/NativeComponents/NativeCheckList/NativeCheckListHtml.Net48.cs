#if NET48
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Native.NativeComponents;

public static partial class NativeCheckListHtml
{
    private static partial Rendered<NativeCheckList, TModel> MvcCheckList<TModel, TValue>(InputField<TModel, TValue> field, CheckListMarkup list)
    {
        var holder = new TagBuilder("div");
        Merges(holder, list.HolderAttributes);
        var inner = new StringBuilder();
        foreach (var box in list.Boxes) inner.Append(OptionOf(box, list).ToString(TagRenderMode.Normal));
        if (list.SendsJoinedText) inner.Append(InputOf(list.JoinedAttributes).ToString(TagRenderMode.SelfClosing));
        holder.InnerHtml = inner.ToString();
        return field.Render<NativeCheckList>(new HtmlString(holder.ToString(TagRenderMode.Normal)));
    }

    private static TagBuilder OptionOf(ListBox box, CheckListMarkup list)
    {
        var option = new TagBuilder("div");
        Merges(option, list.OptionAttributes);
        var inner = new StringBuilder(InputOf(box.Attributes).ToString(TagRenderMode.SelfClosing));
        var label = new TagBuilder("label");
        label.MergeAttribute("for", box.Id);
        label.SetInnerText(box.Option.Text);
        inner.Append(label.ToString(TagRenderMode.Normal));
        if (box.Option.Description is string description)
        {
            var described = new TagBuilder("span");
            described.MergeAttribute("id", box.DescriptionId);
            described.SetInnerText(description);
            inner.Append(described.ToString(TagRenderMode.Normal));
        }

        option.InnerHtml = inner.ToString();
        return option;
    }

    private static TagBuilder InputOf(Dictionary<string, string> attributes)
    {
        var input = new TagBuilder("input");
        Merges(input, attributes);
        return input;
    }

    private static void Merges(TagBuilder element, Dictionary<string, string> attributes)
    {
        foreach (var attribute in attributes) element.MergeAttribute(attribute.Key, attribute.Value);
    }
}
#endif
