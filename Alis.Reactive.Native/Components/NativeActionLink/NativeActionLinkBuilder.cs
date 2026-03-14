using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Reactions;
using Alis.Reactive.Descriptors.Requests;
using Alis.Reactive.Descriptors.Triggers;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    public sealed class NativeActionLinkBuilder<TModel> : IHtmlContent
        where TModel : class
    {
        private readonly string _elementId;
        private readonly string _text;
        private readonly string _href;
        private readonly string _payloadJson;
        private readonly Dictionary<string, string> _attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private string? _cssClass;

        internal NativeActionLinkBuilder(string elementId, string text, string href, string payloadJson)
        {
            _elementId = elementId;
            _text = text;
            _href = href;
            _payloadJson = payloadJson;
        }

        public NativeActionLinkBuilder<TModel> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        public NativeActionLinkBuilder<TModel> Attr(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Attribute name cannot be null or whitespace.", nameof(name));

            if (string.Equals(name, "class", StringComparison.OrdinalIgnoreCase))
            {
                return CssClass(value);
            }

            if (IsReservedAttribute(name))
                throw new InvalidOperationException(
                    $"Attribute '{name}' is reserved by NativeActionLink and cannot be overridden.");

            _attributes[name] = value;
            return this;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            writer.Write("<a");
            writer.Write(" id=\"");
            writer.Write(encoder.Encode(_elementId));
            writer.Write("\"");
            writer.Write(" href=\"");
            writer.Write(encoder.Encode(_href));
            writer.Write("\"");
            writer.Write(" data-reactive-link=\"");
            writer.Write(encoder.Encode(_payloadJson));
            writer.Write("\"");

            if (!string.IsNullOrWhiteSpace(_cssClass))
            {
                writer.Write(" class=\"");
                writer.Write(encoder.Encode(_cssClass));
                writer.Write("\"");
            }

            foreach (var attribute in _attributes)
            {
                writer.Write(" ");
                writer.Write(encoder.Encode(attribute.Key));
                writer.Write("=\"");
                writer.Write(encoder.Encode(attribute.Value));
                writer.Write("\"");
            }

            writer.Write(">");
            writer.Write(encoder.Encode(_text));
            writer.Write("</a>");
        }

        private static bool IsReservedAttribute(string name)
        {
            return string.Equals(name, "id", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "href", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "data-reactive-link", StringComparison.OrdinalIgnoreCase);
        }
    }

    public static class NativeActionLinkHtmlExtensions
    {
        public static NativeActionLinkBuilder<TModel> NativeActionLink<TModel>(
            this IHtmlHelper<TModel> html,
            string linkText,
            string url,
            Func<PipelineBuilder<TModel>, HttpRequestBuilder<TModel>> configure)
            where TModel : class
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var contract = NativeActionLinkSerializer.CreateContract(url, configure);
            var elementId = NativeActionLinkIdGenerator.Next<TModel>(html.ViewContext);
            return new NativeActionLinkBuilder<TModel>(elementId, linkText, url, contract.PayloadJson);
        }
    }

    internal static class NativeActionLinkSerializer
    {
        private static readonly JsonSerializerOptions CompactOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        internal static NativeActionLinkContract CreateContract<TModel>(
            string href,
            Func<PipelineBuilder<TModel>, HttpRequestBuilder<TModel>> configure)
            where TModel : class
        {
            var pipeline = new PipelineBuilder<TModel>();
            var requestBuilder = configure(pipeline);
            if (requestBuilder == null)
                throw new InvalidOperationException(
                    "NativeActionLink configure must return the existing one-request HttpRequestBuilder chain.");

            if (!(pipeline.BuildReaction() is HttpReaction reaction))
            {
                throw new InvalidOperationException(
                    "NativeActionLink supports exactly one HTTP request chain. " +
                    "Start the chain through PipelineBuilder.Get/Post/Put/Delete and return that HttpRequestBuilder.");
            }

            if (!string.Equals(href, reaction.Request.Url, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "NativeActionLink href must match the request URL in the configured one-request chain.");
            }

            EnsureSupportedContract(reaction);

            var tempPlan = new ReactivePlan<TModel>();
            tempPlan.AddEntry(new Entry(new DomReadyTrigger(), reaction));

            using var planDoc = JsonDocument.Parse(tempPlan.Render());
            var reactionElement = planDoc.RootElement
                .GetProperty("entries")[0]
                .GetProperty("reaction")
                .Clone();

            var payloadJson = JsonSerializer.Serialize(
                new NativeActionLinkPayload(tempPlan.PlanId, reactionElement),
                CompactOptions);

            return new NativeActionLinkContract(tempPlan.PlanId, payloadJson);
        }

        private static void EnsureSupportedContract(HttpReaction reaction)
        {
            if (reaction.Request.Chained != null)
            {
                throw new InvalidOperationException(
                    "NativeActionLink supports one request only. Response.Chained(...) is not supported.");
            }

            EnsureHandlersContainNoNestedHttp(reaction.Request.OnSuccess);
            EnsureHandlersContainNoNestedHttp(reaction.Request.OnError);
        }

        private static void EnsureHandlersContainNoNestedHttp(List<StatusHandler>? handlers)
        {
            if (handlers == null)
                return;

            foreach (var handler in handlers)
            {
                if (handler.Reaction != null)
                {
                    EnsureReactionContainsNoHttp(handler.Reaction);
                }
            }
        }

        private static void EnsureReactionContainsNoHttp(Reaction reaction)
        {
            if (reaction is SequentialReaction)
            {
                return;
            }

            var conditional = reaction as ConditionalReaction;
            if (conditional != null)
            {
                foreach (var branch in conditional.Branches)
                {
                    EnsureReactionContainsNoHttp(branch.Reaction);
                }
                return;
            }

            if (reaction is HttpReaction || reaction is ParallelHttpReaction)
            {
                throw new InvalidOperationException(
                    "NativeActionLink response handlers may contain commands and conditional branches, " +
                    "but they cannot start nested HTTP requests.");
            }

            throw new InvalidOperationException("Unsupported NativeActionLink reaction shape.");
        }
    }

    internal sealed class NativeActionLinkContract
    {
        internal NativeActionLinkContract(string planId, string payloadJson)
        {
            PlanId = planId;
            PayloadJson = payloadJson;
        }

        internal string PlanId { get; }
        internal string PayloadJson { get; }
    }

    internal sealed class NativeActionLinkPayload
    {
        public NativeActionLinkPayload(string planId, JsonElement reaction)
        {
            PlanId = planId;
            Reaction = reaction;
        }

        public string PlanId { get; }
        public JsonElement Reaction { get; }
    }

    internal static class NativeActionLinkIdGenerator
    {
        private const string CounterKeyPrefix = "__alis_native_action_link_counter__";

        internal static string Next<TModel>(ViewContext viewContext) where TModel : class
        {
            var scope = IdGenerator.TypeScope(typeof(TModel));
            var counterKey = CounterKeyPrefix + scope;
            var next = 1;

            if (viewContext.HttpContext.Items.TryGetValue(counterKey, out var value) && value is int current)
            {
                next = current + 1;
            }

            viewContext.HttpContext.Items[counterKey] = next;
            return scope + "__native_action_link_" + next;
        }
    }
}
