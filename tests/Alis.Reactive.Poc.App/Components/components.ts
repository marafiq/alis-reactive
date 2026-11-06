import { CareBoardV1, CareBoardV2, type CareBoardHost } from "./CareBoard/care-board";

for (const host of document.querySelectorAll<CareBoardHost>("[data-careboard]")) {
  host.careBoard = host.dataset["careboard"] === "2" ? new CareBoardV2(host) : new CareBoardV1(host);
}
