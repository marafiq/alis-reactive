// The checklist: a group of boxes whose value is the text of every box ticked, in the order shown, and
// which tells its listeners when the user ticks or clears one. It knows nothing of Alis.Reactive;
// Checklist.cs declares its members by these names. The group's element holds it as `checklist`.

type Listener = (payload: unknown) => void;

interface ChecklistHost extends HTMLElement {
  checklist?: Checklist;
}

export class Checklist {
  private readonly listeners = new Map<string, Set<Listener>>();

  constructor(private readonly group: ChecklistHost) {
    group.addEventListener("change", this.tick);
    group.checklist = this;
  }

  /** The text of every box ticked, in the order shown. */
  get ticked(): string[] {
    return Array.from(this.group.querySelectorAll<HTMLInputElement>("input[type=checkbox]"))
      .filter(box => box.checked)
      .map(box => box.value);
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
    this.group.removeEventListener("change", this.tick);
    this.listeners.clear();
    delete this.group.checklist;
  }

  private readonly tick = (): void => {
    for (const listener of [...(this.listeners.get("changed") ?? [])]) listener({});
  };
}

interface MountDetail {
  readonly root: HTMLElement;
}

function hosts(root: HTMLElement): ChecklistHost[] {
  const inside = Array.from(root.querySelectorAll<ChecklistHost>("div.checklist"));
  return root.matches("div.checklist") ? [root as ChecklistHost, ...inside] : inside;
}

document.addEventListener("alis:mount", event => {
  for (const host of hosts((event as CustomEvent<MountDetail>).detail.root)) {
    if (host.checklist === undefined) new Checklist(host);
  }
});

document.addEventListener("alis:unmount", event => {
  for (const host of hosts((event as CustomEvent<MountDetail>).detail.root)) host.checklist?.destroy();
});
