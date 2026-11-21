import { diag, SpanKind, SpanStatusCode, TraceFlags, type HrTime, type SpanContext, type SpanStatus } from "@opentelemetry/api";
import { resourceFromAttributes, type Resource } from "@opentelemetry/resources";
import type { ReadableSpan, SpanExporter, TimedEvent } from "@opentelemetry/sdk-trace";

export type Attribute = string | number | boolean;

type ExportResult = Parameters<Parameters<SpanExporter["export"]>[1]>[0];

export interface SpanEvent {
  readonly name: string;
  readonly time: number;
  readonly attributes: Readonly<Record<string, Attribute>>;
}

export interface EndedSpan {
  readonly name: string;
  readonly kind: "internal" | "client";
  readonly traceId: string;
  readonly spanId: string;
  readonly parentId: string;
  readonly parentIsRemote: boolean;
  readonly start: number;
  readonly end: number;
  readonly outcome: string;
  readonly attributes: Readonly<Record<string, Attribute>>;
  readonly events: readonly SpanEvent[];
}

const scope = { name: "Alis.Reactive" };
const serviceName = "service.name";
const outcome = "alis.outcome";
const responseStatus = "http.response.status_code";
const aBoundaryError = "alis.error";
const firstFailedStatus = 400;
const microsPerMilli = 1000;
const microsPerSecond = 1_000_000;
const nanosPerMicro = 1000;

function hrTimeOf(millis: number): HrTime {
  const micros = Math.round(millis * microsPerMilli);
  return [Math.floor(micros / microsPerSecond), (micros % microsPerSecond) * nanosPerMicro];
}

function contextOf(span: EndedSpan): SpanContext {
  return { traceId: span.traceId, spanId: span.spanId, traceFlags: TraceFlags.SAMPLED, isRemote: false };
}

function statusOf(span: EndedSpan): SpanStatus {
  const answer = span.attributes[responseStatus];
  const failedAnswer = span.kind === "client" && typeof answer === "number" && answer >= firstFailedStatus;
  const heldABoundaryError = span.events.some((event) => event.name === aBoundaryError);
  return failedAnswer || heldABoundaryError ? { code: SpanStatusCode.ERROR } : { code: SpanStatusCode.UNSET };
}

function eventOf(event: SpanEvent): TimedEvent {
  return { name: event.name, time: hrTimeOf(event.time), attributes: event.attributes };
}

function readableOf(span: EndedSpan, resource: Resource): ReadableSpan {
  const context = contextOf(span);
  return {
    name: span.name,
    kind: span.kind === "client" ? SpanKind.CLIENT : SpanKind.INTERNAL,
    spanContext: () => context,
    parentSpanContext: { traceId: span.traceId, spanId: span.parentId, traceFlags: TraceFlags.SAMPLED, isRemote: span.parentIsRemote },
    startTime: hrTimeOf(span.start),
    endTime: hrTimeOf(span.end),
    duration: hrTimeOf(span.end - span.start),
    status: statusOf(span),
    attributes: { ...span.attributes, [outcome]: span.outcome },
    links: [],
    events: span.events.map(eventOf),
    ended: true,
    resource,
    instrumentationScope: scope,
    droppedAttributesCount: 0,
    droppedEventsCount: 0,
    droppedLinksCount: 0,
  };
}

function reportFailure(result: ExportResult): void {
  if (result.error === undefined) return;
  diag.error("The OpenTelemetry exporter could not send a span: " + result.error.message);
}

export class TraceSink {
  readonly #exporter: SpanExporter;
  readonly #resource: Resource;

  constructor(exporter: SpanExporter, service: string) {
    this.#exporter = exporter;
    this.#resource = resourceFromAttributes({ [serviceName]: service });
  }

  ended(span: EndedSpan): void {
    this.#exporter.export([readableOf(span, this.#resource)], reportFailure);
  }
}
