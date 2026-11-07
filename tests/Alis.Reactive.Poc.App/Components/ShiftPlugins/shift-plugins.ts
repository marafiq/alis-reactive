interface Noted {
  readonly note: string;
  readonly count: number;
}

export class ShiftLog extends EventTarget {
  readonly #notes: string[] = [];
  limit = 3;

  get count(): number {
    return this.#notes.length;
  }

  get lastNote(): string {
    return this.#notes.at(-1) ?? "";
  }

  add(note: string): void {
    this.#notes.push(note);
    this.dispatchEvent(new CustomEvent<Noted>("noted", { detail: { note, count: this.#notes.length } }));
  }

  isFull(): boolean {
    return this.#notes.length >= this.limit;
  }
}

export function initials(name: string): string {
  return name
    .split(" ")
    .filter((word) => word.length > 0)
    .map((word) => word.charAt(0).toUpperCase())
    .join("");
}

interface Struck {
  readonly hour: string;
}

export class NightClock {
  onstruck: ((struck: Struck) => void) | null = null;
  #hour = 22;

  strike(): void {
    this.#hour = (this.#hour + 1) % 24;
    this.onstruck?.({ hour: String(this.#hour).padStart(2, "0") + ":00" });
  }
}

interface Taken {
  readonly bed: string;
}

type BedListener = (taken: Taken) => void;

export class BedStore {
  readonly #listeners = new Set<BedListener>();

  subscribe(event: string, listener: BedListener): () => void {
    if (event !== "assigned") throw new Error("The bed store raises 'assigned' only, not '" + event + "'.");
    this.#listeners.add(listener);
    return () => this.#listeners.delete(listener);
  }

  assign(bed: string): void {
    for (const listener of this.#listeners) listener({ bed });
  }
}

export class VisitorBook {
  lastVisitor = "";

  sign(visitor: string): void {
    this.lastVisitor = visitor;
  }
}
