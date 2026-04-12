import type { Level } from "./types";

const VALID_LEVELS: ReadonlySet<string> = new Set(["off", "error", "warn", "info", "debug", "trace"]);

export function isValidLevel(s: string): s is Level {
  return VALID_LEVELS.has(s);
}

export function parseTraceparent(header: string): { traceId: string; spanId: string; flags: string } | undefined {
  const parts = header.split("-");
  if (parts.length !== 4 || parts[0] !== "00") return undefined;
  if (parts[1].length !== 32 || parts[2].length !== 16 || parts[3].length !== 2) return undefined;
  return { traceId: parts[1], spanId: parts[2], flags: parts[3] };
}

export function formatTraceparent(traceId: string, spanId: string, flags: string = "01"): string {
  return `00-${traceId}-${spanId}-${flags}`;
}

export function resolveLevel(
  planTraceLevel: string | undefined,
  elDataTrace: string | undefined,
): Level {
  const sources = [
    planTraceLevel,
    elDataTrace,
    typeof location !== "undefined" ? new URLSearchParams(location.search).get("trace") : null,
    typeof localStorage !== "undefined" ? localStorage.getItem("alis.trace") : null,
  ];
  for (const s of sources) {
    if (s && isValidLevel(s)) return s;
  }
  return "off";
}
