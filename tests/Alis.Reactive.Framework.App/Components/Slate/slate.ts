// The slate: a board that holds a title, a note and rows, and shows them. It knows nothing of
// Alis.Reactive; Slate.cs declares its members by these names.

type Listener = (payload: unknown) => void;

interface SlateHost extends HTMLElement {
  slate?: Slate;
}

export class Slate {
  private readonly listeners = new Map<string, Set<Listener>>();
  private readonly heading: HTMLHeadingElement;
  private readonly noteLine: HTMLParagraphElement;
  private readonly list: HTMLUListElement;
  private readonly addButton: HTMLButtonElement;
  private titleText: string;
  private noteText = "";
  private readonly rows: string[];

  constructor(private readonly host: SlateHost) {
    this.titleText = host.dataset["title"] ?? "";
    this.rows = (host.dataset["rows"] ?? "").split("|").filter(row => row.length > 0);
    this.heading = document.createElement("h2");
    this.noteLine = document.createElement("p");
    this.list = document.createElement("ul");
    this.addButton = document.createElement("button");
    this.addButton.type = "button";
    this.addButton.textContent = "Add row";
    this.addButton.addEventListener("click", this.addRow);
    host.replaceChildren(this.heading, this.noteLine, this.list, this.addButton);
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
    this.rows.push(`Row ${this.rows.length + 1}`);
    this.show();
  };

  private show(): void {
    this.heading.textContent = this.titleText;
    this.noteLine.textContent = this.noteText;
    this.list.replaceChildren(...this.rows.map(row => {
      const item = document.createElement("li");
      item.textContent = row;
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
