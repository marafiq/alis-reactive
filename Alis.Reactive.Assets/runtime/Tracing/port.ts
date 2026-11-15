import { assertNever, type OnTrace, type Source, type Trace } from "../Schema/index";
import type {
  Attempting,
  Attributes,
  EndedSpan,
  EventName,
  Ids,
  Log,
  Sending,
  SinkLookup,
  SpanEvent,
  SpanKind,
  SpanName,
  SpanOutcome,
  TraceHeaders,
  Traced,
} from "./spans";

const version = "00";
const sampled = "01";
const resendCount = "http.request.resend_count";
const traceIdAt = { from: 3, to: 35 };
const parentIdAt = { from: 36, to: 52 };

type Deliver = (span: EndedSpan) => void;

type Minted = { readonly kind: "unminted" } | { readonly kind: "minted"; readonly traceId: string };

function traceparentOf(traceId: string, spanId: string): string {
  return version + "-" + traceId + "-" + spanId + "-" + sampled;
}

function resentOf(attempt: number): TraceHeaders {
  return attempt >= 2 ? { baggage: resendCount + "=" + String(attempt - 1) } : {};
}

function messageOf(error: unknown): string {
  return error instanceof Error ? error.message : String(error);
}

function printed(span: EndedSpan): string {
  return (
    "alis span " + span.name + " " + span.outcome + " in " + String(Math.round(span.end - span.start)) + " ms, trace " + span.traceId + ", span " +
    span.spanId + ", parent " + span.parentId
  );
}

function recordsNothing(): void {
  return undefined;
}

function sourceAt(sources: readonly Source[], index: number): Source {
  const source = sources[index];
  if (source === undefined) throw new Error("The plan's trace holds no source for its handler " + String(index) + ".");
  return source;
}

export class Port {
  readonly #sinkOf: () => SinkLookup;
  readonly #ids: Ids;
  readonly #clock: () => number;
  readonly #log: Log;
  #sinkFailed = false;

  constructor(sinkOf: () => SinkLookup, ids: Ids, clock: () => number, log: Log) {
    this.#sinkOf = sinkOf;
    this.#ids = ids;
    this.#clock = clock;
    this.#log = log;
  }

  planTrace(trace: Trace): Traced {
    switch (trace.kind) {
      case "off":
        return new Unrecorded(this.#ids);
      case "on":
        return new Recorded(
          new PlanContext(trace, this.#ids, this.#clock, (span) => {
            this.#deliver(span, trace.console);
          }),
          "root",
          {},
        );
      default:
        return assertNever(trace);
    }
  }

  #deliver(span: EndedSpan, toConsole: boolean): void {
    if (toConsole) this.#log.info(printed(span));
    if (this.#sinkFailed) return;
    try {
      const lookup = this.#sinkOf();
      if (lookup.kind === "registered") lookup.sink.ended(span);
    } catch (error) {
      this.#sinkFailed = true;
      this.#log.error("The trace sink threw at the span " + span.name + " " + span.spanId + ", so this page calls it no more: " + messageOf(error));
    }
  }
}

class PlanContext {
  readonly traceId: string;
  readonly parentId: string;
  readonly tracestate: string;
  readonly sources: readonly Source[];
  readonly #ids: Ids;
  readonly #clock: () => number;
  readonly #deliver: Deliver;

  constructor(trace: OnTrace, ids: Ids, clock: () => number, deliver: Deliver) {
    this.traceId = trace.traceparent.slice(traceIdAt.from, traceIdAt.to);
    this.parentId = trace.traceparent.slice(parentIdAt.from, parentIdAt.to);
    this.tracestate = trace.tracestate;
    this.sources = trace.sources;
    this.#ids = ids;
    this.#clock = clock;
    this.#deliver = deliver;
  }

  started(name: string, attributes: Attributes): Span {
    return new Span(this, { name, kind: "internal", parentId: this.parentId, parentIsRemote: true }, attributes);
  }

  mintedSpanId(): string {
    return this.#ids.spanId();
  }

  unrecorded(): Traced {
    return new Unrecorded(this.#ids);
  }

  now(): number {
    return this.#clock();
  }

  ended(span: EndedSpan): void {
    this.#deliver(span);
  }
}

interface Placed {
  readonly name: string;
  readonly kind: SpanKind;
  readonly parentId: string;
  readonly parentIsRemote: boolean;
}

class Span {
  readonly spanId: string;
  readonly #plan: PlanContext;
  readonly #placed: Placed;
  readonly #start: number;
  readonly #events: SpanEvent[] = [];
  readonly #children: Span[] = [];
  #attributes: Attributes;
  #open = true;

  constructor(plan: PlanContext, placed: Placed, attributes: Attributes) {
    this.#plan = plan;
    this.#placed = placed;
    this.#attributes = attributes;
    this.spanId = plan.mintedSpanId();
    this.#start = plan.now();
  }

  get isOpen(): boolean {
    return this.#open;
  }

  child(name: string, kind: SpanKind, attributes: Attributes): Span {
    const child = new Span(this.#plan, { name, kind, parentId: this.spanId, parentIsRemote: false }, attributes);
    this.#children.push(child);
    return child;
  }

  records(attributes: Attributes): void {
    this.#attributes = { ...this.#attributes, ...attributes };
  }

  event(name: EventName, attributes: Attributes): void {
    if (!this.#open) return;
    this.#events.push({ name, time: this.#plan.now(), attributes });
  }

  end(outcome: SpanOutcome): void {
    if (!this.#open) return;
    for (const child of this.#children) child.end(outcome);
    this.#open = false;
    this.#plan.ended({
      name: this.#placed.name,
      kind: this.#placed.kind,
      traceId: this.#plan.traceId,
      spanId: this.spanId,
      parentId: this.#placed.parentId,
      parentIsRemote: this.#placed.parentIsRemote,
      start: this.#start,
      end: this.#plan.now(),
      outcome,
      attributes: this.#attributes,
      events: [...this.#events],
    });
  }
}

class Recorded implements Traced {
  readonly #plan: PlanContext;
  readonly #current: Span | "root";
  readonly #runAttributes: Attributes;

  constructor(plan: PlanContext, current: Span | "root", runAttributes: Attributes) {
    this.#plan = plan;
    this.#current = current;
    this.#runAttributes = runAttributes;
  }

  handler(index: number, event: string): Traced {
    const source = sourceAt(this.#plan.sources, index);
    return new Recorded(this.#plan, "root", { "alis.event": event, "code.file.path": source.view, "code.line.number": source.line });
  }

  run(): Traced {
    return new Recorded(this.#plan, this.#plan.started("alis.run", this.#runAttributes), {});
  }

  span(name: SpanName, attributes: Attributes): Traced {
    if (this.#current === "root") return new Recorded(this.#plan, this.#plan.started(name, attributes), {});
    if (!this.#current.isOpen) return this.#plan.unrecorded();
    return new Recorded(this.#plan, this.#current.child(name, "internal", attributes), {});
  }

  send(): Sending {
    if (this.#current !== "root" && !this.#current.isOpen) return this.#plan.unrecorded().send();
    return new RecordedSend(this.#plan, this.#current);
  }

  event(name: EventName, attributes: Attributes): void {
    if (this.#current !== "root") this.#current.event(name, attributes);
  }

  end(outcome: SpanOutcome): void {
    if (this.#current !== "root") this.#current.end(outcome);
  }
}

class RecordedSend implements Sending {
  readonly #plan: PlanContext;
  readonly #parent: Span | "root";
  #span: Span | "unsent" = "unsent";
  #attempts = 0;

  constructor(plan: PlanContext, parent: Span | "root") {
    this.#plan = plan;
    this.#parent = parent;
  }

  attempt(toThePagesOrigin: boolean, method: string, template: string): Attempting {
    this.#attempts += 1;
    const attempt = this.#attempts;
    const resent: Attributes = attempt >= 2 ? { [resendCount]: attempt - 1 } : {};
    const span = this.#sent().child(method + " " + template, "client", { "http.request.method": method, "url.template": template, ...resent });
    return new RecordedAttempt(span, toThePagesOrigin ? this.#headersOf(span, attempt) : {});
  }

  end(outcome: SpanOutcome): void {
    if (this.#span !== "unsent") this.#span.end(outcome);
  }

  #sent(): Span {
    if (this.#span !== "unsent") return this.#span;
    const send = this.#parent === "root" ? this.#plan.started("alis.send", {}) : this.#parent.child("alis.send", "internal", {});
    this.#span = send;
    return send;
  }

  #headersOf(span: Span, attempt: number): TraceHeaders {
    const state: TraceHeaders = this.#plan.tracestate === "" ? {} : { tracestate: this.#plan.tracestate };
    return { traceparent: traceparentOf(this.#plan.traceId, span.spanId), ...state, ...resentOf(attempt) };
  }
}

class RecordedAttempt implements Attempting {
  readonly headers: TraceHeaders;
  readonly #span: Span;

  constructor(span: Span, headers: TraceHeaders) {
    this.#span = span;
    this.headers = headers;
  }

  answered(status: number): void {
    this.#span.records({ "http.response.status_code": status });
    this.#span.end("completed");
  }

  end(outcome: SpanOutcome): void {
    this.#span.end(outcome);
  }
}

class Unrecorded implements Traced {
  readonly #ids: Ids;

  constructor(ids: Ids) {
    this.#ids = ids;
  }

  handler(): Traced {
    return this;
  }

  run(): Traced {
    return this;
  }

  span(): Traced {
    return this;
  }

  send(): Sending {
    return new UnrecordedSend(this.#ids);
  }

  event(): void {
    return undefined;
  }

  end(): void {
    return undefined;
  }
}

class UnrecordedSend implements Sending {
  readonly #ids: Ids;
  #attempts = 0;
  #minted: Minted = { kind: "unminted" };

  constructor(ids: Ids) {
    this.#ids = ids;
  }

  attempt(toThePagesOrigin: boolean): Attempting {
    this.#attempts += 1;
    const retriedHome = this.#attempts >= 2 && toThePagesOrigin;
    const headers: TraceHeaders = retriedHome ? { traceparent: traceparentOf(this.#traceId(), this.#ids.spanId()), ...resentOf(this.#attempts) } : {};
    return { headers, answered: recordsNothing, end: recordsNothing };
  }

  end(): void {
    return undefined;
  }

  #traceId(): string {
    if (this.#minted.kind === "minted") return this.#minted.traceId;
    const traceId = this.#ids.traceId();
    this.#minted = { kind: "minted", traceId };
    return traceId;
  }
}
