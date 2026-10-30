// The slate: a board that holds a title, a note, a header and rows, and shows them. It knows nothing
// of Alis.Reactive; Slate.cs declares its members by these names.

type Listener = (payload: unknown) => void;

/** Listeners by event name, as the slate and its header both keep them. */
class Listeners {
  private readonly named = new Map<string, Set<Listener>>();

  add(name: string, listener: Listener): void {
    const listeners = this.named.get(name) ?? new Set<Listener>();
    listeners.add(listener);
    this.named.set(name, listeners);
  }

  remove(name: string, listener: Listener): void {
    this.named.get(name)?.delete(listener);
  }

  emit(name: string, payload?: unknown): void {
    for (const listener of [...(this.named.get(name) ?? [])]) listener(payload);
  }

  clear(): void {
    this.named.clear();
  }
}

interface SlateHost extends HTMLElement {
  slate?: Slate;
}

/** The header above the title: an object the slate holds. */
export class Header {
  readonly listeners = new Listeners();
  private captionText = "";

  constructor(private readonly changed: () => void) {}

  get caption(): string {
    return this.captionText;
  }

  set caption(text: string) {
    this.captionText = text;
    this.changed();
  }

  rename(caption: string): void {
    this.caption = caption;
    this.listeners.emit("renamed", { caption });
  }

  on(name: string, listener: Listener): void {
    this.listeners.add(name, listener);
  }

  off(name: string, listener: Listener): void {
    this.listeners.remove(name, listener);
  }
}

/** What a changing listener receives; the slate reads cancel, reason and requested back once the listeners ran. */
export interface Changing {
  requested: string;
  readonly current: string;
  cancel: boolean;
  reason: string;
  clamp(): void;
}

/** One row of the slate. */
export class Row {
  private isMarked = false;

  constructor(readonly label: string, private readonly changed: () => void) {}

  get marked(): boolean {
    return this.isMarked;
  }

  mark(): void {
    this.isMarked = true;
    this.changed();
  }
}

export interface Card {
  name: string;
  due: Date;
}

// The slate orders cards by a field's value: a date by its time, anything else as it compares.
function order(a: unknown, b: unknown): number {
  const left = a instanceof Date ? a.getTime() : a;
  const right = b instanceof Date ? b.getTime() : b;
  return left === right ? 0 : (left as string | number) < (right as string | number) ? -1 : 1;
}

const weekdays = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

function dayText(day: Date): string {
  const two = (value: number): string => String(value).padStart(2, "0");
  return `${day.getFullYear()}-${two(day.getMonth() + 1)}-${two(day.getDate())}`;
}

function aMoment(): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, 150));
}

export class Slate {
  readonly header: Header;
  readonly rows: Row[];
  private readonly listeners = new Listeners();
  private readonly captionLine: HTMLParagraphElement;
  private readonly heading: HTMLHeadingElement;
  private readonly noteLine: HTMLParagraphElement;
  private readonly summaryLine: HTMLParagraphElement;
  private readonly list: HTMLUListElement;
  private readonly addButton: HTMLButtonElement;
  private readonly retitleButtons: HTMLButtonElement[];
  private readonly reasonLine: HTMLParagraphElement;
  private readonly placedLine: HTMLParagraphElement;
  private readonly cardsLine: HTMLParagraphElement;
  private readonly limitLine: HTMLParagraphElement;
  private readonly cardList: Card[] = [];
  private latestCard: Card | null = null;
  private limitValue: number | null = null;
  private titleText: string;
  private noteText = "";
  private summaryText = "";
  private placedText = "";
  private runningTotal = 0;
  private saveStatus = "idle";
  private placedDay: Date | null = null;
  private placedWithFlag = false;
  private readonly startedAtMs: number;

  constructor(private readonly host: SlateHost) {
    const changed = (): void => this.show();
    this.header = new Header(changed);
    this.titleText = host.dataset["title"] ?? "";
    this.startedAtMs = Number(host.dataset["started"] ?? "0");
    this.rows = (host.dataset["rows"] ?? "")
      .split("|")
      .filter(label => label.length > 0)
      .map(label => new Row(label, changed));
    this.captionLine = document.createElement("p");
    this.heading = document.createElement("h2");
    this.noteLine = document.createElement("p");
    this.summaryLine = document.createElement("p");
    this.list = document.createElement("ul");
    this.placedLine = document.createElement("p");
    this.cardsLine = document.createElement("p");
    this.limitLine = document.createElement("p");
    this.addButton = document.createElement("button");
    this.addButton.type = "button";
    this.addButton.textContent = "Add row";
    this.addButton.addEventListener("click", this.addRow);
    this.reasonLine = document.createElement("p");
    this.retitleButtons = ["Night round", "Afternoon extended round"].map(title => {
      const button = document.createElement("button");
      button.type = "button";
      button.textContent = `Retitle as ${title}`;
      button.addEventListener("click", () => this.retitle(title));
      return button;
    });
    host.replaceChildren(
      this.captionLine, this.heading, this.reasonLine, this.noteLine, this.summaryLine, this.placedLine,
      this.cardsLine, this.limitLine, this.list, this.addButton, ...this.retitleButtons);
    host.slate = this;
    this.show();
  }

  /** How many rows the slate holds. */
  get count(): number {
    return this.rows.length;
  }

  /** The rows' labels, in order. */
  get labels(): string[] {
    return this.rows.map(row => row.label);
  }

  /** A note shown under the title. Written only: the slate keeps no getter for it. */
  set note(text: string) {
    this.noteText = text;
    this.show();
  }

  get title(): string {
    return this.titleText;
  }

  set title(text: string) {
    this.titleText = text;
    this.show();
  }

  /** The row at a position. */
  rowAt(index: number): Row {
    const row = this.rows[index];
    if (row === undefined) throw new RangeError(`The slate has no row ${index}.`);
    return row;
  }

  /** The row at a position counted from the first row, or from the last. */
  rowFrom(index: number, fromLast: boolean): Row {
    return this.rowAt(fromLast === true ? this.rows.length - 1 - index : index);
  }

  /** The rows marked so far. */
  markedRows(): Row[] {
    return this.rows.filter(row => row.marked);
  }

  /** The labels of a run of rows, from a position, joined by commas. */
  labelsFrom(start: number, count: number): string {
    return this.labels.slice(start, start + count).join(", ");
  }

  /** The labels of a run of rows, from a position, joined by the separator given. */
  labelsJoined(start: number, count: number, separator: string): string {
    return this.labels.slice(start, start + count).join(separator);
  }

  /** Shows the labels as the summary line. */
  summarize(labels: readonly string[]): void {
    this.summaryText = labels.join(" · ");
    this.show();
  }

  /** Takes every row off the slate. */
  clear(): void {
    this.rows.splice(0, this.rows.length);
    this.show();
    this.listeners.emit("cleared");
  }

  /** A placed line of four values; the number joins the total. Each must arrive as its own type. */
  place(text: string, number: number, flag: boolean, day: Date): void {
    this.runningTotal += number;
    this.placedDay = day;
    this.placedWithFlag = flag;
    this.placedText = `${text} · ${number.toFixed(1)} · ${flag === true ? "yes" : "no"} · ${dayText(day)}`;
    this.show();
  }

  /** A card: whose it is and the day it is due, by name and weekday. */
  fill(card: Card): void {
    this.placedText = `${card.name} due ${weekdays[card.due.getDay()]} ${dayText(card.due)}`;
    this.latestCard = { name: card.name, due: card.due };
    this.cardList.push({ name: card.name, due: card.due });
    this.show();
  }

  /** When the slate was started, as a timestamp. */
  get startedAt(): number {
    return this.startedAtMs;
  }

  /** The day of the line placed last, or null before any. */
  get placedOn(): Date | null {
    return this.placedDay;
  }

  /** The flag of the line placed last. */
  get placedFlag(): boolean {
    return this.placedWithFlag;
  }

  /** The card filled last, as data. */
  get lastCard(): Card | null {
    return this.latestCard;
  }

  /** Every card filled, as data. */
  get cards(): readonly Card[] {
    return this.cardList;
  }

  get limit(): number | null {
    return this.limitValue;
  }

  set limit(value: number | null) {
    this.limitValue = value;
    this.show();
  }

  /** Writes one field of a card. */
  setCard(index: number, field: string, value: unknown): void {
    const card = this.cardList[index];
    if (card === undefined) throw new RangeError(`The slate has no card ${index}.`);
    (card as unknown as Record<string, unknown>)[field] = value;
    this.show();
  }

  /** Orders the cards by a field. */
  sortCards(field: string): void {
    this.cardList.sort((a, b) => order((a as unknown as Record<string, unknown>)[field], (b as unknown as Record<string, unknown>)[field]));
    this.show();
  }

  /** Adds a row, saying who asked for it. */
  addRowBy(label: string, by: string): void {
    this.rows.push(new Row(`${label} (by ${by})`, () => this.show()));
    this.show();
  }

  has(label: string): boolean {
    return this.rows.some(row => row.label === label);
  }

  total(): number {
    return this.runningTotal;
  }

  get status(): string {
    return this.saveStatus;
  }

  async save(): Promise<void> {
    this.saveStatus = "saving";
    this.show();
    await aMoment();
    this.saveStatus = "saved";
    this.show();
  }

  async saveRows(): Promise<number> {
    await this.save();
    return this.rows.length;
  }

  async lookup(label: string): Promise<string> {
    await aMoment();
    const position = this.rows.findIndex(row => row.label === label);
    return position === -1 ? `${label} is not on the slate` : `${label} is row ${position + 1}`;
  }

  fail(): void {
    throw new Error("The slate refused.");
  }

  async failLater(): Promise<void> {
    await aMoment();
    throw new Error("The slate refused, a moment later.");
  }

  on(name: string, listener: Listener): void {
    this.listeners.add(name, listener);
  }

  off(name: string, listener: Listener): void {
    this.listeners.remove(name, listener);
  }

  destroy(): void {
    this.addButton.removeEventListener("click", this.addRow);
    this.listeners.clear();
    this.header.listeners.clear();
    delete this.host.slate;
  }

  private readonly addRow = (): void => {
    const label = `Row ${this.rows.length + 1}`;
    this.rows.push(new Row(label, () => this.show()));
    this.show();
    this.listeners.emit("rowAdded", { label, count: this.rows.length });
  };

  // Asks its listeners before it takes a new title: a listener may cancel with a reason, or clamp
  // the requested title; the slate reads the payload once they have all run.
  private retitle(requested: string): void {
    const change: Changing = {
      requested,
      current: this.titleText,
      cancel: false,
      reason: "",
      clamp() {
        this.requested = this.requested.split(" ")[0] ?? this.requested;
      },
    };
    this.listeners.emit("changing", change);
    if (change.cancel) {
      this.reasonLine.textContent = change.reason;
      return;
    }
    this.reasonLine.textContent = "";
    this.title = change.requested;
  }

  private show(): void {
    this.captionLine.textContent = this.header.caption;
    this.heading.textContent = this.titleText;
    this.noteLine.textContent = this.noteText;
    this.summaryLine.textContent = this.summaryText;
    this.placedLine.textContent = this.placedText;
    this.cardsLine.textContent = this.cardList.length === 0 ? "" : `Cards: ${this.cardList.map(card => card.name).join(" · ")}`;
    this.limitLine.textContent = this.limitValue === null ? "No limit" : `Limit: ${this.limitValue}`;
    this.list.replaceChildren(...this.rows.map(row => {
      const item = document.createElement("li");
      item.textContent = row.marked ? `${row.label} (marked)` : row.label;
      return item;
    }));
  }
}

interface MountDetail {
  readonly root: HTMLElement;
}

function hosts(root: HTMLElement): SlateHost[] {
  const inside = Array.from(root.querySelectorAll<SlateHost>("div.slate"));
  return root.matches("div.slate") ? [root as SlateHost, ...inside] : inside;
}

document.addEventListener("alis:mount", event => {
  for (const host of hosts((event as CustomEvent<MountDetail>).detail.root)) {
    if (host.slate === undefined) new Slate(host);
  }
});

document.addEventListener("alis:unmount", event => {
  for (const host of hosts((event as CustomEvent<MountDetail>).detail.root)) host.slate?.destroy();
});
