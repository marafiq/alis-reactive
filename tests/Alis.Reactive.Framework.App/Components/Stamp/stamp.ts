function build(script: HTMLOrSVGScriptElement | null): void {
  if (!(script instanceof HTMLScriptElement)) throw new Error("The stamp is built only by its own script.");
  const id = script.dataset["stampFor"];
  const stamp = id === undefined ? null : document.getElementById(id);
  if (stamp === null) throw new Error(`The stamp's script names no element on the page: ${id ?? "none"}.`);
  stamp.textContent = script.dataset["stampText"] ?? "";
}

(window as unknown as { frameworkStamp: { build: typeof build } }).frameworkStamp = { build };
