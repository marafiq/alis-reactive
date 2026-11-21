import { nothing, Stop, value, type Read } from "./read";

const whiteSpace: ReadonlySet<string> = new Set([
  "\u0009", "\u000A", "\u000B", "\u000C", "\u000D", "\u0020", "\u0085", "\u00A0", "\u1680",
  "\u2000", "\u2001", "\u2002", "\u2003", "\u2004", "\u2005", "\u2006", "\u2007", "\u2008", "\u2009", "\u200A",
  "\u2028", "\u2029", "\u202F", "\u205F", "\u3000",
]);

const upperWhereJavaScriptWritesTwo: ReadonlyMap<string, string> = new Map([
  ["\u1F80", "\u1F88"], ["\u1F81", "\u1F89"], ["\u1F82", "\u1F8A"], ["\u1F83", "\u1F8B"],
  ["\u1F84", "\u1F8C"], ["\u1F85", "\u1F8D"], ["\u1F86", "\u1F8E"], ["\u1F87", "\u1F8F"],
  ["\u1F90", "\u1F98"], ["\u1F91", "\u1F99"], ["\u1F92", "\u1F9A"], ["\u1F93", "\u1F9B"],
  ["\u1F94", "\u1F9C"], ["\u1F95", "\u1F9D"], ["\u1F96", "\u1F9E"], ["\u1F97", "\u1F9F"],
  ["\u1FA0", "\u1FA8"], ["\u1FA1", "\u1FA9"], ["\u1FA2", "\u1FAA"], ["\u1FA3", "\u1FAB"],
  ["\u1FA4", "\u1FAC"], ["\u1FA5", "\u1FAD"], ["\u1FA6", "\u1FAE"], ["\u1FA7", "\u1FAF"],
  ["\u1FB3", "\u1FBC"], ["\u1FC3", "\u1FCC"], ["\u1FF3", "\u1FFC"],
]);

const dotlessI = "\u0131";

const dottedI = "\u0130";

export type Casing = "invariant" | "linguistic" | "turkic";

const upperCasing: Readonly<Record<Casing, ReadonlyMap<string, string>>> = {
  invariant: new Map([[dotlessI, dotlessI]]),
  linguistic: new Map([[dotlessI, "I"]]),
  turkic: new Map([["i", dottedI], [dotlessI, "I"]]),
};

const lowerCasing: Readonly<Record<Casing, ReadonlyMap<string, string>>> = {
  invariant: new Map(),
  linguistic: new Map([[dottedI, "i"]]),
  turkic: new Map([["I", dotlessI], [dottedI, "i"]]),
};

export type Trimmed = "both" | "start" | "end";

export function trimmed(term: Read, sides: Trimmed): Read {
  const held = text(term);
  if (held === undefined) return nothing;
  let start = 0;
  let end = held.length;
  if (sides !== "end") while (start < end && whiteSpace.has(held.charAt(start))) start++;
  if (sides !== "start") while (end > start && whiteSpace.has(held.charAt(end - 1))) end--;
  return value(held.slice(start, end));
}

export function upper(term: Read, casing: Casing): Read {
  const held = text(term);
  if (held === undefined) return nothing;
  const rule = upperCasing[casing];
  return value(Array.from(held, (point) => rule.get(point) ?? single(point.toUpperCase(), upperWhereJavaScriptWritesTwo.get(point) ?? point)).join(""));
}

export function lower(term: Read, casing: Casing): Read {
  const held = text(term);
  if (held === undefined) return nothing;
  const rule = lowerCasing[casing];
  return value(Array.from(held, (point) => rule.get(point) ?? single(point.toLowerCase(), point)).join(""));
}

export function tested(term: Read, part: Read, test: (held: string, sought: string) => boolean): Read {
  const held = text(term);
  if (held === undefined || part.kind === "nothing") return nothing;
  if (typeof part.value !== "string") throw new Stop("A text test was given " + (part.value === null ? "null" : "a value that is not text") + " to look for.");
  return value(test(held, part.value));
}

const patterns = new Map<string, RegExp>();

export function matched(term: Read, pattern: Read): Read {
  return tested(term, pattern, (held, source) => patternOf(source).test(held));
}

function patternOf(source: string): RegExp {
  const known = patterns.get(source);
  if (known !== undefined) return known;
  const pattern = new RegExp(source);
  patterns.set(source, pattern);
  return pattern;
}

export function nullOrEmpty(term: Read): Read {
  const held = text(term);
  return value(held === undefined || held.length === 0);
}

function text(term: Read): string | undefined {
  if (term.kind === "nothing" || term.value === null) return undefined;
  if (typeof term.value !== "string") throw new Stop("A text member was read on a value that is not text.");
  return term.value;
}

function single(mapped: string, otherwise: string): string {
  return Array.from(mapped).length === 1 ? mapped : otherwise;
}
