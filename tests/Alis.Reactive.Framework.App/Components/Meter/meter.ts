class Meter extends HTMLElement {
  private value = 0;
  private readonly levelLine = document.createElement("span");

  connectedCallback(): void {
    if (this.levelLine.isConnected) return;
    this.value = Number(this.getAttribute("level") ?? "0");
    const down = this.stepButton("Lower", -1);
    const up = this.stepButton("Raise", 1);
    const reset = document.createElement("button");
    reset.type = "button";
    reset.textContent = "Reset";
    reset.addEventListener("click", () => this.reset());
    this.replaceChildren(this.levelLine, down, up, reset);
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
    this.dispatchEvent(new Event("change"));
  }

  private reset(): void {
    const allowed = this.dispatchEvent(new CustomEvent("resetting", { cancelable: true }));
    if (!allowed) return;
    this.value = 0;
    this.show();
  }

  private show(): void {
    this.levelLine.textContent = `Level ${this.value} of ${this.max}`;
  }
}

customElements.define("fw-meter", Meter);
