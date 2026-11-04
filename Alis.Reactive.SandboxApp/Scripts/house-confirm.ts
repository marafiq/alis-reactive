export {};

interface PendingHostPlugin {
  readonly name: string;
  readonly instance: object;
}

interface PluginQueueWindow extends Window {
  __alisPlugins?: PendingHostPlugin[];
}

const yes = "yes";

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
