import type { MutateElementCommand, ExecContext } from "../types";
import { scope } from "../core/trace";
import { resolveRoot } from "../resolution/component";
import { assertNever } from "../core/assert-never";
import { resolveCommandValue } from "./values";

const log = scope("element");

export function mutateElement(cmd: MutateElementCommand, ctx?: ExecContext): void {
  const domEl = document.getElementById(cmd.target);
  if (!domEl) throw new Error(`[alis] target not found: ${cmd.target}`);

  const root = cmd.vendor ? resolveRoot(domEl, cmd.vendor) : domEl;
  const m = cmd.mutation;

  switch (m.kind) {
    case "set-prop": {
      const value = resolveCommandValue(m.value, ctx);
      log.trace("set-prop", { target: cmd.target, prop: m.prop, val: value });
      (root as any)[m.prop] = value;
      break;
    }
    case "call": {
      const target = m.chain ? (root as any)[m.chain] : root;
      const resolved = (m.args ?? []).map(a => resolveCommandValue(a, ctx));
      log.trace("call", { target: cmd.target, method: m.method, args: resolved });
      (target as any)[m.method].apply(target, resolved);
      break;
    }

    default:
      assertNever(m, "mutation kind");
  }
}
