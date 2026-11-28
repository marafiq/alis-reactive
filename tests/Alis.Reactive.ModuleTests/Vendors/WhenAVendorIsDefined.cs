using System;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Vendors;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Vendors;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAVendorIsDefined
{

    [Test]
    public void a_path_and_method_pair_find_and_hear_the_component()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<PathBell>("bell", new HtmlString("<bell>Ward</bell>"));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"bell":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"get","path":["devices",2,"bell"],"members":"required","target":{"kind":"ref","name":"element"}}},"listen":{"kind":"method","subscribe":"hear","unsubscribe":"forget"}}},"objects":{"bell":{"kind":"element","id":"bell","vendor":"bell"}},"handlers":[]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_callback_property_hears_the_component()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<CallbackBell>("bell", new HtmlString("<bell>Ward</bell>"));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"bell":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"get","path":["devices",2,"bell"],"members":"required","target":{"kind":"ref","name":"element"}}},"listen":{"kind":"assignment"}}},"objects":{"bell":{"kind":"element","id":"bell","vendor":"bell"}},"handlers":[]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_returned_unsubscriber_hears_the_component()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<ReturnedBell>("bell", new HtmlString("<bell>Ward</bell>"));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"bell":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"get","path":["devices",2,"bell"],"members":"required","target":{"kind":"ref","name":"element"}}},"listen":{"kind":"returned","subscribe":"watch"}}},"objects":{"bell":{"kind":"element","id":"bell","vendor":"bell"}},"handlers":[]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_lookup_and_method_pair_find_and_hear_the_component()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<PluginBell>("bell", new HtmlString("<bell>Ward</bell>"));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"bell":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"call","path":[],"members":"required","target":{"kind":"obj","id":"$plugin:ward-bells"},"args":[{"kind":"ref","name":"element"}]}},"listen":{"kind":"method","subscribe":"hear","unsubscribe":"forget"}}},"objects":{"$plugin:ward-bells":{"kind":"plugin","name":"ward-bells","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"bell":{"kind":"element","id":"bell","vendor":"bell"}},"handlers":[]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_lookup_can_hear_a_callback_property()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<PluginCallbackBell>("bell", new HtmlString("<bell>Ward</bell>"));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"bell":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"call","path":[],"members":"required","target":{"kind":"obj","id":"$plugin:ward-bells"},"args":[{"kind":"ref","name":"element"}]}},"listen":{"kind":"assignment"}}},"objects":{"$plugin:ward-bells":{"kind":"plugin","name":"ward-bells","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"bell":{"kind":"element","id":"bell","vendor":"bell"}},"handlers":[]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_lookup_can_hear_a_returned_unsubscriber()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<PluginReturnedBell>("bell", new HtmlString("<bell>Ward</bell>"));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"bell":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"call","path":[],"members":"required","target":{"kind":"obj","id":"$plugin:ward-bells"},"args":[{"kind":"ref","name":"element"}]}},"listen":{"kind":"returned","subscribe":"watch"}}},"objects":{"$plugin:ward-bells":{"kind":"plugin","name":"ward-bells","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"bell":{"kind":"element","id":"bell","vendor":"bell"}},"handlers":[]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void an_empty_path_locates_the_element_itself()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<ElementBell>("bell", HtmlString.Empty);
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"element-bell":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"bell":{"kind":"element","id":"bell","vendor":"element-bell"}},"handlers":[]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_vendor_is_written_as_its_name()
    {
        Assert.That(ComponentVendor.Define("Ward_Bell-2", "", "on", "off").ToString(), Is.EqualTo("Ward_Bell-2"));
        Assert.That(ComponentVendor.Native.ToString(), Is.EqualTo("native"));
    }
    [TestCase("", "Vendor name '' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen.")]
    [TestCase("9bell", "Vendor name '9bell' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen.")]
    [TestCase("_bell", "Vendor name '_bell' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen.")]
    [TestCase("-bell", "Vendor name '-bell' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen.")]
    [TestCase("ébell", "Vendor name 'ébell' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen.")]
    [TestCase("bell.room", "Vendor name 'bell.room' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen.")]
    [TestCase("bell room", "Vendor name 'bell room' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen.")]
    [TestCase("bellé", "Vendor name 'bellé' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen.")]

    public void a_vendor_name_uses_ascii_letters_then_letters_digits_underscores_or_hyphens(string name, string message)
    {
        Assert.That(() => ComponentVendor.Define(name, "", "on", "off"), Throws.InvalidOperationException.With.Message.EqualTo(message));
    }

    [TestCase("B")]
    [TestCase("b")]
    [TestCase("aZ_09-")]
    public void a_name_inside_the_declared_characters_is_accepted(string name)
    {
        Assert.That(() => ComponentVendor.Define(name, "", "on", "off"), Throws.Nothing);
    }

    [TestCase(".bell", "Path '.bell' contains an empty segment at index 0. Use dot-separated property names without consecutive, leading, or trailing dots.")]
    [TestCase("bell..ring", "Path 'bell..ring' contains an empty segment at index 1. Use dot-separated property names without consecutive, leading, or trailing dots.")]
    [TestCase("bell.", "Path 'bell.' contains an empty segment at index 1. Use dot-separated property names without consecutive, leading, or trailing dots.")]
    public void method_path_refuses_an_empty_object_path_segment(string text, string message)
    {
        Assert.That(() => ComponentVendor.Define("bell", text, "on", "off"), Throws.InvalidOperationException.With.Message.EqualTo(message));
    }

    [TestCase(".bell", "Path '.bell' contains an empty segment at index 0. Use dot-separated property names without consecutive, leading, or trailing dots.")]
    [TestCase("bell..ring", "Path 'bell..ring' contains an empty segment at index 1. Use dot-separated property names without consecutive, leading, or trailing dots.")]
    [TestCase("bell.", "Path 'bell.' contains an empty segment at index 1. Use dot-separated property names without consecutive, leading, or trailing dots.")]
    public void callback_path_refuses_an_empty_object_path_segment(string text, string message)
    {
        Assert.That(() => ComponentVendor.DefineCallbackProperties("bell", text), Throws.InvalidOperationException.With.Message.EqualTo(message));
    }

    [TestCase(".bell", "Path '.bell' contains an empty segment at index 0. Use dot-separated property names without consecutive, leading, or trailing dots.")]
    [TestCase("bell..ring", "Path 'bell..ring' contains an empty segment at index 1. Use dot-separated property names without consecutive, leading, or trailing dots.")]
    [TestCase("bell.", "Path 'bell.' contains an empty segment at index 1. Use dot-separated property names without consecutive, leading, or trailing dots.")]
    public void returned_path_refuses_an_empty_object_path_segment(string text, string message)
    {
        Assert.That(() => ComponentVendor.DefineReturnedUnsubscriber("bell", text, "watch"), Throws.InvalidOperationException.With.Message.EqualTo(message));
    }

    [Test]
    public void method_path_refuses_an_empty_element_path_segment()
    {
        var path = "host.";
        Assert.That(() => ComponentVendor.Define("bell", "", "on", "off", elementPath: path),
            Throws.InvalidOperationException.With.Message.EqualTo("Path 'host.' contains an empty segment at index 1. Use dot-separated property names without consecutive, leading, or trailing dots."));
    }

    [Test]
    public void callback_path_refuses_an_empty_element_path_segment()
    {
        var path = "host.";
        Assert.That(() => ComponentVendor.DefineCallbackProperties("bell", "", elementPath: path),
            Throws.InvalidOperationException.With.Message.EqualTo("Path 'host.' contains an empty segment at index 1. Use dot-separated property names without consecutive, leading, or trailing dots."));
    }

    [Test]
    public void returned_path_refuses_an_empty_element_path_segment()
    {
        var path = "host.";
        Assert.That(() => ComponentVendor.DefineReturnedUnsubscriber("bell", "", "watch", elementPath: path),
            Throws.InvalidOperationException.With.Message.EqualTo("Path 'host.' contains an empty segment at index 1. Use dot-separated property names without consecutive, leading, or trailing dots."));
    }

    [Test]
    public void method_lookup_refuses_an_empty_element_path_segment()
    {
        var path = "host.";
        Assert.That(() => ComponentVendor.Define("bell", VendorLookup.ByPlugin("ward-bells"), "on", "off", elementPath: path),
            Throws.InvalidOperationException.With.Message.EqualTo("Path 'host.' contains an empty segment at index 1. Use dot-separated property names without consecutive, leading, or trailing dots."));
    }

    [Test]
    public void callback_lookup_refuses_an_empty_element_path_segment()
    {
        var path = "host.";
        Assert.That(() => ComponentVendor.DefineCallbackProperties("bell", VendorLookup.ByPlugin("ward-bells"), elementPath: path),
            Throws.InvalidOperationException.With.Message.EqualTo("Path 'host.' contains an empty segment at index 1. Use dot-separated property names without consecutive, leading, or trailing dots."));
    }

    [Test]
    public void returned_lookup_refuses_an_empty_element_path_segment()
    {
        var path = "host.";
        Assert.That(() => ComponentVendor.DefineReturnedUnsubscriber("bell", VendorLookup.ByPlugin("ward-bells"), "watch", elementPath: path),
            Throws.InvalidOperationException.With.Message.EqualTo("Path 'host.' contains an empty segment at index 1. Use dot-separated property names without consecutive, leading, or trailing dots."));
    }
}
