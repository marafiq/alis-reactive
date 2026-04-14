export type TraceLevel = "off" | "error" | "warn" | "info" | "debug" | "trace";

const LEVELS: Record<TraceLevel, number> = {
  off: 0, error: 1, warn: 2, info: 3, debug: 4, trace: 5,
};

let active = LEVELS.off;

export function setLevel(level: TraceLevel): void {
  active = LEVELS[level];
}

export interface Logger {
  error(msg: string, data?: unknown): void;
  warn(msg: string, data?: unknown): void;
  info(msg: string, data?: unknown): void;
  debug(msg: string, data?: unknown): void;
  trace(msg: string, data?: unknown): void;
}

export function scope(name: string): Logger {
  const tag = `[alis:${name}]`;
  return {
    error: (msg, data) => emit(LEVELS.error, tag, msg, data),
    warn: (msg, data) => emit(LEVELS.warn, tag, msg, data),
    info: (msg, data) => emit(LEVELS.info, tag, msg, data),
    debug: (msg, data) => emit(LEVELS.debug, tag, msg, data),
    trace: (msg, data) => emit(LEVELS.trace, tag, msg, data),
  };
}

function emit(level: number, tag: string, msg: string, data?: unknown): void {
  if (level > active) return;
  const out = level <= LEVELS.error ? console.error
            : level <= LEVELS.warn  ? console.warn
            : console.log;
  // Dual form: JSON embedded in the message (so console-text scrapers and
  // log aggregators can substring-match on payload keys) AND the live object
  // as a second arg (so DevTools renders it as an expandable tree).
  if (data !== undefined) out(`${tag} ${msg} ${safeStringify(data)}`, data);
  else                    out(`${tag} ${msg}`);
}

function safeStringify(data: unknown): string {
  try {
    return JSON.stringify(data);
  } catch {
    // Circular reference or BigInt — fall back to a marker so text scrapers
    // still see SOMETHING while DevTools handles the live object gracefully.
    return "[unserializable]";
  }
}
