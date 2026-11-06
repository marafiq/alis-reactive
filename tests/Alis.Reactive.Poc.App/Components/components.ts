import { CareBoardV1, CareBoardV2, type CareBoardHost } from "./CareBoard/care-board";
import { DutyRoster, DutyRosterV2, type DutyRosterHost } from "./DutyRoster/duty-roster";
import { NurseStation, StationCensus, type NurseStationHost, type StationCensusHost } from "./NurseStation/nurse-station";

for (const host of document.querySelectorAll<CareBoardHost>("[data-careboard]")) {
  host.careBoard = host.dataset["careboard"] === "2" ? new CareBoardV2(host) : new CareBoardV1(host);
}

for (const host of document.querySelectorAll<DutyRosterHost>("[data-duty-roster]")) {
  host.instances = [host.dataset["dutyRoster"] === "2" ? new DutyRosterV2(host) : new DutyRoster(host)];
}

for (const host of document.querySelectorAll<NurseStationHost>("[data-nurse-station]")) {
  host.station = new NurseStation(host);
}

for (const host of document.querySelectorAll<StationCensusHost>("[data-station-census]")) {
  host.census = new StationCensus(host);
}
