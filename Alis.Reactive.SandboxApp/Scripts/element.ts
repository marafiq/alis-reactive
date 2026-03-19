import type { MutateElementCommand, MethodArg, ExecContext } from "./types";
import { scope } from "./core/trace";
import { resolveSource, coerce } from "./resolver";
import { resolveRoot } from "./component";
import { assertNever } from "./core/assert-never";

const log = scope("element");

function resolveArg(arg: MethodArg, ctx?: ExecContext): unknown {
  switch (arg.kind) {
    case "literal": return arg.value;
    case "source": {
      const raw = resolveSource(arg.source, ctx);
      return arg.coerce ? coerce(raw, arg.coerce) : raw;
    }
    default: assertNever(arg, "method arg kind");
  }
}

export function mutateElement(cmd: MutateElementCommand, ctx?: ExecContext): void {
  const domEl = document.getElementById(cmd.target);
  if (!domEl) throw new Error(`[alis] target not found: ${cmd.target}`);

  const root = cmd.vendor ? resolveRoot(domEl, cmd.vendor) : domEl;
  const m = cmd.mutation;

  switch (m.kind) {
    case "set-prop": {
      const val = cmd.source ? resolveSource(cmd.source, ctx) : cmd.value;
      const coerced = m.coerce ? coerce(val, m.coerce) : val;
      log.trace("set-prop", { target: cmd.target, prop: m.prop, val: coerced });
      (root as any)[m.prop] = coerced;
      break;
    }
    case "call": {
      const target = m.chain ? (root as any)[m.chain] : root;
      const resolved = (m.args ?? []).map(a => resolveArg(a, ctx));
      log.trace("call", { target: cmd.target, method: m.method, args: resolved });
      (target as any)[m.method].apply(target, resolved);
      break;
    }

    default:
      assertNever(m, "mutation kind");
  }
}
