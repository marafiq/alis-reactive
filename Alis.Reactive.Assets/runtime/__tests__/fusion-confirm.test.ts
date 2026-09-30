import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { init } from "../components/fusion/confirm";

// Stands in for the vendor dialog, which takes over the host element rendered by the layout.
class DialogStandIn {
  header = "";
  content = "";
  buttons: unknown[] = [];
  close: (() => void) | null = null;
  appendTo(): void {}
  show(): void {}
  hide(): void {}
}

let pageListeners: AbortController;
let keysThePageSaw: string[];
let keysTheDialogSaw: string[];

function pressInConfirm(key: string): void {
  const okButton = document.getElementById("confirm-ok") as HTMLElement;
  okButton.dispatchEvent(new KeyboardEvent("keydown", { key, bubbles: true }));
}

beforeEach(() => {
  document.body.innerHTML = `<div id="alisConfirmDialog"><button id="confirm-ok">OK</button></div>`;
  vi.stubGlobal("ej", { popups: { Dialog: DialogStandIn } });
  init();

  keysThePageSaw = [];
  keysTheDialogSaw = [];
  pageListeners = new AbortController();
  document.addEventListener("keydown", event => keysThePageSaw.push(event.key), { signal: pageListeners.signal });
  // The vendor dialog binds its own keydown handler to the host when it shows, after init.
  document.getElementById("alisConfirmDialog")!
    .addEventListener("keydown", event => keysTheDialogSaw.push(event.key), { signal: pageListeners.signal });
});

afterEach(() => {
  pageListeners.abort();
  vi.unstubAllGlobals();
  document.body.innerHTML = "";
});

describe("confirm dialog keyboard", () => {
  it("keeps an Escape pressed in the confirm from reaching the page beneath it", () => {
    pressInConfirm("Escape");

    expect(keysTheDialogSaw).toEqual(["Escape"]);
    expect(keysThePageSaw).toEqual([]);
  });

  it("lets every other key pressed in the confirm through", () => {
    pressInConfirm("Enter");

    expect(keysTheDialogSaw).toEqual(["Enter"]);
    expect(keysThePageSaw).toEqual(["Enter"]);
  });
});
