import { announcedRoot, mountsTheElement } from "../../NativeComponents/mounted";

const confirmId = "alis-confirm";
const messageId = "alis-confirm-message";
const okClass = "alis-confirm__ok";
const cancelClass = "alis-confirm__cancel";

interface Ask {
  readonly message: string;
  readonly signal: AbortSignal;
  readonly settles: (yes: boolean) => void;
}

function buttonOf(dialog: HTMLDialogElement, className: string): HTMLButtonElement {
  const button = dialog.querySelector("." + className);
  if (!(button instanceof HTMLButtonElement)) throw new Error("The confirm '" + dialog.id + "' holds no button " + className + ".");
  return button;
}

function messageOf(): HTMLElement {
  const message = document.getElementById(messageId);
  if (message === null) throw new Error("The confirm's message '" + messageId + "' is missing.");
  return message;
}

function textOf(message: unknown): string {
  return typeof message === "string" ? message : "";
}

function signalOf(signal: unknown): AbortSignal {
  if (!(signal instanceof AbortSignal)) throw new Error("The confirm's ask takes the run's abort signal as its second argument.");
  return signal;
}

function mountsConfirm(dialog: HTMLDialogElement): void {
  const message = messageOf();
  const waiting: Ask[] = [];
  let open: Ask | undefined;

  const shows = (): void => {
    if (open !== undefined || dialog.open) return;
    const next = waiting.shift();
    if (next === undefined) return;
    if (next.signal.aborted) {
      next.settles(false);
      shows();
      return;
    }
    open = next;
    message.textContent = next.message;
    dialog.showModal();
  };

  const answersTheOpen = (yes: boolean): void => {
    const answered = open;
    if (answered === undefined) return;
    open = undefined;
    answered.settles(yes);
  };

  const answers = (yes: boolean): void => {
    answersTheOpen(yes);
    dialog.close();
    shows();
  };

  const endsTheRun = (ended: Ask): void => {
    if (open === ended) {
      answers(false);
      return;
    }
    const place = waiting.indexOf(ended);
    if (place < 0) return;
    waiting.splice(place, 1);
    ended.settles(false);
  };

  Object.defineProperty(dialog, "ask", {
    configurable: true,
    value: (asked: unknown, given: unknown): Promise<boolean> => {
      const signal = signalOf(given);
      return new Promise<boolean>((resolve) => {
        const ask: Ask = {
          message: textOf(asked),
          signal,
          settles: (yes) => {
            signal.removeEventListener("abort", ends);
            resolve(yes);
          },
        };
        const ends = (): void => {
          endsTheRun(ask);
        };
        signal.addEventListener("abort", ends);
        waiting.push(ask);
        shows();
      });
    },
  });
  buttonOf(dialog, okClass).addEventListener("click", () => {
    answers(true);
  });
  buttonOf(dialog, cancelClass).addEventListener("click", () => {
    answers(false);
  });
  dialog.addEventListener("keydown", (event) => {
    if (event.key === "Escape") event.stopPropagation();
  });
  dialog.addEventListener("cancel", () => {
    answersTheOpen(false);
  });
  dialog.addEventListener("close", () => {
    shows();
  });
  document.addEventListener("alis:unmount", (event) => {
    if (!announcedRoot(event).contains(dialog)) return;
    answersTheOpen(false);
    for (const ask of waiting.splice(0)) ask.settles(false);
    dialog.close();
  });
}

export function mountsTheConfirm(): void {
  mountsTheElement(confirmId, HTMLDialogElement, mountsConfirm);
}
