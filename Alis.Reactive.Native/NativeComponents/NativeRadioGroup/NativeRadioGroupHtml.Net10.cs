#if NET10_0_OR_GREATER
using System.Collections.Generic;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.NativeComponents;

public static partial class NativeRadioGroupHtml
{
    private static partial Rendered<NativeRadioGroup, TModel> MvcRadioGroup<TModel, TValue>(InputField<TModel, TValue> field, IReadOnlyList<GroupRadio> radios, NativeRadioGroupBuilder group)
    {
        var holder = new TagBuilder("div");
        Merges(holder, group.HolderAttributes);
        var input = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
        Merges(input, GroupInputAttributes(field, radios));
        holder.InnerHtml.AppendHtml(input);
        foreach (var radio in radios) holder.InnerHtml.AppendHtml(OptionOf(field, radio, group));
        return field.Render<NativeRadioGroup>(holder);
    }

    private static TagBuilder OptionOf<TModel, TValue>(InputField<TModel, TValue> field, GroupRadio radio, NativeRadioGroupBuilder group)
    {
        var option = new TagBuilder("div");
        Merges(option, group.OptionAttributes);
        option.InnerHtml.AppendHtml(field.Html.RadioButtonFor(field.Member, radio.Option.Value, radio.Attributes));
        var label = new TagBuilder("label");
        label.MergeAttribute("for", radio.Id);
        label.InnerHtml.Append(radio.Option.Text);
        option.InnerHtml.AppendHtml(label);
        if (radio.Option.Description is not string description) return option;
        var described = new TagBuilder("span");
        described.MergeAttribute("id", radio.DescriptionId);
        described.InnerHtml.Append(description);
        option.InnerHtml.AppendHtml(described);
        return option;
    }

    private static void Merges(TagBuilder element, Dictionary<string, string> attributes)
    {
        foreach (var attribute in attributes) element.MergeAttribute(attribute.Key, attribute.Value);
    }
}
#endif
