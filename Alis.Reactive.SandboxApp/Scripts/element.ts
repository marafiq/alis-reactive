import type { MutateElementCommand, ExecContext } from "./types";
import { scope } from "./trace";
import { resolveSource, coerce } from "./resolver";
import { resolveRoot } from "./component";

const log = scope("element");

export function mutateElement(cmd: MutateElementCommand, ctx?: ExecContext): void {
  const domEl = document.getElementById(cmd.target);
  if (!domEl) throw new Error(`[alis] target not found: ${cmd.target}`);

  const root = cmd.vendor ? resolveRoot(domEl, cmd.vendor) : domEl;
  const val = cmd.source ? resolveSource(cmd.source, ctx) : cmd.value;

  if (cmd.prop) {
    const coerced = cmd.coerce ? coerce(val, cmd.coerce) : val;
    log.trace("prop", { target: cmd.target, prop: cmd.prop, val: coerced });
    (root as any)[cmd.prop] = coerced;
    return;
  }

  if (cmd.method) {
    const target = cmd.chain ? (root as any)[cmd.chain] : root;
    const fn = (target as any)[cmd.method];
    if (cmd.args) {
      log.trace("method", { target: cmd.target, method: cmd.method, args: cmd.args });
      fn.apply(target, cmd.args);
    } else if (val !== undefined) {
      log.trace("method", { target: cmd.target, method: cmd.method, val });
      fn.call(target, val);
    } else {
      log.trace("method", { target: cmd.target, method: cmd.method });
      fn.call(target);
    }
    return;
  }
}
