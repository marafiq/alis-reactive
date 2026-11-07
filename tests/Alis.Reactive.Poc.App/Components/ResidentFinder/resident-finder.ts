export interface ResidentFinderHost extends HTMLInputElement {
  finder?: ResidentFinder;
}

export interface Wing {
  readonly name: string;
  readonly floor: number;
}

export interface ResidentMatch {
  readonly name: string;
  readonly room: string;
  readonly wing: Wing | null;
}

const register: ReadonlyMap<string, ResidentMatch> = new Map([
  ["Helen Park", { name: "Helen Park", room: "12", wing: { name: "Rose", floor: 2 } }],
  ["Sam Park", { name: "Sam Park", room: "4", wing: null }],
]);

export class Lookup {
  readonly term: string;
  readonly #match: Promise<ResidentMatch | null>;

  constructor(term: string, match: Promise<ResidentMatch | null>) {
    this.term = term;
    this.#match = match;
  }

  match(): Promise<ResidentMatch | null> {
    return this.#match;
  }

  count(): Promise<number> {
    return this.#match.then((found) => (found === null ? 0 : 1));
  }
}

type LookedListener = (lookup: Lookup) => void;

type Pending = { readonly kind: "idle" } | { readonly kind: "waiting"; readonly answer: () => void; readonly fail: () => void };

const idle: Pending = { kind: "idle" };

export class ResidentFinder {
  readonly #listeners = new Set<LookedListener>();
  readonly #status = lineOf("finder-status", "Lookup: idle");
  #pending: Pending = idle;

  constructor(host: ResidentFinderHost) {
    const answer = buttonOf("Answer the lookup", () => this.#settle((pending) => pending.answer(), "answered"));
    const fail = buttonOf("Fail the lookup", () => this.#settle((pending) => pending.fail(), "failed"));
    host.after(this.#status, answer, fail);
    host.addEventListener("change", () => this.#look(host.value));
    host.finder = this;
  }

  on(event: "looked", listener: LookedListener): void {
    if (event !== "looked") throw new Error("A resident finder raises 'looked' only, not '" + event + "'.");
    this.#listeners.add(listener);
  }

  off(event: "looked", listener: LookedListener): void {
    if (event !== "looked") throw new Error("A resident finder raises 'looked' only, not '" + event + "'.");
    this.#listeners.delete(listener);
  }

  #look(term: string): void {
    const match = new Promise<ResidentMatch | null>((resolve, reject) => {
      this.#pending = {
        kind: "waiting",
        answer: () => resolve(register.get(term) ?? null),
        fail: () => reject(new Error("the register is offline")),
      };
    });
    this.#status.textContent = "Lookup: waiting for " + term;
    const lookup = new Lookup(term, match);
    for (const listener of [...this.#listeners]) listener(lookup);
  }

  #settle(settle: (pending: Extract<Pending, { kind: "waiting" }>) => void, outcome: string): void {
    const pending = this.#pending;
    if (pending.kind === "idle") return;
    this.#pending = idle;
    this.#status.textContent = "Lookup: " + outcome;
    settle(pending);
  }
}

function lineOf(className: string, text: string): HTMLParagraphElement {
  const line = document.createElement("p");
  line.className = className;
  line.textContent = text;
  return line;
}

function buttonOf(text: string, click: () => void): HTMLButtonElement {
  const button = document.createElement("button");
  button.type = "button";
  button.textContent = text;
  button.addEventListener("click", click);
  return button;
}
