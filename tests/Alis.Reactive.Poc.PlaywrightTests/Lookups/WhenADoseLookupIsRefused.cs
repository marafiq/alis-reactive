namespace Alis.Reactive.Poc.PlaywrightTests.Lookups;

[TestFixture]
public sealed class WhenADoseLookupIsRefused : AppPageTest
{
    [Test]
    public async Task a_collection_that_can_compare_by_its_own_rules_is_refused_with_the_array_door()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/LookedUpInACaseBlindSet");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'wardSet.Contains(d.Ward)' looks in a collection that can compare by its own rules, as a HashSet's comparer does; copy it into an array before the lambda, such as var wards = Model.Wards.ToArray();. (Parameter 'predicate')");
    }

    [Test]
    public async Task a_hash_sets_own_contains_is_refused_with_the_array_door()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/LookedUpInAHashSet");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'wardSet.Contains(d.Ward)' looks in a collection that can compare by its own rules, as a HashSet's comparer does; copy it into an array before the lambda, such as var wards = Model.Wards.ToArray();. (Parameter 'predicate')");
    }

    [Test]
    public async Task contains_over_a_list_the_item_holds_is_refused_with_the_any_door()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/LookedUpInADosesHours");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'd.Hours.Contains(8)' looks for a value in a list the item holds; use Any, such as d.Hours.Any(hour => hour == 8). (Parameter 'predicate')");
    }

    [Test]
    public async Task contains_of_an_object_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/LookedUpAnObject");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'leads.Contains(d.Carer)' looks for an object, and the browser compares objects by reference; look for a member, such as names.Contains(d.Carer.Name). (Parameter 'predicate')");
    }

    [Test]
    public async Task contains_over_a_list_that_is_null_when_the_view_renders_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/LookedUpInANullList");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'none.Contains(d.Ward)' looks in a list that is null when the view renders. (Parameter 'predicate')");
    }

    [Test]
    public async Task contains_with_an_equality_comparer_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/LookedUpWithAComparer");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'given.Contains(d.Status)' passes an equality comparer, which the browser does not have; look up without it, or move the work to the server. (Parameter 'predicate')");
    }
}
