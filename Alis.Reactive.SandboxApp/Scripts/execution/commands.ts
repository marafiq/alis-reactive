import type { Command, MutateEventCommand, ExecContext } from "../types";
import { mutateElement } from "./element";
import { showServerErrors } from "../validation";
import { injectHtml } from "./inject";
import { scope } from "../core/trace";
import { assertNever } from "../core/assert-never";
import { resolveCommandValue, resolveDispatchPayload } from "./values";

const log = scope("command");

function executeMutateEvent(cmd: MutateEventCommand, ctx: ExecContext): void {
  if (!ctx?.evt) throw new Error("[alis] mutate-event requires event context — was this command used outside an event handler?");
  const m = cmd.mutation;
  switch (m.kind) {
    case "set-prop": {
      const value = resolveCommandValue(m.value, ctx);
      log.trace("mutate-event", { prop: m.prop, val: value });
      (ctx.evt as any)[m.prop] = value;
      break;
    }
    case "call": {
      const resolved = (m.args ?? []).map(a => resolveCommandValue(a, ctx));
      log.trace("mutate-event", { method: m.method, args: resolved });
      (ctx.evt as any)[m.method](...resolved);
      break;
    }
    default: assertNever(m, "event mutation kind");
  }
}

/** Execute a single command. */
export function executeCommand(cmd: Command, ctx?: ExecContext): void {
  switch (cmd.kind) {
    case "dispatch": {
      const detail = resolveDispatchPayload(cmd.payload, ctx);
      log.trace("dispatch", { event: cmd.event, payload: detail });
      document.dispatchEvent(new CustomEvent(cmd.event, { detail }));
      break;
    }

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

    case "mutate-event":
      executeMutateEvent(cmd, ctx!);
      break;

    case "into": {
      const container = document.getElementById(cmd.target);
      if (!container) throw new Error(`[alis] Into("${cmd.target}") target not found. Is the element rendered?`);
      if (ctx?.responseBody != null) {
        if (typeof ctx.responseBody !== "string") {
          throw new Error(
            `[alis] Into("${cmd.target}") received ${typeof ctx.responseBody} body. ` +
            `Into expects text/html responses. Use a different handler for JSON.`
          );
        }
        injectHtml(container, ctx.responseBody);
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
