import type { BindSource } from "../types/sources";
import type { CoercionType } from "../core/coerce";
import type { CommandValue, DispatchPayload } from "../types/commands";

export function literalValue(value: unknown, coerce?: CoercionType): CommandValue {
  return coerce == null
    ? { kind: "literal", value }
    : { kind: "literal", value, coerce };
}

export function sourceValue(source: BindSource, coerce?: CoercionType): CommandValue {
  return coerce == null
    ? { kind: "source", source }
    : { kind: "source", source, coerce };
}

export function dispatchPayload(fields: Record<string, unknown>): DispatchPayload {
  const payload: DispatchPayload = {};

  for (const [name, value] of Object.entries(fields)) {
    payload[name] = literalValue(value);
  }

  return payload;
}
