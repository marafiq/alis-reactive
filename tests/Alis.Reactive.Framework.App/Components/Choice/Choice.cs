using System.Text.Encodings.Web;

namespace Alis.Reactive.Framework.App.Components;

/// <summary>
/// A choice of one member of an enum (choice.ts): a select whose options are the members by name, which
/// the select element keeps as <c>choice</c>. It takes listeners through <c>on</c> and <c>off</c>, and is
/// committed when the user picks a member, by its own event, <c>changed</c>.
/// </summary>
/// <typeparam name="TEnum">The enum whose members are offered.</typeparam>
public sealed class Choice<TEnum> : Component, IInputComponent
    where TEnum : struct, Enum
{
    public override ComponentVendor Vendor => ChoiceHtml.ChoiceVendor;

    /// <summary>The member chosen, by its name; empty while the blank option of a member that may hold none is chosen.</summary>
    public Property<TEnum> Value => Property<TEnum>();

    /// <summary>Raised when the user picks a member.</summary>
    public Event<ChoicePicked> Changed => Event<ChoicePicked>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Changed);
}

/// <summary>Payload of <see cref="Choice{TEnum}.Changed"/>: the picking itself, with nothing to read.</summary>
public sealed class ChoicePicked : JsObject;

public static class ChoiceHtml
{
    public static ComponentVendor ChoiceVendor { get; } = ComponentVendor.Define(
        "choice",
        objectPath: "choice",
        subscribe: "on",
        unsubscribe: "off",
        payload: VendorEventPayload.Argument);

    /// <summary>A choice of the enum's members for the member the input field binds, the model's member chosen.</summary>
    public static RenderedComponent<Choice<TEnum>, TModel> Choice<TModel, TEnum>(this InputBoundField<TModel, TEnum> field)
        where TModel : class
        where TEnum : struct, Enum =>
        field.Render<Choice<TEnum>>(writer => Write<TEnum>(writer, field.ElementId, Held(field), blank: false));

    /// <summary>A choice of the enum's members, after a blank option for none, for a member that may hold none.</summary>
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

    // The select is written to the field's writer an option at a time, as a vendor helper that writes its
    // markup instead of returning it does.
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
