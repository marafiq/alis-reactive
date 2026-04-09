using Alis.Reactive.Fusion.Templates;

namespace Alis.Reactive.Fusion.UnitTests.Templates;

[TestFixture]
public class WhenBuildingDivTemplates
{
    [Test]
    public void Empty_div_renders_bare_tags()
    {
        var html = FusionTemplate.Create<TemplateTestModel>().Render();
        Assert.That(html, Is.EqualTo("<div></div>"));
    }

    [Test]
    public void Div_with_class_renders_class_attribute()
    {
        var html = FusionTemplate.Create<TemplateTestModel>()
            .Class("p-3 space-y-2")
            .Render();
        Assert.That(html, Is.EqualTo("<div class=\"p-3 space-y-2\"></div>"));
    }

    [Test]
    public void Div_with_multiple_classes_joins_them()
    {
        var html = FusionTemplate.Create<TemplateTestModel>()
            .Class("font-semibold")
            .Class("text-base")
            .Render();
        Assert.That(html, Is.EqualTo("<div class=\"font-semibold text-base\"></div>"));
    }

    [Test]
    public void Div_with_id_renders_id_attribute()
    {
        var html = FusionTemplate.Create<TemplateTestModel>()
            .Id("quickinfo-header")
            .Render();
        Assert.That(html, Is.EqualTo("<div id=\"quickinfo-header\"></div>"));
    }

    [Test]
    public void Div_with_custom_attribute_renders_it()
    {
        var html = FusionTemplate.Create<TemplateTestModel>()
            .Attr("data-role", "header")
            .Render();
        Assert.That(html, Is.EqualTo("<div data-role=\"header\"></div>"));
    }

    [Test]
    public void Span_with_property_binding_renders_camelCase()
    {
        var html = FusionTemplate.Create<TemplateTestModel>()
            .Span(m => m.Subject, "font-semibold")
            .Render();
        Assert.That(html, Is.EqualTo("<div><span class=\"font-semibold\">${subject}</span></div>"));
    }

    [Test]
    public void Span_with_static_text_renders_text()
    {
        var html = FusionTemplate.Create<TemplateTestModel>()
            .Span(" | ")
            .Render();
        Assert.That(html, Is.EqualTo("<div><span> | </span></div>"));
    }

    [Test]
    public void Text_with_property_binding_renders_raw_binding()
    {
        var html = FusionTemplate.Create<TemplateTestModel>()
            .Text(m => m.Description)
            .Render();
        Assert.That(html, Is.EqualTo("<div>${description}</div>"));
    }

    [Test]
    public void Badge_with_text_renders_with_custom_css()
    {
        var html = FusionTemplate.Create<TemplateTestModel>()
            .Badge("NEEDS COVERAGE", "px-2 py-1 rounded text-xs font-bold bg-red-100 text-red-700")
            .Render();
        Assert.That(html, Does.Contain("class=\"px-2 py-1 rounded text-xs font-bold bg-red-100 text-red-700\""));
        Assert.That(html, Does.Contain("NEEDS COVERAGE"));
    }

    [Test]
    public void Badge_with_property_uses_default_e_badge_class()
    {
        var html = FusionTemplate.Create<TemplateTestModel>()
            .Badge(m => m.StaffRole)
            .Render();
        Assert.That(html, Is.EqualTo("<div><span class=\"e-badge\">${staffRole}</span></div>"));
    }

    [Test]
    public void Icon_renders_sf_icon_classes()
    {
        var html = FusionTemplate.Create<TemplateTestModel>()
            .Icon("edit")
            .Render();
        Assert.That(html, Is.EqualTo("<div><span class=\"e-icons e-edit\"></span></div>"));
    }

    [Test]
    public void Icon_with_extra_css_appends_classes()
    {
        var html = FusionTemplate.Create<TemplateTestModel>()
            .Icon("edit", "text-accent")
            .Render();
        Assert.That(html, Is.EqualTo("<div><span class=\"e-icons e-edit text-accent\"></span></div>"));
    }

    [Test]
    public void Img_renders_src_from_property_binding()
    {
        var html = FusionTemplate.Create<TemplateTestModel>()
            .Img(m => m.ProfileUrl, "rounded-full", "Staff photo")
            .Render();
        Assert.That(html, Does.Contain("src=\"${profileUrl}\""));
        Assert.That(html, Does.Contain("class=\"rounded-full\""));
        Assert.That(html, Does.Contain("alt=\"Staff photo\""));
    }

    [Test]
    public void Button_uses_e_btn_base_class_by_default()
    {
        var html = FusionTemplate.Create<TemplateTestModel>()
            .Button("Save", "handleSave()")
            .Render();
        Assert.That(html, Does.Contain("class=\"e-btn\""));
        Assert.That(html, Does.Contain("onclick=\"handleSave()\""));
        Assert.That(html, Does.Contain(">Save</button>"));
    }

    [Test]
    public void Button_with_design_system_css_prepends_e_btn()
    {
        var html = FusionTemplate.Create<TemplateTestModel>()
            .Button("Save", "handleSave()", "e-primary e-small")
            .Render();
        Assert.That(html, Does.Contain("class=\"e-btn e-primary e-small\""));
    }

    [Test]
    public void EventButton_uses_quot_escaping_for_sf_template_engine()
    {
        var html = FusionTemplate.Create<TemplateTestModel>()
            .EventButton("Edit", "schedule:edit", m => m.Id, "e-primary e-small")
            .Render();

        // Must use &quot; not regular quotes — SF template engine converts single quotes to double
        Assert.That(html, Does.Contain("&quot;schedule:edit&quot;"));
        Assert.That(html, Does.Contain("detail:{id:${id}}"));
        Assert.That(html, Does.Contain("class=\"e-btn e-primary e-small\""));
        Assert.That(html, Does.Contain(">Edit</button>"));
    }

    [Test]
    public void EventButton_with_no_css_uses_e_btn_default()
    {
        var html = FusionTemplate.Create<TemplateTestModel>()
            .EventButton("Assign", "schedule:assign", m => m.Id)
            .Render();
        Assert.That(html, Does.Contain("class=\"e-btn\""));
    }

    [Test]
    public void Link_renders_href_and_text_from_property_bindings()
    {
        var html = FusionTemplate.Create<TemplateTestModel>()
            .Link(m => m.ProfileUrl, m => m.Subject, "text-accent underline")
            .Render();
        Assert.That(html, Does.Contain("href=\"${profileUrl}\""));
        Assert.That(html, Does.Contain("class=\"text-accent underline\""));
        Assert.That(html, Does.Contain(">${subject}</a>"));
    }

    [Test]
    public void Nested_div_renders_inside_parent()
    {
        var html = FusionTemplate.Create<TemplateTestModel>()
            .Class("outer")
            .Div(inner => inner.Class("inner").Text(m => m.Subject))
            .Render();
        Assert.That(html, Is.EqualTo(
            "<div class=\"outer\"><div class=\"inner\">${subject}</div></div>"));
    }

    [Test]
    public void Raw_adds_unescaped_html()
    {
        var html = FusionTemplate.Create<TemplateTestModel>()
            .Raw("<hr/>")
            .Render();
        Assert.That(html, Is.EqualTo("<div><hr/></div>"));
    }

    [Test]
    public void ToString_delegates_to_Render()
    {
        var builder = FusionTemplate.Create<TemplateTestModel>().Class("test");
        Assert.That(builder.ToString(), Is.EqualTo(builder.Render()));
    }

    [Test]
    public void When_renders_sf_if_else_template_syntax()
    {
        var html = FusionTemplate.Create<TemplateTestModel>()
            .When(m => m.IsUnassigned,
                then: t => t.Badge("NEEDS COVERAGE", "bg-red-100 text-red-700"),
                @else: e => e.Span(m => m.StaffRole, "font-medium"))
            .Render();

        Assert.That(html, Does.Contain("${if(isUnassigned)}"));
        Assert.That(html, Does.Contain("NEEDS COVERAGE"));
        Assert.That(html, Does.Contain("${else}"));
        Assert.That(html, Does.Contain("${staffRole}"));
        Assert.That(html, Does.Contain("${/if}"));
    }

    [Test]
    public void ShowIf_renders_sf_if_without_else()
    {
        var html = FusionTemplate.Create<TemplateTestModel>()
            .ShowIf(m => m.IsUnassigned, t => t.Badge("URGENT"))
            .Render();

        Assert.That(html, Does.Contain("${if(isUnassigned)}"));
        Assert.That(html, Does.Contain("URGENT"));
        Assert.That(html, Does.Not.Contain("${else}"));
        Assert.That(html, Does.Contain("${/if}"));
    }

    [Test]
    public void Schedule_quickinfo_content_template_renders_correctly()
    {
        // Real-world test: matches the Schedule sandbox view's quickInfoContent template
        var html = FusionTemplate.Create<TemplateTestModel>()
            .Class("p-3 space-y-2")
            .Div(d => d.Class("font-semibold text-base").Text(m => m.Subject))
            .Div(d => d.Class("text-sm text-gray-600").Text(m => m.Description))
            .When(m => m.IsUnassigned,
                then: t => t.Badge("NEEDS COVERAGE", "px-2 py-1 rounded text-xs font-bold bg-red-100 text-red-700"),
                @else: e => e.Div(d => d.Class("text-xs text-gray-500")
                    .Span(m => m.StaffRole, "font-medium")
                    .Span(" | ")
                    .Span(m => m.StaffPhone)))
            .Render();

        Assert.That(html, Does.StartWith("<div class=\"p-3 space-y-2\">"));
        Assert.That(html, Does.Contain("${subject}"));
        Assert.That(html, Does.Contain("${description}"));
        Assert.That(html, Does.Contain("${if(isUnassigned)}"));
        Assert.That(html, Does.Contain("NEEDS COVERAGE"));
        Assert.That(html, Does.Contain("${staffRole}"));
        Assert.That(html, Does.Contain("${staffPhone}"));
        Assert.That(html, Does.EndWith("</div>"));
    }

    [Test]
    public void Schedule_quickinfo_footer_template_renders_event_buttons()
    {
        // Real-world test: matches the Schedule sandbox view's quickInfoFooter template
        var html = FusionTemplate.Create<TemplateTestModel>()
            .Class("flex gap-2 p-2 border-t")
            .When(m => m.IsUnassigned,
                then: t => t
                    .EventButton("Assign Staff", "schedule:edit", m => m.Id, "e-primary e-small"),
                @else: e => e
                    .EventButton("Edit", "schedule:edit", m => m.Id, "e-small")
                    .EventButton("Reassign", "schedule:reassign", m => m.Id, "e-small"))
            .Render();

        Assert.That(html, Does.Contain("class=\"flex gap-2 p-2 border-t\""));
        Assert.That(html, Does.Contain("&quot;schedule:edit&quot;"));
        Assert.That(html, Does.Contain("&quot;schedule:reassign&quot;"));
        Assert.That(html, Does.Contain("Assign Staff"));
        Assert.That(html, Does.Contain("e-btn e-primary e-small"));
    }
}
