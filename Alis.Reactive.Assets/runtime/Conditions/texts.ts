import { described, nothing, Stop, value, type Read } from "../ValuesAndSources/index";

export function textTested(text: Read, part: Read, holds: (held: string, sought: string) => boolean): Read {
  if (text.kind === "nothing" || text.value === null || part.kind === "nothing") return nothing;
  if (typeof text.value !== "string") throw noText(text.value);
  if (typeof part.value !== "string") throw new Stop("The value " + described(part.value) + " is looked for in a text, and it is no text.");
  return value(holds(text.value, part.value));
}

export function nullOrEmpty(text: Read): Read {
  if (text.kind === "nothing" || text.value === null) return value(true);
  if (typeof text.value !== "string") throw noText(text.value);
  return value(text.value === "");
}

export function matched(text: Read, pattern: Read): Read {
  if (text.kind === "nothing" || text.value === null) return nothing;
  if (typeof text.value !== "string") throw noText(text.value);
  if (pattern.kind === "nothing" || typeof pattern.value !== "string") throw new Error("The plan's matches carries no pattern text.");
  return value(new RegExp(pattern.value).test(text.value));
}

function noText(held: unknown): Stop {
  return new Stop("The value " + described(held) + " is tested as text, and it is no text.");
}
