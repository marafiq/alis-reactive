class ShiftLog extends EventTarget {
  override addEventListener(type: "signed", callback: ((event: Event) => void) | null, options?: boolean | AddEventListenerOptions): void;
  override addEventListener(type: string, callback: EventListenerOrEventListenerObject | null, options?: boolean | AddEventListenerOptions): void;
  override addEventListener(type: string, callback: EventListenerOrEventListenerObject | null, options?: boolean | AddEventListenerOptions): void {
    super.addEventListener(type, callback, options);
  }

  entries = 0;

  readonly #notes: string[] = [];

  add(note: string): void {
    this.#notes.push(note);
    this.entries += 1;
  }

  label(resident: string): string {
    return resident + ": " + String(this.entries);
  }
}

interface NightClock {
  onStruck: (() => void) | null;
}

interface BedFreed {
  readonly bed: string;
}

type FreedListener = (freed: BedFreed) => void;

const log = new ShiftLog();

const clock: NightClock = { onStruck: null };

const freedListeners = new Set<FreedListener>();

interface BedStore {
  subscribe(event: "freed", listener: FreedListener): () => void;
}

const bedStore: BedStore = {
  subscribe(event: string, listener: FreedListener): () => void {
    if (event !== "freed") throw new Error("The bed store raises no event '" + event + "'.");
    freedListeners.add(listener);
    return () => {
      freedListeners.delete(listener);
    };
  },
};

function initials(name: string): string {
  return name
    .split(" ")
    .filter((word) => word !== "")
    .map((word) => word.charAt(0))
    .join("");
}

function provide(name: string, instance: unknown): void {
  const queue: unknown = Reflect.get(window, "__alisPlugins");
  if (Array.isArray(queue)) {
    queue.push({ name, instance });
    return;
  }
  Reflect.set(window, "__alisPlugins", [{ name, instance }]);
}

function elementOf(id: string): HTMLElement {
  const element = document.getElementById(id);
  if (element === null) throw new Error("The page holds no element '" + id + "'.");
  return element;
}

function whenClicked(id: string, act: () => void): void {
  elementOf(id).addEventListener("click", act);
}

provide("shiftlog", log);
provide("nightclock", clock);
provide("bedstore", bedStore);
provide("initials", initials);

whenClicked("sign-the-log", () => {
  log.dispatchEvent(new Event("signed"));
});

whenClicked("strike-midnight", () => {
  clock.onStruck?.();
});

whenClicked("free-bed-4", () => {
  for (const listener of [...freedListeners]) listener({ bed: "4" });
});
