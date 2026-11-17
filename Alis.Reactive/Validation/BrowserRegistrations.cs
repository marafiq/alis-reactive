using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using Alis.Reactive.ArrayLambdas;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Validation;

internal static partial class BrowserRegistrations
{
    internal const string DefaultRuleSet = "default";

    internal static Registration Of<T>(ReactiveValidator<T> validator, string key, string ruleSet, AppJson json, CultureInfo rendersUnder, IReadOnlyCollection<Type> flags)
    {
        var scalars = new BrowserScalars(json);
        var selection = new ValidationRun(true, CancellationToken.None, ruleSet, [], flags);
        var binding = Contracts.Binding(json.App);
        var carried = new CarriedMembers(json.App);
        var scope = new BrowserScope<T>(selection, json, HeldForm.Live(json, rendersUnder), scalars, carried, [], MemberPath.Empty, "", false, () => JsonSerializer.Deserialize<T>("{}", binding)!);
        var rules = new RuleGroup([], ItemLambda.EveryItem(), [], validator.BrowserGroups(scope));
        var starts = carried.Writing(json.LiveForm);
        var objects = new List<BoundObject>();
        var lists = new List<BoundList>();
        Holders(json, starts, json.App.GetTypeInfo(typeof(T)), [], [typeof(T)], objects, lists);
        return new Registration(key, ruleSet, starts.GetTypeInfo(typeof(T)).StartOf(), scalars.Named, objects, lists, rules);
    }

    private static partial BoundObjectJson ObjectJson(JsonPropertyInfo property, JsonTypeInfo holder);

    private static partial BoundListJson ListJson(JsonPropertyInfo property, JsonTypeInfo holder);

    private static void Holders(AppJson json, JsonSerializerOptions starts, JsonTypeInfo holder, IReadOnlyList<RegistrationStep> path, IReadOnlyList<Type> around, List<BoundObject> objects, List<BoundList> lists)
    {
        foreach (var property in holder.Properties)
        {
            if (property.CustomConverter is not null) continue;
            if (property.Get is null) continue;
            var member = json.App.GetTypeInfo(property.PropertyType);
            IReadOnlyList<RegistrationStep> at = [.. path, new MemberStep(property.Name)];
            if (Creates(member, around))
            {
                objects.Add(new BoundObject(at, starts.GetTypeInfo(member.Type).StartOf(), ObjectJson(property, holder)));
                Holders(json, starts, member, at, [.. around, member.Type], objects, lists);
                continue;
            }

            if (!member.IsList(out var item)) continue;
            var itemCreated = Creates(item, around);
            lists.Add(new BoundList(at, itemCreated ? starts.GetTypeInfo(item.Type).StartOf() : JsonSerializer.SerializeToElement<object?>(null), ListJson(property, holder)));
            if (itemCreated) Holders(json, starts, item, [.. at, EachRegistrationStep.Instance], [.. around, item.Type], objects, lists);
        }
    }

    private static bool Creates(JsonTypeInfo member, IReadOnlyList<Type> around)
        => member.Kind == JsonTypeInfoKind.Object && member.CreateObject is not null && !around.Contains(member.Type, EqualityComparer<Type>.Default);
}
