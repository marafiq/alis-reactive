export interface CallBellHost extends HTMLElement {
  bell?: CallBell;
}

export interface WardFeedHost extends HTMLElement {
  feed?: WardFeed;
}

interface BellRing {
  readonly room: string;
}

interface FeedUpdate {
  readonly message: string;
}

type UpdateListener = (update: FeedUpdate) => void;

type LoadListener = (query: string, answer: (items: readonly string[]) => void) => void;

declare global {
  interface HTMLElementEventMap {
    readonly "bed-assigned": CustomEvent<{ readonly bed: string }>;
  }
}

export class CallBell {
  onring: ((ring: BellRing) => void) | null = null;

  constructor(host: HTMLElement) {
    host.replaceChildren(buttonOf("Ring the bell", () => this.onring?.({ room: "12" })));
  }
}

export class WardFeed {
  readonly #listeners = new Map<string, Set<UpdateListener>>();

  constructor(host: HTMLElement) {
    host.replaceChildren(buttonOf("Post an update", () => this.#post("update", { message: "Bed 3 is free" })));
  }

  subscribe(event: "update", listener: UpdateListener): () => void {
    const listening = this.#listeners.get(event) ?? new Set<UpdateListener>();
    listening.add(listener);
    this.#listeners.set(event, listening);
    return () => listening.delete(listener);
  }

  #post(event: string, update: FeedUpdate): void {
    for (const listener of this.#listeners.get(event) ?? []) listener(update);
  }
}

export interface MedicineSearchHost extends HTMLElement {
  search?: MedicineSearch;
}

export class MedicineSearch {
  readonly #listeners = new Map<string, Set<LoadListener>>();
  readonly #results: HTMLParagraphElement;

  constructor(host: HTMLElement) {
    this.#results = document.createElement("p");
    this.#results.className = "search-results";
    this.#results.textContent = "no results";
    host.replaceChildren(this.#results, buttonOf("Search for In", () => this.#load("In")));
  }

  on(event: "load", listener: LoadListener): void {
    const listening = this.#listeners.get(event) ?? new Set<LoadListener>();
    listening.add(listener);
    this.#listeners.set(event, listening);
  }

  off(event: "load", listener: LoadListener): void {
    this.#listeners.get(event)?.delete(listener);
  }

  #load(query: string): void {
    const answer = (items: readonly string[]): void => {
      this.#results.textContent = items.join(", ");
    };
    for (const listener of this.#listeners.get("load") ?? []) listener(query, answer);
  }
}

export function mountBedBoard(host: HTMLElement): void {
  host.replaceChildren(
    buttonOf("Assign bed 7", () => host.dispatchEvent(new CustomEvent("bed-assigned", { detail: { bed: "7" } }))),
    buttonOf("Assign no bed", () => host.dispatchEvent(new CustomEvent("bed-assigned"))),
  );
}

function buttonOf(text: string, clicked: () => void): HTMLButtonElement {
  const button = document.createElement("button");
  button.type = "button";
  button.textContent = text;
  button.addEventListener("click", clicked);
  return button;
}
