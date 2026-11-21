using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.Vendors;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Care;

[Mirrors("Care/AdmissionClinical/admission-form-designer", "FormBuilder")]
public sealed class AdmissionFormBuilder : Component
{
    public static ComponentVendor Declared { get; } = ComponentVendor.DefineCallbackProperties("admission-form-builder", "ej2_instances.0", "element");

    public override ComponentVendor Vendor => Declared;

    public Event<FieldEdit> OnFieldPropertyChanged => Event<FieldEdit>();

    [Mirrors("Care/AdmissionClinical/admission-form-designer", "FieldEdit")]
    public sealed class FieldEdit : JsObject
    {
        public ReadOnly<string> PropertyName => ReadOnly<string>();
        public ReadOnly<string> FieldName => ReadOnly<string>();
        public ReadOnly<bool?> RequiredValue => ReadOnly<bool?>(name: "propertyValue");
    }
}

[Mirrors("Care/AdmissionClinical/admission-form-designer", "FormRenderer")]
public sealed class AdmissionFormRenderer : Component
{
    public override ComponentVendor Vendor => AdmissionFormBuilder.Declared;

    public Event<Submission> Submit => Event<Submission>();

    [Mirrors("Care/AdmissionClinical/admission-form-designer", "Submission")]
    public sealed class Submission : JsObject
    {
        public ReadOnly<AdmissionAnswers> Data => ReadOnly<AdmissionAnswers>();
    }

    public sealed record AdmissionAnswers([property: JsonPropertyName("arrivalRoute")] string ArrivalRoute);
}

public static class AdmissionFormHtml
{
    public static Rendered<AdmissionFormBuilder, TModel> AdmissionFormBuilder<TModel>(this ReactivePlan<TModel> plan, string id, AdmissionSchema? schema)
    {
        var host = new TagBuilder("div");
        host.Attributes["id"] = id;
        host.Attributes["data-admission-form-builder"] = plan.Json(new DesignerOptions(schema, true));
        return plan.Render<AdmissionFormBuilder>(id, host);
    }

    public static Rendered<AdmissionFormRenderer, TModel> AdmissionFormRenderer<TModel>(this ReactivePlan<TModel> plan, string id, AdmissionSchema schema)
    {
        var host = new TagBuilder("div");
        host.Attributes["id"] = id;
        host.Attributes["data-admission-form-renderer"] = plan.Json(new RendererOptions(schema));
        return plan.Render<AdmissionFormRenderer>(id, host);
    }

    private sealed record DesignerOptions(
        [property: JsonPropertyName("schema"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] AdmissionSchema? Schema,
        [property: JsonPropertyName("enablePersistence")] bool EnablePersistence);

    private sealed record RendererOptions([property: JsonPropertyName("schema")] AdmissionSchema Schema);
}
