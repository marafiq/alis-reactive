using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Commands;

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

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Command>? WhileLoading { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<StatusHandler>? OnSuccess { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<StatusHandler>? OnError { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public RequestDescriptor? Chained { get; }

        public RequestDescriptor(
            string verb,
            string url,
            List<GatherItem>? gather = null,
            List<Command>? whileLoading = null,
            List<StatusHandler>? onSuccess = null,
            List<StatusHandler>? onError = null,
            RequestDescriptor? chained = null)
        {
            Verb = verb;
            Url = url;
            Gather = gather;
            WhileLoading = whileLoading;
            OnSuccess = onSuccess;
            OnError = onError;
            Chained = chained;
        }
    }
}
