import type { FlagValue } from "../Schema/index";
import { flagRows, nothing, value, type Read } from "../ValuesAndSources/index";

export function flagNames(held: unknown, rows: readonly FlagValue[]): readonly string[] | undefined {
  return flagRows(held, rows)?.map(row => row.name);
}

export function inFlags(held: Read, rows: Read): Read {
  if (rows.kind === "nothing" || !Array.isArray(rows.value)) throw new Error("The plan's inFlags carries no rows.");
  if (held.kind === "nothing" || held.value === null) return nothing;
  return value(flagNames(held.value, rows.value as readonly FlagValue[]) !== undefined);
}
