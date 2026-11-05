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

registeredPlugins.push({
  name: "community",
  instance: {
    bedsPerWard: 5,
  },
});

const twoDigits = (value: number): string => String(value).padStart(2, "0");

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

registeredPlugins.push({
  name: "announce",
  instance: (line: unknown) => {
    const region = document.getElementById("board-announcer");
    if (region !== null) region.textContent = String(line ?? "");
  },
});

registeredPlugins.push({
  name: "initials",
  instance: (name: unknown) =>
    String(name ?? "")
      .split(/\s+/)
      .filter(part => part !== "")
      .map(part => part[0]!.toUpperCase())
      .join(""),
});

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

const ymd = (day: Date): string => `${day.getFullYear()}-${twoDigits(day.getMonth() + 1)}-${twoDigits(day.getDate())}`;
const text = (value: unknown): string => (typeof value === "string" ? value : "?");

registeredPlugins.push({
  name: "moveInLetter",
  instance: {
    opening: (applicant: unknown, wing: unknown, moveIn: unknown) =>
      `${text(applicant)} moves into the ${text(wing)} wing on ${moveIn instanceof Date ? ymd(moveIn) : "?"}.`,
    standing: (age: unknown, veteran: unknown, position: unknown) => {
      const service = veteran === true ? "a veteran" : veteran === false ? "a civilian" : "?";
      const number = typeof position === "number" ? `number ${position}` : position === null ? "no number yet" : "?";
      return `Aged ${typeof age === "number" ? age : "?"}, ${service}, ${number} on the waiting list.`;
    },
    contact: (phone: unknown, onFile: unknown, summary: unknown) => {
      const file = onFile !== null && typeof onFile === "object" ? (onFile as { relation?: unknown; phone?: unknown }) : undefined;
      return `Call ${text(phone)}; on file: ${file === undefined ? "?" : `${text(file.relation)} ${text(file.phone)}`}; ${text(summary)}.`;
    },
    allergies: (allergies: unknown, source: unknown) =>
      `Allergies: ${Array.isArray(allergies) ? allergies.map(String).join(", ") : "?"} (from ${text(source)}).`,
    notice: (applicant: unknown, relation: unknown) => `Notice for ${text(applicant)} (${text(relation)}).`,
  },
});
