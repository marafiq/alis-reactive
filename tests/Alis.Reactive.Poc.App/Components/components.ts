import { CareBoardV1, CareBoardV2, type CareBoardHost } from "./CareBoard/care-board";
import { DoseChart, type DoseChartHost } from "./DoseChart/dose-chart";
import { FluidBalance, type FluidBalanceHost } from "./FluidBalance/fluid-balance";
import { DutyRoster, DutyRosterV2, type DutyRosterHost } from "./DutyRoster/duty-roster";
import { CurrentInfusionPump, OlderInfusionPump, type InfusionPumpHost } from "./InfusionPump/infusion-pump";
import { MedicationCart, type MedicationCartHost } from "./MedicationCart/medication-cart";
import { VitalsMonitor, type VitalsMonitorHost } from "./VitalsMonitor/vitals-monitor";
import { CallBell, MedicineSearch, WardFeed, mountBedBoard, type CallBellHost, type MedicineSearchHost, type WardFeedHost } from "./WardSignals/ward-signals";
import { NurseStation, StationCensus, type NurseStationHost, type StationCensusHost } from "./NurseStation/nurse-station";
import { WardPicker, type WardPickerHost } from "./WardPicker/ward-picker";
import { onMount } from "./mount";

onMount<CareBoardHost>("[data-careboard]", (host) => {
  host.careBoard = host.dataset["careboard"] === "2" ? new CareBoardV2(host) : new CareBoardV1(host);
});

onMount<DutyRosterHost>("[data-duty-roster]", (host) => {
  host.instances = [host.dataset["dutyRoster"] === "2" ? new DutyRosterV2(host) : new DutyRoster(host)];
});

onMount<NurseStationHost>("[data-nurse-station]", (host) => {
  host.station = new NurseStation(host);
});

onMount<StationCensusHost>("[data-station-census]", (host) => {
  host.census = new StationCensus(host);
});

onMount<MedicationCartHost>("[data-medication-cart]", (host) => {
  host.cart = new MedicationCart(host);
});

onMount<InfusionPumpHost>("[data-infusion-pump]", (host) => {
  host.pump = host.dataset["infusionPump"] === "older" ? new OlderInfusionPump(host) : new CurrentInfusionPump(host);
});

onMount<VitalsMonitorHost>("[data-vitals-monitor]", (host) => {
  host.monitor = new VitalsMonitor(host, host.dataset["vitalsMonitor"] === "older");
});

onMount<CallBellHost>("[data-call-bell]", (host) => {
  host.bell = new CallBell(host);
});

onMount<WardFeedHost>("[data-ward-feed]", (host) => {
  host.feed = new WardFeed(host);
});

onMount<HTMLElement>("[data-bed-board]", (host) => {
  mountBedBoard(host);
});

onMount<MedicineSearchHost>("[data-medicine-search]", (host) => {
  host.search = new MedicineSearch(host);
});

onMount<DoseChartHost>("[data-dose-chart]", (host) => {
  host.chart = new DoseChart(host);
});

onMount<FluidBalanceHost>("[data-fluid-balance]", (host) => {
  host.balance = new FluidBalance(host);
});

onMount<WardPickerHost>("[data-ward-picker]", (host) => {
  host.picker = new WardPicker(host);
});
