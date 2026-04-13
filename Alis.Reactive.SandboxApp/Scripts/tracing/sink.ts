import type { TraceEvent, SpanData, Level } from "./types";

export function serializeError(err: Error): { type: string; message: string; stack?: string; cause?: string } {
  const result: { type: string; message: string; stack?: string; cause?: string } = {
    type: err.constructor.name,
    message: err.message,
    stack: err.stack,
  };
  const cause = (err as unknown as { cause?: unknown }).cause;
  if (cause instanceof Error) {
    result.cause = `${cause.constructor.name}: ${cause.message}`;
  }
  return result;
}

export class ConsoleSink {
  emit(event: TraceEvent): void {
    const tag = `%c[alis:${event.scope}]%c ${event.event} %c${event.level.toUpperCase()}`;
    const styles = [
      "color:#6366f1;font-weight:bold",
      "color:inherit",
      levelColor(event.level),
    ];

    // Data is stringified inline for greppability (Playwright, log aggregators)
    // and human readability. Passing data as a second arg too would print it
    // twice in Chrome DevTools — once inline, once as an expandable Object.
    const dataStr = event.data ? " " + JSON.stringify(event.data) : "";
    const args: unknown[] = [tag + dataStr, ...styles];
    if (event.error) args.push(event.error);

    switch (event.level) {
      case "error": console.error(...args); break;
      case "warn":  console.warn(...args); break;
      case "info":  console.info(...args); break;
      default:      console.log(...args); break;
    }

    if (event.traceId) {
      console.log(`  trace: ${event.traceId}  span: ${event.spanId}`);
    }

    if (event.level === "error" && event.breadcrumbs && event.breadcrumbs.length > 0) {
      console.groupCollapsed(`  breadcrumbs (${event.breadcrumbs.length})`);
      console.table(event.breadcrumbs.map(b => ({
        time: b.time.toFixed(1),
        event: b.event,
        scope: b.scope,
        level: b.level,
      })));
      console.groupEnd();
    }
  }

  span(data: SpanData): void {
    const status = data.status === "error" ? " ERROR" : "";
    const label = `[alis:${data.scope}] ${data.name}  ${data.durationMs.toFixed(1)}ms${status}`;
    console.groupCollapsed(label);
    if (Object.keys(data.attributes).length > 0) {
      console.table(data.attributes);
    }
    if (data.events.length > 0) {
      console.table(data.events.map(e => ({
        event: e.name,
        offset_ms: (e.time - data.startTime).toFixed(1),
        ...e.attributes,
      })));
    }
    console.groupEnd();
  }

  flush(): void {}
}

function levelColor(level: Level): string {
  switch (level) {
    case "error": return "color:#ef4444;font-weight:bold";
    case "warn":  return "color:#f59e0b";
    case "info":  return "color:#3b82f6";
    case "debug": return "color:#6b7280";
    case "trace": return "color:#9ca3af";
    default:      return "color:inherit";
  }
}
