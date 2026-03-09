namespace Alis.Reactive.UnitTests;

/// <summary>
/// Tests ExpressionPathHelper.ToElementId() — converts model expressions to DOM element IDs.
/// Element IDs use underscore separators matching ASP.NET Html.IdFor() convention.
/// </summary>
[TestFixture]
public class WhenResolvingElementIds
{
    public class Address
    {
        public string? City { get; set; }
        public string? State { get; set; }
    }

    public class Contact
    {
        public Address? HomeAddress { get; set; }
    }

    public class Model
    {
        public string? Name { get; set; }
        public Address? Address { get; set; }
        public Contact? Contact { get; set; }
    }

    [Test]
    public void Flat_property_produces_simple_id()
    {
        var id = ExpressionPathHelper.ToElementId<Model>(m => m.Name);
        Assert.That(id, Is.EqualTo("Name"));
    }

    [Test]
    public void Nested_property_uses_underscores()
    {
        var id = ExpressionPathHelper.ToElementId<Model>(m => m.Address!.City);
        Assert.That(id, Is.EqualTo("Address_City"));
    }

    [Test]
    public void Two_levels_deep_uses_underscores()
    {
        var id = ExpressionPathHelper.ToElementId<Model>(m => m.Contact!.HomeAddress!.City);
        Assert.That(id, Is.EqualTo("Contact_HomeAddress_City"));
    }

    [Test]
    public void Property_name_differs_from_binding_path()
    {
        var elementId = ExpressionPathHelper.ToElementId<Model>(m => m.Address!.City);
        var bindingPath = ExpressionPathHelper.ToPropertyName<Model>(m => m.Address!.City);

        Assert.That(elementId, Is.EqualTo("Address_City"));
        Assert.That(bindingPath, Is.EqualTo("Address.City"));
    }

    [Test]
    public void Event_path_uses_camel_case_dots()
    {
        var eventPath = ExpressionPathHelper.ToEventPath<Address>(a => a.City);
        Assert.That(eventPath, Is.EqualTo("evt.city"));
    }

    [Test]
    public void Nested_event_path_uses_camel_case_dots()
    {
        var eventPath = ExpressionPathHelper.ToEventPath<Contact>(c => c.HomeAddress!.City);
        Assert.That(eventPath, Is.EqualTo("evt.homeAddress.city"));
    }
}
