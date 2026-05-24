namespace Alis.Reactive.UnitTests.ValidationEnrichment;

[TestFixture]
public class WhenRegisteringComponents
{
    [Test]
    public void Duplicate_binding_path_with_different_component_throws()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();

        plan.RegisterInputComponent(ComponentRegistration.RegisteredInput(Alis.Reactive.RegisteredComponentIdentity.For("name-input", "native"), Alis.Reactive.RegisteredComponentBinding.For("Name", "value"), Alis.Reactive.PlanModel.ComponentKind.Of("textbox"), Alis.Reactive.PlanModel.Shape.String));

        Assert.Throws<InvalidOperationException>(() =>
            plan.RegisterInputComponent(ComponentRegistration.RegisteredInput(Alis.Reactive.RegisteredComponentIdentity.For("other-input", "fusion"), Alis.Reactive.RegisteredComponentBinding.For("Name", "value"), Alis.Reactive.PlanModel.ComponentKind.Of("autocomplete"), Alis.Reactive.PlanModel.Shape.String)));
    }

    [Test]
    public void Same_registration_is_idempotent()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();

        plan.RegisterInputComponent(ComponentRegistration.RegisteredInput(Alis.Reactive.RegisteredComponentIdentity.For("name-input", "native"), Alis.Reactive.RegisteredComponentBinding.For("Name", "value"), Alis.Reactive.PlanModel.ComponentKind.Of("textbox"), Alis.Reactive.PlanModel.Shape.String));
        plan.RegisterInputComponent(ComponentRegistration.RegisteredInput(Alis.Reactive.RegisteredComponentIdentity.For("name-input", "native"), Alis.Reactive.RegisteredComponentBinding.For("Name", "value"), Alis.Reactive.PlanModel.ComponentKind.Of("textbox"), Alis.Reactive.PlanModel.Shape.String));

        Assert.That(plan.RegisteredInputComponents.Count, Is.EqualTo(1));
    }

    [Test]
    public void Same_component_identity_cannot_be_registered_for_two_binding_paths()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();

        plan.RegisterInputComponent(ComponentRegistration.RegisteredInput(Alis.Reactive.RegisteredComponentIdentity.For("shared-input", "native"), Alis.Reactive.RegisteredComponentBinding.For("Name", "value"), Alis.Reactive.PlanModel.ComponentKind.Of("textbox"), Alis.Reactive.PlanModel.Shape.String));

        var ex = Assert.Throws<InvalidOperationException>(() =>
            plan.RegisterInputComponent(ComponentRegistration.RegisteredInput(Alis.Reactive.RegisteredComponentIdentity.For("shared-input", "native"), Alis.Reactive.RegisteredComponentBinding.For("Email", "value"), Alis.Reactive.PlanModel.ComponentKind.Of("textbox"), Alis.Reactive.PlanModel.Shape.String)));

        Assert.That(ex!.Message, Does.Contain("shared-input"));
        Assert.That(ex.Message, Does.Contain("Name"));
        Assert.That(ex.Message, Does.Contain("Email"));
    }

    [Test]
    public void Same_path_same_ids_different_shape_throws()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();

        plan.RegisterInputComponent(ComponentRegistration.RegisteredInput(Alis.Reactive.RegisteredComponentIdentity.For("amount-input", "fusion"), Alis.Reactive.RegisteredComponentBinding.For("Amount", "value"), Alis.Reactive.PlanModel.ComponentKind.Of("numerictextbox"), Alis.Reactive.PlanModel.Shape.Number));

        // Same component, same IDs — but Shape differs. This is a plan bug
        // (e.g., two different TProp bindings to the same element). Must throw, not silently ignore.
        Assert.Throws<InvalidOperationException>(() =>
            plan.RegisterInputComponent(ComponentRegistration.RegisteredInput(Alis.Reactive.RegisteredComponentIdentity.For("amount-input", "fusion"), Alis.Reactive.RegisteredComponentBinding.For("Amount", "value"), Alis.Reactive.PlanModel.ComponentKind.Of("numerictextbox"), Alis.Reactive.PlanModel.Shape.String)));
    }

    [Test]
    public void Same_registration_including_shape_is_idempotent()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();

        plan.RegisterInputComponent(ComponentRegistration.RegisteredInput(Alis.Reactive.RegisteredComponentIdentity.For("date-input", "fusion"), Alis.Reactive.RegisteredComponentBinding.For("Date", "value"), Alis.Reactive.PlanModel.ComponentKind.Of("datepicker"), Alis.Reactive.PlanModel.Shape.Date));
        plan.RegisterInputComponent(ComponentRegistration.RegisteredInput(Alis.Reactive.RegisteredComponentIdentity.For("date-input", "fusion"), Alis.Reactive.RegisteredComponentBinding.For("Date", "value"), Alis.Reactive.PlanModel.ComponentKind.Of("datepicker"), Alis.Reactive.PlanModel.Shape.Date));

        Assert.That(plan.RegisteredInputComponents.Count, Is.EqualTo(1));
    }

    /// <summary>
    /// Bug F3: When input component registration throws for a duplicate registration, the exception message
    /// only includes ComponentId and Vendor from the existing and new registrations. It omits
    /// ValueMember, ComponentType, and Shape — which are the fields most likely to actually differ
    /// when the same element ID is reused with a different binding shape. The developer cannot
    /// diagnose the conflict without all differing fields in the message.
    /// </summary>
    [Test]
    public void Duplicate_exception_message_includes_valueMember_of_both_registrations()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();

        // Register with valueMember = "value"
        plan.RegisterInputComponent(ComponentRegistration.RegisteredInput(Alis.Reactive.RegisteredComponentIdentity.For("id", "native"), Alis.Reactive.RegisteredComponentBinding.For("Name", "value"), Alis.Reactive.PlanModel.ComponentKind.Of("textbox"), Alis.Reactive.PlanModel.Shape.String));

        // Re-register same ComponentId + Vendor, but different ValueMember ("checked") and Shape (boolean)
        var ex = Assert.Throws<InvalidOperationException>(() =>
            plan.RegisterInputComponent(ComponentRegistration.RegisteredInput(Alis.Reactive.RegisteredComponentIdentity.For("id", "native"), Alis.Reactive.RegisteredComponentBinding.For("Name", "checked"), Alis.Reactive.PlanModel.ComponentKind.Of("checkbox"), Alis.Reactive.PlanModel.Shape.Boolean)));

        // Bug: the message only contains ComponentId ("id") and Vendor ("native"), NOT valueMember.
        // A developer seeing this error cannot tell which valueMember was registered vs attempted.
        Assert.That(ex!.Message, Does.Contain("value"),
            "Exception message must include the existing valueMember ('value') so the developer can diagnose the conflict");
        Assert.That(ex.Message, Does.Contain("checked"),
            "Exception message must include the new valueMember ('checked') so the developer can diagnose the conflict");
    }

    [Test]
    public void Duplicate_exception_message_includes_shape_of_both_registrations()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();

        // Register with shape = Shape.Number
        plan.RegisterInputComponent(ComponentRegistration.RegisteredInput(Alis.Reactive.RegisteredComponentIdentity.For("amount-id", "fusion"), Alis.Reactive.RegisteredComponentBinding.For("Amount", "value"), Alis.Reactive.PlanModel.ComponentKind.Of("numerictextbox"), Alis.Reactive.PlanModel.Shape.Number));

        // Re-register same ComponentId + Vendor + ValueMember, but different Shape
        var ex = Assert.Throws<InvalidOperationException>(() =>
            plan.RegisterInputComponent(ComponentRegistration.RegisteredInput(Alis.Reactive.RegisteredComponentIdentity.For("amount-id", "fusion"), Alis.Reactive.RegisteredComponentBinding.For("Amount", "value"), Alis.Reactive.PlanModel.ComponentKind.Of("numerictextbox"), Alis.Reactive.PlanModel.Shape.String)));

        // Bug: the message omits Shape — the developer cannot see that "number" vs "string" is
        // the actual difference causing the conflict.
        Assert.That(ex!.Message, Does.Contain("number"),
            "Exception message must include the existing shape ('number') so the developer can diagnose the conflict");
        Assert.That(ex.Message, Does.Contain("string"),
            "Exception message must include the new shape ('string') so the developer can diagnose the conflict");
    }

    [Test]
    public void Duplicate_exception_message_includes_componentType_of_both_registrations()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();

        // Register as textbox
        plan.RegisterInputComponent(ComponentRegistration.RegisteredInput(Alis.Reactive.RegisteredComponentIdentity.For("name-id", "native"), Alis.Reactive.RegisteredComponentBinding.For("Name", "value"), Alis.Reactive.PlanModel.ComponentKind.Of("textbox"), Alis.Reactive.PlanModel.Shape.String));

        // Re-register same ComponentId + Vendor + ValueMember + Shape, but different ComponentType
        var ex = Assert.Throws<InvalidOperationException>(() =>
            plan.RegisterInputComponent(ComponentRegistration.RegisteredInput(Alis.Reactive.RegisteredComponentIdentity.For("name-id", "native"), Alis.Reactive.RegisteredComponentBinding.For("Name", "value"), Alis.Reactive.PlanModel.ComponentKind.Of("password"), Alis.Reactive.PlanModel.Shape.String)));

        // Bug: the message omits ComponentType — the developer cannot see that "textbox" vs "password"
        // is the actual difference causing the conflict.
        Assert.That(ex!.Message, Does.Contain("textbox"),
            "Exception message must include the existing componentType ('textbox') so the developer can diagnose the conflict");
        Assert.That(ex.Message, Does.Contain("password"),
            "Exception message must include the new componentType ('password') so the developer can diagnose the conflict");
    }
}
