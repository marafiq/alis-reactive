export function objectRecordFrom(value: unknown): Record<string, unknown> | undefined {
  const valueCanExposeProperties = typeof value === "object" && value !== null;
  if (!valueCanExposeProperties) return undefined;

  return value as Record<string, unknown>;
}

export function plainObjectRecordFrom(value: unknown): Record<string, unknown> | undefined {
  const record = objectRecordFrom(value);
  if (record === undefined) return undefined;
  if (Array.isArray(value)) return undefined;

  return record;
}
