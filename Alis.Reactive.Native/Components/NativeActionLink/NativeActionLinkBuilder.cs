using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Reactions;
using Alis.Reactive.Descriptors.Requests;
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
            Action<PipelineBuilder<TModel>> configure)
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
            Action<PipelineBuilder<TModel>> configure)
            where TModel : class
        {
            var pipeline = new PipelineBuilder<TModel>();
            configure(pipeline);

            var reaction = pipeline.BuildReaction();
            var state = new RequestProjectionState();
            var projectedReaction = ProjectReaction(reaction, href, state);
            if (state.RequestCount != 1)
            {
                throw new InvalidOperationException(
                    "NativeActionLink supports exactly one request in its click reaction tree.");
            }

            var payloadJson = JsonSerializer.Serialize(
                new NativeActionLinkPayload(projectedReaction),
                CompactOptions);

            return new NativeActionLinkContract(payloadJson);
        }

        private static Reaction ProjectReaction(
            Reaction reaction,
            string href,
            RequestProjectionState state)
        {
            if (reaction is SequentialReaction sequential)
            {
                return new SequentialReaction(sequential.Commands);
            }

            if (reaction is ConditionalReaction conditional)
            {
                var projectedBranches = new List<Branch>();
                foreach (var branch in conditional.Branches)
                {
                    projectedBranches.Add(new Branch(branch.Guard, ProjectReaction(branch.Reaction, href, state)));
                }

                var commands = conditional.Commands == null
                    ? null
                    : new List<Command>(conditional.Commands);
                return new ConditionalReaction(commands, projectedBranches);
            }

            if (reaction is HttpReaction http)
            {
                state.RequestCount++;
                if (state.RequestCount > 1)
                {
                    throw new InvalidOperationException(
                        "NativeActionLink supports exactly one request in its click reaction tree.");
                }

                if (!string.Equals(href, http.Request.Url, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "NativeActionLink href must match the request URL in the configured request chain.");
                }

                var preFetch = http.PreFetch == null
                    ? null
                    : new List<Command>(http.PreFetch);
                var request = ProjectRequest(http.Request);
                return new HttpReaction(preFetch, request);
            }

            if (reaction is ParallelHttpReaction)
            {
                throw new InvalidOperationException(
                    "NativeActionLink does not support Parallel(...) request chains.");
            }

            throw new InvalidOperationException("Unsupported NativeActionLink reaction shape.");
        }

        private static RequestDescriptor ProjectRequest(RequestDescriptor request)
        {
            if (request.Chained != null)
            {
                throw new InvalidOperationException(
                    "NativeActionLink supports exactly one request. Response.Chained(...) is not supported.");
            }

            if (request.Validation != null)
            {
                throw new InvalidOperationException(
                    "NativeActionLink does not support validation. Use a plan-backed trigger for validated flows.");
            }

            if (request.Gather != null)
            {
                foreach (var item in request.Gather)
                {
                    if (item is AllGather)
                    {
                        throw new InvalidOperationException(
                            "NativeActionLink does not support IncludeAll(). Use explicit gather instead.");
                    }
                }
            }

            return new RequestDescriptor(
                request.Verb,
                string.Empty,
                request.Gather == null ? null : new List<GatherItem>(request.Gather),
                request.ContentType,
                request.WhileLoading == null ? null : new List<Command>(request.WhileLoading),
                ProjectHandlers(request.OnSuccess, new RequestProjectionState { RequestCount = 1 }),
                ProjectHandlers(request.OnError, new RequestProjectionState { RequestCount = 1 }),
                chained: null,
                validation: null);
        }

        private static List<StatusHandler>? ProjectHandlers(List<StatusHandler>? handlers, RequestProjectionState state)
        {
            if (handlers == null || handlers.Count == 0)
            {
                return null;
            }

            var projected = new List<StatusHandler>();
            foreach (var handler in handlers)
            {
                if (handler.Reaction != null)
                {
                    var reaction = ProjectReaction(handler.Reaction, string.Empty, state);
                    projected.Add(handler.StatusCode.HasValue
                        ? new StatusHandler(handler.StatusCode.Value, reaction)
                        : new StatusHandler(reaction));
                    continue;
                }

                if (handler.Commands != null)
                {
                    projected.Add(handler.StatusCode.HasValue
                        ? new StatusHandler(handler.StatusCode.Value, handler.Commands)
                        : new StatusHandler(handler.Commands));
                }
            }

            return projected.Count == 0 ? null : projected;
        }

        private sealed class RequestProjectionState
        {
            public int RequestCount { get; set; }
        }
    }

    internal sealed class NativeActionLinkContract
    {
        internal NativeActionLinkContract(string payloadJson)
        {
            PayloadJson = payloadJson;
        }

        internal string PayloadJson { get; }
    }

    internal sealed class NativeActionLinkPayload
    {
        public NativeActionLinkPayload(Reaction reaction)
        {
            Reaction = reaction;
        }

        public Reaction Reaction { get; }
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
