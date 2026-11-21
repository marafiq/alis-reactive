interface Answered {
  readonly by: string;
  logged: boolean;
}

type AnsweredListener = (answered: Answered) => void;

export class PagerCall {
  readonly #listeners = new Set<AnsweredListener>();
  readonly #logged: (by: string) => void;

  constructor(logged: (by: string) => void) {
    this.#logged = logged;
  }

  on(event: "answered", listener: AnsweredListener): void {
    if (event !== "answered") throw new Error("A pager call raises 'answered' only, not '" + event + "'.");
    this.#listeners.add(listener);
  }

  off(event: "answered", listener: AnsweredListener): void {
    if (event !== "answered") throw new Error("A pager call raises 'answered' only, not '" + event + "'.");
    this.#listeners.delete(listener);
  }

  acknowledge(): Promise<void> {
    return Promise.resolve();
  }

  answer(by: string): void {
    const answered: Answered = { by, logged: false };
    for (const listener of [...this.#listeners]) listener(answered);
    if (answered.logged) this.#logged(by);
  }
}

interface Paged {
  readonly room: string;
  handled: boolean;
  readonly call: PagerCall;
  readonly relief: PagerCall | null;
}

type PagedListener = (paged: Paged) => void;

export class WardPager {
  readonly #listeners = new Set<PagedListener>();
  readonly #calls = new Map<string, PagerCall>();
  lastHandled = "none";
  lastLogged = "none";

  on(event: "paged", listener: PagedListener): void {
    if (event !== "paged") throw new Error("The ward pager raises 'paged' only, not '" + event + "'.");
    this.#listeners.add(listener);
  }

  off(event: "paged", listener: PagedListener): void {
    if (event !== "paged") throw new Error("The ward pager raises 'paged' only, not '" + event + "'.");
    this.#listeners.delete(listener);
  }

  callFor(room: string): PagerCall {
    const call = this.#calls.get(room) ?? new PagerCall((by) => (this.lastLogged = by));
    this.#calls.set(room, call);
    return call;
  }

  page(room: string): void {
    const call = this.callFor(room);
    const paged: Paged = { room, handled: false, call, relief: null };
    for (const listener of [...this.#listeners]) listener(paged);
    if (paged.handled) this.lastHandled = room;
  }

  answer(room: string, by: string): void {
    const call = this.#calls.get(room);
    if (call === undefined) throw new Error("Room " + room + " was never paged.");
    call.answer(by);
  }
}
