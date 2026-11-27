import type { Dialog } from "@syncfusion/ej2-popups";
import { announcedRoot } from "../../FusionComponents/mounted";

const hostId = "alisConfirmDialog";

interface Ask {
  readonly message: string;
  readonly signal: AbortSignal;
  readonly settle: (accepted: boolean) => void;
}

function memberOf(holder: unknown, member: string): unknown {
  return typeof holder === "object" && holder !== null ? Reflect.get(holder, member) : undefined;
}

function dialogClass(host: HTMLElement): typeof Dialog {
  const defined = memberOf(memberOf(memberOf(window, "ej"), "popups"), "Dialog");
  if (typeof defined !== "function") throw new Error("The confirm '" + host.id + "' finds no EJ2 Dialog: link ej2.min.js before alis-reactive-fusion.js.");
  return defined as typeof Dialog;
}

function labelOf(host: HTMLElement, attribute: string): string {
  const written = host.getAttribute(attribute);
  if (written === null) throw new Error("The confirm '" + host.id + "' carries no " + attribute + ".");
  return written;
}

function textElement(text: string): HTMLSpanElement {
  const span = document.createElement("span");
  span.textContent = text;
  return span;
}

export class FusionConfirm extends EventTarget {
  readonly #dialog: Dialog;
  readonly #title: string;
  readonly #ok: string;
  readonly #cancel: string;
  readonly #waiting: Ask[] = [];
  #active: Ask | undefined;
  #requestedAnswer: boolean | undefined;
  #destroyed = false;

  constructor(host: HTMLDivElement) {
    super();
    this.#title = labelOf(host, "data-alis-title");
    this.#ok = labelOf(host, "data-alis-ok");
    this.#cancel = labelOf(host, "data-alis-cancel");
    const DialogType = dialogClass(host);
    this.#dialog = new DialogType({
      isModal: true,
      visible: false,
      width: "400px",
      animationSettings: { effect: "None" },
      showCloseIcon: false,
      closeOnEscape: true,
      target: document.body,
      close: () => { this.#closed(); },
    });
    this.#dialog.appendTo(host);
  }

  ask(message: string | null, signal: AbortSignal): Promise<boolean> {
    if (this.#destroyed || signal.aborted) return Promise.resolve(false);
    return new Promise<boolean>((resolve) => {
      const ask: Ask = {
        message: message ?? "",
        signal,
        settle: (accepted) => {
          signal.removeEventListener("abort", abort);
          resolve(accepted);
        },
      };
      const abort = (): void => { this.#abort(ask); };
      signal.addEventListener("abort", abort, { once: true });
      this.#waiting.push(ask);
      this.#showNext();
    });
  }

  destroy(): void {
    this.#destroyed = true;
    const active = this.#active;
    this.#active = undefined;
    active?.settle(false);
    for (const ask of this.#waiting.splice(0)) ask.settle(false);
    this.#dialog.destroy();
  }

  #showNext(): void {
    if (this.#active !== undefined) return;
    const next = this.#waiting.shift();
    if (next === undefined) return;
    this.#active = next;
    this.#dialog.header = textElement(this.#title);
    this.#dialog.content = textElement(next.message);
    this.#dialog.buttons = [
      { click: () => { this.#close(true); }, buttonModel: { content: textElement(this.#ok).outerHTML, isPrimary: true } },
      { click: () => { this.#close(false); }, buttonModel: { content: textElement(this.#cancel).outerHTML } },
    ];
    this.#dialog.dataBind();
    this.#dialog.show();
  }

  #close(accepted: boolean): void {
    if (this.#active === undefined || this.#requestedAnswer !== undefined) return;
    this.#requestedAnswer = accepted;
    this.#dialog.hide();
  }

  #closed(): void {
    const active = this.#active;
    if (active === undefined) return;
    queueMicrotask(() => {
      if (this.#active !== active) return;
      const accepted = this.#requestedAnswer === true && !active.signal.aborted;
      this.#active = undefined;
      this.#requestedAnswer = undefined;
      active.settle(accepted);
      this.#showNext();
    });
  }

  #abort(ask: Ask): void {
    if (this.#active === ask) {
      this.#close(false);
      return;
    }
    const index = this.#waiting.indexOf(ask);
    if (index < 0) return;
    this.#waiting.splice(index, 1);
    ask.settle(false);
  }
}

export function mountsFusionConfirm(): void {
  document.addEventListener("alis:mount", (event) => {
    const root = announcedRoot(event);
    const host = root.id === hostId ? root : root.querySelector("#" + hostId);
    if (host === null || Object.hasOwn(host, "alisConfirm")) return;
    if (!(host instanceof HTMLDivElement)) throw new Error("The Fusion confirm's host '" + hostId + "' is no div.");
    const originalParent = host.parentElement;
    const confirm = new FusionConfirm(host);
    const lifetime = new AbortController();
    Object.defineProperty(host, "alisConfirm", { configurable: true, value: confirm });
    host.addEventListener("keydown", (key) => {
      if (key.key === "Escape") key.stopPropagation();
    }, { signal: lifetime.signal });
    document.addEventListener("alis:unmount", (unmount) => {
      const leaving = announcedRoot(unmount);
      if (!leaving.contains(host) && (originalParent === null || !leaving.contains(originalParent))) return;
      lifetime.abort();
      confirm.destroy();
      Reflect.deleteProperty(host, "alisConfirm");
    }, { signal: lifetime.signal });
  });
}
