using System;

namespace Alis.Reactive.RequestsAndResponses;

internal static class WaitingBodyDoors
{
    internal static NotImplementedException Container(string door)
        => new($"{door} sends the framework inputs of a container, which waits on the container's capture schema.md states: alis.model's sent, and SentInputs' captured container with its id, body and inputs.");

    internal static NotImplementedException Form(string door)
        => new($"{door} sends a native form's submission set, which waits on the container's capture schema.md states: SentInputs' captured container with its id, body and inputs.");

    internal static NotImplementedException Input(string door)
        => new($"{door} sends one input of the view, which waits on the input's wire form: schema.md's InputBinding carries its name, path, value, committed, invalidClass and message, and no wire form.");
}
