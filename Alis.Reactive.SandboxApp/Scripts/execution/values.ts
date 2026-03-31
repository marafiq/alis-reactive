import type { CommandValue, DispatchPayload, ExecContext } from "../types";
import { coerceOrThrow } from "../core/coerce";
import { resolveSource } from "../resolution/resolver";
import { assertNever } from "../core/assert-never";

/** Resolves a command-carried value into the raw JS value consumed at execution time. */
export function resolveCommandValue(value: CommandValue, ctx?: ExecContext): unknown {
  switch (value.kind) {
    case "literal":
      return value.coerce ? coerceOrThrow(value.value, value.coerce) : value.value;
    case "source": {
      const raw = resolveSource(value.source, ctx);
      return value.coerce ? coerceOrThrow(raw, value.coerce) : raw;
    }
    default:
      assertNever(value, "command value kind");
  }
}

/** Resolves a dispatch payload field map into the final CustomEvent.detail object. */
export function resolveDispatchPayload(payload?: DispatchPayload, ctx?: ExecContext): Record<string, unknown> {
  if (!payload) return {};

  const detail: Record<string, unknown> = {};
  for (const [name, value] of Object.entries(payload)) {
    detail[name] = resolveCommandValue(value, ctx);
  }

  return detail;
}
