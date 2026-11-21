const days = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

function shown(at: Date): string {
  return days[at.getDay()] + " " + String(at.getHours()).padStart(2, "0") + ":" + String(at.getMinutes()).padStart(2, "0");
}

interface PlannedVisit {
  readonly resident: string;
  readonly at: Date;
}

class Suggestion implements PlannedVisit {
  constructor(
    readonly resident: string,
    readonly at: Date,
  ) {}
}

interface VisitPlannerEvents {
  readonly requested: CustomEvent<{ readonly at: string }>;
}

export interface VisitPlanner {
  addEventListener<K extends keyof VisitPlannerEvents>(type: K, listener: (event: VisitPlannerEvents[K]) => void, options?: AddEventListenerOptions | boolean): void;
  addEventListener(type: string, listener: EventListenerOrEventListenerObject | null, options?: AddEventListenerOptions | boolean): void;
}

export class VisitPlanner extends EventTarget {
  booked = "nothing booked";
  next = new Date(2026, 10, 1, 9, 0, 0);

  get nextDay(): string {
    return shown(this.next);
  }

  book(at: Date): void {
    this.booked = shown(at);
  }

  bookVisit(visit: PlannedVisit): void {
    this.booked = visit.resident + ", " + shown(visit.at);
  }

  bookAll(times: readonly Date[]): void {
    this.booked = times.map(shown).join(" and ");
  }

  bookAt(moment: Date): void {
    this.booked = moment.toISOString();
  }

  reschedule(at: Date | null): void {
    this.booked = at === null ? "unscheduled" : shown(at);
  }

  suggest(): PlannedVisit {
    return new Suggestion("Sam Park", new Date(2026, 10, 8, 16, 45, 0));
  }

  request(): void {
    this.dispatchEvent(new CustomEvent("requested", { detail: { at: "2026-11-10T10:00:00" } }));
  }
}
