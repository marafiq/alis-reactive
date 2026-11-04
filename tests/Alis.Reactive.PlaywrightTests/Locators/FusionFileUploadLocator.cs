using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionFileUploadLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionFileUploadLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    public ILocator Root => _page.Locator($".e-upload:has(#{_componentId})");

    public ILocator Choices => Root.Locator(".e-upload-file-list");

    public ILocator InvalidChoices => Root.Locator(".e-upload-file-list.e-file-invalid");

    public async Task Choose(params FilePayload[] files) => await Input.SetInputFilesAsync(files);

    public async Task Remove(string fileName) =>
        await Choices.Filter(new() { HasText = fileName }).Locator(".e-file-remove-btn").ClickAsync();
}
