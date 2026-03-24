import type { Command, MutateEventCommand, MethodArg, ExecContext } from "../types";
import { mutateElement } from "./element";
import { evaluateGuard, isConfirmGuard } from "../conditions/conditions";
import { showServerErrors } from "../validation";
import { injectHtml } from "./inject";
import { resolveSource, coerce } from "../resolution/resolver";
import { scope } from "../core/trace";
import { assertNever } from "../core/assert-never";

const log = scope("command");

function resolveMethodArg(arg: MethodArg, ctx?: ExecContext): unknown {
  switch (arg.kind) {
    case "literal": return arg.value;
    case "source": {
      const raw = resolveSource(arg.source, ctx);
      return arg.coerce ? coerce(raw, arg.coerce) : raw;
    }
    default: assertNever(arg, "method arg kind");
  }
}

function executeMutateEvent(cmd: MutateEventCommand, ctx: ExecContext): void {
  if (!ctx?.evt) throw new Error("[alis] mutate-event requires event context — was this command used outside an event handler?");
  const m = cmd.mutation;
  switch (m.kind) {
    case "set-prop": {
      const val = cmd.source ? resolveSource(cmd.source, ctx) : cmd.value;
      const coerced = m.coerce ? coerce(val, m.coerce) : val;
      log.trace("mutate-event", { prop: m.prop, val: coerced });
      (ctx.evt as any)[m.prop] = coerced;
      break;
    }
    case "call": {
      const resolved = (m.args ?? []).map(a => resolveMethodArg(a, ctx));
      log.trace("mutate-event", { method: m.method, args: resolved });
      (ctx.evt as any)[m.method](...resolved);
      break;
    }
    default: assertNever(m, "event mutation kind");
  }
}

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
