using System.Text.Encodings.Web;

namespace Alis.Reactive.Framework.App.Components;

public sealed class Choice<TEnum> : Component, IInputComponent
    where TEnum : struct, Enum
{
    public override ComponentVendor Vendor => ChoiceHtml.ChoiceVendor;

    public Property<TEnum> Value => Property<TEnum>();

    public Event<ChoicePicked> Changed => Event<ChoicePicked>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Changed);
}

public sealed class ChoicePicked : JsObject;

public static class ChoiceHtml
{
    public static ComponentVendor ChoiceVendor { get; } = ComponentVendor.Define(
        "choice",
        objectPath: "choice",
        subscribe: "on",
        unsubscribe: "off",
        payload: VendorEventPayload.Argument);

    public static RenderedComponent<Choice<TEnum>, TModel> Choice<TModel, TEnum>(this InputBoundField<TModel, TEnum> field)
        where TModel : class
        where TEnum : struct, Enum =>
        field.Render<Choice<TEnum>>(writer => Write<TEnum>(writer, field.ElementId, Held(field), blank: false));

    public static RenderedComponent<Choice<TEnum>, TModel> Choice<TModel, TEnum>(this InputBoundField<TModel, TEnum?> field)
        where TModel : class
        where TEnum : struct, Enum =>
        field.Render<Choice<TEnum>>(writer => Write<TEnum>(writer, field.ElementId, Held(field), blank: true));

    private static object? Held<TModel, TProp>(InputBoundField<TModel, TProp> field)
        where TModel : class
    {
        var model = field.Helper.ViewData.Model;
        return model == null ? null : field.Expression.Compile()(model);
    }

    private static void Write<TEnum>(TextWriter writer, string id, object? held, bool blank)
        where TEnum : struct, Enum
    {
        writer.Write($"<select class=\"choice\" id=\"{HtmlEncoder.Default.Encode(id)}\">");
        if (blank) writer.Write($"<option value=\"\"{(held == null ? " selected" : "")}></option>");
        foreach (var member in Enum.GetValues<TEnum>())
        {
            var name = HtmlEncoder.Default.Encode(member.ToString());
            writer.Write($"<option value=\"{name}\"{(member.Equals(held) ? " selected" : "")}>{name}</option>");
        }
        writer.Write("</select>");
    }
}
