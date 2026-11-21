interface LineMessage {
  readonly id: string;
  readonly text: string;
}

export class CoordinatorLineFormat {
  members(names: readonly string[]): string {
    return names.join(", ");
  }

  transcript(messages: readonly LineMessage[]): string {
    return messages.length === 0 ? "No messages." : messages.map(message => message.id + " · " + message.text).join("\n");
  }
}
