using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using Alis.Reactive.Execution;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Schema;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.RenderingAndDelivery;

internal sealed class RowAddress
{
    private const long LastWholeNumberJavaScriptHolds = 9007199254740991L;
    private const string AButton = "render a button for the action";

    private readonly string _request;
    private readonly IReadOnlyList<RequestUrlParts.Part> _parts;
    private readonly string _ownQuery;
    private readonly IReadOnlyList<LinkMember> _members;

    private RowAddress(string request, RequestPartsWriter parts, IReadOnlyList<LinkMember> members)
    {
        _request = request;
        _parts = parts.UrlParts;
        _ownQuery = parts.OwnQuery;
        _members = members;
    }

    internal static RowAddress Of(BlockWriter writer, string rowsId, ListenStatement listen, RowReads reads)
    {
        var requests = listen.Do.OfType<RequestStatement>().ToArray();
        if (requests.Length == 0)
            throw new InvalidOperationException($"The row link on '{rowsId}' sends no request at its steps' own level, so its links have no address: send one GET there, or {AButton}.");
        if (requests.Length > 1)
            throw new InvalidOperationException($"The row link on '{rowsId}' sends {requests.Length.ToString(CultureInfo.InvariantCulture)} requests at its steps' own level, and a link opens one address: send one GET there, or {AButton}.");
        var (method, parts) = RequestScopes.SentAs(writer, requests[0].Argument);
        var request = method.Text + " " + parts.Url;
        if (method != RequestMethod.Get)
            throw new InvalidOperationException($"The row link on '{rowsId}' sends '{request}', and a link opens its address by GET alone: send a GET, or {AButton}.");
        var carried = parts.SendsAHeader ? "a header" : parts.SendsAnInput ? "an input's value" : parts.ContainerId is { } container ? $"the inputs of '{container}'" : "";
        if (carried.Length > 0)
            throw new InvalidOperationException($"The row link on '{rowsId}' sends '{request}' with {carried}, which a link's address cannot carry: send the URL's members alone, or {AButton}.");
        return new RowAddress(request, parts, parts.SentMembers.Select(member => LinkMember.Of(member, rowsId, reads)).ToArray());
    }

    internal string For(string id, JsonElement row)
    {
        var address = new StringBuilder();
        foreach (var part in _parts)
            address.Append(part.Member.Length == 0 ? part.Text : Segment(id, _members.First(member => member.Sent.InPath && member.Sent.Name.Equals(part.Member, StringComparison.OrdinalIgnoreCase)), row));
        if (_ownQuery.Length > 0) address.Append('?').Append(_ownQuery);
        var fields = _members.Where(member => !member.Sent.InPath).SelectMany(member => Fields(id, member, row)).ToArray();
        if (fields.Length > 0) address.Append(_ownQuery.Length == 0 ? '?' : '&').Append(string.Join("&", fields));
        return address.ToString();
    }

    private string Segment(string id, LinkMember member, JsonElement row)
    {
        var value = member.Value(id, row);
        var text = value is { } held ? Text(id, member, held) : "";
        var refused = value is null ? "nothing" : text.Length == 0 ? "empty text" : text.IndexOf('/') >= 0 ? $"'{text}', which holds '/'" : "";
        if (refused.Length > 0)
        {
            throw new InvalidOperationException(
                $"Row link '{id}' fills '{member.Sent.Written}' of '{_request}' with {refused}, and a path segment holds nonempty text with no '/': give the row a value there, or send the member in the query.");
        }

        return Encoded(text, PathKeeps, false);
    }

    private static IEnumerable<string> Fields(string id, LinkMember member, JsonElement row)
    {
        var value = member.Value(id, row);
        var name = Encoded(member.Sent.Name, FormKeeps, true);
        if (value is { } held) return [name + "=" + Encoded(Text(id, member, held), FormKeeps, true)];
        return member.Sent.WhenNothing == WhenNothing.Empty ? [name + "="] : [];
    }

    private static string Text(string id, LinkMember member, JsonElement value)
    {
        var text = member.Sent.Form switch
        {
            TextScalarWireForm => value.ValueKind == JsonValueKind.String ? value.GetString() : null,
            BooleanScalarWireForm => value.ValueKind == JsonValueKind.True ? "true" : value.ValueKind == JsonValueKind.False ? "false" : null,
            NumberScalarWireForm => WholeNumber(value),
            WrittenScalarWireForm => value.ValueKind == JsonValueKind.String ? value.GetString() : WholeNumber(value),
            EnumScalarWireForm enumeration => enumeration.Values.FirstOrDefault(row => JsonElement.DeepEquals(row.Held, value))?.Name,
            _ => null,
        };
        return text ?? throw new InvalidOperationException(
            $"Row link '{id}' fills '{member.Sent.Written}' with {value.GetRawText()}, which its form '{member.Sent.Form.Kind}' writes no text for: give the row a value of that form, or {AButton}.");
    }

    private static string? WholeNumber(JsonElement value)
    {
        var whole = value.ValueKind == JsonValueKind.Number ? value.TryGetInt64(out var number) ? number : (long?)null
            : value.ValueKind == JsonValueKind.String && long.TryParse(value.GetString(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var written) ? written : null;
        return whole is >= -LastWholeNumberJavaScriptHolds and <= LastWholeNumberJavaScriptHolds ? whole.Value.ToString(CultureInfo.InvariantCulture) : null;
    }

    private static bool PathKeeps(byte unit) => unit is >= (byte)'A' and <= (byte)'Z' or >= (byte)'a' and <= (byte)'z' or >= (byte)'0' and <= (byte)'9' or (byte)'-' or (byte)'.' or (byte)'_' or (byte)'~';

    private static bool FormKeeps(byte unit) => unit is >= (byte)'A' and <= (byte)'Z' or >= (byte)'a' and <= (byte)'z' or >= (byte)'0' and <= (byte)'9' or (byte)'*' or (byte)'-' or (byte)'.' or (byte)'_';

    private static string Encoded(string text, Func<byte, bool> keeps, bool spaceAsPlus)
    {
        var encoded = new StringBuilder();
        foreach (var unit in Encoding.UTF8.GetBytes(text))
        {
            if (keeps(unit)) encoded.Append((char)unit);
            else if (spaceAsPlus && unit == (byte)' ') encoded.Append('+');
            else encoded.Append('%').Append(unit.ToString("X2", CultureInfo.InvariantCulture));
        }

        return encoded.ToString();
    }

    private sealed class LinkMember
    {
        private readonly JsonElement? _literal;
        private readonly IReadOnlyList<DataStep>? _rowPath;

        private LinkMember(SentMember sent, JsonElement? literal, IReadOnlyList<DataStep>? rowPath)
        {
            Sent = sent;
            _literal = literal;
            _rowPath = rowPath;
        }

        internal SentMember Sent { get; }

        internal static LinkMember Of(SentMember sent, string rowsId, RowReads reads)
        {
            var unwritten = UnwrittenForm(sent);
            if (unwritten.Length > 0)
            {
                throw new InvalidOperationException(
                    $"'{sent.Written}' of the row link on '{rowsId}' holds {unwritten}, which a link's address does not write: {AButton}, or send the value as text the view formats.");
            }

            var read = sent.Read is AsExpression shaped ? shaped.Expr : sent.Read;
            if (read is LitExpression literal) return new LinkMember(sent, literal.Value, null);
            if (read is DataExpression data && reads.IsTheRow(data.Target)) return new LinkMember(sent, null, data.Path);
            if (reads.IsTheRow(read)) return new LinkMember(sent, null, []);
            throw new InvalidOperationException(
                $"'{sent.Written}' of the row link on '{rowsId}' reads a value neither the view nor the row holds, which a link's address cannot carry: fill it with a value or a read of the row, or {AButton}.");
        }

        internal JsonElement? Value(string id, JsonElement row)
        {
            var value = _literal ?? Read(id, row);
            return value is { ValueKind: not (JsonValueKind.Null or JsonValueKind.Undefined) } held ? held : null;
        }

        private static string UnwrittenForm(SentMember sent) => sent.Form switch
        {
            ListWireForm => "a list",
            FlagsScalarWireForm => "flags",
            DateScalarWireForm => "a date",
            InstantScalarWireForm => "a date with its offset",
            NumberScalarWireForm when ComparedTypes.IsFraction(sent.ValueType) => "a fraction",
            _ => "",
        };

        private JsonElement? Read(string id, JsonElement row)
        {
            var holder = row;
            foreach (var step in _rowPath!)
            {
                if (holder.ValueKind == JsonValueKind.Null) return null;
                var stops = holder.ValueKind != JsonValueKind.Object || step.Requires.Any(required => !holder.TryGetProperty(required, out _));
                if (stops) throw Stops(id);
                if (holder.TryGetProperty(step.Name, out var member))
                {
                    holder = member;
                    continue;
                }

                return step.Absent switch
                {
                    StartAbsence start => start.Value,
                    NothingAbsence => null,
                    _ => throw Stops(id),
                };
            }

            return holder;
        }

        private InvalidOperationException Stops(string id)
            => new($"Row link '{id}' reads '{Sent.Written}' past what its row's JSON holds, so its GET stops before it is sent: {AButton}.");
    }
}
