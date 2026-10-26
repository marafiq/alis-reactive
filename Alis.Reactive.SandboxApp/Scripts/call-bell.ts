// A resident's call bell: a widget the sandbox wrote itself, with no knowledge of Alis.Reactive. Its
// object lives on its host element as `callBell`; BroughtComponents/CallBell/CallBell.cs declares it to
// the framework. It takes a listener the way a chart takes options.onClick: a function assigned to a
// property named for the event, one function per property.
//
//   properties  room (read and written), ringing (read only), presses (read only)
//   methods     clear()
//   callbacks   onPressed = fn (room, presses), called when the resident presses the bell;
//               onCleared = fn (room, presses), called when the ring is cleared.
//               With no function assigned to onPressed, a press shows that no one is listening.

import "./call-bell.css";

/** What the bell hands a callback: which room, and how many times it has been pressed. */
export interface BellCall {
  readonly room: string;
  readonly presses: number;
}

export type BellCallback = ((call: BellCall) => void) | null;

export interface CallBellHost extends HTMLElement {
  callBell?: CallBell;
}

export function callBellHosts(root: HTMLElement): CallBellHost[] {
  return Array.from(root.querySelectorAll<CallBellHost>("div.call-bell"));
}

export class CallBell {
  private readonly host: CallBellHost;
  private roomName: string;
  private isRinging = false;
  private pressCount = 0;
  private status = "";

  /** Called with each press; the bell shows "No one is listening" while nothing is assigned. */
  onPressed: BellCallback = null;

  /** Called when the ring is cleared. */
  onCleared: BellCallback = null;

  constructor(host: CallBellHost) {
    this.host = host;
    this.roomName = host.dataset.room ?? "";
    host.addEventListener("click", this.onClick);
    host.callBell = this;
    this.render();
  }

  get room(): string {
    return this.roomName;
  }

  set room(name: string) {
    this.roomName = name;
    this.render();
  }

  /** Pressed and not yet cleared. */
  get ringing(): boolean {
    return this.isRinging;
  }

  /** How many times the bell has been pressed. */
  get presses(): number {
    return this.pressCount;
  }

  /** Clears the ring, as answering the call does; tells whoever listens for it. */
  clear(): void {
    this.isRinging = false;
    this.status = "";
    this.render();
    this.onCleared?.({ room: this.roomName, presses: this.pressCount });
  }

  destroy(): void {
    this.host.removeEventListener("click", this.onClick);
    this.host.replaceChildren();
    delete this.host.callBell;
  }

  private press(): void {
    this.isRinging = true;
    this.pressCount += 1;
    const listener = this.onPressed;
    this.status = listener === null ? "No one is listening" : "Call placed";
    this.render();
    listener?.({ room: this.roomName, presses: this.pressCount });
  }

  private readonly onClick = (event: Event): void => {
    const button = (event.target as Element).closest<HTMLButtonElement>("button.cb-press, button.cb-clear");
    if (button === null) return;
    if (button.classList.contains("cb-press")) this.press();
    else this.clear();
  };

  private render(): void {
    const head = element("div", "cb-head");
    head.append(element("strong", "cb-room", `Room ${this.roomName}`), element("span", "cb-state", this.isRinging ? "Ringing" : "Quiet"));

    const controls = element("div", "cb-controls");
    controls.append(this.button("cb-press", "Call", "Call the desk"), this.button("cb-clear", "Clear", "Clear the call"));

    const count = this.pressCount === 1 ? "1 call tonight" : `${this.pressCount} calls tonight`;
    this.host.replaceChildren(head, controls, element("span", "cb-count", count), element("p", "cb-status", this.status));
  }

  private button(className: string, text: string, label: string): HTMLButtonElement {
    const button = element("button", className, text) as HTMLButtonElement;
    button.type = "button";
    button.setAttribute("aria-label", label);
    return button;
  }
}

function element(tag: string, className: string, text = ""): HTMLElement {
  const created = document.createElement(tag);
  created.className = className;
  created.textContent = text;
  return created;
}
