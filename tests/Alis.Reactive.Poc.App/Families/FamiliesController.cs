using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Families;

public sealed class FamiliesController : Controller
{
    [HttpGet]
    public IActionResult Family() => View(Helen());

    [HttpPost]
    public IActionResult Family(FamilyForm form) => View("FamilySaved", form);

    public IActionResult FamilyByTagHelper() => View(Helen());

    public IActionResult FamilyByEditor() => View(Helen());

    public IActionResult ContactsByForeach() => View(Helen());

    public IActionResult MobileContact() => View(Helen());

    public IActionResult FamilyByNamedTemplate() => View(Helen());

    public IActionResult FamilyByHint() => View(Helen());

    public IActionResult ContactsByEditor() => View(Helen());

    public IActionResult FamilyBySlot() => View(Helen());

    public IActionResult FamilyPrefixMovedInsidePlan() => View(Helen());

    public IActionResult NextOfKinWithPrefix()
    {
        ViewData.TemplateInfo.HtmlFieldPrefix = "NextOfKin";
        return PartialView("_NextOfKin", Helen().NextOfKin);
    }

    private static FamilyForm Helen() => new()
    {
        ResidentName = "Helen Park",
        NextOfKin = new Relative { Name = "Sam Park", Address = new Address { City = "York" } },
        Contacts = [new Contact { Kind = "Home", Phone = "01632 960001" }, new Contact { Kind = "Mobile", Phone = "07700 900123" }],
    };
}

public sealed class FamilyForm
{
    public string ResidentName { get; set; } = "";

    public Relative NextOfKin { get; set; } = new();

    public List<Contact> Contacts { get; set; } = [];

    [UIHint("Guardian")]
    public Relative? Guardian { get; set; }
}

public sealed class Relative
{
    public string Name { get; set; } = "";

    public Address Address { get; set; } = new();
}

public sealed class Address
{
    public string City { get; set; } = "";
}

public sealed class Contact
{
    public string Kind { get; set; } = "";

    public string Phone { get; set; } = "";
}

public sealed class VisitRequest
{
    public string ResidentName { get; set; } = "";
}
