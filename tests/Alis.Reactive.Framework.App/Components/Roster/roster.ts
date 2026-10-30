// The roster: the names it is given, one line each, in the order given. It knows nothing of
// Alis.Reactive; Roster.cs declares its members by these names.

interface RosterHost extends HTMLElement {
  roster?: Roster;
}

export class Roster {
  private readonly list = document.createElement("ul");

  constructor(private readonly host: RosterHost) {
    host.replaceChildren(this.list);
    host.roster = this;
  }

  /** Shows the names in place of the lines it showed. */
  show(names: readonly string[]): void {
    this.list.replaceChildren(...names.map(name => {
      const line = document.createElement("li");
      line.textContent = name;
      return line;
    }));
  }

  destroy(): void {
    delete this.host.roster;
  }
}

interface MountDetail {
  readonly root: HTMLElement;
}

function hosts(root: HTMLElement): RosterHost[] {
  const inside = Array.from(root.querySelectorAll<RosterHost>("div.roster"));
  return root.matches("div.roster") ? [root as RosterHost, ...inside] : inside;
}

document.addEventListener("alis:mount", event => {
  for (const host of hosts((event as CustomEvent<MountDetail>).detail.root)) {
    if (host.roster === undefined) new Roster(host);
  }
});

document.addEventListener("alis:unmount", event => {
  for (const host of hosts((event as CustomEvent<MountDetail>).detail.root)) host.roster?.destroy();
});
