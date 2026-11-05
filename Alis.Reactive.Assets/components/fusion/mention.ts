import { onMount } from "../hooks";

interface SyncfusionHost extends HTMLElement {
  ej2_instances: unknown[];
}

onMount("[data-alis-mention]", element => {
  const bridge = element as SyncfusionHost;
  const mention = document.getElementById(bridge.dataset["alisMention"]!) as SyncfusionHost;
  bridge.ej2_instances = mention.ej2_instances;
});
