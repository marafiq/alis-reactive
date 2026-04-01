# Issue #86 Activity / Workflow Diagrams

## 1. Root Resolution And Shared Value Flow

```mermaid
flowchart TD
    A["Readable root"] --> B["Resolve root object"]
    B --> C["Apply access steps in order"]
    C --> D["Raw JS value"]
    D --> E["Shape if needed"]
    E --> F["Consume"]

    A1["trigger"] --> A
    A2["response"] --> A
    A3["component(id + vendor)"] --> A
    A4["element(id)"] --> A
    A5["document"] --> A

    F1["guard"] --> F
    F2["request gather"] --> F
    F3["dispatch payload"] --> F
    F4["apply.set value"] --> F
    F5["validation"] --> F
```

## 2. Component Registry And Optional Binding Participation

```mermaid
flowchart TD
    A["components[componentId]"] --> B["vendor"]
    A --> C["optional binding"]
    C --> D["binding.path"]
    C --> E["binding.access"]

    F["explicit component ref"] --> G["componentId + vendor"]
    G --> H["component root"]

    E --> I["canonical semantic value"]
    H --> J["generic surface algebra"]

    I1["includeAll"] --> I
    I2["validation"] --> I
    I3["request gather by bindingValue"] --> I

    J1["member read"] --> J
    J2["invoke read"] --> J
    J3["set property"] --> J
    J4["call method"] --> J
    J5["subscribe to event"] --> J
```

## 3. Request Unit Lifecycle

```mermaid
flowchart TD
    A["Request"] --> B["gather"]
    B --> C["freeze request snapshot"]
    C --> D["whileLoading commands"]
    D --> E["validate"]
    E --> F["transport"]
    F --> G["response.onSuccess pipelines"]
    F --> H["response.onError pipelines"]
    G --> I["response.chained request"]

    B1["literal"] --> B
    B2["trigger value"] --> B
    B3["response value"] --> B
    B4["component access"] --> B
    B5["includeAll(binding participants)"] --> B

    E1["targets -> componentId"] --> E
    E2["rules + conditions"] --> E
```

## 4. Trigger Families Into The Same Ordered Pipeline

```mermaid
flowchart TD
    A["domReady"] --> Z["ordered pipeline steps[]"]
    B["documentEvent"] --> Z
    C["componentEvent"] --> Z
    D["sse"] --> Z
    E["signalR"] --> Z

    C --> C1["payload = none | host | build"]
    D --> D1["payload = host | build"]
    E --> E1["payload = host | build"]
    B --> B1["payload = host | build"]

    Z --> P1["command"]
    Z --> P2["when"]
    Z --> P3["request"]
    Z --> P4["parallel"]

    P2 --> Z
    P3 --> Z
```

## 5. Partial Merge Lifecycle

```mermaid
flowchart TD
    A["incoming plan"] --> B{"has sourceId?"}
    B -->|no| C["root plan boot/register"]
    B -->|yes| D["scope ownership by planId + sourceId"]
    D --> E["remove prior fragment-owned reactions/components in same plan only"]
    E --> F["merge new components"]
    F --> G["wire new reactions"]
    G --> H["lazy consumers read latest component registry"]

    H1["validation"] --> H
    H2["includeAll"] --> H

    G1["component-event triggers are self-sufficient at wire time"] --> G
```

## 6. Mixed Workflow: Trigger -> Condition -> Request -> Success -> Component Apply

```mermaid
flowchart TD
    A["trigger payload arrives"] --> B["when guard reads trigger/component values"]
    B -->|pass| C["request.gather"]
    C --> D["request snapshot frozen"]
    D --> E["HTTP transport"]
    E --> F["response.onSuccess"]
    F --> G["read nested response path"]
    G --> H["apply to explicit component root"]

    B -->|fail| X["alternate pipeline or stop"]
```

## 7. Mixed Workflow: Component Event -> Dispatch -> Document Reaction

```mermaid
flowchart TD
    A["componentEvent"] --> B["trigger payload"]
    B --> C["pipeline step: dispatch"]
    C --> D["documentEvent"]
    D --> E["second reaction pipeline"]
    E --> F["condition / request / apply"]
```

## 8. Mixed Workflow: SSE / SignalR -> Request -> Response -> Non-Input Component

```mermaid
flowchart TD
    A["sse or signalR trigger"] --> B["host payload becomes trigger root"]
    B --> C["ordered pipeline"]
    C --> D["request.gather from trigger + bindingValue"]
    D --> E["response.onSuccess"]
    E --> F["read response path"]
    F --> G["apply.set or apply.call on explicit non-input component"]
```
