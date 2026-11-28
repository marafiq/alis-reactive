import { Stop } from "./read";

export interface PageAddress {
  readonly pathname: string;
  readonly search: string;
}

const apart = "/";
const placeholderOpen = "{";

export function queried(address: PageAddress, key: string): string | null {
  for (const [name, text] of new URLSearchParams(address.search)) if (equalIgnoringCase(name, key)) return text;
  return null;
}

export function routed(address: PageAddress, template: readonly string[], place: number): string {
  const segments = segmentsOf(address.pathname);
  const matches = segments.length === template.length && template.every((part, at) => fits(segments[at], part));
  const held = segments[place];
  if (!matches || held === undefined) {
    throw new Stop("The page's path '" + address.pathname + "' does not match its template '" + apart + template.join(apart) + "'.");
  }
  return held;
}

function fits(segment: string | undefined, part: string): boolean {
  return segment !== undefined && (part.startsWith(placeholderOpen) || equalIgnoringCase(segment, part));
}

function segmentsOf(pathname: string): string[] {
  const parts = pathname.split(apart).slice(1);
  if (parts[parts.length - 1] === "") parts.pop();
  return parts.map((part) => decoded(part, pathname));
}

function decoded(part: string, pathname: string): string {
  try {
    return decodeURIComponent(part);
  } catch {
    throw new Stop("The page's path '" + pathname + "' holds '" + part + "', which is no percent-encoded text.");
  }
}

function equalIgnoringCase(one: string, other: string): boolean {
  if (one.length !== other.length) return false;
  for (let at = 0; at < one.length; at++) if (upper(one.charAt(at)) !== upper(other.charAt(at))) return false;
  return true;
}

function upper(unit: string): string {
  const raised = unit.toUpperCase();
  return raised.length === 1 ? raised : unit;
}
