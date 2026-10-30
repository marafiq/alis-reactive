// The runtime's console log. Errors and warnings always show; the page raises the level with
// data-alis-trace on <html> (info, debug or trace), the one knob, read once at boot.

export type TraceLevel = "off" | "error" | "warn" | "info" | "debug" | "trace";

const LEVELS: Record<TraceLevel, number> = {
  off: 0, error: 1, warn: 2, info: 3, debug: 4, trace: 5,
};

let active = LEVELS.warn;

export function setLevel(level: TraceLevel): void {
  active = LEVELS[level];
}

/** The level the page asks for, from `data-alis-trace` on its root element; the default when it names none. */
export function levelOf(root: HTMLElement): TraceLevel {
  const named = root.dataset["alisTrace"];
  if (named === undefined) return "warn";
  if (!(named in LEVELS)) throw new Error(`[alis] data-alis-trace="${named}" names no level; one of ${Object.keys(LEVELS).join(", ")}`);
  return named as TraceLevel;
}

export interface Logger {
  error(msg: string, details?: unknown): void;
  warn(msg: string, details?: unknown): void;
  info(msg: string, details?: unknown): void;
  debug(msg: string, details?: unknown): void;
  trace(msg: string, details?: unknown): void;
}

export function logger(name: string): Logger {
  const tag = `[alis:${name}]`;
  return {
    error: (msg, details) => emit(LEVELS.error, tag, msg, details),
    warn: (msg, details) => emit(LEVELS.warn, tag, msg, details),
    info: (msg, details) => emit(LEVELS.info, tag, msg, details),
    debug: (msg, details) => emit(LEVELS.debug, tag, msg, details),
    trace: (msg, details) => emit(LEVELS.trace, tag, msg, details),
  };
}

function emit(level: number, tag: string, msg: string, details?: unknown): void {
  if (level > active) return;
  const consoleMethod = level <= LEVELS.error ? console.error
                      : level <= LEVELS.warn  ? console.warn
                      : level <= LEVELS.info  ? console.info
                      : console.log;
  // Emit searchable JSON text and the live object DevTools can expand.
  if (details !== undefined) {
    consoleMethod(`${tag} ${msg} ${safeStringify(details)}`, details);
    return;
  }

  consoleMethod(`${tag} ${msg}`);
}

function safeStringify(details: unknown): string {
  try {
    return JSON.stringify(details);
  } catch {
    // Circular references and BigInt values still get a text marker.
    return "[unserializable]";
  }
}
