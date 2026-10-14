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
    track: (_event: string) => undefined,
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

// The community's fixed facts: how many beds each ward holds.
registeredPlugins.push({
  name: "community",
  instance: {
    bedsPerWard: 5,
  },
});

const twoDigits = (value: number): string => String(value).padStart(2, "0");

// A visitor pass's lines. Each argument is shown only when it arrives as its declared kind (a boolean,
// a number, a Date), so a literal sent in the wrong shape reads "?".
registeredPlugins.push({
  name: "visitorPass",
  instance: {
    heading: (escorted: unknown, passNumber: unknown, day: unknown) => {
      const who = typeof escorted === "boolean" ? (escorted ? "Escorted visitor" : "Visitor") : "?";
      const pass = typeof passNumber === "number" ? String(passNumber) : "?";
      const date = day instanceof Date
        ? `${day.getFullYear()}-${twoDigits(day.getMonth() + 1)}-${twoDigits(day.getDate())}`
        : "?";
      return `${who} · pass ${pass} · ${date}`;
    },
    charges: (parking: unknown, hours: unknown) => {
      const fee = typeof parking === "number" ? parking.toFixed(2) : "?";
      const length = typeof hours === "number" ? String(hours) : "?";
      return `Parking $${fee} · ${length} hours`;
    },
  },
});

// The visit board's activity list: each line recorded is added to #board-activity, and entries
// says how many lines the list holds.
const visitLogLines: string[] = [];

registeredPlugins.push({
  name: "visitLog",
  instance: {
    record: (line: unknown) => {
      const text = String(line ?? "");
      visitLogLines.push(text);
      const list = document.getElementById("board-activity");
      if (list === null) return;
      const item = document.createElement("li");
      item.textContent = text;
      list.appendChild(item);
    },
    get entries(): number {
      return visitLogLines.length;
    },
  },
});

// The visit board's spoken status line: the plugin is itself the function, and returns nothing.
registeredPlugins.push({
  name: "announce",
  instance: (line: unknown) => {
    const region = document.getElementById("board-announcer");
    if (region !== null) region.textContent = String(line ?? "");
  },
});
