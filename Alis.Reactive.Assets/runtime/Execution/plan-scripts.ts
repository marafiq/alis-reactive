export function planScriptsWithin(root: ParentNode): HTMLScriptElement[] {
  return [...root.querySelectorAll<HTMLScriptElement>('script[type="application/json"][data-reactive-plan]')];
}
