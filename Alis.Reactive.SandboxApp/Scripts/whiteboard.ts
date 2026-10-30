// A room whiteboard: a widget the sandbox wrote itself, with no knowledge of Alis.Reactive. Its object
// lives on its host element as `whiteboard`; BroughtComponents/Whiteboard/Whiteboard.cs declares it to
// the framework. It is the board on a resident's wall that the nurse keeps up to date:
//
//   properties  nurse and goal (read and written), nextCheckMinutes (read and written, null when no
//               check is scheduled), notes and lastRefresh (read only)
//   methods     page(who), highlight(row, effect), addNote(number, text), wipe()
//   functions   nextNoteNumber() (each call draws the next number), refresh() (a promise that settles
//               after a moment and then records the refresh), loadMotto() (a promise of the wing's motto)
//   events      noteAdded (number, text); wiped (no payload).
//               Listeners are added with on(name, fn) and removed with off(name, fn).

import "./whiteboard.css";

export interface NoteAdded {
  readonly number: number;
  readonly text: string;
}

type Listener = (payload: unknown) => void;

const ROWS = ["nurse", "goal", "nextCheck"] as const;
type Row = (typeof ROWS)[number];

const SETTLE_MS = 150;

export interface WhiteboardHost extends HTMLElement {
  whiteboard?: Whiteboard;
}

export function whiteboardHosts(root: HTMLElement): WhiteboardHost[] {
  return Array.from(root.querySelectorAll<WhiteboardHost>("div.whiteboard"));
}

export class Whiteboard {
  private readonly host: WhiteboardHost;
  private readonly listeners = new Map<string, Set<Listener>>();
  private nurseName: string;
  private goalText = "";
  private nextCheck: number | null = null;
  private readonly noteLines: string[] = [];
  private drawn = 0;
  private refreshes = 0;
  private status = "";
  private readonly effects = new Map<Row, string>();

  constructor(host: WhiteboardHost) {
    this.host = host;
    this.nurseName = host.dataset.nurse ?? "";
    host.whiteboard = this;
    this.render();
  }

  get nurse(): string {
    return this.nurseName;
  }

  set nurse(name: string) {
    this.nurseName = name;
    this.render();
  }

  get goal(): string {
    return this.goalText;
  }

  set goal(text: string) {
    this.goalText = text;
    this.render();
  }

  /** Minutes until the next check; null when no check is scheduled. */
  get nextCheckMinutes(): number | null {
    return this.nextCheck;
  }

  set nextCheckMinutes(minutes: number | null) {
    this.nextCheck = minutes;
    this.render();
  }

  /** How many notes are on the board. */
  get notes(): number {
    return this.noteLines.length;
  }

  /** What the last refresh recorded; empty until a refresh has settled. */
  get lastRefresh(): string {
    return this.refreshes === 0 ? "" : `Refreshed ${this.refreshes}`;
  }

  /** Pages someone from the board. */
  page(who: string): void {
    this.status = `Paged ${who}`;
    this.render();
  }

  /** Marks a row with an effect, shown as the row's style; "plain" when the caller names none. */
  highlight(row: string, effect = "plain"): void {
    if (!isRow(row)) throw new Error(`whiteboard has no row "${row}"`);
    this.effects.set(row, effect);
    this.render();
  }

  /** Draws the next note number. */
  nextNoteNumber(): number {
    this.drawn += 1;
    return this.drawn;
  }

  /** Adds a numbered note and tells whoever listens. */
  addNote(number: number, text: string): void {
    this.noteLines.push(`${number}. ${text}`);
    this.render();
    this.emit("noteAdded", { number, text } satisfies NoteAdded);
  }

  /** Clears the notes and tells whoever listens; the listener receives nothing. */
  wipe(): void {
    this.noteLines.length = 0;
    this.render();
    this.emit("wiped", undefined);
  }

  /** Settles after a moment, then records the refresh. */
  refresh(): Promise<void> {
    return new Promise(resolve => {
      setTimeout(() => {
        this.refreshes += 1;
        this.render();
        resolve();
      }, SETTLE_MS);
    });
  }

  /** The wing's motto, after a moment. */
  loadMotto(): Promise<string> {
    return new Promise(resolve => {
      setTimeout(() => resolve("Kindness first"), SETTLE_MS);
    });
  }

  on(name: string, listener: Listener): void {
    let listeners = this.listeners.get(name);
    if (listeners === undefined) {
      listeners = new Set();
      this.listeners.set(name, listeners);
    }
    listeners.add(listener);
  }

  off(name: string, listener: Listener): void {
    this.listeners.get(name)?.delete(listener);
  }

  destroy(): void {
    this.listeners.clear();
    this.host.replaceChildren();
    delete this.host.whiteboard;
  }

  private emit(name: string, payload: unknown): void {
    for (const listener of [...(this.listeners.get(name) ?? [])]) listener(payload);
  }

  private render(): void {
    const rows = element("ul", "wb-rows");
    rows.append(
      this.row("nurse", "Nurse", this.nurseName),
      this.row("goal", "Today's goal", this.goalText),
      this.row("nextCheck", "Next check", this.nextCheck === null ? "No check scheduled" : `In ${this.nextCheck} min`),
    );

    const notes = element("ol", "wb-notes");
    for (const line of this.noteLines) notes.append(element("li", "wb-note", line));

    const foot = element("div", "wb-foot");
    foot.append(element("span", "wb-refresh", this.lastRefresh));

    this.host.replaceChildren(rows, notes, foot, element("p", "wb-status", this.status));
  }

  private row(row: Row, label: string, value: string): HTMLElement {
    const item = element("li", "wb-row");
    item.dataset["row"] = row;
    const effect = this.effects.get(row);
    if (effect !== undefined) item.classList.add(`wb-effect-${effect}`);
    item.append(element("span", "wb-row-label", label), element("span", "wb-row-value", value));
    return item;
  }
}

function isRow(name: string): name is Row {
  return (ROWS as readonly string[]).includes(name);
}

function element(tag: string, className: string, text = ""): HTMLElement {
  const created = document.createElement(tag);
  created.className = className;
  created.textContent = text;
  return created;
}
