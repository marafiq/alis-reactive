using System.IO;
using System.Text.Encodings.Web;
using Alis.Reactive.Native.Builders;

namespace Alis.Reactive.Native.UnitTests;

[TestFixture]
public class WhenRenderingAField
{
    [Test]
    public void Renders_label_with_for_attribute()
    {
        var output = RenderField("Full Name", false, "FullName");
        Assert.That(output, Does.Contain("<label"));
        Assert.That(output, Does.Contain("for=\"FullName\""));
        Assert.That(output, Does.Contain("Full Name"));
    }

    [Test]
    public void Renders_required_marker_when_required()
    {
        var output = RenderField("Email", true, "Email");
        Assert.That(output, Does.Contain("text-danger"));
        Assert.That(output, Does.Contain("*"));
    }

    [Test]
    public void Does_not_render_required_marker_when_optional()
    {
        var output = RenderField("Notes", false, "Notes");
        Assert.That(output, Does.Not.Contain("<span class=\"text-danger ml-0.5\">*</span>"));
    }

    [Test]
    public void Renders_validation_placeholder()
    {
        var output = RenderField("Status", true, "Status");
        Assert.That(output, Does.Contain("data-valmsg-for=\"Status\""));
    }

    [Test]
    public void Renders_wrapper_div_with_flex_layout()
    {
        var output = RenderField("Name", false, "Name");
        Assert.That(output, Does.StartWith("<div class=\"flex flex-col gap-1.5\">"));
        Assert.That(output, Does.EndWith("</div>"));
    }

    [Test]
    public void Renders_child_content_between_label_and_validation()
    {
        var writer = new StringWriter();
        var b = new FieldBuilder(writer, "Test").Label("Test").ForId("Test");
        using (b.Begin()) { writer.Write("<select id=\"Test\"></select>"); }
        var output = writer.ToString();

        Assert.That(output, Does.Contain("<select id=\"Test\"></select>"));
        // child content appears between label and validation span
        var selectIndex = output.IndexOf("<select");
        var labelEnd = output.IndexOf("</label>") + "</label>".Length;
        var valStart = output.IndexOf("data-valmsg-for");
        Assert.That(selectIndex, Is.GreaterThanOrEqualTo(labelEnd));
        Assert.That(selectIndex, Is.LessThan(valStart));
    }

    private static string RenderField(string label, bool required, string name)
    {
        var writer = new StringWriter();
        var b = new FieldBuilder(writer, name).Label(label).ForId(name);
        if (required) b.Required();
        using (b.Begin()) { writer.Write("<input />"); }
        return writer.ToString();
    }
}
