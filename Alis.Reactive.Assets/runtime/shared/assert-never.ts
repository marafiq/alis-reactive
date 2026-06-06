// Exhaustiveness check for discriminated union switches. Compile-time error if a case is missing.
export function assertNever(value: never, context: string): never {
  const unhandled = value as unknown;
  const description = hasKind(unhandled) ? `${unhandled.kind}` : `${unhandled}`;
  throw new Error(`[alis] Unhandled ${context}: ${description}`);
}

function hasKind(value: unknown): value is { readonly kind: unknown } {
  return typeof value === "object" && value !== null && "kind" in value;
}
