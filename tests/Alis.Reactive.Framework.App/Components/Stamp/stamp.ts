// The stamp: a widget its own script builds where the script is written, as a vendor's render helper writes
// the script that constructs its widget after the markup. It knows nothing of Alis.Reactive; the script
// names the stamp's element and its text in its own data attributes.

function build(script: HTMLOrSVGScriptElement | null): void {
  if (!(script instanceof HTMLScriptElement)) throw new Error("The stamp is built only by its own script.");
  const id = script.dataset["stampFor"];
  const stamp = id === undefined ? null : document.getElementById(id);
  if (stamp === null) throw new Error(`The stamp's script names no element on the page: ${id ?? "none"}.`);
  stamp.textContent = script.dataset["stampText"] ?? "";
}

(window as unknown as { frameworkStamp: { build: typeof build } }).frameworkStamp = { build };
