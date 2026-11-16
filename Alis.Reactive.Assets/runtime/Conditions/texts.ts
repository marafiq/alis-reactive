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

export function textChecked(text: Read, holds: (held: string) => boolean): Read {
  if (text.kind === "nothing" || text.value === null) return nothing;
  if (typeof text.value !== "string") throw noText(text.value);
  return value(holds(text.value));
}

export function isEmailAddress(text: string): boolean {
  const at = text.indexOf("@");
  return at > 0 && at < text.length - 1 && at === text.lastIndexOf("@");
}

export function holdsLuhnSum(text: string): boolean {
  let sum = 0;
  let doubled = false;
  for (let at = text.length - 1; at >= 0; at -= 1) {
    const unit = text.charCodeAt(at);
    if (unit === 0x20 || unit === 0x2d) continue;
    if (unit < 0x30 || unit > 0x39) return false;
    const weighted = (unit - 0x30) * (doubled ? 2 : 1);
    sum += Math.floor(weighted / 10) + (weighted % 10);
    doubled = !doubled;
  }
  return sum % 10 === 0;
}

export function isWebAddress(text: string): boolean {
  if (!URL.canParse(text)) return false;
  const scheme = new URL(text).protocol;
  return scheme === "http:" || scheme === "https:";
}

function noText(held: unknown): Stop {
  return new Stop("The value " + described(held) + " is tested as text, and it is no text.");
}
