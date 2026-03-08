import type { MutateElementCommand, ExecContext } from "./types";
import { scope } from "./trace";
import { resolveToString } from "./resolver";

const log = scope("element");

const fnCache = new Map<string, (el: HTMLElement, val: unknown) => void>();

function compile(jsEmit: string): (el: HTMLElement, val: unknown) => void {
  let fn = fnCache.get(jsEmit);
  if (!fn) {
    fn = new Function("el", "val", jsEmit) as (el: HTMLElement, val: unknown) => void;
    fnCache.set(jsEmit, fn);
  }
  return fn;
}

export function mutateElement(cmd: MutateElementCommand, ctx?: ExecContext): void {
  const el = document.getElementById(cmd.target);
  if (!el) {
    log.warn("target not found", { target: cmd.target });
    return;
  }

  const val = cmd.source ? resolveToString(cmd.source, ctx) : cmd.value;
  log.trace("exec", { target: cmd.target, jsEmit: cmd.jsEmit, val });
  compile(cmd.jsEmit)(el, val);
}
