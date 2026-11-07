export interface MealTrayHost extends HTMLElement {
  tray?: MealTray;
}

export type TrayBuild = "current" | "older" | "unset";

interface Delivery {
  readonly room: string;
  readonly note?: string;
}

interface Dish {
  readonly name: string;
}

type DeliveryListener = (delivery: Delivery) => void;

export class TrayCover {
  readonly label = "Cloche";
}

export class TraySide {
  readonly name: string;

  constructor(name: string) {
    this.name = name;
  }
}

export class TrayLid {
  readonly label: string;

  constructor(label: string) {
    this.label = label;
  }
}

export class TrayServer {
  readonly badge?: string;

  constructor(badge: string) {
    this.badge = badge;
  }
}

const optionalMembers = ["portion", "slip", "cover", "sides", "warm", "lastServed", "reheat", "temperature", "lid", "extras", "garnish"] as const;

export function trayBuildOf(host: HTMLElement): TrayBuild {
  const build = host.dataset["mealTray"];
  if (build === "current" || build === "older" || build === "unset") return build;
  throw new Error("A meal tray's build is current, older or unset, and this one is '" + String(build) + "'.");
}

export class MealTray {
  declare portion?: string;
  declare slip?: string;
  declare readonly cover?: TrayCover;
  declare readonly sides?: readonly TraySide[];
  readonly server: TrayServer | null;
  readonly relief: TrayServer | null;
  readonly sideOrders: readonly TraySide[] | null;
  declare warm?: () => void;
  declare lastServed?: () => string;
  declare reheat?: () => Promise<void>;
  declare temperature?: () => Promise<string>;
  declare lid?: () => TrayLid;
  declare extras?: () => readonly TraySide[];
  declare garnish?: (dish: Dish) => string;
  readonly #build: TrayBuild;
  readonly #listeners = new Set<DeliveryListener>();
  readonly #state: HTMLParagraphElement;
  readonly #portioned: HTMLParagraphElement;
  readonly #printed: HTMLParagraphElement;
  readonly #plated: HTMLParagraphElement;

  constructor(host: HTMLElement, build: TrayBuild) {
    this.#build = build;
    this.#state = lineOf("tray-state", "cold");
    this.#portioned = lineOf("tray-portion", "unshown");
    this.#printed = lineOf("tray-slip", "unprinted");
    this.#plated = lineOf("tray-plated", "unplated");
    this.server = build === "older" ? null : new TrayServer("Ines");
    this.relief = build === "older" ? null : new TrayServer("Omar");
    this.sideOrders = build === "older" ? null : [new TraySide("Soup")];
    if (build === "current") {
      this.portion = "Regular";
      this.slip = "Standard";
      this.garnish = () => "Parsley";
      this.cover = new TrayCover();
      this.sides = [new TraySide("Rice"), new TraySide("Salad")];
      this.warm = () => this.#show("warmed");
      this.lastServed = () => "12:30";
      this.reheat = () => Promise.resolve().then(() => this.#show("reheated"));
      this.temperature = () => Promise.resolve("68°F");
      this.lid = () => new TrayLid("Steel lid");
      this.extras = () => [new TraySide("Bread")];
    }
    if (build === "unset") holdAsClassFieldsDo(this);
    const delivery: Delivery = build === "older" ? { room: "8" } : { room: "8", note: "No salt" };
    host.replaceChildren(
      this.#state,
      this.#portioned,
      this.#printed,
      this.#plated,
      buttonOf("Deliver the tray", () => this.#listeners.forEach((listener) => listener(delivery))),
      buttonOf("Show the portion", () => (this.#portioned.textContent = this.portion ?? "no portion")),
      buttonOf("Print the slip", () => (this.#printed.textContent = this.slip ?? "no slip")),
      buttonOf("Plate the dish", () => (this.#plated.textContent = this.garnish?.({ name: "Soup" }) ?? "no garnish")),
    );
  }

  spareLid(): TrayLid | null {
    return this.#build === "older" ? null : new TrayLid("Glass lid");
  }

  spareSides(): readonly TraySide[] | null {
    return this.#build === "older" ? null : [new TraySide("Fruit")];
  }

  on(event: "delivered", listener: DeliveryListener): void {
    if (event === "delivered") this.#listeners.add(listener);
  }

  off(event: "delivered", listener: DeliveryListener): void {
    if (event === "delivered") this.#listeners.delete(listener);
  }

  #show(state: string): void {
    this.#state.textContent = state;
  }
}

function holdAsClassFieldsDo(tray: MealTray): void {
  for (const member of optionalMembers) Object.defineProperty(tray, member, { value: undefined, writable: true, enumerable: true, configurable: true });
}

function buttonOf(text: string, clicked: () => void): HTMLButtonElement {
  const button = document.createElement("button");
  button.type = "button";
  button.textContent = text;
  button.addEventListener("click", clicked);
  return button;
}

function lineOf(className: string, text: string): HTMLParagraphElement {
  const line = document.createElement("p");
  line.className = className;
  line.textContent = text;
  return line;
}
