import { scope } from "../../diagnostics/trace";

const log = scope("confirm");
const ELEMENT_ID = "alisConfirmDialog";

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

interface AlisWindow extends Window {
  alis?: {
    confirm?: (message: string) => Promise<boolean>;
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

export function init(): void {
  // App-level singleton rendered by @Html.FusionConfirmDialog() in Layout.
  // Not a plan component — getElementById is correct.
  const dialogHostElement = document.getElementById(ELEMENT_ID);
  if (!dialogHostElement) {
    log.warn("element.not-found", { id: ELEMENT_ID });
    return;
  }

  const hostWindow = window as unknown as SyncfusionWindow & AlisWindow;
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

  hostWindow.alis = hostWindow.alis || {};
  hostWindow.alis.confirm = function (message: string): Promise<boolean> {
    return new Promise<boolean>((outerResolve) => {
      queue = queue.then(() => showConfirmDialog(dialog, message, outerResolve));
    });
  };

  log.info("initialized", { id: ELEMENT_ID });
}
