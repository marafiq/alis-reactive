using System.Collections.Generic;
using System.Text.Json;
using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Vendors;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.InputsAndIds;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAnInputsValueIsAListOfObjects
{
    [Test]
    public void over_a_member_that_holds_one_file_the_input_reads_the_first_object_the_list_holds()
    {
        var html = Views.Of(new CarePlanForm());
        var plan = html.ReactivePlan<CarePlanForm>();
        html.InputField(plan, m => m.CarePlan).Render<FileDrop>(new HtmlString("<input>"));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        using var document = JsonDocument.Parse(json);
        var inputs = document.RootElement.GetProperty("objects").GetProperty("$model").GetProperty("data").GetProperty("inputs");

        Assert.That(inputs.GetProperty("Alis_Reactive_ModuleTests_InputsAndIds_CarePlanForm__CarePlan").GetProperty("value").GetRawText(),
            Is.EqualTo("""{"kind":"op","op":"coalesce","args":[{"kind":"as","expr":{"kind":"get","path":[0],"members":"optional","target":{"kind":"call","path":["map"],"members":"requiredIfHeld","target":{"kind":"call","path":["getFilesData"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_CarePlanForm__CarePlan"},"args":[]},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"get","path":["rawFile"],"members":"required","target":{"kind":"ref","name":"liveItem1"}}}]}},"shape":{"kind":"nullable","inner":{"kind":"any"}}},{"kind":"lit","value":null}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void over_a_member_that_lists_files_the_input_reads_the_whole_list()
    {
        var html = Views.Of(new CarePlanForm());
        var plan = html.ReactivePlan<CarePlanForm>();
        html.InputField(plan, m => m.Scans).Render<FileDrop>(new HtmlString("<input>"));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        using var document = JsonDocument.Parse(json);
        var inputs = document.RootElement.GetProperty("objects").GetProperty("$model").GetProperty("data").GetProperty("inputs");

        Assert.That(inputs.GetProperty("Alis_Reactive_ModuleTests_InputsAndIds_CarePlanForm__Scans").GetProperty("value").GetRawText(),
            Is.EqualTo("""{"kind":"op","op":"coalesce","args":[{"kind":"as","expr":{"kind":"call","path":["map"],"members":"requiredIfHeld","target":{"kind":"call","path":["getFilesData"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_CarePlanForm__Scans"},"args":[]},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"get","path":["rawFile"],"members":"required","target":{"kind":"ref","name":"liveItem1"}}}]},"shape":{"kind":"nullable","inner":{"kind":"array","item":{"kind":"any"}}}},{"kind":"lit","value":[]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }
}

public sealed class CarePlanForm
{
    public IFormFile? CarePlan { get; set; }
    public List<IFormFile> Scans { get; set; } = [];
}

public sealed class FileDrop : Component, IInputComponent
{
    public override ComponentVendor Vendor => ComponentVendor.Native;
    public LiveObjects<BrowserFile> Files => GetFilesData().Select(file => file.RawFile);
    public LiveObjects<DroppedFile> GetFilesData() => ReturnedObjects<DroppedFile>();
    public Event FilesChanged => Event();
    ComponentInput IInputComponent.Input => ComponentInput.Of(Files, committedBy: FilesChanged);
}

public sealed class DroppedFile : JsObject
{
    public BrowserFile RawFile => LiveObject<BrowserFile>(nullable: true);
}
