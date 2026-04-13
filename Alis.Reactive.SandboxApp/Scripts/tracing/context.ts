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
