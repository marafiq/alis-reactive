export function toJavaScriptString(value: unknown): string {
  return globalThis.String(value);
}
