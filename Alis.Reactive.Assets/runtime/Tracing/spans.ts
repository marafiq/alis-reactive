export type SpanName =
  | "alis.boot"
  | "alis.run"
  | "alis.send"
  | "alis.method"
  | "alis.confirm"
  | "alis.hub"
  | "alis.all"
  | "alis.slot.load"
  | "alis.slot.unload";

export type EventName = "alis.stop" | "alis.missing" | "alis.refused" | "alis.error";

export type SpanOutcome = "completed" | "stopped" | "replaced" | "disposed";

export type SpanKind = "internal" | "client";

export type Attribute = string | number | boolean;

export type Attributes = Readonly<Record<string, Attribute>>;

export interface SpanEvent {
  readonly name: EventName;
  readonly time: number;
  readonly attributes: Attributes;
}

export interface EndedSpan {
  readonly name: string;
  readonly kind: SpanKind;
  readonly traceId: string;
  readonly spanId: string;
  readonly parentId: string;
  readonly parentIsRemote: boolean;
  readonly start: number;
  readonly end: number;
  readonly outcome: SpanOutcome;
  readonly attributes: Attributes;
  readonly events: readonly SpanEvent[];
}

export interface TraceSink {
  ended(span: EndedSpan): void;
}

export type SinkLookup = { readonly kind: "none" } | { readonly kind: "registered"; readonly sink: TraceSink };

export interface Log {
  error(message: string): void;
  info(message: string): void;
}

export interface Ids {
  traceId(): string;
  spanId(): string;
}

export type TraceHeaders = Readonly<Record<string, string>>;

export interface Attempting {
  readonly headers: TraceHeaders;
  answered(status: number): void;
  end(outcome: SpanOutcome): void;
}

export interface Sending {
  attempt(toThePagesOrigin: boolean, method: string, template: string): Attempting;
  end(outcome: SpanOutcome): void;
}

export interface Traced {
  handler(index: number, event: string): Traced;
  run(): Traced;
  span(name: SpanName, attributes: Attributes): Traced;
  send(): Sending;
  event(name: EventName, attributes: Attributes): void;
  end(outcome: SpanOutcome): void;
}
