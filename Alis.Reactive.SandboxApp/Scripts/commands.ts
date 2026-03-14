import type { Command, ExecContext } from "./types";
import { mutateElement } from "./element";
import { evaluateGuard, isConfirmGuard } from "./conditions";
import { showServerErrors } from "./validation";
import { injectHtml } from "./inject";
import { scope } from "./trace";

const log = scope("command");

/** Execute a single command with per-action When guard support. */
export function executeCommand(cmd: Command, ctx?: ExecContext): void {
  if (cmd.when) {
    if (isConfirmGuard(cmd.when)) {
      log.warn("ConfirmGuard on per-action When is not supported. Use branch-level Confirm.");
      return;
    }
    if (!evaluateGuard(cmd.when, ctx)) {
      log.trace("per-action-when-skipped", { kind: cmd.kind });
      return;
    }
  }

  switch (cmd.kind) {
    case "dispatch":
      log.trace("dispatch", { event: cmd.event, payload: cmd.payload });
      document.dispatchEvent(new CustomEvent(cmd.event, { detail: cmd.payload ?? {} }));
      break;

    case "mutate-element":
      log.trace("mutate-element", { target: cmd.target, mutation: cmd.mutation.kind });
      mutateElement(cmd, ctx);
      break;

    case "validation-errors": {
      if (ctx?.responseBody) {
        // Server-only validation: formId from command, no client fields needed.
        // showServerErrors uses data-valmsg-for spans, not the field list.
        const desc = ctx.validationDesc ?? { formId: cmd.formId, fields: [] };
        showServerErrors(desc, ctx.responseBody);
      }
      break;
    }

    case "into": {
      const container = document.getElementById(cmd.target);
      if (container && ctx?.responseBody != null) {
        injectHtml(container, String(ctx.responseBody));
      }
      break;
    }
  }
}

/** Execute a list of commands. */
export function executeCommands(commands: Command[], ctx?: ExecContext): void {
  for (const cmd of commands) {
    executeCommand(cmd, ctx);
  }
}
