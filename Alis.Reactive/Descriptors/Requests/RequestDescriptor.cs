using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Reactions;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Descriptors.Requests
{
    public sealed class StatusHandler
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? StatusCode { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Command>? Commands { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Reaction? Reaction { get; }

        /// <summary>Default handler — sequential commands only.</summary>
        public StatusHandler(List<Command> commands)
        {
            StatusCode = null;
            Commands = commands;
        }

        /// <summary>Status-specific handler — sequential commands only.</summary>
        public StatusHandler(int statusCode, List<Command> commands)
        {
            StatusCode = statusCode;
            Commands = commands;
        }

        /// <summary>Default handler — full reaction (conditional, http, etc.).</summary>
        public StatusHandler(Reaction reaction)
        {
            StatusCode = null;
            Reaction = reaction;
        }

        /// <summary>Status-specific handler — full reaction.</summary>
        public StatusHandler(int statusCode, Reaction reaction)
        {
            StatusCode = statusCode;
            Reaction = reaction;
        }
    }

    public sealed class RequestDescriptor
    {
        public string Verb { get; }
        public string Url { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<GatherItem>? Gather { get; internal set; }

        /// <summary>
        /// Request body format: "json" (default) or "form-data" (multipart/form-data for file uploads).
        /// Omitted from JSON when "json" (default). Only serialized when explicitly set to "form-data".
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ContentType { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Command>? WhileLoading { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<StatusHandler>? OnSuccess { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<StatusHandler>? OnError { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public RequestDescriptor? Chained { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ValidationDescriptor? Validation { get; internal set; }

        [JsonIgnore]
        internal Type? ValidatorType { get; set; }

        public RequestDescriptor(
            string verb,
            string url,
            List<GatherItem>? gather = null,
            string? contentType = null,
            List<Command>? whileLoading = null,
            List<StatusHandler>? onSuccess = null,
            List<StatusHandler>? onError = null,
            RequestDescriptor? chained = null,
            ValidationDescriptor? validation = null)
        {
            Verb = verb;
            Url = url;
            Gather = gather;
            ContentType = contentType;
            WhileLoading = whileLoading;
            OnSuccess = onSuccess;
            OnError = onError;
            Chained = chained;
            Validation = validation;
        }
    }
}
