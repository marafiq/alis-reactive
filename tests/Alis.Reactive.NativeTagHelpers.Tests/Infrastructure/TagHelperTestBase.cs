using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Tests.Infrastructure;

public abstract class TagHelperTestBase
{
    protected static TagHelperContext CreateContext(string tagName = "div")
    {
        return new TagHelperContext(
            tagName: tagName,
            allAttributes: new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: Guid.NewGuid().ToString());
    }

    protected static TagHelperOutput CreateOutput(string tagName = "div")
    {
        return new TagHelperOutput(
            tagName: tagName,
            attributes: new TagHelperAttributeList(),
            getChildContentAsync: (useCachedResult, encoder) =>
            {
                var content = new DefaultTagHelperContent();
                return Task.FromResult<TagHelperContent>(content);
            });
    }
}
