import { CareBoardV1, CareBoardV2, type CareBoardHost } from "./CareBoard/care-board";
import { DoseChart, type DoseChartHost } from "./DoseChart/dose-chart";
import { DutyRoster, DutyRosterV2, type DutyRosterHost } from "./DutyRoster/duty-roster";
import { CurrentInfusionPump, OlderInfusionPump, type InfusionPumpHost } from "./InfusionPump/infusion-pump";
import { MedicationCart, type MedicationCartHost } from "./MedicationCart/medication-cart";
import { VitalsMonitor, type VitalsMonitorHost } from "./VitalsMonitor/vitals-monitor";
import { CallBell, MedicineSearch, WardFeed, mountBedBoard, type CallBellHost, type MedicineSearchHost, type WardFeedHost } from "./WardSignals/ward-signals";
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

for (const host of document.querySelectorAll<MedicationCartHost>("[data-medication-cart]")) {
  host.cart = new MedicationCart(host);
}

for (const host of document.querySelectorAll<InfusionPumpHost>("[data-infusion-pump]")) {
  host.pump = host.dataset["infusionPump"] === "older" ? new OlderInfusionPump(host) : new CurrentInfusionPump(host);
}

for (const host of document.querySelectorAll<VitalsMonitorHost>("[data-vitals-monitor]")) {
  host.monitor = new VitalsMonitor(host, host.dataset["vitalsMonitor"] === "older");
}

for (const host of document.querySelectorAll<CallBellHost>("[data-call-bell]")) {
  host.bell = new CallBell(host);
}

for (const host of document.querySelectorAll<WardFeedHost>("[data-ward-feed]")) {
  host.feed = new WardFeed(host);
}

for (const host of document.querySelectorAll<HTMLElement>("[data-bed-board]")) {
  mountBedBoard(host);
}

for (const host of document.querySelectorAll<MedicineSearchHost>("[data-medicine-search]")) {
  host.search = new MedicineSearch(host);
}

for (const host of document.querySelectorAll<DoseChartHost>("[data-dose-chart]")) {
  host.chart = new DoseChart(host);
}
