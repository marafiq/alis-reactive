const aPlanScript = 'script[type="application/json"][data-reactive-plan]';

export function planScriptsWithin(root: ParentNode): HTMLScriptElement[] {
  return [...root.querySelectorAll<HTMLScriptElement>(aPlanScript)];
}

export function scriptsOtherThanPlansWithin(root: ParentNode): HTMLScriptElement[] {
  return [...root.querySelectorAll<HTMLScriptElement>("script:not(" + aPlanScript + ")")];
}
