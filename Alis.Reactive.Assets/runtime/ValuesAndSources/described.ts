export function described(held: unknown): string {
  if (held instanceof Date) return Number.isNaN(held.getTime()) ? "a Date of no time" : "the Date " + held.toISOString();
  if (typeof held === "string") return JSON.stringify(held);
  if (typeof held === "number" || typeof held === "boolean") return String(held);
  if (held === null) return "null";
  return typeof held === "object" ? "an object" : "a " + typeof held;
}
