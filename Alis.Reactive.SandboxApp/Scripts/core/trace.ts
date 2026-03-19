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
  const line = data !== undefined ? `${tag} ${msg} ${JSON.stringify(data)}` : `${tag} ${msg}`;
  if (level <= LEVELS.error) console.error(line);
  else if (level <= LEVELS.warn) console.warn(line);
  else console.log(line);
}
