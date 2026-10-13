// alis.confirm: the app-level confirmation the core's p.Confirm(..) asks (grammar.md 6.8). The Fusion
// package implements it with a Syncfusion Dialog rendered by @Html.FusionConfirmDialog(), made when
// the dialog's element mounts; the plan locates it as a plugin.
// C1 ask(message) resolves true on OK, false on Cancel, close or Escape.
// C2 Asks queue first in, first out.
// C3 Escape inside the dialog never reaches the document.
// C4 It answers only on a page that renders the dialog. A page without it may give alis.confirm from
//    its own code, and the core asks that one; a page with neither has no alis.confirm to ask.

import { onMount, providePlugin, whenParsed } from "../hooks";

const dialogId = "alisConfirmDialog";

let queue = Promise.resolve();

interface SyncfusionDialogButton {
  readonly click: () => void;
  readonly buttonModel: {
    readonly content: string;
    readonly isPrimary?: boolean;
    readonly cssClass?: string;
  };
}

interface SyncfusionDialog {
  header: string;
  content: string;
  buttons: SyncfusionDialogButton[];
  close: (() => void) | null;
  appendTo(element: HTMLElement): void;
  show(): void;
  hide(): void;
}

interface SyncfusionDialogOptions {
  readonly isModal: boolean;
  readonly visible: boolean;
  readonly width: string;
  readonly animationSettings: { readonly effect: string };
  readonly showCloseIcon: boolean;
  readonly closeOnEscape: boolean;
  readonly target: HTMLElement;
}

interface SyncfusionWindow extends Window {
  readonly ej: {
    readonly popups: {
      readonly Dialog: new (options: SyncfusionDialogOptions) => SyncfusionDialog;
    };
  };
}

function showConfirmDialog(
  dialog: SyncfusionDialog,
  message: string,
  outerResolve: (value: boolean) => void
): Promise<void> {
  return new Promise<void>((resolve) => {
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
  });
}

// The confirm is modal: an Escape pressed in it is its own and never reaches the page beneath, where
// an open drawer would close on it too. The dialog still closes on it, unless another popup is open.
function keepEscapeInConfirm(event: KeyboardEvent): void {
  if (event.key === "Escape") event.stopPropagation();
}

let ask: ((message: string) => Promise<boolean>) | undefined;

// App-level singleton rendered by @Html.FusionConfirmDialog() in the layout; fixed id.
onMount(`#${dialogId}`, dialogHostElement => {
  const hostWindow = window as unknown as SyncfusionWindow;
  const dialog = new hostWindow.ej.popups.Dialog({
    isModal: true,
    visible: false,
    width: "400px",
    animationSettings: { effect: "None" },
    showCloseIcon: false,
    closeOnEscape: true,
    target: document.body,
  });
  dialog.appendTo(dialogHostElement);
  dialogHostElement.addEventListener("keydown", keepEscapeInConfirm);

  ask = (message: string) => new Promise<boolean>(outerResolve => {
    queue = queue.then(() => showConfirmDialog(dialog, message, outerResolve));
  });
});

whenParsed(() => {
  if (document.getElementById(dialogId) === null) return;
  providePlugin("alis.confirm", {
    ask(message: string): Promise<boolean> {
      if (!ask) throw new Error(`[alis] #${dialogId} is on the page but its Syncfusion Dialog was not made`);
      return ask(message);
    },
  });
});
