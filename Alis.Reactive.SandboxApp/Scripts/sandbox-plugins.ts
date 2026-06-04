// Sandbox plugins enqueue before runtime boot so root.ts can register them
// before plan execution starts.

interface PendingHostPlugin {
  readonly name: string;
  readonly instance: Record<string, unknown> | ((...args: unknown[]) => unknown);
}

interface PluginQueueWindow extends Window {
  __alisPlugins?: PendingHostPlugin[];
}

type LengthBearingValue = ArrayLike<unknown> | string | undefined;
type PluginRecord = Record<string, unknown>;
type PluginRecordList = readonly PluginRecord[] | undefined;

const pluginHost = window as PluginQueueWindow;
const registeredPlugins = pluginHost.__alisPlugins ??= [];

const readField = (item: PluginRecord, key: string): unknown => item[key];

const fieldMatches = (item: PluginRecord, key: string, expected: unknown): boolean =>
  readField(item, key) === expected;

registeredPlugins.push({
  name: "array",
  instance: {
    count:  (items: LengthBearingValue) => items?.length ?? 0,
    pluck:  (items: PluginRecordList, index: number, key: string) => items?.[index]?.[key],
    filter: (items: PluginRecordList, key: string, expected: unknown) =>
      items?.filter(item => fieldMatches(item, key, expected)) ?? [],
    sum:    (items: PluginRecordList, key: string) =>
      items?.reduce((total, item) => total + (Number(readField(item, key)) || 0), 0) ?? 0,
    some:   (items: PluginRecordList, key: string, expected: unknown) =>
      items?.some(item => fieldMatches(item, key, expected)) ?? false,
  }
});

registeredPlugins.push({
  name: "analytics",
  instance: {
    track: (_event: string) => { /* sandbox no-op */ },
  }
});

registeredPlugins.push({
  name: "slugify",
  instance: (value: unknown) =>
    String(value ?? "")
      .trim()
      .toLowerCase()
      .replace(/\s+/g, "-"),
});

const orderMarks = new Map<string, string[]>();

registeredPlugins.push({
  name: "order",
  instance: (key: unknown, mark: unknown) => {
    const sequenceKey = String(key ?? "");
    const marks = orderMarks.get(sequenceKey) ?? [];
    marks.push(String(mark ?? ""));
    orderMarks.set(sequenceKey, marks);
    return marks.join(">");
  },
});
