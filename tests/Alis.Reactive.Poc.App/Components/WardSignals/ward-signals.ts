export interface CallBellHost extends HTMLElement {
  bell?: CallBell;
}

export interface WardFeedHost extends HTMLElement {
  feed?: WardFeed;
}

type Listener = (...args: unknown[]) => void;

export class CallBell {
  onring: Listener | null = null;

  constructor(host: HTMLElement) {
    host.replaceChildren(buttonOf("Ring the bell", () => this.onring?.({ room: "12" })));
  }
}

export class WardFeed {
  readonly #listeners = new Map<string, Set<Listener>>();

  constructor(host: HTMLElement) {
    host.replaceChildren(buttonOf("Post an update", () => this.#post("update", { message: "Bed 3 is free" })));
  }

  subscribe(event: string, listener: Listener): () => void {
    const listening = this.#listeners.get(event) ?? new Set<Listener>();
    listening.add(listener);
    this.#listeners.set(event, listening);
    return () => listening.delete(listener);
  }

  #post(event: string, update: object): void {
    for (const listener of this.#listeners.get(event) ?? []) listener(update);
  }
}

export interface MedicineSearchHost extends HTMLElement {
  search?: MedicineSearch;
}

export class MedicineSearch {
  readonly #listeners = new Map<string, Set<Listener>>();
  readonly #results: HTMLParagraphElement;

  constructor(host: HTMLElement) {
    this.#results = document.createElement("p");
    this.#results.className = "search-results";
    this.#results.textContent = "no results";
    host.replaceChildren(this.#results, buttonOf("Search for In", () => this.#load("In")));
  }

  on(event: string, listener: Listener): void {
    const listening = this.#listeners.get(event) ?? new Set<Listener>();
    listening.add(listener);
    this.#listeners.set(event, listening);
  }

  off(event: string, listener: Listener): void {
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
