using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Descriptors.Requests
{
    public class StatusHandler
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? StatusCode { get; }

        public List<Command> Commands { get; }

        public StatusHandler(int? statusCode, List<Command> commands)
        {
            StatusCode = statusCode;
            Commands = commands;
        }
    }

    public class RequestDescriptor
    {
        public string Verb { get; }
        public string Url { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<GatherItem>? Gather { get; }

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

        [JsonIgnore]
        internal string? ValidationPrefix { get; set; }

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
