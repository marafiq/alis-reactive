import type { Command, ExecContext } from "../types";
import { mutateElement } from "./element";
import { evaluateGuard, isConfirmGuard } from "../conditions/conditions";
import { showServerErrors } from "../validation";
import { injectHtml } from "./inject";
import { scope } from "../core/trace";
import { assertNever } from "../core/assert-never";

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
      if (!ctx?.responseBody) break;
      if (!ctx.validationDesc) {
        throw new Error(
          `[alis] ValidationErrors("${cmd.formId}") requires a validation descriptor. ` +
          `Use .Validate<TValidator>(formId) on the request to attach one.`);
      }
      showServerErrors(ctx.validationDesc, ctx.responseBody);
      break;
    }

    case "into": {
      const container = document.getElementById(cmd.target);
      if (!container) throw new Error(`[alis] Into("${cmd.target}") target not found. Is the element rendered?`);
      if (ctx?.responseBody != null) {
        injectHtml(container, String(ctx.responseBody));
      }
      break;
    }

    default:
      assertNever(cmd, "command kind");
  }
}

/** Execute a list of commands. */
export function executeCommands(commands: Command[], ctx?: ExecContext): void {
  for (const cmd of commands) {
    executeCommand(cmd, ctx);
  }
}
