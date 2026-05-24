using Alis.Reactive.PlanModel;
using PlanPath = Alis.Reactive.PlanModel.Path;

namespace Alis.Reactive.UnitTests.PlanModel;

[TestFixture]
public sealed class WhenDeclaringInputValueContracts
{
    [Test]
    public void registered_input_value_enriches_existing_component_contract()
    {
        var typeKey = TypeKey.Component(ComponentVendor.Native, ComponentId.Of("care-unit"));
        var types = new JsTypeCatalog();
        types.EnsureEmpty(typeKey);
        types.EnsureProperty(
            typeKey,
            JsPropertyContract.Create(
                MemberName.Of("value"),
                PlanPath.Parse("value"),
                Shape.String,
                MemberAccess.Write));

        var registration = ComponentRegistration.RegisteredInput(
            RegisteredComponentIdentity.For("care-unit", "native"),
            RegisteredComponentBinding.For("CareUnit", "value"),
            ComponentKind.Of("hidden"),
            Shape.String);

        ComponentRegistrationMatch.Found(registration).EnsureType(types, typeKey);

        var property = types.Require(typeKey).Properties["value"];
        Assert.That(property.Access, Is.EqualTo("readwrite"));
    }

    [Test]
    public void non_value_input_contract_declares_canonical_value_alias()
    {
        var typeKey = TypeKey.Component(ComponentVendor.Native, ComponentId.Of("resident-name"));
        var types = new JsTypeCatalog();

        types.EnsureInputValueContract(
            typeKey,
            InputValueContract.For("currentText", Shape.String));

        var jsType = types.Require(typeKey);
        var sourceMember = jsType.Properties["currentText"];
        var canonicalMember = jsType.Properties["value"];

        Assert.That(canonicalMember.Path, Is.EqualTo(sourceMember.Path));
        Assert.That(canonicalMember.Shape, Is.EqualTo(sourceMember.Shape));
        Assert.That(canonicalMember.Access, Is.EqualTo("read"));
    }
}
