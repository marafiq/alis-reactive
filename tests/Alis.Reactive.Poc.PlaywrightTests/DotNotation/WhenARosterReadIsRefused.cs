namespace Alis.Reactive.Poc.PlaywrightTests.DotNotation;

[TestFixture]
public sealed class WhenARosterReadIsRefused : AppPageTest
{
    [Test]
    public async Task a_read_of_data_that_is_not_a_path_of_members_is_refused_with_its_lambda()
    {
        await Page.GotoAsync(BaseUrl + "/Rosters/ReadOfACall");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'nurse => nurse.Name.ToUpper()' is not a path of members of its value, such as n => n.Pager.Number. (Parameter 'path')");
    }

    [Test]
    public async Task a_read_of_a_member_the_apps_json_options_do_not_write_is_refused_with_its_type()
    {
        await Page.GotoAsync(BaseUrl + "/Rosters/ReadOfAnUnwrittenMember");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'nurse => nurse.Ward' reads a member of 'Alis.Reactive.Poc.App.Components.OnCallNurse' that the app's JSON options do not write, so the browser never holds it. (Parameter 'path')");
    }

    [Test]
    public async Task a_read_of_an_override_the_apps_json_options_ignore_is_refused_with_its_type()
    {
        await Page.GotoAsync(BaseUrl + "/Rosters/ReadOfAnIgnoredOverride");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'nurse => nurse.Station' reads a member of 'Alis.Reactive.Poc.App.Components.OnCallNurse' that the app's JSON options do not write, so the browser never holds it. (Parameter 'path')");
    }
}
