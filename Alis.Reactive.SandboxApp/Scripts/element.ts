import type { MutateElementCommand, ExecContext } from "./types";
import { scope } from "./trace";
import { resolveToString } from "./resolver";

const log = scope("element");

export function mutateElement(cmd: MutateElementCommand, ctx?: ExecContext): void {
  const el = document.getElementById(cmd.target);
  if (!el) {
    log.warn("target not found", { target: cmd.target });
    return;
  }

  switch (cmd.action) {
    case "add-class":
      if (cmd.value) el.classList.add(cmd.value);
      break;
    case "remove-class":
      if (cmd.value) el.classList.remove(cmd.value);
      break;
    case "toggle-class":
      if (cmd.value) el.classList.toggle(cmd.value);
      break;
    case "set-text":
      el.textContent = cmd.source ? resolveToString(cmd.source, ctx) : (cmd.value ?? "");
      break;
    case "set-html":
      el.innerHTML = cmd.source ? resolveToString(cmd.source, ctx) : (cmd.value ?? "");
      break;
    case "show":
      el.removeAttribute("hidden");
      break;
    case "hide":
      el.setAttribute("hidden", "");
      break;
  }
}
