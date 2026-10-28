// The choice: a select of named options that tells its listeners when the user picks one. It knows
// nothing of Alis.Reactive; Choice.cs declares its members by these names. The select element holds
// it as `choice`.

type Listener = (payload: unknown) => void;

interface ChoiceHost extends HTMLSelectElement {
  choice?: Choice;
}

export class Choice {
  private readonly listeners = new Map<string, Set<Listener>>();

  constructor(private readonly select: ChoiceHost) {
    select.addEventListener("change", this.pick);
    select.choice = this;
  }

  /** The name of the option chosen; empty for the blank option. */
  get value(): string {
    return this.select.value;
  }

  set value(name: string) {
    this.select.value = name;
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
    this.select.removeEventListener("change", this.pick);
    this.listeners.clear();
    delete this.select.choice;
  }

  private readonly pick = (): void => {
    for (const listener of [...(this.listeners.get("changed") ?? [])]) listener({});
  };
}

interface MountDetail {
  readonly root: HTMLElement;
}

function hosts(root: HTMLElement): ChoiceHost[] {
  const inside = Array.from(root.querySelectorAll<ChoiceHost>("select.choice"));
  return root.matches("select.choice") ? [root as ChoiceHost, ...inside] : inside;
}

document.addEventListener("alis:mount", event => {
  for (const host of hosts((event as CustomEvent<MountDetail>).detail.root)) {
    if (host.choice === undefined) new Choice(host);
  }
});

document.addEventListener("alis:unmount", event => {
  for (const host of hosts((event as CustomEvent<MountDetail>).detail.root)) host.choice?.destroy();
});
