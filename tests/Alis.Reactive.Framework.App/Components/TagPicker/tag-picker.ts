type Listener = (payload: unknown) => void;

interface PickedTag {
  readonly code?: string;
  readonly label: string;
}

interface TagPickerHost extends HTMLElement {
  tagPicker?: TagPicker;
}

export class TagPicker {
  private readonly listeners = new Map<string, Set<Listener>>();
  private readonly tags: PickedTag[] = [];
  private readonly line = document.createElement("p");

  constructor(private readonly host: TagPickerHost) {
    host.replaceChildren(
      this.button("Pick the east wing tag", { code: "east", label: "East wing" }),
      this.button("Pick the north wing tag", { code: "north", label: "North wing" }),
      this.button("Pick a tag with no code", { label: "Visitor" }),
      this.line);
    host.tagPicker = this;
  }

  get picked(): readonly PickedTag[] {
    return this.tags;
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
    this.listeners.clear();
    delete this.host.tagPicker;
  }

  private button(text: string, tag: PickedTag): HTMLButtonElement {
    const button = document.createElement("button");
    button.type = "button";
    button.textContent = text;
    button.addEventListener("click", () => this.pick(tag));
    return button;
  }

  private pick(tag: PickedTag): void {
    this.tags.push(tag);
    this.line.textContent = `Picked: ${this.tags.map(picked => picked.label).join(" · ")}`;
    for (const listener of [...(this.listeners.get("changed") ?? [])]) listener({});
  }
}

interface MountDetail {
  readonly root: HTMLElement;
}

function hosts(root: HTMLElement): TagPickerHost[] {
  const inside = Array.from(root.querySelectorAll<TagPickerHost>("div.tag-picker"));
  return root.matches("div.tag-picker") ? [root as TagPickerHost, ...inside] : inside;
}

document.addEventListener("alis:mount", event => {
  for (const host of hosts((event as CustomEvent<MountDetail>).detail.root)) {
    if (host.tagPicker === undefined) new TagPicker(host);
  }
});

document.addEventListener("alis:unmount", event => {
  for (const host of hosts((event as CustomEvent<MountDetail>).detail.root)) host.tagPicker?.destroy();
});
