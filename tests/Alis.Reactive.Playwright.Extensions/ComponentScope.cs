using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Scoped entry point for locating Alis.Reactive components on a page.
/// Knows the IdGenerator pattern: {TypeScope}__{PropertyName}
///
/// Usage:
///   var scope = new ComponentScope(Page, typeof(ResidentModel));
///   var physician = scope.AutoComplete("Physician");
///   await physician.TypeAndSelect("smi", "Dr. Smith");
///   await Expect(scope.Element("echo")).ToContainTextAsync("smith");
/// </summary>
public sealed class ComponentScope
{
    private readonly IPage _page;
    private readonly string _prefix;

    public ComponentScope(IPage page, Type modelType)
    {
        _page = page;
        _prefix = modelType.FullName!.Replace(".", "_") + "__";
    }

    public ComponentScope(IPage page, string prefix)
    {
        _page = page;
        _prefix = prefix;
    }

    /// <summary>The generated element ID for a model property.</summary>
    public string IdFor(string propertyName) => _prefix + propertyName;

    // ─── Component Locators ───

    /// <summary>FusionAutoComplete interaction primitives.</summary>
    public AutoCompleteLocator AutoComplete(string propertyName)
        => new(_page, IdFor(propertyName));

    /// <summary>FusionDropDownList interaction primitives.</summary>
    public DropDownListLocator DropDownList(string propertyName)
        => new(_page, IdFor(propertyName));

    /// <summary>FusionNumericTextBox interaction primitives.</summary>
    public NumericTextBoxLocator NumericTextBox(string propertyName)
        => new(_page, IdFor(propertyName));

    /// <summary>FusionDatePicker interaction primitives.</summary>
    public DatePickerLocator DatePicker(string propertyName)
        => new(_page, IdFor(propertyName));

    /// <summary>FusionTimePicker interaction primitives.</summary>
    public TimePickerLocator TimePicker(string propertyName)
        => new(_page, IdFor(propertyName));

    /// <summary>FusionDateTimePicker interaction primitives.</summary>
    public DateTimePickerLocator DateTimePicker(string propertyName)
        => new(_page, IdFor(propertyName));

    /// <summary>FusionDateRangePicker interaction primitives.</summary>
    public DateRangePickerLocator DateRangePicker(string propertyName)
        => new(_page, IdFor(propertyName));

    /// <summary>FusionMultiColumnComboBox interaction primitives.</summary>
    public MultiColumnComboBoxLocator MultiColumnComboBox(string propertyName)
        => new(_page, IdFor(propertyName));

    /// <summary>FusionInputMask interaction primitives.</summary>
    public InputMaskLocator InputMask(string propertyName)
        => new(_page, IdFor(propertyName));

    /// <summary>FusionRichTextEditor interaction primitives.</summary>
    public RichTextEditorLocator RichTextEditor(string propertyName)
        => new(_page, IdFor(propertyName));

    /// <summary>FusionMultiSelect interaction primitives.</summary>
    public MultiSelectLocator MultiSelect(string propertyName)
        => new(_page, IdFor(propertyName));

    // ─── Page Surfaces ───

    /// <summary>Any element by raw ID — for status spans, echo divs, results.</summary>
    public ILocator Element(string elementId) => _page.Locator($"#{elementId}");
}
