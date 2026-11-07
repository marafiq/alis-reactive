namespace Alis.Reactive.Poc.PlaywrightTests.Inputs;

[TestFixture]
public sealed class WhenAnInputIsBound : AppPageTest
{
    [Test]
    public async Task a_text_box_shows_the_value_its_member_holds()
    {
        await Open("/Admissions/Intake");

        await Expect(Page.GetByLabel("Resident name")).ToHaveValueAsync("Helen Park");
    }

    [Test]
    public async Task a_component_whose_value_member_is_not_value_shows_the_ward_its_member_holds()
    {
        await Open("/Admissions/Intake");

        await Expect(Page.GetByRole(AriaRole.Group, new() { Name = "Ward" }).Locator("p")).ToHaveTextAsync("ward South");
    }

    [Test]
    public async Task an_inputs_id_is_the_class_that_declares_its_member_then_its_path()
    {
        await Open("/Admissions/Intake");

        await Expect(Page.Locator("#Alis_Reactive_Poc_App_Admissions_AdmissionBase__ResidentName")).ToHaveValueAsync("Helen Park");
    }

    [Test]
    public async Task a_member_a_nested_class_declares_takes_the_nested_class_in_its_id()
    {
        await Open("/Admissions/Intake");

        await Expect(Page.Locator("#Alis_Reactive_Poc_App_Admissions_IntakeForm_Kin__NextOfKin_Name")).ToHaveValueAsync("Sam Park");
    }

    [Test]
    public async Task a_list_item_takes_the_class_that_declares_its_list_and_its_position_in_its_id()
    {
        await Open("/Admissions/Allergies");

        await Expect(Page.Locator("#Alis_Reactive_Poc_App_Admissions_IntakeForm__Allergies_0_")).ToHaveValueAsync("penicillin");
    }

    [Test]
    public async Task an_array_item_takes_the_class_that_declares_its_array_and_its_position_in_its_id()
    {
        await Open("/Admissions/Aids");

        await Expect(Page.Locator("#Alis_Reactive_Poc_App_Admissions_IntakeForm__Aids_1_")).ToHaveValueAsync("hearing aid");
    }

    [Test]
    public async Task a_saved_form_binds_a_component_under_its_members_name_not_its_json_name()
    {
        await Open("/Admissions/Intake");
        await Page.GetByRole(AriaRole.Button, new() { Name = "East" }).ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save intake" }).ClickAsync();

        await Expect(Page.Locator("#saved-ward")).ToHaveTextAsync("East");
    }

    [Test]
    public async Task a_pipeline_reads_a_text_box_by_its_member()
    {
        await Open("/Admissions/Intake");
        await Page.GetByLabel("Resident name").FillAsync("Ada Byron");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Preview the door card" }).ClickAsync();

        await Expect(Page.Locator("#card-resident")).ToHaveTextAsync("Ada Byron");
    }

    [Test]
    public async Task a_pipeline_reads_the_input_of_a_member_a_nested_class_declares()
    {
        await Open("/Admissions/Intake");
        await Page.GetByLabel("Next of kin").FillAsync("Max Park");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Preview the door card" }).ClickAsync();

        await Expect(Page.Locator("#card-kin")).ToHaveTextAsync("Max Park");
    }

    [Test]
    public async Task a_pipeline_reads_a_components_member_that_is_not_value_by_the_member_it_binds()
    {
        await Open("/Admissions/Intake");

        await Page.GetByRole(AriaRole.Button, new() { Name = "East" }).ClickAsync();

        await Expect(Page.Locator("#chosen-ward")).ToHaveTextAsync("East");
    }
}
