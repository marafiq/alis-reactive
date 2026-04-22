/** Exhaustiveness check for discriminated union switches. Compile-time error if a case is missing. */
export function assertNever(value: never, context: string): never {
  throw new Error(`[alis] Unhandled ${context}: ${(value as any).kind ?? value}`);
}
