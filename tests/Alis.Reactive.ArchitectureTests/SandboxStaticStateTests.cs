using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

/// <summary>
/// Isolation clause 3 made mechanical (memory/bdd-principles.md, Nested
/// Vertical Slices): a journey's data is unreachable from other journeys and
/// other worlds. A static store in the sandbox is process-global — every
/// browser world sees the same instance, so parallel tests collide.
/// Readonly seed lists are fine; stores are not.
/// </summary>
[TestFixture]
public class SandboxStaticStateTests
{
    // The only registry of exceptions. Two categories: keyed stores whose key
    // already isolates (per flow id or per world), and process-global debt with
    // a named removal trigger. New statics get a per-world mechanism (the Grid
    // Billing session store is the exemplar), not an allowlist entry.
    private static readonly string[] Allowlist =
    [
        // -- Keyed per flow id or per world: each wizard run / drill world reads
        //    its own key, so worlds do not collide. Migrate to session stores as
        //    touched.
        "Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Patterns.AdmissionWizardController.Step1Drafts",
        "Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Patterns.AdmissionWizardController.Step2Drafts",
        "Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Patterns.AdmissionWizardController.Step3Drafts",
        "Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Patterns.AdmissionWizardController.Step4Drafts",
        "Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions.AdmissionAssessmentController.Step1Drafts",
        "Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions.AdmissionAssessmentController.Step2Drafts",
        "Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions.AdmissionAssessmentController.Step3Drafts",
        "Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions.AdmissionAssessmentController.Step4Drafts",
        "Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion.StepperController.IntakeDrafts",
        "Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion.StepperController.CareDrafts",
        "Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion.StepperController.ContactDrafts",
        "Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion.StepperController.ReviewDrafts",
        "Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline.RealTimeController.DrillWorlds",

        // -- Process-global debt: shared across browser worlds, real collision
        //    surface. Each names its removal trigger.
        // Schedule pre-pattern store, keyed by facility id, not by world —
        // remove with the Schedule journey-slice refactor (recorded in
        // docs/superpowers/plans/claude-setup-experiments-rc3.md).
        "Alis.Reactive.SandboxApp.Areas.Sandbox.Models.FakeScheduleData.Store",
        // Deleted-row set shared across worlds — remove when the ActionLink
        // slice gets a per-world store.
        "Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline.HttpController._deletedNativeActionLinkRows",
        // Kanban board shared across worlds — remove when the Kanban slice
        // gets a per-world store.
        "Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion.KanbanController.BoardCards",
    ];

    [Test]
    public void sandbox_statics_hold_no_mutable_stores()
    {
        var violations = SandboxStaticFields()
            .Where(f => IsStoreShaped(f.Field.FieldType)
                        || (IsCollection(f.Field.FieldType) && !f.Field.IsInitOnly))
            .Where(f => !Allowlist.Contains(f.FullName))
            .Select(f => $"{f.FullName} ({f.Field.FieldType.Name})"
                         + (f.Field.IsInitOnly ? " — store-shaped" : " — non-readonly collection"))
            .ToList();

        Assert.That(violations, Is.Empty,
            "Static mutable stores cross browser worlds and couple journeys. "
            + "Move the data into a per-world store owned by the journey's "
            + "controller, or allowlist with a removal trigger:\n"
            + string.Join("\n", violations));
    }

    [Test]
    public void allowlist_entries_still_exist()
    {
        var existing = SandboxStaticFields().Select(f => f.FullName).ToHashSet();
        var stale = Allowlist.Where(entry => !existing.Contains(entry)).ToList();

        Assert.That(stale, Is.Empty,
            "Allowlist entries whose fields no longer exist must be deleted:\n"
            + string.Join("\n", stale));
    }

    private static IEnumerable<(FieldInfo Field, string FullName)> SandboxStaticFields()
    {
        var assembly = Assembly.Load("Alis.Reactive.SandboxApp");

        foreach (var type in assembly.GetTypes())
        {
            if (type.Namespace?.Contains("Areas.Sandbox") != true)
                continue;
            if (type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
                continue;

            var fields = type.GetFields(
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            foreach (var field in fields)
            {
                if (field.IsLiteral)
                    continue;
                if (field.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
                    continue;

                yield return (field, $"{type.FullName}.{field.Name}");
            }
        }
    }

    // Keyed lookups are stores: their content mutates per key across worlds,
    // readonly or not.
    private static bool IsStoreShaped(Type type) =>
        typeof(IDictionary).IsAssignableFrom(type)
        || type.Namespace == "System.Collections.Concurrent"
        || ImplementsGenericInterface(type, typeof(IDictionary<,>))
        || ImplementsGenericInterface(type, typeof(ISet<>));

    private static bool IsCollection(Type type) =>
        type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);

    private static bool ImplementsGenericInterface(Type type, Type genericInterface) =>
        type.GetInterfaces().Append(type).Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == genericInterface);
}
