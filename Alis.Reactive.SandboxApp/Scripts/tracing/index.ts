export { createTracer as tracer, configure, flush, getRootSpan } from "./trace";
export { ConsoleSink } from "./sink";
export { NOOP_SPAN } from "./span";
export type {
  Level, TraceEvent, SpanData, Breadcrumb, TraceSink,
  TraceConfig, Span, ScopedTracer, TraceRoot,
} from "./types";
export { LEVELS, SEVERITY } from "./types";
