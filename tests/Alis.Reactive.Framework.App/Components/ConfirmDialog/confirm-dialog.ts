// The app's own confirmation dialog: the question a page's p.Confirm(..) asks, with a Yes and a No.
// It knows nothing of Alis.Reactive beyond the plugin name a page's confirmation is asked through,
// alis.confirm, which an app without the Fusion dialog gives from its own code. It queues itself on
// window.__alisPlugins for the runtime to register at boot.

interface PendingPlugin {
  readonly name: string;
  readonly instance: object;
}

interface PluginQueue {
  __alisPlugins?: PendingPlugin[];
}

let asked: Promise<unknown> = Promise.resolve();

function dialogAnswering(message: string, answer: (agreed: boolean) => void): HTMLDivElement {
  const dialog = document.createElement("div");
  dialog.setAttribute("role", "dialog");
  dialog.setAttribute("aria-label", "Confirm");
  const question = document.createElement("p");
  question.textContent = message;
  const choice = (text: string, agreed: boolean): HTMLButtonElement => {
    const button = document.createElement("button");
    button.type = "button";
    button.textContent = text;
    button.addEventListener("click", () => {
      dialog.remove();
      answer(agreed);
    });
    return button;
  };
  dialog.replaceChildren(question, choice("Yes", true), choice("No", false));
  return dialog;
}

export const confirmDialog = {
  /** Shows the question and settles with the answer: true for Yes, false for No. Questions wait their turn. */
  ask(message: string): Promise<boolean> {
    const answered = asked.then(() => new Promise<boolean>(answer => {
      document.body.append(dialogAnswering(message, answer));
    }));
    asked = answered;
    return answered;
  },
};

const queue = window as unknown as PluginQueue;
(queue.__alisPlugins ??= []).push({ name: "alis.confirm", instance: confirmDialog });
