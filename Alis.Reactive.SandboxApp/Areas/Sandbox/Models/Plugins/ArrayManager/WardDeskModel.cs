namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Plugins.ArrayManager
{
    /// <summary>The ward desk: the ward's census, tonight's staffing request and a visitor pass. No inputs.</summary>
    public sealed class WardDeskModel
    {
    }

    /// <summary>
    /// A ward's census. Each resident is a plain object (<c>name</c>, <c>status</c>, <c>condition</c>,
    /// <c>careHours</c>) that the page's array plugin reads by field name.
    /// </summary>
    public sealed class WardCensusResponse
    {
        /// <summary>The ward's name, as the desk heads it.</summary>
        public string Ward { get; set; } = "";

        public object[] Residents { get; set; } = Array.Empty<object>();
    }

    /// <summary>What the desk asks the staffing service: <c>?inResidence=</c>, the residents in residence.</summary>
    public sealed class WardStaffingQuery
    {
        public int InResidence { get; set; }
    }

    /// <summary>The staffing service's answer for tonight.</summary>
    public sealed class WardStaffingPlan
    {
        public string Plan { get; set; } = "";
    }
}
