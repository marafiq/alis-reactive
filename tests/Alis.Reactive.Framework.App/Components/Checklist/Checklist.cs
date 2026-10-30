using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Components;

/// <summary>
/// A checklist for a model member that holds a list of text (checklist.ts): a box for each option, inside a
/// group that keeps the checklist as <c>checklist</c>. Its value is the text of every box ticked, in the order
/// shown. It takes listeners through <c>on</c> and <c>off</c>, and is committed by its own event,
/// <c>changed</c>, when the user ticks or clears a box. Each box is named by the field's binding path, so a
/// plain form post sends it too.
/// </summary>
public sealed class Checklist : Component, IInputComponent
{
    public static ComponentVendor ChecklistVendor { get; } = ComponentVendor.Define(
        "checklist",
        objectPath: "checklist",
        subscribe: "on",
        unsubscribe: "off",
        payload: VendorEventPayload.Argument);

    public override ComponentVendor Vendor => ChecklistVendor;

    /// <summary>The text of every box ticked, in the order shown; none while no box is ticked.</summary>
    public ReadOnly<IReadOnlyList<string>> Ticked => ReadOnly<IReadOnlyList<string>>();

    /// <summary>Raised when the user ticks or clears a box.</summary>
    public Event<ChecklistChanged> Changed => Event<ChecklistChanged>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Ticked, committedBy: Changed);
}

/// <summary>Payload of <see cref="Checklist.Changed"/>: the ticking itself, with nothing to read.</summary>
public sealed class ChecklistChanged : JsObject;

public static class ChecklistHtml
{
    /// <summary>A box for each option, for the list of text the input field binds, the model's items ticked.</summary>
    public static RenderedComponent<Checklist, TModel> Checklist<TModel, TProp>(this InputBoundField<TModel, TProp> field, params string[] options)
        where TModel : class
        where TProp : IEnumerable<string>
    {
        var model = field.Helper.ViewData.Model;
        var held = model == null ? [] : field.Expression.Compile()(model)?.ToList() ?? [];
        var name = HtmlEncoder.Default.Encode(field.BindingPath);
        var boxes = string.Concat(options.Select(option =>
        {
            var text = HtmlEncoder.Default.Encode(option);
            var ticked = held.Contains(option) ? " checked" : "";
            return $"<label><input type=\"checkbox\" name=\"{name}\" value=\"{text}\"{ticked}> {text}</label>";
        }));
        return field.Render<Checklist>(new HtmlString(
            $"<div class=\"checklist\" id=\"{HtmlEncoder.Default.Encode(field.ElementId)}\" role=\"group\">{boxes}</div>"));
    }
}
