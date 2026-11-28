import { mountsEach, providesPlugin } from "../../NativeComponents/mounted";

const marker = "data-native-loader";

const elementsThatHoldNoChild = new Set(["AREA", "BASE", "BR", "COL", "EMBED", "HR", "IMG", "INPUT", "LINK", "META", "SOURCE", "TRACK", "WBR"]);

type Shown = { readonly kind: "overlay"; readonly overlay: HTMLElement } | { readonly kind: "childless" };

function targetOf(id: string): HTMLElement {
  const target = document.getElementById(id);
  if (!(target instanceof HTMLElement)) throw new Error("The busy target '" + id + "' is not on the page.");
  return target;
}

function overlayOn(target: HTMLElement): Shown {
  if (elementsThatHoldNoChild.has(target.tagName)) return { kind: "childless" };
  const overlay = document.createElement("span");
  overlay.id = "alis-loader--" + target.id;
  overlay.className = "alis-loader alis-loader--target";
  overlay.setAttribute("aria-hidden", "true");
  target.append(overlay);
  return { kind: "overlay", overlay };
}

class NativeLoader {
  readonly #indicator: HTMLElement;
  readonly #shown = new Map<string, Shown>();

  constructor(indicator: HTMLElement) {
    this.#indicator = indicator;
  }

  show(target?: string): void {
    if (target === undefined) {
      this.#indicator.hidden = false;
      return;
    }
    this.#shown.set(target, overlayOn(targetOf(target)));
  }

  hide(target?: string): void {
    if (target === undefined) {
      this.#indicator.hidden = true;
      return;
    }
    const shown = this.#shown.get(target);
    if (shown === undefined) throw new Error("The busy target '" + target + "' is hidden but was never shown.");
    this.#shown.delete(target);
    if (shown.kind === "overlay" && shown.overlay.isConnected) shown.overlay.remove();
  }
}

export function mountsLoaders(): void {
  mountsEach(marker, HTMLElement, (indicator) => {
    providesPlugin("busy", new NativeLoader(indicator));
  });
}
