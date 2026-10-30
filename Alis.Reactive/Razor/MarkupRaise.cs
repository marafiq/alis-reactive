using System;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Grammar;

namespace Alis.Reactive.Razor
{
    /// <summary>
    /// The attributes that make an element raise a declared event when it is clicked (grammar.md 6.1, D4):
    /// <c>data-alis-raise</c> names the event, and each member of the payload is one <c>data-alis-detail-</c>
    /// attribute holding the member's value as text, which the listener's typed read converts by the member's type.
    /// </summary>
    internal static class MarkupRaise
    {
        // A member's value as the text JSON gives it, an enum by its member's name, as a condition compares it.
        private static readonly JsonSerializerOptions ValueText = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
        };

        internal static string Of(string eventName) => $"data-alis-raise=\"{WebUtility.HtmlEncode(eventName)}\"";

        // A member left null carries no attribute, so the listener reads it as missing.
        internal static string Of<TPayload>(string eventName, TPayload payload)
        {
            var attributes = new StringBuilder(Of(eventName));
            foreach (var member in Shape.SerializedProperties(typeof(TPayload)))
            {
                var value = Carried<TPayload>(member).GetValue(payload);
                if (value == null) continue;
                var text = TextOf(JsonSerializer.SerializeToElement(value, member.PropertyType, ValueText));
                attributes.Append(' ').Append(DatasetAttribute(JsonMemberName.Of(member.Name)))
                    .Append("=\"").Append(WebUtility.HtmlEncode(text)).Append('"');
            }
            return attributes.ToString();
        }

        private static PropertyInfo Carried<TPayload>(PropertyInfo member)
        {
            if (Shape.FromClrType(member.PropertyType).IsScalar) return member;
            throw new InvalidOperationException(
                $"'{CSharpTypeName.Of(typeof(TPayload))}.{member.Name}' is a {CSharpTypeName.Of(member.PropertyType)}, and markup " +
                "carries each member of the payload it raises as the text of one attribute, so a member it carries is text, a " +
                "number, a bool, a date, a Guid or an enum. Raise a payload that holds a record or a list from a pipeline, with p.Dispatch.");
        }

        private static string TextOf(JsonElement value) =>
            value.ValueKind == JsonValueKind.String ? value.GetString()! : value.GetRawText();

        // The attribute the DOM's dataset names alisDetail followed by the member's name with its first letter
        // capitalized: each capital is a dash and the small letter, so residentId is data-alis-detail-resident-id.
        private static string DatasetAttribute(string member)
        {
            var key = "alisDetail" + char.ToUpperInvariant(member[0]) + member.Substring(1);
            var attribute = new StringBuilder("data-");
            foreach (var letter in key)
            {
                if (letter >= 'A' && letter <= 'Z') attribute.Append('-').Append(char.ToLowerInvariant(letter));
                else attribute.Append(letter);
            }
            return attribute.ToString();
        }
    }
}
