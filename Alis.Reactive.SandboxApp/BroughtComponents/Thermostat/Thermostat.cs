using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.SandboxApp.BroughtComponents
{
    public sealed class Thermostat : Component, IInputComponent
    {
        public static ComponentVendor ThermostatVendor { get; } = ComponentVendor.Define(
            "thermostat",
            objectPath: "thermostat",
            subscribe: "on",
            unsubscribe: "off",
            payload: VendorEventPayload.Argument);

        public override ComponentVendor Vendor => ThermostatVendor;

        public Property<int> Value => Property<int>();

        public Property<ThermostatMode> Mode => Property<ThermostatMode>();

        public Property<ThermostatFan> Fan => Property<ThermostatFan>();

        public ReadOnly<int> Current => ReadOnly<int>();

        public ReadOnly<bool> Locked => ReadOnly<bool>();

        public WriteOnly<string> Pin => WriteOnly<string>();

        public ThermostatSchedule Schedule => Object<ThermostatSchedule>();

        public Objects<ThermostatZone> Zones => Objects<ThermostatZone>();

        public void Increase() => Method();

        public void Decrease() => Method();

        public void Hold(Arg<decimal> minutes) => Method(minutes);

        public void SetRange(Arg<int> min, Arg<int> max) => Method(min, max);

        public void SetQuietHours(Arg<IReadOnlyList<string>> hours) => Method(hours);

        public void ApplyPlan(Arg<ComfortPlan> plan) => Method(plan);

        public TypedSource<bool> IsComfortable() => Method<bool>();

        public TypedSource<int> DistanceTo(Arg<int> target) => Method<int>(target);

        public TypedSource<int> Estimate(Arg<int> target, Arg<ThermostatMode> mode) => Method<int>(target, mode);

        public TypedSource<string> Describe(Arg<ComfortPlan> plan) => Method<string>(plan);

        public ThermostatReading Reading() => Object<ThermostatReading>();

        public Event Unlocked => Event();

        public Event<ThermostatTargetChangingArgs> TargetChanging => Event<ThermostatTargetChangingArgs>();

        public Event<ThermostatTargetChangedArgs> TargetChanged => Event<ThermostatTargetChangedArgs>();

        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: TargetChanged);
    }

    public sealed class ThermostatSchedule : JsObject
    {
        public Property<int> Night => Property<int>();

        public ReadOnly<bool> Active => ReadOnly<bool>();

        public void Apply() => Method();

        public TypedSource<string> NextChange() => Method<string>();

        public Event<ThermostatScheduleAppliedArgs> Applied => Event<ThermostatScheduleAppliedArgs>();
    }

    public sealed class ThermostatScheduleAppliedArgs : JsObject
    {
        public ReadOnly<int> Target => ReadOnly<int>();
    }

    public sealed class ThermostatZone : JsObject
    {
        public ReadOnly<string> Name => ReadOnly<string>();

        public Property<int> Target => Property<int>();

        public void Boost() => Method();

        public TypedSource<bool> IsBoosted() => Method<bool>();
    }

    public sealed class ThermostatReading : JsObject
    {
        public ReadOnly<int> Value => ReadOnly<int>();

        public ReadOnly<string> Unit => ReadOnly<string>();

        public TypedSource<string> Label() => Method<string>();
    }

    public sealed class ThermostatTargetChangingArgs : JsObject
    {
        public ReadOnly<int> Requested => ReadOnly<int>();

        public ReadOnly<int> Previous => ReadOnly<int>();

        public Property<bool> Cancel => Property<bool>();

        public Property<string> Reason => Property<string>();

        public TypedSource<int> Clamp() => Method<int>();

        public void Accept() => Method();
    }

    public sealed class ThermostatTargetChangedArgs : JsObject
    {
        public ReadOnly<int> Value => ReadOnly<int>();

        public ReadOnly<int> Previous => ReadOnly<int>();
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ThermostatMode
    {
        [JsonStringEnumMemberName("heat")]
        Heat,

        [JsonStringEnumMemberName("cool")]
        Cool,

        [JsonStringEnumMemberName("off")]
        Off,
    }

    public enum ThermostatFan
    {
        [JsonStringEnumMemberName("auto")]
        Auto,

        [JsonStringEnumMemberName("on")]
        On,
    }

    public sealed class ComfortPlan
    {
        public int Target { get; set; }

        public ThermostatMode Mode { get; set; }

        public List<NightSetting> Nights { get; set; } = new List<NightSetting>();
    }

    public sealed class NightSetting
    {
        public string Day { get; set; } = "";

        public int Target { get; set; }
    }

    public static class ThermostatHtmlExtensions
    {
        public static RenderedComponent<Thermostat, TModel> Thermostat<TModel>(
            this InputBoundField<TModel, int> field,
            int currentTemperature,
            string pin,
            bool locked,
            IEnumerable<string> zones)
            where TModel : class
        {
            var html = new StringBuilder()
                .Append("<div class=\"thermostat\" id=\"").Append(Encode(field.ElementId))
                .Append("\" data-target=\"").Append(Target(field))
                .Append("\" data-current=\"").Append(currentTemperature)
                .Append("\" data-pin=\"").Append(Encode(pin))
                .Append("\" data-locked=\"").Append(locked ? "true" : "false")
                .Append("\" data-zones=\"").Append(Encode(string.Join(",", zones)))
                .Append("\"></div>");
            return field.Render<Thermostat>(new HtmlString(html.ToString()));
        }

        private static int Target<TModel>(InputBoundField<TModel, int> field)
            where TModel : class
        {
            var model = field.Helper.ViewData.Model;
            return model == null ? 70 : field.Expression.Compile()(model);
        }

        private static string Encode(string value) => HtmlEncoder.Default.Encode(value);
    }
}
