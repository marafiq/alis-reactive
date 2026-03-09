import type { Command, ExecContext } from "./types";
import { showFieldErrors } from "./forms";
import { scope } from "./trace";

const log = scope("commands");

/** Execute a list of commands synchronously — shared by pipeline.ts and http.ts. */
export function executeCommands(commands: Command[], ctx?: ExecContext): void {
  for (const cmd of commands) {
    switch (cmd.kind) {
      case "dispatch":
        log.trace("dispatch", { event: cmd.event });
        document.dispatchEvent(new CustomEvent(cmd.event, { detail: cmd.payload ?? {} }));
        break;
      case "mutate-element": {
        const el = document.getElementById(cmd.target);
        if (!el) break;
        const val = cmd.value;
        new Function("el", "val", cmd.jsEmit)(el, val);
        break;
      }
      case "validation-errors": {
        if (ctx?.responseBody) {
          showFieldErrors(cmd.formId, ctx.responseBody);
        }
        break;
      }
    }
  }
}
