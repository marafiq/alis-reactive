interface RosterHost extends HTMLElement {
  roster?: Roster;
}

export class Roster {
  private readonly list = document.createElement("ul");

  constructor(private readonly host: RosterHost) {
    host.replaceChildren(this.list);
    host.roster = this;
  }

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
