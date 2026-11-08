import { CareBoardV1, CareBoardV2, type CareBoardHost } from "./CareBoard/care-board";
import { DoseChart, type DoseChartHost } from "./DoseChart/dose-chart";
import { FluidBalance, type FluidBalanceHost } from "./FluidBalance/fluid-balance";
import { HandoverBoard, type HandoverBoardHost } from "./HandoverBoard/handover-board";
import { DutyRoster, DutyRosterV2, type DutyRosterHost } from "./DutyRoster/duty-roster";
import { CurrentInfusionPump, OlderInfusionPump, type InfusionPumpHost } from "./InfusionPump/infusion-pump";
import { MealTray, trayBuildOf, type MealTrayHost } from "./MealTray/meal-tray";
import { MedicationCart, type MedicationCartHost } from "./MedicationCart/medication-cart";
import { VitalsMonitor, type VitalsMonitorHost } from "./VitalsMonitor/vitals-monitor";
import { CallBell, MedicineSearch, WardFeed, mountBedBoard, type CallBellHost, type MedicineSearchHost, type WardFeedHost } from "./WardSignals/ward-signals";
import { NurseStation, StationCensus, type NurseStationHost, type StationCensusHost } from "./NurseStation/nurse-station";
import { WardPicker, type WardPickerHost } from "./WardPicker/ward-picker";
import { DoseCard, type DoseCardHost } from "./DoseCard/dose-card";
import { BedStore, NightClock, ShiftLog, VisitorBook, initials } from "./ShiftPlugins/shift-plugins";
import { OxygenGauges } from "./OxygenGauge/oxygen-gauge";
import { VisitPlanner } from "./VisitPlanner/visit-planner";
import { CallLog, type CallLogHost } from "./CallLog/call-log";
import { WardRadio } from "./WardRadio/ward-radio";
import { BedsideMonitor, type BedsideMonitorHost } from "./BedsideMonitor/bedside-monitor";
import { WardPager } from "./WardPager/ward-pager";
import { MedicineRound, type MedicineRoundHost } from "./MedicineRound/medicine-round";
import { DeskMonitor, MonitorDesk, type DeskMonitorHost, type MonitorDeskHost } from "./MonitorDesk/monitor-desk";
import { ResidentFinder, type ResidentFinderHost } from "./ResidentFinder/resident-finder";
import { onMount, onUnmount, providePlugin } from "./mount";

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

onMount<MealTrayHost>("[data-meal-tray]", (host) => {
  host.tray = new MealTray(host, trayBuildOf(host));
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

onMount<HandoverBoardHost>("[data-handover-board]", (host) => {
  host.board = new HandoverBoard(host);
});

onMount<WardPickerHost>("[data-ward-picker]", (host) => {
  host.picker = new WardPicker(host);
});

onMount<DoseCardHost>("[data-dose-card]", (host) => {
  host.card = new DoseCard(host);
});

onMount<CallLogHost>("[data-call-log]", (host) => {
  host.log = new CallLog(host);
});

onMount<BedsideMonitorHost>("[data-bedside-monitor]", (host) => {
  host.monitor = new BedsideMonitor(host);
});

onUnmount<BedsideMonitorHost>("[data-bedside-monitor]", (host) => {
  host.monitor?.destroy();
});

onMount<MedicineRoundHost>("[data-medicine-round]", (host) => {
  host.round = new MedicineRound(host);
});

onMount<DeskMonitorHost>("[data-desk-monitor]", (host) => {
  new DeskMonitor(host);
});

onMount<MonitorDeskHost>("[data-monitor-desk]", (host) => {
  host.desk = new MonitorDesk(host);
});

onMount<ResidentFinderHost>("[data-resident-finder]", (host) => {
  new ResidentFinder(host);
});

providePlugin("shift-log", new ShiftLog());
providePlugin("initials", initials);
providePlugin("night-clock", new NightClock());
providePlugin("bed-store", new BedStore());

onMount<HTMLButtonElement>("[data-opens-visitor-book]", (button) => {
  button.addEventListener("click", () => providePlugin("visitor-book", new VisitorBook()), { once: true });
});

onMount("[data-oxygen-gauge]", (host) => OxygenGauges.mount(host));
providePlugin("oxygen-gauges", (host: HTMLElement) => OxygenGauges.of(host));
providePlugin("visit-planner", new VisitPlanner());
providePlugin("ward-radio", new WardRadio());
providePlugin("night-radio", new WardRadio());
providePlugin("ward-pager", new WardPager());
