import { mountsEach, providesPlugin } from "../../NativeComponents/mounted";

const marker = "data-native-toast";

interface Regions {
  readonly status: HTMLElement;
  readonly alert: HTMLElement;
}

type Clock = { readonly kind: "running"; readonly timer: ReturnType<typeof setTimeout>; readonly startedAt: number } | { readonly kind: "paused" };

function settingOf(toasts: HTMLElement, name: string): string {
  const written = toasts.getAttribute(marker + "-" + name);
  if (written === null) throw new Error("The toast region '" + toasts.id + "' carries no " + marker + "-" + name + ".");
  return written;
}

function shownForOf(toasts: HTMLElement): number {
  const shownFor = Number(settingOf(toasts, "shown-for"));
  if (!Number.isInteger(shownFor) || shownFor <= 0) throw new Error("The toast region '" + toasts.id + "' is shown for no time.");
  return shownFor;
}

function regionOf(toasts: HTMLElement, at: number, role: string): HTMLElement {
  const region = toasts.children.item(at);
  if (!(region instanceof HTMLElement) || region.getAttribute("role") !== role) throw new Error("The toast region '" + toasts.id + "' holds no " + role + " region.");
  return region;
}

function regionsOf(toasts: HTMLElement): Regions {
  return { status: regionOf(toasts, 0, "status"), alert: regionOf(toasts, 1, "alert") };
}

function textOf(text: unknown): string {
  if (typeof text === "string") return text;
  if (text === null || text === undefined) return "";
  throw new Error("A toast's text is no text.");
}

function leavesAfter(toast: HTMLElement, shownFor: number): () => void {
  let left = shownFor;
  let clock: Clock = { kind: "paused" };
  let hovered = false;
  let focused = false;
  const pause = (): void => {
    if (clock.kind === "paused") return;
    clearTimeout(clock.timer);
    left -= Date.now() - clock.startedAt;
    clock = { kind: "paused" };
  };
  const run = (): void => {
    if (hovered || focused || clock.kind === "running") return;
    clock = { kind: "running", timer: setTimeout(() => { toast.remove(); }, left), startedAt: Date.now() };
  };
  toast.addEventListener("mouseenter", () => { hovered = true; pause(); });
  toast.addEventListener("mouseleave", () => { hovered = false; run(); });
  toast.addEventListener("focusin", () => { focused = true; pause(); });
  toast.addEventListener("focusout", () => { focused = false; run(); });
  run();
  return pause;
}

class NativeToast {
  readonly #regions: Regions;
  readonly #shownFor: number;
  readonly #dismiss: string;

  constructor(toasts: HTMLElement) {
    this.#regions = regionsOf(toasts);
    this.#shownFor = shownForOf(toasts);
    this.#dismiss = settingOf(toasts, "dismiss");
  }

  show(text: unknown, kind: unknown): void {
    const toast = document.createElement("div");
    const message = document.createElement("p");
    message.textContent = textOf(text);
    const dismiss = document.createElement("button");
    dismiss.type = "button";
    dismiss.textContent = this.#dismiss;
    toast.append(message, dismiss);
    switch (kind) {
      case "success":
      case "info":
        this.#leavesAfterItsTime(toast, dismiss, kind, this.#regions.status);
        return;
      case "warning":
        this.#leavesAfterItsTime(toast, dismiss, kind, this.#regions.alert);
        return;
      case "error":
        toast.className = "alis-toast alis-toast--error";
        dismiss.addEventListener("click", () => { toast.remove(); });
        this.#regions.alert.append(toast);
        return;
      default:
        throw new Error("A toast's kind is none of success, info, warning and error.");
    }
  }

  #leavesAfterItsTime(toast: HTMLElement, dismiss: HTMLButtonElement, kind: string, region: HTMLElement): void {
    toast.className = "alis-toast alis-toast--" + kind;
    const stops = leavesAfter(toast, this.#shownFor);
    dismiss.addEventListener("click", () => { stops(); toast.remove(); });
    region.append(toast);
  }
}

export function mountsToasts(): void {
  mountsEach(marker, HTMLElement, (toasts) => {
    const notice = new NativeToast(toasts);
    toasts.showPopover();
    providesPlugin("notice", notice);
  });
}
