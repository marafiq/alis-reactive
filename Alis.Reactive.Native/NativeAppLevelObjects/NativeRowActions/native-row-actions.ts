import { providesPlugin } from "../../NativeComponents/mounted";

interface RowEntry {
  readonly id: string;
  readonly row: unknown;
}

class RowActivated extends Event {
  readonly row: unknown;
  readonly element: HTMLElement;

  constructor(row: unknown, element: HTMLElement) {
    super("activated");
    this.row = row;
    this.element = element;
  }
}

function entryOf(entry: unknown): RowEntry {
  if (typeof entry !== "object" || entry === null || !("id" in entry) || typeof entry.id !== "string" || !("row" in entry)) {
    throw new Error("A row action's entry names no control id and row.");
  }
  return { id: entry.id, row: entry.row };
}

function entriesOf(entries: unknown): RowEntry[] {
  if (!Array.isArray(entries)) throw new Error("A row action's controls are no list.");
  return entries.map((entry: unknown) => entryOf(entry));
}

function controlOf(entry: RowEntry): HTMLElement {
  const control = document.getElementById(entry.id);
  if (!(control instanceof HTMLElement)) throw new Error("The row action's control '" + entry.id + "' is not on the page.");
  return control;
}

function isAPlainPrimaryClick(click: MouseEvent): boolean {
  return click.button === 0 && !click.ctrlKey && !click.metaKey && !click.shiftKey && !click.altKey;
}

function controls(entries: unknown): EventTarget {
  const activated = new EventTarget();
  for (const entry of entriesOf(entries)) {
    const control = controlOf(entry);
    control.addEventListener("click", (click) => {
      if (!isAPlainPrimaryClick(click)) return;
      if (control instanceof HTMLAnchorElement) click.preventDefault();
      activated.dispatchEvent(new RowActivated(entry.row, control));
    });
  }
  return activated;
}

export function providesRowActions(): void {
  providesPlugin("native-row-actions", { controls });
}
