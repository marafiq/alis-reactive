// The day picker: a few days, one of which the user may pick, holding none until they do. It knows
// nothing of Alis.Reactive; DayPicker.cs declares its members by these names. Asked whether the picked
// day is free, it answers a moment later and shows the check on its own line.

type Listener = (payload: unknown) => void;

interface DayPickerHost extends HTMLElement {
  dayPicker?: DayPicker;
}

function dayText(day: Date): string {
  const two = (value: number): string => String(value).padStart(2, "0");
  return `${day.getFullYear()}-${two(day.getMonth() + 1)}-${two(day.getDate())}`;
}

// A "YYYY-MM-DD" day as the person picks it: local midnight.
function dayOf(text: string): Date {
  const [year, month, date] = text.split("-").map(Number);
  return new Date(year ?? 0, (month ?? 1) - 1, date ?? 1);
}

function listed(text: string | undefined): string[] {
  return (text ?? "").split("|").filter(day => day.length > 0);
}

function aMoment(): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, 150));
}

export class DayPicker {
  private readonly listeners = new Map<string, Set<Listener>>();
  private readonly booked: Set<string>;
  private readonly pickedLine: HTMLParagraphElement;
  private readonly checkLine: HTMLParagraphElement;
  private readonly buttons: HTMLButtonElement[];
  private picked: Date | null = null;

  constructor(private readonly host: DayPickerHost) {
    this.booked = new Set(listed(host.dataset["booked"]));
    this.pickedLine = document.createElement("p");
    this.checkLine = document.createElement("p");
    this.checkLine.textContent = "Availability not checked";
    this.buttons = listed(host.dataset["days"]).map(day => this.button(`Pick ${day}`, () => this.pick(dayOf(day))));
    this.buttons.push(this.button("Clear the day", () => this.pick(null)));
    host.replaceChildren(this.pickedLine, ...this.buttons, this.checkLine);
    host.dayPicker = this;
    this.show();
  }

  /** The day picked, or null. */
  get day(): Date | null {
    return this.picked;
  }

  set day(value: Date | null) {
    this.picked = value;
    this.show();
  }

  /** Whether the picked day is free; it shows that it is checking at once, and the answer when it has it. */
  async isFree(): Promise<boolean> {
    const day = this.picked;
    this.checkLine.textContent = day === null ? "Checking, with no day picked" : `Checking ${dayText(day)}`;
    await aMoment();
    if (day === null) {
      this.checkLine.textContent = "No day to check";
      return false;
    }
    const free = !this.booked.has(dayText(day));
    this.checkLine.textContent = `${dayText(day)} is ${free ? "free" : "booked"}`;
    return free;
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
    delete this.host.dayPicker;
  }

  private button(text: string, click: () => void): HTMLButtonElement {
    const button = document.createElement("button");
    button.type = "button";
    button.textContent = text;
    button.addEventListener("click", click);
    return button;
  }

  private pick(day: Date | null): void {
    this.day = day;
    for (const listener of [...(this.listeners.get("changed") ?? [])]) listener(undefined);
  }

  private show(): void {
    this.pickedLine.textContent = this.picked === null ? "No day picked" : `Picked ${dayText(this.picked)}`;
  }
}

interface MountDetail {
  readonly root: HTMLElement;
}

function hosts(root: HTMLElement): DayPickerHost[] {
  const inside = Array.from(root.querySelectorAll<DayPickerHost>("div.day-picker"));
  return root.matches("div.day-picker") ? [root as DayPickerHost, ...inside] : inside;
}

document.addEventListener("alis:mount", event => {
  for (const host of hosts((event as CustomEvent<MountDetail>).detail.root)) {
    if (host.dayPicker === undefined) new DayPicker(host);
  }
});

document.addEventListener("alis:unmount", event => {
  for (const host of hosts((event as CustomEvent<MountDetail>).detail.root)) host.dayPicker?.destroy();
});
