// The slate: a board that holds a title, a note, a header and rows, and shows them. It knows nothing
// of Alis.Reactive; Slate.cs declares its members by these names.

type Listener = (payload: unknown) => void;

interface SlateHost extends HTMLElement {
  slate?: Slate;
}

/** The header above the title: an object the slate holds. */
export class Header {
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
  }
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
  readonly name: string;
  readonly due: Date;
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
  private readonly listeners = new Map<string, Set<Listener>>();
  private readonly captionLine: HTMLParagraphElement;
  private readonly heading: HTMLHeadingElement;
  private readonly noteLine: HTMLParagraphElement;
  private readonly summaryLine: HTMLParagraphElement;
  private readonly list: HTMLUListElement;
  private readonly addButton: HTMLButtonElement;
  private readonly placedLine: HTMLParagraphElement;
  private titleText: string;
  private noteText = "";
  private summaryText = "";
  private placedText = "";
  private runningTotal = 0;
  private saveStatus = "idle";

  constructor(private readonly host: SlateHost) {
    const changed = (): void => this.show();
    this.header = new Header(changed);
    this.titleText = host.dataset["title"] ?? "";
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
    this.addButton = document.createElement("button");
    this.addButton.type = "button";
    this.addButton.textContent = "Add row";
    this.addButton.addEventListener("click", this.addRow);
    host.replaceChildren(this.captionLine, this.heading, this.noteLine, this.summaryLine, this.placedLine, this.list, this.addButton);
    host.slate = this;
    this.show();
  }

  /** How many rows the slate holds. */
  get count(): number {
    return this.rows.length;
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

  /** The rows marked so far. */
  markedRows(): Row[] {
    return this.rows.filter(row => row.marked);
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
  }

  /** A placed line of four values; the number joins the total. Each must arrive as its own type. */
  place(text: string, number: number, flag: boolean, day: Date): void {
    this.runningTotal += number;
    this.placedText = `${text} · ${number.toFixed(1)} · ${flag === true ? "yes" : "no"} · ${dayText(day)}`;
    this.show();
  }

  /** A card: whose it is and the day it is due, by name and weekday. */
  fill(card: Card): void {
    this.placedText = `${card.name} due ${weekdays[card.due.getDay()]} ${dayText(card.due)}`;
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
    const named = this.listeners.get(name) ?? new Set<Listener>();
    named.add(listener);
    this.listeners.set(name, named);
  }

  off(name: string, listener: Listener): void {
    this.listeners.get(name)?.delete(listener);
  }

  destroy(): void {
    this.addButton.removeEventListener("click", this.addRow);
    this.listeners.clear();
    delete this.host.slate;
  }

  private readonly addRow = (): void => {
    this.rows.push(new Row(`Row ${this.rows.length + 1}`, () => this.show()));
    this.show();
  };

  private show(): void {
    this.captionLine.textContent = this.header.caption;
    this.heading.textContent = this.titleText;
    this.noteLine.textContent = this.noteText;
    this.summaryLine.textContent = this.summaryText;
    this.placedLine.textContent = this.placedText;
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
