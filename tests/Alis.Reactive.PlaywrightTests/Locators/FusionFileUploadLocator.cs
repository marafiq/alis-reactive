using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

/// <summary>A Syncfusion Uploader rendered for a form (files are sent with the form, not uploaded on their own).</summary>
public sealed class FusionFileUploadLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionFileUploadLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The file input the file dialog fills.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    /// <summary>The uploader around the input: browse button, drop area and the list of chosen files.</summary>
    public ILocator Root => _page.Locator($".e-upload:has(#{_componentId})");

    /// <summary>The uploader's list: in a form it shows one item for each choice, naming every file chosen.</summary>
    public ILocator Choices => Root.Locator(".e-upload-file-list");

    /// <summary>A choice the uploader marks invalid, such as a file of a kind it does not take.</summary>
    public ILocator InvalidChoices => Root.Locator(".e-upload-file-list.e-file-invalid");

    /// <summary>Chooses files the way the file dialog does.</summary>
    public async Task Choose(params FilePayload[] files) => await Input.SetInputFilesAsync(files);

    /// <summary>Removes the choice that lists the file, by clicking its remove icon.</summary>
    public async Task Remove(string fileName) =>
        await Choices.Filter(new() { HasText = fileName }).Locator(".e-file-remove-btn").ClickAsync();
}
