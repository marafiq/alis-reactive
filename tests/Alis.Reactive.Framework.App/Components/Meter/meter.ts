// The meter: a custom element with a level and a maximum, stepped by its own buttons. It knows nothing
// of Alis.Reactive. The element is the object; a change is a DOM event whose detail says what changed.

class Meter extends HTMLElement {
  private value = 0;
  private readonly levelLine = document.createElement("span");

  connectedCallback(): void {
    if (this.levelLine.isConnected) return;
    this.value = Number(this.getAttribute("level") ?? "0");
    const down = this.stepButton("Lower", -1);
    const up = this.stepButton("Raise", 1);
    this.replaceChildren(this.levelLine, down, up);
    this.show();
  }

  get level(): number {
    return this.value;
  }

  set level(value: number) {
    this.value = value;
    this.show();
  }

  get max(): number {
    return Number(this.getAttribute("max") ?? "0");
  }

  private stepButton(label: string, delta: number): HTMLButtonElement {
    const button = document.createElement("button");
    button.type = "button";
    button.textContent = label;
    button.addEventListener("click", () => this.step(delta));
    return button;
  }

  private step(delta: number): void {
    const previous = this.value;
    this.value = Math.min(this.max, Math.max(0, previous + delta));
    this.show();
    this.dispatchEvent(new CustomEvent("levelChanged", { detail: { level: this.value, previous } }));
  }

  private show(): void {
    this.levelLine.textContent = `Level ${this.value} of ${this.max}`;
  }
}

customElements.define("fw-meter", Meter);
