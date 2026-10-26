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

// A person's initials: the plugin is itself the function, and returns a value.
registeredPlugins.push({
  name: "initials",
  instance: (name: unknown) =>
    String(name ?? "")
      .split(/\s+/)
      .filter(part => part !== "")
      .map(part => part[0]!.toUpperCase())
      .join(""),
});

// The wing's staff pager: an object with every kind of member a plugin can have. The channel is read
// and written; the access code is written only and unlocks priority paging; send() returns a promise
// that settles once the page is delivered, and only then counts it; status is an object with members
// of its own; devices is a list of objects.
interface PagerDevice {
  readonly name: string;
  readonly battery: number;
}

const pagerDevices: PagerDevice[] = [
  { name: "Desk", battery: 80 },
  { name: "Charge nurse", battery: 55 },
  { name: "Night aide", battery: 95 },
];

const DELIVERY_MS = 150;
const PAGER_ACCESS_CODE = "4321";

const pager = {
  channel: "day",
  delivered: 0,
  priorityUnlocked: false,
  set accessCode(code: string) {
    this.priorityUnlocked = code === PAGER_ACCESS_CODE;
  },
  status: {
    online: true,
    queued: 0,
    label(): string {
      return `${this.online ? "online" : "offline"}, ${this.queued} queued`;
    },
  },
  devices: pagerDevices,
  estimate(count: unknown, urgent: unknown): number {
    const perDevice = urgent === true ? 2 : 5;
    return (typeof count === "number" ? count : 0) * perDevice;
  },
  send(message: unknown): Promise<void> {
    return new Promise(resolve => {
      setTimeout(() => {
        pager.delivered += 1;
        pager.lastMessage = String(message ?? "");
        resolve();
      }, DELIVERY_MS);
    });
  },
  sendWithReceipt(message: unknown): Promise<string> {
    return pager.send(message).then(() => `Delivered to ${pagerDevices.length} pagers: ${pager.lastMessage}`);
  },
  lastMessage: "",
};

registeredPlugins.push({ name: "pager", instance: pager });
