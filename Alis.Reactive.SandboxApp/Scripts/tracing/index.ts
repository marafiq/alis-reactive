/**
 * Public barrel for the structured tracing module.
 *
 * Exactly 3 runtime exports (`tracer`, `configure`, `ConsoleSink`) and
 * 5 type exports (`Level`, `TraceEvent`, `TraceSink`, `TraceConfig`,
 * `ScopedTracer`). Nothing else. Span primitives, `flush`, `withSpan`,
 * `getRootSpan`, and every other span-lifecycle surface are deliberately
 * absent — lifecycle lives inside `interactions.ts` and never crosses
 * this barrier.
 */

export { configure, tracer } from "./trace";
export { ConsoleSink } from "./sink";
export type {
  Level,
  ScopedTracer,
  TraceConfig,
  TraceEvent,
  TraceSink,
} from "./types";
