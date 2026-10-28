// The file picker: a file input that holds the files the person chose, as the browser's own File
// objects, and tells its listeners when the person chooses. It knows nothing of Alis.Reactive. The
// input element holds it as `filePicker`.

type Listener = (payload: unknown) => void;

interface FilePickerHost extends HTMLInputElement {
  filePicker?: FilePicker;
}

export class FilePicker {
  private readonly listeners = new Map<string, Set<Listener>>();

  constructor(private readonly input: FilePickerHost) {
    input.addEventListener("change", this.choose);
    input.filePicker = this;
  }

  get files(): File[] {
    return Array.from(this.input.files ?? []);
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
    this.input.removeEventListener("change", this.choose);
    this.listeners.clear();
    delete this.input.filePicker;
  }

  private readonly choose = (): void => {
    for (const listener of [...(this.listeners.get("chosen") ?? [])]) listener({});
  };
}

interface MountDetail {
  readonly root: HTMLElement;
}

function hosts(root: HTMLElement): FilePickerHost[] {
  const inside = Array.from(root.querySelectorAll<FilePickerHost>("input.file-picker"));
  return root.matches("input.file-picker") ? [root as FilePickerHost, ...inside] : inside;
}

document.addEventListener("alis:mount", event => {
  for (const host of hosts((event as CustomEvent<MountDetail>).detail.root)) {
    if (host.filePicker === undefined) new FilePicker(host);
  }
});

document.addEventListener("alis:unmount", event => {
  for (const host of hosts((event as CustomEvent<MountDetail>).detail.root)) host.filePicker?.destroy();
});
