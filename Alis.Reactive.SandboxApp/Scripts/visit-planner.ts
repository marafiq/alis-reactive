import "./visit-planner.css";

export interface VisitSlot {
  readonly id: string;
  readonly label: string;
  readonly day: string;
  readonly quiet: boolean;
}

interface BookingNote {
  readonly visitors: number;
  readonly note: string;
}

type Listener = (payload: unknown) => void;

export interface VisitPlannerHost extends HTMLElement {
  visitPlanner?: VisitPlanner;
}

export class VisitPlanner {
  private readonly host: VisitPlannerHost;
  private readonly listeners = new Map<string, Set<Listener>>();
  private readonly notes = new Map<string, BookingNote>();
  private readonly waitlisted = new Set<string>();
  private readonly takenBy = new Map<string, string>();
  private booked: string[] = [];
  private offered: VisitSlot[] = [];
  private weeklyCapacity: number;
  private wingName = "";
  private locked = false;
  private emptyText = "No visit times yet.";
  private prepared = "";

  readonly labels: { empty: string };

  constructor(host: VisitPlannerHost) {
    this.host = host;
    this.weeklyCapacity = Number(host.dataset.capacity ?? "1");
    this.booked = (host.dataset.value ?? "").split(",").filter(id => id !== "");
    this.labels = Object.defineProperty({} as { empty: string }, "empty", {
      get: () => this.emptyText,
      set: (text: string) => {
        this.emptyText = text;
        this.render();
      },
      enumerable: true,
    });
    host.addEventListener("click", this.onClick);
    host.visitPlanner = this;
    this.render();
  }

  get value(): string[] {
    return [...this.booked];
  }

  set value(ids: string[]) {
    this.booked = this.inOfferedOrder(ids ?? []);
    this.render();
  }

  get capacity(): number {
    return this.weeklyCapacity;
  }

  set capacity(visits: number) {
    this.weeklyCapacity = visits;
    this.render();
  }

  get wing(): string {
    return this.wingName;
  }

  set wing(name: string) {
    this.wingName = name ?? "";
    this.render();
  }

  get readOnly(): boolean {
    return this.locked;
  }

  set readOnly(locked: boolean) {
    this.locked = locked;
    this.render();
  }

  get slots(): VisitSlot[] {
    return this.offered.map(slot => ({ ...slot }));
  }

  set slots(slots: VisitSlot[]) {
    this.offered = (slots ?? []).map(slot => ({ id: slot.id, label: slot.label, day: slot.day, quiet: Boolean(slot.quiet) }));
    this.booked = this.inOfferedOrder(this.booked);
    this.render();
    this.emit("slotsLoaded", { count: this.offered.length, wing: this.wingName });
  }

  get count(): number {
    return this.booked.length;
  }

  remaining(): number {
    return Math.max(this.weeklyCapacity - this.booked.length, 0);
  }

  isBooked(id: string | null): boolean {
    return id !== null && this.booked.includes(id);
  }

  firstOpen(): string {
    return this.offered.find(slot => !slot.quiet && !this.booked.includes(slot.id))?.id ?? "";
  }

  firstBooked(): string {
    return this.booked[0] ?? "";
  }

  select(id: string): void {
    const slot = this.offered.find(candidate => candidate.id === id);
    if (slot === undefined || this.booked.includes(id) || this.takenBy.has(id)) return;

    const request = { slot: { ...slot }, cancel: false };
    this.emit("beforeBook", request);
    if (request.cancel) return;

    if (this.booked.length >= this.weeklyCapacity) {
      const waitlist = (): void => {
        this.waitlisted.add(id);
        this.render();
      };
      this.emit("full", { capacity: this.weeklyCapacity, attempted: id, waitlist });
      return;
    }

    this.booked = this.inOfferedOrder([...this.booked, id]);
    this.waitlisted.delete(id);
    this.render();
    this.emitChange(id, null);
  }

  release(id: string): void {
    if (!this.booked.includes(id)) return;
    this.booked = this.booked.filter(booked => booked !== id);
    this.notes.delete(id);
    this.render();
    this.emitChange(null, id);
  }

  clear(): void {
    if (this.booked.length === 0) return;
    this.booked = [];
    this.notes.clear();
    this.waitlisted.clear();
    this.render();
    this.emitChange(null, null);
  }

  book(id: string, visitors: number, note: string): void {
    if (!this.booked.includes(id)) return;
    this.notes.set(id, { visitors, note: note ?? "" });
    this.render();
  }

  markTaken(id: string, family: string): void {
    if (!this.offered.some(slot => slot.id === id)) return;
    this.takenBy.set(id, family ?? "");
    this.render();
  }

  prepareSummary(): Promise<void> {
    return later(300).then(() => {
      const times = this.offered.filter(slot => this.booked.includes(slot.id)).map(slot => slot.label);
      this.prepared = times.length === 0 ? "" : `${this.wingName}: ${times.join(", ")}`;
    });
  }

  summary(): Promise<string> {
    const text = this.prepared;
    return later(100).then(() => text);
  }

  focus(): void {
    this.host.querySelector<HTMLButtonElement>("button.vp-slot:not([disabled])")?.focus();
  }

  on(name: string, listener: Listener): void {
    let named = this.listeners.get(name);
    if (named === undefined) {
      named = new Set();
      this.listeners.set(name, named);
    }
    named.add(listener);
  }

  off(name: string, listener: Listener): void {
    this.listeners.get(name)?.delete(listener);
  }

  destroy(): void {
    this.host.removeEventListener("click", this.onClick);
    this.listeners.clear();
    this.host.replaceChildren();
    delete this.host.visitPlanner;
  }

  private readonly onClick = (event: Event): void => {
    const button = (event.target as Element).closest<HTMLButtonElement>("button.vp-slot");
    if (button === null || this.locked) return;
    const id = button.dataset.slot ?? "";
    if (this.booked.includes(id)) this.release(id);
    else this.select(id);
  };

  private emitChange(added: string | null, removed: string | null): void {
    this.emit("weekChanged", { value: this.value, added, removed, count: this.count, remaining: this.remaining() });
  }

  private emit(name: string, payload: unknown): void {
    for (const listener of [...(this.listeners.get(name) ?? [])]) listener(payload);
  }

  private inOfferedOrder(ids: string[]): string[] {
    if (this.offered.length === 0) return [...ids];
    const order = this.offered.map(slot => slot.id);
    return ids.filter(id => order.includes(id)).sort((a, b) => order.indexOf(a) - order.indexOf(b));
  }

  private render(): void {
    const head = element("div", "vp-head");
    head.append(element("strong", "vp-wing", this.wingName), element("span", "vp-left", leftText(this.remaining())));

    const body = this.offered.length === 0 ? element("p", "vp-empty", this.emptyText) : this.renderSlots();
    this.host.replaceChildren(head, body);
  }

  private renderSlots(): HTMLElement {
    const list = element("ul", "vp-slots");
    list.setAttribute("aria-label", "Visit times");
    for (const slot of this.offered) {
      const booked = this.booked.includes(slot.id);
      const button = element("button", slot.quiet ? "vp-slot vp-quiet" : "vp-slot", slot.label) as HTMLButtonElement;
      button.type = "button";
      button.dataset.slot = slot.id;
      button.setAttribute("aria-pressed", String(booked));
      const takenBy = this.takenBy.get(slot.id);
      button.disabled = this.locked || takenBy !== undefined;
      if (slot.quiet) button.title = "Quiet hours";

      const item = element("li", "vp-item");
      item.append(button);
      const note = this.notes.get(slot.id);
      if (booked && note !== undefined) item.append(element("span", "vp-note", noteText(note)));
      if (this.waitlisted.has(slot.id)) item.append(element("span", "vp-waitlist", "On the waitlist"));
      if (takenBy !== undefined) item.append(element("span", "vp-taken", `Taken by the ${takenBy} family`));
      list.append(item);
    }
    return list;
  }
}

function later(milliseconds: number): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, milliseconds));
}

function element(tag: string, className: string, text?: string): HTMLElement {
  const created = document.createElement(tag);
  created.className = className;
  if (text !== undefined) created.textContent = text;
  return created;
}

function leftText(remaining: number): string {
  return remaining === 1 ? "1 visit left" : `${remaining} visits left`;
}

function noteText(note: BookingNote): string {
  const visitors = note.visitors === 1 ? "1 visitor" : `${note.visitors} visitors`;
  return note.note === "" ? visitors : `${visitors} · ${note.note}`;
}

export function visitPlannerHosts(root: HTMLElement): VisitPlannerHost[] {
  const hosts = Array.from(root.querySelectorAll<VisitPlannerHost>(".visit-planner"));
  return root.matches(".visit-planner") ? [root as VisitPlannerHost, ...hosts] : hosts;
}
