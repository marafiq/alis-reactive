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
  const m = cmd.mutation;

  switch (m.kind) {
    case "set-prop": {
      const coerced = m.coerce ? coerce(val, m.coerce) : val;
      log.trace("set-prop", { target: cmd.target, prop: m.prop, val: coerced });
      (root as any)[m.prop] = coerced;
      break;
    }
    case "call-void": {
      const target = m.chain ? (root as any)[m.chain] : root;
      log.trace("call-void", { target: cmd.target, method: m.method });
      (target as any)[m.method].call(target);
      break;
    }
    case "call-val": {
      const target = m.chain ? (root as any)[m.chain] : root;
      log.trace("call-val", { target: cmd.target, method: m.method, val });
      (target as any)[m.method].call(target, val);
      break;
    }
    case "call-args": {
      const target = m.chain ? (root as any)[m.chain] : root;
      log.trace("call-args", { target: cmd.target, method: m.method, args: m.args });
      (target as any)[m.method].apply(target, m.args);
      break;
    }
  }
}
