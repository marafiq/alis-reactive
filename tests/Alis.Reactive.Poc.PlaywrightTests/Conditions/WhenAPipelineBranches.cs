namespace Alis.Reactive.Poc.PlaywrightTests.Conditions;

[TestFixture]
public sealed class WhenAPipelineBranches : AppPageTest
{
    [Test]
    public async Task the_first_arm_whose_condition_holds_runs_and_no_other()
    {
        await Open("/Balances/Branches");
        await Balance("Add 500 ml").ClickAsync();
        await Balance("Add 100 ml").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Assess the intake" }).ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("high");
    }

    [Test]
    public async Task an_else_if_runs_when_its_condition_alone_holds()
    {
        await Open("/Balances/Branches");
        await Balance("Add 100 ml").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Assess the intake" }).ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("normal");
    }

    [Test]
    public async Task else_runs_when_no_condition_holds()
    {
        await Open("/Balances/Branches");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Assess the intake" }).ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("none");
    }

    [Test]
    public async Task a_branch_runs_all_its_steps_in_order()
    {
        await Open("/Balances/ThreeSteps");
        await Balance("Weigh 70 kg").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Record the weighed intake" }).ClickAsync();

        await Expect(Page.Locator("#log")).ToHaveTextAsync("three");
        await Expect(Page.Locator("#balance .balance-intake")).ToHaveTextAsync("250 ml");
    }

    [Test]
    public async Task a_branch_whose_condition_orders_a_null_runs_none_of_its_steps()
    {
        await Open("/Balances/ThreeSteps");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Record the weighed intake" }).ClickAsync();

        await Expect(Page.Locator("#balance .balance-intake")).ToHaveTextAsync("0 ml");
        await Expect(Page.Locator("#log")).ToHaveTextAsync("");
    }

    [Test]
    public async Task and_holds_only_when_both_its_terms_hold()
    {
        await Open("/Balances/BothHold");
        await Balance("Weigh 70 kg").ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Assess the weight and the intake" }).ClickAsync();
        await Expect(Page.Locator("#both")).ToHaveTextAsync("not both");

        await Balance("Add 100 ml").ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Assess the weight and the intake" }).ClickAsync();

        await Expect(Page.Locator("#both")).ToHaveTextAsync("both");
    }

    [Test]
    public async Task the_steps_after_a_branch_that_awaits_run_once_its_method_settles()
    {
        await Open("/Balances/Rechecks");
        await Balance("Add 100 ml").ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Recheck a balance with intake" }).ClickAsync();
        await Expect(Page.Locator("#balance .balance-recheck")).ToHaveTextAsync("rechecking");
        await Expect(Page.Locator("#after")).ToHaveTextAsync("waiting");

        await Balance("Finish the recheck").ClickAsync();

        await Expect(Page.Locator("#after")).ToHaveTextAsync("done");
    }

    [Test]
    public async Task the_steps_after_a_branch_not_taken_run_at_once()
    {
        await Open("/Balances/Rechecks");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Recheck a balance with intake" }).ClickAsync();

        await Expect(Page.Locator("#after")).ToHaveTextAsync("done");
        await Expect(Page.Locator("#balance .balance-recheck")).ToHaveTextAsync("idle");
    }

    private ILocator Balance(string button) => Page.Locator("#balance").GetByRole(AriaRole.Button, new() { Name = button });
}
