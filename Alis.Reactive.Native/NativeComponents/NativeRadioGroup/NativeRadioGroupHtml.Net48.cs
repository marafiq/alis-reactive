#if NET48
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Native.NativeComponents;

public static partial class NativeRadioGroupHtml
{
    private static partial Rendered<NativeRadioGroup, TModel> MvcRadioGroup<TModel, TValue>(InputField<TModel, TValue> field, IReadOnlyList<GroupRadio> radios, NativeRadioGroupBuilder group)
    {
        var holder = new TagBuilder("div");
        Merges(holder, group.HolderAttributes);
        var input = new TagBuilder("input");
        Merges(input, GroupInputAttributes(field, radios));
        var inner = new StringBuilder(input.ToString(TagRenderMode.SelfClosing));
        foreach (var radio in radios) inner.Append(OptionOf(field, radio, group).ToString(TagRenderMode.Normal));
        holder.InnerHtml = inner.ToString();
        return field.Render<NativeRadioGroup>(new HtmlString(holder.ToString(TagRenderMode.Normal)));
    }

    private static TagBuilder OptionOf<TModel, TValue>(InputField<TModel, TValue> field, GroupRadio radio, NativeRadioGroupBuilder group)
    {
        var option = new TagBuilder("div");
        Merges(option, group.OptionAttributes);
        var inner = new StringBuilder(field.Html.RadioButtonFor(field.Member, radio.Option.Value, radio.Attributes).ToHtmlString());
        var label = new TagBuilder("label");
        label.MergeAttribute("for", radio.Id);
        label.SetInnerText(radio.Option.Text);
        inner.Append(label.ToString(TagRenderMode.Normal));
        if (radio.Option.Description is string description)
        {
            var described = new TagBuilder("span");
            described.MergeAttribute("id", radio.DescriptionId);
            described.SetInnerText(description);
            inner.Append(described.ToString(TagRenderMode.Normal));
        }

        option.InnerHtml = inner.ToString();
        return option;
    }

    private static void Merges(TagBuilder element, Dictionary<string, string> attributes)
    {
        foreach (var attribute in attributes) element.MergeAttribute(attribute.Key, attribute.Value);
    }
}
#endif
