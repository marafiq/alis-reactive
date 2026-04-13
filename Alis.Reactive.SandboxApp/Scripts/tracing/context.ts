/**
 * W3C Trace Context helpers and ID generation for the tracing module.
 *
 * Internal to the tracing module. Not exported from `index.ts`.
 *
 * Wire format reference: https://www.w3.org/TR/trace-context/#traceparent-header
 *   traceparent = version "-" trace-id "-" parent-id "-" trace-flags
 *   version     = 2 lowercase hex digits (00)
 *   trace-id    = 32 lowercase hex digits, not all zero
 *   parent-id   = 16 lowercase hex digits, not all zero
 *   trace-flags = 2 lowercase hex digits
 */

import { LEVELS, type Level } from "./types";

const VERSION = "00";
const TRACEPARENT_RE = /^([0-9a-f]{2})-([0-9a-f]{32})-([0-9a-f]{16})-([0-9a-f]{2})$/;
const INVALID_TRACE_ID = "00000000000000000000000000000000";
const INVALID_SPAN_ID = "0000000000000000";

/** Parsed components of a W3C traceparent header. */
export interface ParsedTraceparent {
  readonly version: string;
  readonly traceId: string;
  readonly parentId: string;
  readonly flags: string;
}

/**
 * Parse a W3C traceparent header. Returns undefined when the input is
 * missing, malformed, or uses the reserved all-zero trace-id or span-id.
 *
 * Only version 00 is currently recognized; future versions are treated as
 * unparseable to keep the runtime strict about unknown shapes.
 */
export function parseTraceparent(raw: string | undefined): ParsedTraceparent | undefined {
  if (!raw) return undefined;
  const match = TRACEPARENT_RE.exec(raw);
  if (!match) return undefined;
  const [, version, traceId, parentId, flags] = match;
  if (version !== VERSION) return undefined;
  if (traceId === INVALID_TRACE_ID) return undefined;
  if (parentId === INVALID_SPAN_ID) return undefined;
  return { version, traceId, parentId, flags };
}

/**
 * Format a W3C traceparent header from its components.
 * Trace-flags default to "01" (sampled) if the caller omits them.
 */
export function formatTraceparent(traceId: string, spanId: string, flags = "01"): string {
  return `${VERSION}-${traceId}-${spanId}-${flags}`;
}

/**
 * Coerce user input into a valid {@link Level}. Returns the fallback when
 * the input is missing or unrecognized. `data-trace` HTML attribute takes
 * precedence over `plan.traceLevel` when both are present.
 */
export function resolveLevel(
  planLevel: string | undefined,
  datasetLevel: string | undefined,
  fallback: Level = "off",
): Level {
  if (isValidLevel(datasetLevel)) return datasetLevel;
  if (isValidLevel(planLevel)) return planLevel;
  return fallback;
}

/** Type guard: does `candidate` name one of the six {@link Level} values? */
export function isValidLevel(candidate: string | undefined): candidate is Level {
  return candidate !== undefined && Object.prototype.hasOwnProperty.call(LEVELS, candidate);
}

/** Shape of a plan element read by `root.ts` (dataset.trace attribute). */
interface TraceablePlanElement {
  readonly dataset?: { readonly trace?: string };
}

/** Shape of a parsed plan carrying optional tracing config. */
interface TraceablePlan {
  readonly traceLevel?: string;
  readonly traceparent?: string;
}

/**
 * Resolve tracing configuration from the full set of discovered initial
 * plans and their DOM elements.
 *
 * Multi-plan pages (see `composeInitialPlans`) have more than one
 * `[data-reactive-plan]` script on the page, and each can independently
 * carry tracing config. Because `configure()` is a process-level singleton,
 * we must pick one `{level, traceparent}` pair. Resolution rules:
 *
 * - **level**: the MOST VERBOSE level across all plans. Per-plan, the
 *   existing dataset-over-plan precedence is preserved via `resolveLevel`;
 *   across plans, `trace > debug > info > warn > error > off` wins. If
 *   any plan asks for tracing, we enable it for the whole page so no
 *   plan loses its diagnostics.
 *
 * - **traceparent**: the FIRST VALID `plan.traceparent` found while
 *   iterating plans in document order. "Valid" means the string parses
 *   cleanly via `parseTraceparent` (version 00, lowercase hex, non-zero
 *   trace-id / span-id). Malformed or stale leading plans do NOT poison
 *   selection: the walk continues past them and returns the first
 *   well-formed traceparent it finds. Each rejected candidate is
 *   reported via `invalidTraceparents` so the caller can surface a
 *   structured warning instead of silently losing correlation.
 *
 * The caller (`root.ts`) feeds the result into `configure()` and logs
 * any rejected candidates via the root tracer.
 */
export interface InitialTracingConfig {
  readonly level: Level;
  readonly traceparent: string | undefined;
  readonly invalidTraceparents: readonly { index: number; value: string }[];
}

/**
 * Running state for incremental tracing promotion during the plan
 * parse loop. Each successfully-parsed plan contributes its resolved
 * level and (optionally) its traceparent to this accumulator, and
 * `root.ts` calls `configure()` with the new state between plans so
 * a subsequent `plan.parse.fail` event on a malformed later plan
 * emits at the verbosity the user asked for via `plan.traceLevel`.
 */
export interface IncrementalTracingState {
  readonly level: Level;
  readonly traceparent: string | undefined;
}

/**
 * Fold one more (planEl, plan) pair into the incremental tracing state.
 * Level promotion is upward-only (most verbose wins); traceparent is
 * "first valid wins" and skips malformed candidates with a rejection
 * report so the caller can emit a structured warning.
 *
 * Pure function — no side effects, no module state mutation. Called
 * once per plan from `root.ts` during the parse loop.
 */
export function promoteTracingConfig(
  state: IncrementalTracingState,
  planEl: TraceablePlanElement,
  plan: TraceablePlan,
  planIndex: number,
): {
  readonly state: IncrementalTracingState;
  readonly rejectedTraceparent: { index: number; value: string } | undefined;
} {
  const perPlan = resolveLevel(plan.traceLevel, planEl?.dataset?.trace, "off");
  const nextLevel: Level =
    LEVELS[perPlan] > LEVELS[state.level] ? perPlan : state.level;

  let nextTraceparent = state.traceparent;
  let rejectedTraceparent: { index: number; value: string } | undefined;
  if (!nextTraceparent && plan.traceparent) {
    const parsed = parseTraceparent(plan.traceparent);
    if (parsed) {
      nextTraceparent = plan.traceparent;
    } else {
      rejectedTraceparent = { index: planIndex, value: plan.traceparent };
    }
  }

  return {
    state: { level: nextLevel, traceparent: nextTraceparent },
    rejectedTraceparent,
  };
}

export function resolveInitialTracingConfig(
  planEls: readonly TraceablePlanElement[],
  plans: readonly TraceablePlan[],
): InitialTracingConfig {
  let level: Level = "off";
  let traceparent: string | undefined;
  const invalidTraceparents: { index: number; value: string }[] = [];

  const limit = Math.min(planEls.length, plans.length);
  for (let i = 0; i < limit; i++) {
    const perPlan = resolveLevel(plans[i].traceLevel, planEls[i]?.dataset?.trace, "off");
    if (LEVELS[perPlan] > LEVELS[level]) {
      level = perPlan;
    }
    if (!traceparent && plans[i].traceparent) {
      const raw = plans[i].traceparent as string;
      const parsed = parseTraceparent(raw);
      if (parsed) {
        traceparent = raw;
      } else {
        // Rejected candidate: keep scanning for a valid traceparent from
        // later plans instead of silently suppressing their correlation.
        invalidTraceparents.push({ index: i, value: raw });
      }
    }
  }

  return { level, traceparent, invalidTraceparents };
}

/** Generate a 32-hex-digit trace-id per W3C. Uses `crypto.getRandomValues`. */
export function generateTraceId(): string {
  return randomHex(16);
}

/** Generate a 16-hex-digit span-id per W3C. Uses `crypto.getRandomValues`. */
export function generateSpanId(): string {
  return randomHex(8);
}

function randomHex(byteLength: number): string {
  const bytes = new Uint8Array(byteLength);
  crypto.getRandomValues(bytes);
  let out = "";
  for (let i = 0; i < bytes.length; i++) {
    out += bytes[i].toString(16).padStart(2, "0");
  }
  return out;
}
