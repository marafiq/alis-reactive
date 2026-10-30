// FusionMention: a Syncfusion Mention with a target turns its own element into the suggestion popup
// and renames it when the popup first opens. The component's element is therefore a hidden span,
// rendered with data-alis-mention naming the Mention's element, that carries the Mention's instance.

import { onMount } from "../hooks";

interface SyncfusionHost extends HTMLElement {
  ej2_instances: unknown[];
}

onMount("[data-alis-mention]", element => {
  const bridge = element as SyncfusionHost;
  const mention = document.getElementById(bridge.dataset["alisMention"]!) as SyncfusionHost;
  bridge.ej2_instances = mention.ej2_instances;
});
