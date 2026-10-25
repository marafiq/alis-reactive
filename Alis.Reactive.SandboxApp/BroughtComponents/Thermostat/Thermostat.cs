using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.SandboxApp.BroughtComponents
{
    // The thermostat (Scripts/thermostat.ts) is a widget the sandbox wrote itself, with no knowledge of
    // Alis.Reactive. It has every kind of member a JavaScript object can have, and this file is everything
    // the framework needs to drive it, written against the framework's public surface only: where its
    // object lives, each member by its JavaScript name, its event payloads, and a render helper.

    /// <summary>A room's thermostat; its value is the target temperature.</summary>
    /// <remarks>
    /// Each member is declared by the thermostat's own JavaScript name, which is the C# name camel-cased.
    /// A page reaches the thermostat and uses those members directly, one statement each:
    /// <code>
    /// var thermostat = p.Component&lt;Thermostat&gt;(m =&gt; m.Target);
    /// thermostat.Increase();
    /// thermostat.SetRange(policy.Get(x =&gt; x.Min), policy.Get(x =&gt; x.Max));
    /// p.When(thermostat.IsComfortable()).Truthy().Then(..);
    /// p.Element("next-change").Text.Set(thermostat.Schedule.NextChange());
    /// </code>
    /// </remarks>
    public sealed class Thermostat : Component, IInputComponent
    {
        /// <summary>
        /// The thermostat keeps itself on its host element as <c>thermostat</c>, adds listeners with
        /// <c>on</c>/<c>off</c>, and hands a listener the event's payload.
        /// </summary>
        public static ComponentVendor ThermostatVendor { get; } = ComponentVendor.Define(
            "thermostat",
            objectPath: "thermostat",
            subscribe: "on",
            unsubscribe: "off",
            payload: VendorEventPayload.Argument);

        /// <inheritdoc />
        public override ComponentVendor Vendor => ThermostatVendor;

        // Properties: read and written, read only, written only, and one that is an object of its own.

        /// <summary>The target temperature. Setting it raises no event.</summary>
        public Prop<int> Value => Prop<int>();

        /// <summary>Heat, cool or off.</summary>
        public Prop<ThermostatMode> Mode => Prop<ThermostatMode>();

        /// <summary>The room's measured temperature.</summary>
        public ReadOnly<int> Current => ReadOnly<int>();

        /// <summary>Whether the thermostat's own buttons are locked.</summary>
        public ReadOnly<bool> Locked => ReadOnly<bool>();

        /// <summary>The code that unlocks the thermostat. It is written only: the thermostat never gives it back.</summary>
        public WriteOnly<string> Pin => WriteOnly<string>();

        /// <summary>The night schedule: an object with members of its own.</summary>
        public ThermostatSchedule Schedule => Object<ThermostatSchedule>();

        // Methods that return nothing: with no argument, one, several, and an object.

        /// <summary>Raises the target by one degree, through <see cref="TargetChanging"/> and <see cref="TargetChanged"/>.</summary>
        public void Increase() => Call();

        /// <summary>Lowers the target by one degree, through <see cref="TargetChanging"/> and <see cref="TargetChanged"/>.</summary>
        public void Decrease() => Call();

        /// <summary>Keeps the target for a while; the thermostat shows how long.</summary>
        /// <param name="minutes">How long the target is held.</param>
        public void Hold(Arg<decimal> minutes) => Call(minutes);

        /// <summary>Sets the lowest and highest target; the thermostat shows them as its range.</summary>
        /// <param name="min">The lowest target.</param>
        /// <param name="max">The highest target.</param>
        public void SetRange(Arg<int> min, Arg<int> max) => Call(min, max);

        /// <summary>
        /// Applies a plan: its target and mode now, its first night setting for the night. Raises
        /// <see cref="TargetChanged"/> when the target changed.
        /// </summary>
        /// <param name="plan">The plan, written in the page or read from a response.</param>
        public void ApplyPlan(Arg<ComfortPlan> plan) => Call(plan);

        // Methods that return a value: with no argument, one, several, an object, and one that returns an object.

        /// <summary>Whether the room is within a degree of the target and the thermostat is not off.</summary>
        public TypedSource<bool> IsComfortable() => Invoke<bool>();

        /// <summary>How many degrees the room is from a target.</summary>
        /// <param name="target">The target to measure against.</param>
        public TypedSource<int> DistanceTo(Arg<int> target) => Invoke<int>(target);

        /// <summary>How many minutes a mode takes to bring the room to a target.</summary>
        /// <param name="target">The target to reach.</param>
        /// <param name="mode">The mode that brings the room there.</param>
        public TypedSource<int> Estimate(Arg<int> target, Arg<ThermostatMode> mode) => Invoke<int>(target, mode);

        /// <summary>A plan in words.</summary>
        /// <param name="plan">The plan to describe.</param>
        public TypedSource<string> Describe(Arg<ComfortPlan> plan) => Invoke<string>(plan);

        /// <summary>The room's reading: an object with members of its own.</summary>
        public ThermostatReading Reading() => InvokeObject<ThermostatReading>();

        // Events: one without a payload, one whose payload the page reads, writes and calls, and the one
        // that commits the value.

        /// <summary>Raised when the right <see cref="Pin"/> unlocks the thermostat. It carries nothing.</summary>
        public TypedEvent<ThermostatUnlockedArgs> Unlocked => Event<ThermostatUnlockedArgs>();

        /// <summary>Raised before the target changes; the payload says what was asked and can refuse or cap it.</summary>
        public TypedEvent<ThermostatTargetChangingArgs> TargetChanging => Event<ThermostatTargetChangingArgs>();

        /// <summary>Raised when the target changed. As the input's commit event, a form checks the field again on it.</summary>
        public TypedEvent<ThermostatTargetChangedArgs> TargetChanged => Event<ThermostatTargetChangedArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: TargetChanged);
    }

    /// <summary>The thermostat's <c>schedule</c>: the night setting, with members of its own.</summary>
    public sealed class ThermostatSchedule : JsObject
    {
        /// <summary>The target for the night.</summary>
        public Prop<int> Night => Prop<int>();

        /// <summary>Whether the night setting has been applied.</summary>
        public ReadOnly<bool> Active => ReadOnly<bool>();

        /// <summary>Makes the night setting the target, through the thermostat's events.</summary>
        public void Apply() => Call();

        /// <summary>When the night setting applies, in words.</summary>
        public TypedSource<string> NextChange() => Invoke<string>();
    }

    /// <summary>What <see cref="Thermostat.Reading"/> returns: the room's reading.</summary>
    public sealed class ThermostatReading : JsObject
    {
        /// <summary>The measured temperature.</summary>
        public ReadOnly<int> Value => ReadOnly<int>();

        /// <summary>The unit, <c>F</c>.</summary>
        public ReadOnly<string> Unit => ReadOnly<string>();

        /// <summary>The reading in words, such as <c>68°F</c>.</summary>
        public TypedSource<string> Label() => Invoke<string>();
    }

    /// <summary>Payload of <see cref="Thermostat.Unlocked"/>: the event carries nothing.</summary>
    public class ThermostatUnlockedArgs : JsObject
    {
    }

    /// <summary>
    /// Payload of <see cref="Thermostat.TargetChanging"/>. The thermostat reads <see cref="Cancel"/> and
    /// <see cref="Reason"/> back once the page's handler ran.
    /// </summary>
    public class ThermostatTargetChangingArgs : JsObject
    {
        /// <summary>The target that was asked for.</summary>
        public ReadOnly<int> Requested => ReadOnly<int>();

        /// <summary>The target before the change.</summary>
        public ReadOnly<int> Previous => ReadOnly<int>();

        /// <summary>Set to <see langword="true"/> to refuse the change.</summary>
        public Prop<bool> Cancel => Prop<bool>();

        /// <summary>Why the change was refused; the thermostat shows it.</summary>
        public Prop<string> Reason => Prop<string>();

        /// <summary>The requested target brought within the range.</summary>
        public TypedSource<int> Clamp() => Invoke<int>();

        /// <summary>Accepts the change; the thermostat says so.</summary>
        public void Accept() => Call();
    }

    /// <summary>Payload of <see cref="Thermostat.TargetChanged"/>.</summary>
    public class ThermostatTargetChangedArgs : JsObject
    {
        /// <summary>The target after the change.</summary>
        public ReadOnly<int> Value => ReadOnly<int>();

        /// <summary>The target before the change.</summary>
        public ReadOnly<int> Previous => ReadOnly<int>();
    }

    /// <summary>The thermostat's modes, by the text its JavaScript uses.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ThermostatMode
    {
        /// <summary>Warms the room to the target.</summary>
        [JsonStringEnumMemberName("heat")]
        Heat,

        /// <summary>Cools the room to the target.</summary>
        [JsonStringEnumMemberName("cool")]
        Cool,

        /// <summary>Neither; the room is never comfortable by the thermostat's measure.</summary>
        [JsonStringEnumMemberName("off")]
        Off,
    }

    /// <summary>A plan the thermostat applies: the argument of <see cref="Thermostat.ApplyPlan"/> and <see cref="Thermostat.Describe"/>.</summary>
    public sealed class ComfortPlan
    {
        /// <summary>The target now.</summary>
        public int Target { get; set; }

        /// <summary>The mode now.</summary>
        public ThermostatMode Mode { get; set; }

        /// <summary>The night settings; the first one is the thermostat's night setting.</summary>
        public List<NightSetting> Nights { get; set; } = new List<NightSetting>();
    }

    /// <summary>A night's target in a <see cref="ComfortPlan"/>.</summary>
    public sealed class NightSetting
    {
        /// <summary>Which nights, such as <c>Weeknights</c>.</summary>
        public string Day { get; set; } = "";

        /// <summary>The target for those nights.</summary>
        public int Target { get; set; }
    }

    /// <summary>Renders a <see cref="Thermostat"/> for a model-bound target temperature.</summary>
    public static class ThermostatHtmlExtensions
    {
        /// <summary>
        /// Renders the thermostat's host with the framework's element ID and binding name, the room's
        /// reading, the code that unlocks it and whether it starts locked; the app's bundle constructs the
        /// thermostat on it.
        /// </summary>
        /// <typeparam name="TModel">The view's model.</typeparam>
        /// <param name="field">The model-bound target temperature the thermostat renders into.</param>
        /// <param name="currentTemperature">The room's measured temperature.</param>
        /// <param name="pin">The code that unlocks the thermostat's own buttons.</param>
        /// <param name="locked">Whether the thermostat's own buttons start locked.</param>
        /// <returns>The rendered thermostat; wire its events with <c>.Reactive(evt =&gt; evt.TargetChanged, ...)</c>.</returns>
        public static RenderedComponent<Thermostat, TModel> Thermostat<TModel>(
            this InputBoundField<TModel, int> field,
            int currentTemperature,
            string pin,
            bool locked)
            where TModel : class
        {
            var html = new StringBuilder()
                .Append("<div class=\"thermostat\" id=\"").Append(Encode(field.ElementId))
                .Append("\" data-name=\"").Append(Encode(field.BindingPath))
                .Append("\" data-target=\"").Append(Target(field))
                .Append("\" data-current=\"").Append(currentTemperature)
                .Append("\" data-pin=\"").Append(Encode(pin))
                .Append("\" data-locked=\"").Append(locked ? "true" : "false")
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
