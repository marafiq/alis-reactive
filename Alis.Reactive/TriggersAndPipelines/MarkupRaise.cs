using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.TriggersAndPipelines;

internal static class MarkupRaise
{
    private const string RaiseAttribute = "data-alis-raise";
    private const string MemberAttribute = "data-alis-detail-";
    private const string NullAttribute = "data-alis-null";
    private const char NamesApart = ' ';

    internal static string Of(AppEvent appEvent) => Raised(appEvent.Name);

    internal static string Of<TPayload>(AppEvent<TPayload> appEvent) => Raised(appEvent.Name);

    internal static string Of<TPayload>(AppEvent<TPayload> appEvent, TPayload payload, AppJson json)
    {
        var contract = json.LiveForm.GetTypeInfo(typeof(TPayload));
        if (contract.Kind != JsonTypeInfoKind.Object) throw new NotImplementedException();
        var carried = contract.Properties.Where(member => member.Get is not null).ToList();
        foreach (var member in carried) RefuseWhatNoAttributeCarries(contract, member);
        var written = JsonSerializer.SerializeToElement(payload, contract);
        if (written.ValueKind == JsonValueKind.Null) return Raised(appEvent.Name);
        var attributes = new StringBuilder(Raised(appEvent.Name));
        var holdingNull = new List<string>();
        foreach (var member in carried)
        {
            if (!written.TryGetProperty(member.Name, out var held)) continue;
            if (held.ValueKind == JsonValueKind.Null)
            {
                holdingNull.Add(member.Name);
                continue;
            }

            attributes.Append(' ').Append(MemberAttribute).Append(InAnAttributesName(member.Name)).Append(Quoted(TextOf(held)));
        }

        return attributes.Append(' ').Append(NullAttribute).Append(Quoted(string.Join(NamesApart.ToString(), holdingNull))).ToString();
    }

    private static string Raised(string appEvent) => RaiseAttribute + Quoted(appEvent);

    private static void RefuseWhatNoAttributeCarries(JsonTypeInfo payload, JsonPropertyInfo member)
    {
        if (payload.Options.GetTypeInfo(member.PropertyType).Kind != JsonTypeInfoKind.None) throw HoldsARecordOrAList(payload, member);
        if (!IsCarriedBack(member.Name)) throw NamedSoNoAttributeCarriesIt(payload, member);
    }

    private static InvalidOperationException HoldsARecordOrAList(JsonTypeInfo payload, JsonPropertyInfo member)
        => new($"'{AsCSharpNamesIt(payload, member)}' is a {ClassNames.AsCSharpWritesIt(member.PropertyType)}, and markup carries each member of the payload it raises as the text of one "
            + "attribute, so a member it carries is text, a number, a bool, a date, a Guid or an enum. "
            + "Raise a payload that holds a record or a list from a pipeline, with p.Dispatch.");

    private static InvalidOperationException NamedSoNoAttributeCarriesIt(JsonTypeInfo payload, JsonPropertyInfo member)
        => new($"'{AsCSharpNamesIt(payload, member)}' is named '{member.Name}', and markup carries each member of the payload it raises as an "
            + "attribute whose name the browser lowercases, so a name it carries is camel case: a lowercase first letter, then letters, digits or underscores. "
            + "Rename it, or raise the payload from a pipeline, with p.Dispatch.");

    private static string AsCSharpNamesIt(JsonTypeInfo payload, JsonPropertyInfo member)
        => ClassNames.AsCSharpWritesIt(payload.Type) + "." + WrittenMember.AsCSharpDeclaresIt(payload, member);

    private static string Quoted(string text) => "=\"" + WebUtility.HtmlEncode(text) + "\"";

    private static bool IsCarriedBack(string name)
        => name.Length > 0 && name[0] is >= 'a' and <= 'z' && name.All(letter => char.IsLetterOrDigit(letter) || letter == '_');

    private static string InAnAttributesName(string member)
    {
        var name = new StringBuilder(member.Length);
        foreach (var letter in member)
        {
            if (letter is >= 'A' and <= 'Z') name.Append('-').Append(char.ToLowerInvariant(letter));
            else name.Append(letter);
        }

        return name.ToString();
    }

    private static string TextOf(JsonElement held) => held.ValueKind switch
    {
        JsonValueKind.String => held.ToString(),
        JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => held.GetRawText(),
        _ => throw new NotImplementedException(),
    };
}
