import type { MutateElementCommand, ExecContext } from "./types";
import { scope } from "./trace";
import { resolveSource } from "./resolver";
import { resolveRoot } from "./component";

const log = scope("element");

export function mutateElement(cmd: MutateElementCommand, ctx?: ExecContext): void {
  const domEl = document.getElementById(cmd.target);
  if (!domEl) throw new Error(`[alis] target not found: ${cmd.target}`);

  const el = cmd.vendor ? resolveRoot(domEl, cmd.vendor) : domEl;
  const val = cmd.source ? resolveSource(cmd.source, ctx) : cmd.value;
  log.trace("exec", { target: cmd.target, jsEmit: cmd.jsEmit, val });
  new Function("el", "val", cmd.jsEmit).call(null, el, val);
}
