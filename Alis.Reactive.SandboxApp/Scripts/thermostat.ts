// A room thermostat: a widget the sandbox wrote itself, with no knowledge of Alis.Reactive. Its object
// lives on its host element as `thermostat`; BroughtComponents/Thermostat/Thermostat.cs declares it to
// the framework. It has every kind of member a JavaScript object can have:
//
//   properties  value (the target; read and written), mode (read and written), current (read only),
//               locked (read only), pin (written only), schedule.night (read and written),
//               schedule.active (read only)
//   methods     increase(), decrease(), hold(minutes), setRange(min, max), applyPlan(plan),
//               schedule.apply()
//   functions   isComfortable(), distanceTo(target), estimate(target, mode), describe(plan),
//               reading() (an object with value, unit and label()), schedule.nextChange()
//   events      unlocked (no payload); targetChanging (a payload the page reads, writes and calls:
//               requested, previous, cancel, reason, clamp(), accept()); targetChanged (value, previous).
//               Listeners are added with on(name, fn) and removed with off(name, fn).

import "./thermostat.css";

export type ThermostatMode = "heat" | "cool" | "off";

export interface NightSetting {
  readonly day: string;
  readonly target: number;
}

export interface ComfortPlan {
  readonly target: number;
  readonly mode: ThermostatMode;
  readonly nights: readonly NightSetting[];
}

/** What a targetChanging listener receives; cancel and reason are read back once the listeners ran. */
export interface TargetChange {
  readonly requested: number;
  readonly previous: number;
  cancel: boolean;
  reason: string;
  clamp(): number;
  accept(): void;
}

export interface Reading {
  readonly value: number;
  readonly unit: string;
  label(): string;
}

export interface Schedule {
  night: number;
  readonly active: boolean;
  apply(): void;
  nextChange(): string;
}

type Listener = (payload: unknown) => void;

export interface ThermostatHost extends HTMLElement {
  thermostat?: Thermostat;
}

export function thermostatHosts(root: HTMLElement): ThermostatHost[] {
  return Array.from(root.querySelectorAll<ThermostatHost>("div.thermostat"));
}

const MINUTES_PER_DEGREE: Record<ThermostatMode, number> = { heat: 4, cool: 3, off: 0 };

export class Thermostat {
  private readonly host: ThermostatHost;
  private readonly listeners = new Map<string, Set<Listener>>();
  private readonly pinCode: string;
  private readonly unit = "F";
  private readonly room: number;
  private target: number;
  private modeValue: ThermostatMode = "heat";
  private lockedValue: boolean;
  private min = 50;
  private max = 90;
  private status = "";
  private nightTarget = 0;
  private nightActive = false;

  /** The night schedule, an object of its own: night is read and written, active read, apply() and nextChange() called. */
  readonly schedule: Schedule;

  constructor(host: ThermostatHost) {
    this.host = host;
    this.target = Number(host.dataset.target ?? "70");
    this.room = Number(host.dataset.current ?? "68");
    this.pinCode = host.dataset.pin ?? "";
    this.lockedValue = host.dataset.locked === "true";
    this.schedule = this.createSchedule();
    host.addEventListener("click", this.onClick);
    host.thermostat = this;
    this.render();
  }

  get value(): number {
    return this.target;
  }

  /** Replaces the target without raising an event, as a form field's value would. */
  set value(target: number) {
    this.target = target;
    this.render();
  }

  get mode(): ThermostatMode {
    return this.modeValue;
  }

  set mode(mode: ThermostatMode) {
    this.modeValue = mode;
    this.render();
  }

  /** The room's measured temperature. */
  get current(): number {
    return this.room;
  }

  /** Whether the thermostat's own buttons are locked. */
  get locked(): boolean {
    return this.lockedValue;
  }

  /** Written only: the right code unlocks the thermostat and raises unlocked; a wrong one is reported. */
  set pin(code: string) {
    if (code !== this.pinCode) {
      this.status = "Wrong PIN";
      this.render();
      return;
    }
    this.lockedValue = false;
    this.status = "";
    this.render();
    this.emit("unlocked", undefined);
  }

  increase(): void {
    this.change(this.target + 1);
  }

  decrease(): void {
    this.change(this.target - 1);
  }

  /** Keeps the target for a while; the thermostat shows how long. */
  hold(minutes: number): void {
    this.status = `Held for ${minutes} min`;
    this.render();
  }

  setRange(min: number, max: number): void {
    this.min = min;
    this.max = max;
    this.render();
  }

  /** Applies a plan: its target and mode now, its first night setting for the night. */
  applyPlan(plan: ComfortPlan): void {
    const previous = this.target;
    this.modeValue = plan.mode;
    this.nightTarget = plan.nights[0]?.target ?? this.nightTarget;
    this.target = plan.target;
    this.status = "Plan applied";
    this.render();
    if (previous !== this.target) this.emit("targetChanged", { value: this.target, previous });
  }

  isComfortable(): boolean {
    return this.modeValue !== "off" && Math.abs(this.target - this.room) <= 1;
  }

  distanceTo(target: number): number {
    return Math.abs(target - this.room);
  }

  /** Minutes a mode takes to bring the room to a target. */
  estimate(target: number, mode: ThermostatMode): number {
    return Math.abs(target - this.room) * MINUTES_PER_DEGREE[mode];
  }

  describe(plan: ComfortPlan): string {
    const nights = plan.nights.length === 1 ? "1 night setting" : `${plan.nights.length} night settings`;
    return `${plan.mode} to ${plan.target}°${this.unit}, ${nights}`;
  }

  /** The room's reading as an object of its own. */
  reading(): Reading {
    const value = this.room;
    const unit = this.unit;
    return { value, unit, label: () => `${value}°${unit}` };
  }

  on(name: string, listener: Listener): void {
    let named = this.listeners.get(name);
    if (named === undefined) {
      named = new Set();
      this.listeners.set(name, named);
    }
    named.add(listener);
  }

  off(name: string, listener: Listener): void {
    this.listeners.get(name)?.delete(listener);
  }

  destroy(): void {
    this.host.removeEventListener("click", this.onClick);
    this.listeners.clear();
    this.host.replaceChildren();
    delete this.host.thermostat;
  }

  private createSchedule(): Schedule {
    const thermostat = this;
    return {
      get night(): number {
        return thermostat.nightTarget;
      },
      set night(target: number) {
        thermostat.nightTarget = target;
        thermostat.render();
      },
      get active(): boolean {
        return thermostat.nightActive;
      },
      apply(): void {
        if (thermostat.nightTarget === 0) return;
        thermostat.nightActive = true;
        thermostat.change(thermostat.nightTarget);
      },
      nextChange(): string {
        return thermostat.nightTarget === 0 ? "no night setting" : `${thermostat.nightTarget}°${thermostat.unit} at 9:00 PM`;
      },
    };
  }

  /** Changes the target: targetChanging listeners may refuse it; a change raises targetChanged. */
  private change(requested: number): void {
    const previous = this.target;
    let accepted = false;
    const change: TargetChange = {
      requested,
      previous,
      cancel: false,
      reason: "",
      clamp: () => Math.min(Math.max(requested, this.min), this.max),
      accept: () => {
        change.cancel = false;
        accepted = true;
      },
    };
    this.emit("targetChanging", change);
    if (change.cancel) {
      this.status = change.reason;
      this.render();
      return;
    }
    this.target = requested;
    this.status = accepted ? `Accepted ${requested}°${this.unit}` : "";
    this.render();
    this.emit("targetChanged", { value: this.target, previous });
  }

  private readonly onClick = (event: Event): void => {
    const button = (event.target as Element).closest<HTMLButtonElement>("button.th-up, button.th-down");
    if (button === null || this.lockedValue) return;
    if (button.classList.contains("th-up")) this.increase();
    else this.decrease();
  };

  private emit(name: string, payload: unknown): void {
    for (const listener of [...(this.listeners.get(name) ?? [])]) listener(payload);
  }

  private render(): void {
    const head = element("div", "th-head");
    head.append(element("strong", "th-target", `${this.target}°${this.unit}`), element("span", "th-mode", this.modeValue));

    const body = element("div", "th-body");
    body.append(
      element("span", "th-current", `Room ${this.room}°${this.unit}`),
      element("span", "th-range", `${this.min}–${this.max}°${this.unit}`),
      element("span", "th-night", this.nightTarget === 0 ? "No night setting" : `Night ${this.nightTarget}°${this.unit}`),
    );

    const controls = element("div", "th-controls");
    controls.append(
      this.button("th-down", "−", "Cooler"),
      this.button("th-up", "+", "Warmer"),
      element("span", "th-lock", this.lockedValue ? "Locked" : "Unlocked"),
    );

    this.host.replaceChildren(head, body, controls, element("p", "th-status", this.status));
  }

  private button(className: string, text: string, label: string): HTMLButtonElement {
    const button = element("button", className, text) as HTMLButtonElement;
    button.type = "button";
    button.setAttribute("aria-label", label);
    button.disabled = this.lockedValue;
    return button;
  }
}

function element(tag: string, className: string, text = ""): HTMLElement {
  const created = document.createElement(tag);
  created.className = className;
  created.textContent = text;
  return created;
}
