import { scope } from "./trace";

const log = scope("confirm");
const ELEMENT_ID = "alisConfirmDialog";

let queue = Promise.resolve();

/**
 * Initializes the SF Dialog on the confirm element and wires window.alis.confirm().
 * Called once from auto-boot before any plans are processed.
 * The element must exist in the DOM (rendered by @Html.FusionConfirmDialog() in Layout).
 */
export function init(): void {
  const el = document.getElementById(ELEMENT_ID);
  if (!el) {
    log.warn("confirm element not found", { id: ELEMENT_ID });
    return;
  }

  const dialog = new (window as any).ej.popups.Dialog({
    isModal: true,
    visible: false,
    width: "400px",
    animationSettings: { effect: "None" },
    showCloseIcon: false,
    closeOnEscape: true,
    target: document.body,
  });
  dialog.appendTo(el);

  (window as any).alis = (window as any).alis || {};
  (window as any).alis.confirm = function (message: string): Promise<boolean> {
    return new Promise<boolean>((outerResolve) => {
      queue = queue.then(
        () =>
          new Promise<void>((resolve) => {
            dialog.header = "Confirm";
            dialog.content = message;
            dialog.buttons = [
              {
                click: () => { dialog.close = null; dialog.hide(); resolve(); outerResolve(true); },
                buttonModel: { content: "OK", isPrimary: true, cssClass: "e-primary" },
              },
              {
                click: () => { dialog.close = null; dialog.hide(); resolve(); outerResolve(false); },
                buttonModel: { content: "Cancel" },
              },
            ];
            dialog.close = () => { resolve(); outerResolve(false); };
            dialog.show();
          })
      );
    });
  };

  log.info("initialized");
}
