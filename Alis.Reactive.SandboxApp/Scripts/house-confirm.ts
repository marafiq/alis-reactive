// The community's own confirmation, which a page renders in place of the framework's Syncfusion one:
// a <dialog id="house-confirm"> whose form closes it with the button pressed. It answers the plan's
// p.Confirm(..) under the name the core asks, alis.confirm. A page renders one confirm or the other;
// with both, the runtime refuses the second alis.confirm when it boots.

export {};

interface PendingHostPlugin {
  readonly name: string;
  readonly instance: object;
}

interface PluginQueueWindow extends Window {
  __alisPlugins?: PendingHostPlugin[];
}

const yes = "yes";

// The runtime takes plugins when the document is parsed; this script runs before it, so it looks for
// the dialog then too.
function whenParsed(act: () => void): void {
  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", act, { once: true });
  else act();
}

function ask(dialog: HTMLDialogElement, message: HTMLElement, question: string): Promise<boolean> {
  message.textContent = question;
  dialog.returnValue = "";
  dialog.showModal();
  return new Promise(resolve => {
    dialog.addEventListener("close", () => resolve(dialog.returnValue === yes), { once: true });
  });
}

whenParsed(() => {
  const dialog = document.getElementById("house-confirm");
  const message = document.getElementById("house-confirm-message");
  if (!(dialog instanceof HTMLDialogElement) || message === null) return;

  const host = window as PluginQueueWindow;
  (host.__alisPlugins ??= []).push({
    name: "alis.confirm",
    instance: { ask: (question: string) => ask(dialog, message, question) },
  });
});
