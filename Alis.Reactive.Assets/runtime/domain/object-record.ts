export class ObjectRecord {
  private constructor(private readonly record: Record<string, unknown>) {}

  static tryFrom(value: unknown): ObjectRecord | undefined {
    const valueCanExposeProperties = typeof value === "object" && value !== null;
    if (!valueCanExposeProperties) return undefined;

    return new ObjectRecord(value as Record<string, unknown>);
  }

  get raw(): Record<string, unknown> {
    return this.record;
  }

  entries(): [string, unknown][] {
    return Object.entries(this.record);
  }

  get(name: string): unknown {
    return this.record[name];
  }
}

export class PlainObjectRecord {
  private constructor(private readonly record: ObjectRecord) {}

  static tryFrom(value: unknown): PlainObjectRecord | undefined {
    const record = ObjectRecord.tryFrom(value);
    if (record === undefined) return undefined;
    if (Array.isArray(value)) return undefined;

    return new PlainObjectRecord(record);
  }

  get raw(): Record<string, unknown> {
    return this.record.raw;
  }

  entries(): [string, unknown][] {
    return this.record.entries();
  }

  get(name: string): unknown {
    return this.record.get(name);
  }
}
