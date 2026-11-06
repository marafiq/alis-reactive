import { CareBoardV1, CareBoardV2, type CareBoardHost } from "./CareBoard/care-board";
import { DutyRoster, type DutyRosterHost } from "./DutyRoster/duty-roster";

for (const host of document.querySelectorAll<CareBoardHost>("[data-careboard]")) {
  host.careBoard = host.dataset["careboard"] === "2" ? new CareBoardV2(host) : new CareBoardV1(host);
}

for (const host of document.querySelectorAll<DutyRosterHost>("[data-duty-roster]")) {
  host.instances = [new DutyRoster(host)];
}
