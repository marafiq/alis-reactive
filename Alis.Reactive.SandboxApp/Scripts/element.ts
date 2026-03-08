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

  const val = cmd.source ? resolveToString(cmd.source, ctx) : cmd.value;
  log.trace("exec", { target: cmd.target, jsEmit: cmd.jsEmit, val });
  new Function("el", "val", cmd.jsEmit)(el, val);
}
