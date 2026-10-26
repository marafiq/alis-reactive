using Alis.Reactive.SandboxApp.BroughtComponents;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought
{
    // Journey: the admitting nurse reconciles a new resident's medication list, line by line, against the
    // pharmacy's delivery. The list is a widget the sandbox wrote itself (BroughtComponents/MedicationList):
    // a list of objects with members of their own; the delivery is the pharmacy's answer.
    public class MedicationReconciliationModel
    {
        /// <summary>The id of the resident's medication list on the page.</summary>
        public const string ListId = "medications";

        /// <summary>
        /// Margaret Hale's list from the hospital discharge: five lines, one controlled, one an injection,
        /// one already reconciled, three due before noon.
        /// </summary>
        public List<MedicationItem> Lines { get; set; } =
        [
            Line("Metoprolol 25 mg", units: 1, cost: 0.40m, hours: 12, MedicationRoute.Oral, hour: 8),
            Line("Warfarin 2 mg", units: 2, cost: 1.25m, hours: 24, MedicationRoute.Oral, hour: 18),
            Line("Lorazepam 0.5 mg", units: 1, cost: 2.10m, hours: 8, MedicationRoute.Oral, hour: 8, controlled: true),
            Line("Insulin glargine", units: 20, cost: 12.75m, hours: 24, MedicationRoute.Injection, hour: 7),
            Line("Lidocaine patch", units: 1, cost: 3.50m, hours: 12, MedicationRoute.Topical, hour: 20, reconciled: true),
        ];

        public static MedicationItem Line(
            string name, int units, decimal cost, double hours, MedicationRoute route, int hour,
            bool controlled = false, bool reconciled = false) =>
            new MedicationItem
            {
                Name = name,
                Units = units,
                DailyCost = cost,
                HoursApart = hours,
                Route = route,
                ScheduledAt = new DateTime(2026, 6, 2, hour, 0, 0),
                Controlled = controlled,
                Reconciled = reconciled,
            };
    }

    /// <summary>The pharmacy's answer: the lines it delivered, and the delivery itself as the list records it.</summary>
    public sealed class DeliveryResponse
    {
        /// <summary>The lines delivered.</summary>
        public List<MedicationItem> Lines { get; set; } = new List<MedicationItem>();

        /// <summary>The deliveries made, each with the pharmacy that made it.</summary>
        public List<PharmacyDelivery> Deliveries { get; set; } = new List<PharmacyDelivery>();
    }
}
