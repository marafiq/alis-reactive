/**
 * Default `TraceSink` — renders events to the browser DevTools console
 * with CSS-styled scope tags, inline JSON data, and breadcrumb tables
 * on error.
 *
 * Rendering rules (derived from browser-verification lessons on the
 * abandoned tracing branch):
 *
 * - Scope tag and level get %c CSS styling so DevTools colors them.
 * - Data is stringified inline in the message, NOT passed as a separate
 *   console argument — passing both causes DevTools to print the data
 *   twice (once in the formatted line, once as an expandable Object).
 * - Errors ARE passed as a separate argument so DevTools can expand
 *   the stack trace.
 * - Breadcrumbs render as a collapsed `console.table` only on error
 *   events, so the ring-buffer history is visible when debugging a
 *   failure.
 */

import type { Level, TraceEvent, TraceSink } from "./types";

/**
 * JSON.stringify wrapper that survives values the default serializer
 * cannot handle: circular references, BigInt, Symbol, functions, Map,
 * Set, and Error instances. Trace call sites pass raw runtime values
 * (`reaction.set` component refs, `gather.value` raw payloads, DOM
 * event details, etc.) and any of those can contain non-JSON-safe
 * shapes. A sink that throws in that scenario would escape through
 * the tracer and crash the reaction — so the ConsoleSink uses this
 * helper and the outer `makeEmit` also guards with try/catch as a
 * second line of defense.
 */
export function safeStringify(value: unknown): string {
  const seen = new WeakSet<object>();
  const replacer = (_key: string, val: unknown): unknown => {
    if (typeof val === "bigint") return `${val.toString()}n`;
    if (typeof val === "symbol") return val.toString();
    if (typeof val === "function") return `[Function ${val.name || "anonymous"}]`;
    if (val instanceof Error) {
      return { name: val.name, message: val.message };
    }
    if (typeof val === "object" && val !== null) {
      if (seen.has(val as object)) return "[Circular]";
      seen.add(val as object);
      if (val instanceof Map) return Object.fromEntries(val);
      if (val instanceof Set) return Array.from(val);
      // DOM nodes have circular parent/child references and a lot of
      // noise — represent them compactly.
      if (typeof Node !== "undefined" && val instanceof Node) {
        const el = val as { nodeName?: string; id?: string };
        const id = el.id ? `#${el.id}` : "";
        return `[Node ${el.nodeName ?? "unknown"}${id}]`;
      }
    }
    return val;
  };
  try {
    return JSON.stringify(value, replacer) ?? "undefined";
  } catch {
    return "[unserializable]";
  }
}

export class ConsoleSink implements TraceSink {
  emit(event: TraceEvent): void {
    const dataStr = event.data ? " " + safeStringify(event.data) : "";
    const tag = `%c[alis:${event.scope}]%c ${event.event} %c${event.level.toUpperCase()}${dataStr}`;
    const styles = [
      "color:#6366f1;font-weight:bold",
      "color:inherit",
      levelColor(event.level),
    ];

    const args: unknown[] = [tag, ...styles];
    if (event.error) {
      args.push(event.error);
    }

    switch (event.level) {
      case "error":
        console.error(...args);
        break;
      case "warn":
        console.warn(...args);
        break;
      case "info":
        console.info(...args);
        break;
      default:
        console.log(...args);
        break;
    }

    if (event.traceId) {
      console.log(`  trace: ${event.traceId}  span: ${event.spanId ?? ""}`);
    }

    if (event.level === "error" && event.breadcrumbs && event.breadcrumbs.length > 0) {
      console.groupCollapsed(`  breadcrumbs (${event.breadcrumbs.length})`);
      console.table(
        event.breadcrumbs.map((b) => ({
          time: b.time.toFixed(1),
          event: b.event,
          scope: b.scope,
          level: b.level,
        })),
      );
      console.groupEnd();
    }
  }

  flush(): void {
    // ConsoleSink is synchronous — nothing to flush.
  }
}

function levelColor(level: Exclude<Level, "off">): string {
  switch (level) {
    case "error":
      return "color:#ef4444;font-weight:bold";
    case "warn":
      return "color:#f59e0b;font-weight:bold";
    case "info":
      return "color:#10b981;font-weight:bold";
    case "debug":
      return "color:#6b7280";
    case "trace":
      return "color:#9ca3af";
  }
}
