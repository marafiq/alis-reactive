using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Locates model-bound components by their generated component IDs.
/// </summary>
/// <remarks>
/// Component IDs use the framework's <c>{TypeScope}__{PropertyName}</c> format.
/// </remarks>
public sealed class ComponentScope
{
    private readonly IPage _page;
    private readonly string _componentIdPrefix;

    /// <summary>Model-type prefix for generated component IDs.</summary>
    public ComponentScope(IPage page, Type modelType)
    {
        _page = page;
        _componentIdPrefix = modelType.FullName!.Replace(".", "_") + "__";
    }

    /// <summary>Precomputed component ID prefix, including its separator.</summary>
    public ComponentScope(IPage page, string prefix)
    {
        _page = page;
        _componentIdPrefix = prefix;
    }

    public string IdFor(string propertyName) => _componentIdPrefix + propertyName;

    public AutoCompleteLocator AutoComplete(string propertyName)
        => new(_page, IdFor(propertyName));

    public DropDownListLocator DropDownList(string propertyName)
        => new(_page, IdFor(propertyName));

    public NumericTextBoxLocator NumericTextBox(string propertyName)
        => new(_page, IdFor(propertyName));

    public DatePickerLocator DatePicker(string propertyName)
        => new(_page, IdFor(propertyName));

    public TimePickerLocator TimePicker(string propertyName)
        => new(_page, IdFor(propertyName));

    public DateTimePickerLocator DateTimePicker(string propertyName)
        => new(_page, IdFor(propertyName));

    public DateRangePickerLocator DateRangePicker(string propertyName)
        => new(_page, IdFor(propertyName));

    public MultiColumnComboBoxLocator MultiColumnComboBox(string propertyName)
        => new(_page, IdFor(propertyName));

    public InputMaskLocator InputMask(string propertyName)
        => new(_page, IdFor(propertyName));

    public RichTextEditorLocator RichTextEditor(string propertyName)
        => new(_page, IdFor(propertyName));

    public MultiSelectLocator MultiSelect(string propertyName)
        => new(_page, IdFor(propertyName));

    public ILocator Element(string elementId) => _page.Locator($"#{elementId}");
}
