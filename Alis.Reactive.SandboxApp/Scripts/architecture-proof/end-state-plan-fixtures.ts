import type { EndStatePlan } from "./end-state-plan-types";

export const testWidgetNativeChangePlan = {
  planId: "Proof.TestWidgetNative",
  components: {
    "Resident.Name": {
      id: "name-widget",
      vendor: "native",
      componentType: "test-widget-native",
      value: { path: "value", shape: "string" },
    },
  },
  entries: [
    {
      trigger: {
        kind: "component-event",
        target: {
          componentId: "name-widget",
          vendor: "native",
          jsEvent: "change",
        },
        payload: {
          kind: "object",
          fields: {
            value: {
              kind: "read",
              access: {
                source: { kind: "component", componentId: "name-widget", vendor: "native" },
                path: "value",
                shape: "string",
              },
            },
          },
        },
      },
      reaction: {
        kind: "sequential",
        commands: [
          {
            kind: "dispatch",
            event: "resident-name-changed",
            payload: {
              kind: "object",
              fields: {
                name: {
                  kind: "read",
                  access: {
                    source: { kind: "payload", scope: "trigger" },
                    path: "value",
                    shape: "string",
                  },
                },
              },
            },
          },
        ],
      },
    },
  ],
} satisfies EndStatePlan;

export const nativeButtonClickPlan = {
  planId: "Proof.NativeButton",
  components: {},
  entries: [
    {
      trigger: {
        kind: "component-event",
        target: {
          componentId: "save-button",
          vendor: "native",
          jsEvent: "click",
        },
        payload: { kind: "none" },
      },
      reaction: {
        kind: "sequential",
        commands: [
          { kind: "dispatch", event: "save-requested" },
        ],
      },
    },
  ],
} satisfies EndStatePlan;

export const testWidgetSyncFusionChangedPlan = {
  planId: "Proof.TestWidgetSyncFusion.Changed",
  components: {
    "Resident.Filter": {
      id: "filter-widget",
      vendor: "fusion",
      componentType: "test-widget-syncfusion",
      value: { path: "value", shape: "string" },
    },
  },
  entries: [
    {
      trigger: {
        kind: "component-event",
        target: {
          componentId: "filter-widget",
          vendor: "fusion",
          jsEvent: "change",
        },
        payload: { kind: "callback" },
      },
      reaction: {
        kind: "sequential",
        commands: [
          {
            kind: "mutate-element",
            target: "filter-summary",
            mutation: {
              kind: "set-prop",
              prop: "textContent",
              value: {
                kind: "read",
                access: {
                  source: { kind: "payload", scope: "trigger" },
                  path: "newValue",
                  shape: "string",
                },
              },
            },
          },
        ],
      },
    },
  ],
} satisfies EndStatePlan;

export const testWidgetSyncFusionItemsChangedPlan = {
  planId: "Proof.TestWidgetSyncFusion.ItemsChanged",
  components: {},
  entries: [
    {
      trigger: {
        kind: "component-event",
        target: {
          componentId: "items-widget",
          vendor: "fusion",
          jsEvent: "items-changed",
        },
        payload: { kind: "callback" },
      },
      reaction: {
        kind: "sequential",
        commands: [
          {
            kind: "dispatch",
            event: "items-counted",
            payload: {
              kind: "object",
              fields: {
                count: {
                  kind: "read",
                  access: {
                    source: { kind: "payload", scope: "trigger" },
                    path: "count",
                    shape: "number",
                  },
                },
              },
            },
          },
        ],
      },
    },
  ],
} satisfies EndStatePlan;

export const customEventDispatchPlan = {
  planId: "Proof.CustomEventDispatch",
  components: {},
  entries: [
    {
      trigger: { kind: "custom-event", event: "resident-loaded" },
      reaction: {
        kind: "sequential",
        commands: [
          {
            kind: "mutate-element",
            target: "resident-name",
            mutation: {
              kind: "set-prop",
              prop: "textContent",
              value: {
                kind: "read",
                access: {
                  source: { kind: "payload", scope: "trigger" },
                  path: "resident.name",
                  shape: "string",
                },
              },
            },
          },
        ],
      },
    },
  ],
} satisfies EndStatePlan;

export const filteringMutationPlan = {
  planId: "Proof.FilteringMutation",
  components: {},
  entries: [
    {
      trigger: { kind: "custom-event", event: "filtering" },
      reaction: {
        kind: "http",
        request: {
          verb: "GET",
          url: "/api/filter",
          gather: [
            {
              kind: "field",
              name: "query",
              value: {
                kind: "read",
                access: {
                  source: { kind: "payload", scope: "trigger" },
                  path: "text",
                  shape: "string",
                },
              },
            },
          ],
          onSuccess: [
            {
              reaction: {
                kind: "sequential",
                commands: [
                  {
                    kind: "mutate-payload",
                    mutation: {
                      kind: "set-prop",
                      prop: "preventDefaultAction",
                      value: { kind: "literal", value: true, shape: "boolean" },
                    },
                  },
                  {
                    kind: "mutate-payload",
                    mutation: {
                      kind: "call",
                      method: "updateData",
                      args: [
                        {
                          kind: "read",
                          access: {
                            source: { kind: "payload", scope: "response" },
                            path: "items",
                            shape: "array",
                            elementShape: "string",
                          },
                        },
                      ],
                    },
                  },
                ],
              },
            },
          ],
        },
      },
    },
  ],
} satisfies EndStatePlan;

export const requestSinkAndResponsePlan = {
  planId: "Proof.RequestSinkAndResponse",
  components: {
    "Resident.Name": {
      id: "resident-name-input",
      vendor: "native",
      componentType: "textbox",
      value: { path: "value", shape: "string" },
    },
    "Resident.Email": {
      id: "resident-email-input",
      vendor: "native",
      componentType: "textbox",
      value: { path: "value", shape: "string" },
    },
  },
  entries: [
    {
      trigger: { kind: "custom-event", event: "submit-resident" },
      reaction: {
        kind: "http",
        preFetch: [
          {
            kind: "mutate-element",
            target: "spinner",
            mutation: {
              kind: "set-prop",
              prop: "hidden",
              value: { kind: "literal", value: false, shape: "boolean" },
            },
          },
        ],
        request: {
          verb: "POST",
          url: "/api/residents",
          gather: [
            {
              kind: "field",
              name: "name",
              value: {
                kind: "read",
                access: {
                  source: { kind: "component", componentId: "resident-name-input", vendor: "native" },
                  path: "value",
                  shape: "string",
                },
              },
            },
            {
              kind: "field",
              name: "email",
              value: {
                kind: "read",
                access: {
                  source: { kind: "component", componentId: "resident-email-input", vendor: "native" },
                  path: "value",
                  shape: "string",
                },
              },
            },
          ],
          validation: {
            formId: "resident-form",
            fields: [
              {
                modelPath: "Resident.Name",
                rules: [{ rule: "required", message: "Name is required" }],
              },
              {
                modelPath: "Resident.Email",
                rules: [{ rule: "required", message: "Email is required" }],
              },
            ],
          },
          onSuccess: [
            {
              reaction: {
                kind: "sequential",
                commands: [
                  {
                    kind: "dispatch",
                    event: "resident-saved",
                    payload: {
                      kind: "object",
                      fields: {
                        id: {
                          kind: "read",
                          access: {
                            source: { kind: "payload", scope: "response" },
                            path: "id",
                            shape: "number",
                          },
                        },
                        name: {
                          kind: "read",
                          access: {
                            source: { kind: "payload", scope: "response" },
                            path: "name",
                            shape: "string",
                          },
                        },
                      },
                    },
                  },
                ],
              },
            },
          ],
          onError: [
            {
              statusCode: 400,
              reaction: {
                kind: "sequential",
                commands: [{ kind: "validation-errors", formId: "resident-form" }],
              },
            },
          ],
        },
      },
    },
  ],
} satisfies EndStatePlan;

export const chainedRequestResponseContinuityPlan = {
  planId: "Proof.ChainedResponseContinuity",
  components: {},
  entries: [
    {
      trigger: { kind: "dom-ready" },
      reaction: {
        kind: "http",
        request: {
          verb: "POST",
          url: "/api/residents/init",
          onSuccess: [
            {
              reaction: {
                kind: "sequential",
                commands: [
                  {
                    kind: "mutate-element",
                    target: "status",
                    mutation: {
                      kind: "set-prop",
                      prop: "textContent",
                      value: { kind: "literal", value: "initialized", shape: "string" },
                    },
                  },
                ],
              },
            },
          ],
          chained: {
            verb: "GET",
            url: "/api/residents/detail",
            gather: [
              {
                kind: "field",
                name: "id",
                value: {
                  kind: "read",
                  access: {
                    source: { kind: "payload", scope: "response" },
                    path: "residentId",
                    shape: "number",
                  },
                },
              },
            ],
            onSuccess: [
              {
                reaction: {
                  kind: "sequential",
                  commands: [
                    {
                      kind: "mutate-element",
                      target: "resident-name",
                      mutation: {
                        kind: "set-prop",
                        prop: "textContent",
                        value: {
                          kind: "read",
                          access: {
                            source: { kind: "payload", scope: "response" },
                            path: "name",
                            shape: "string",
                          },
                        },
                      },
                    },
                  ],
                },
              },
            ],
          },
        },
      },
    },
  ],
} satisfies EndStatePlan;

export const parallelRequestPlan = {
  planId: "Proof.ParallelRequests",
  components: {},
  entries: [
    {
      trigger: { kind: "dom-ready" },
      reaction: {
        kind: "parallel-http",
        requests: [
          { verb: "GET", url: "/api/residents" },
          { verb: "GET", url: "/api/facilities" },
        ],
        onAllSettled: [
          {
            kind: "dispatch",
            event: "parallel-loaded",
          },
        ],
      },
    },
  ],
} satisfies EndStatePlan;

export const ajaxPartialValidationRootPlan = {
  planId: "Resident.Model",
  components: {
    "Resident.Name": {
      id: "Resident_Name",
      vendor: "native",
      componentType: "textbox",
      value: { path: "value", shape: "string" },
    },
    "Resident.Email": {
      id: "Resident_Email",
      vendor: "native",
      componentType: "textbox",
      value: { path: "value", shape: "string" },
    },
  },
  entries: [
    {
      trigger: { kind: "custom-event", event: "submit-resident" },
      reaction: {
        kind: "http",
        request: {
          verb: "POST",
          url: "/api/residents/save",
          gather: [{ kind: "all" }],
          validation: {
            formId: "resident-form",
            fields: [
              { modelPath: "Resident.Name", rules: [{ rule: "required", message: "required" }] },
              { modelPath: "Resident.Email", rules: [{ rule: "required", message: "required" }] },
              { modelPath: "Resident.Address.Street", rules: [{ rule: "required", message: "required" }] },
              { modelPath: "Resident.Address.City", rules: [{ rule: "required", message: "required" }] },
              { modelPath: "Resident.Address.ZipCode", rules: [{ rule: "required", message: "required" }] },
            ],
          },
          onError: [
            {
              statusCode: 400,
              reaction: {
                kind: "sequential",
                commands: [{ kind: "validation-errors", formId: "resident-form" }],
              },
            },
          ],
        },
      },
    },
  ],
} satisfies EndStatePlan;

export const ajaxPartialAddressFragmentPlan = {
  planId: "Resident.Model",
  components: {
    "Resident.Address.Street": {
      id: "Resident_Address_Street",
      vendor: "native",
      componentType: "textbox",
      value: { path: "value", shape: "string" },
    },
    "Resident.Address.City": {
      id: "Resident_Address_City",
      vendor: "native",
      componentType: "textbox",
      value: { path: "value", shape: "string" },
    },
    "Resident.Address.ZipCode": {
      id: "Resident_Address_ZipCode",
      vendor: "native",
      componentType: "textbox",
      value: { path: "value", shape: "string" },
    },
  },
  entries: [
    {
      trigger: {
        kind: "component-event",
        target: {
          componentId: "Resident_Address_ZipCode",
          vendor: "native",
          jsEvent: "change",
        },
        payload: {
          kind: "object",
          fields: {
            value: {
              kind: "read",
              access: {
                source: { kind: "component", componentId: "Resident_Address_ZipCode", vendor: "native" },
                path: "value",
                shape: "string",
              },
            },
          },
        },
      },
      reaction: {
        kind: "sequential",
        commands: [
          {
            kind: "mutate-element",
            target: "zipcode-status",
            mutation: {
              kind: "set-prop",
              prop: "textContent",
              value: { kind: "literal", value: "Zip validated", shape: "string" },
            },
          },
        ],
      },
    },
  ],
} satisfies EndStatePlan;

export const serverPushPlan = {
  planId: "Proof.ServerPush",
  components: {},
  entries: [
    {
      trigger: { kind: "server-push", url: "/api/residents/live", eventType: "resident-updated" },
      reaction: {
        kind: "sequential",
        commands: [
          {
            kind: "mutate-element",
            target: "resident-status",
            mutation: {
              kind: "set-prop",
              prop: "textContent",
              value: {
                kind: "read",
                access: {
                  source: { kind: "payload", scope: "trigger" },
                  path: "message",
                  shape: "string",
                },
              },
            },
          },
        ],
      },
    },
  ],
} satisfies EndStatePlan;

export const signalRPlan = {
  planId: "Proof.SignalR",
  components: {},
  entries: [
    {
      trigger: { kind: "signalr", hubUrl: "/hubs/residents", methodName: "ReceiveResidentUpdate" },
      reaction: {
        kind: "sequential",
        commands: [
          {
            kind: "dispatch",
            event: "resident-update-received",
            payload: {
              kind: "object",
              fields: {
                id: {
                  kind: "read",
                  access: {
                    source: { kind: "payload", scope: "trigger" },
                    path: "id",
                    shape: "number",
                  },
                },
                status: {
                  kind: "read",
                  access: {
                    source: { kind: "payload", scope: "trigger" },
                    path: "status",
                    shape: "string",
                  },
                },
              },
            },
          },
        ],
      },
    },
  ],
} satisfies EndStatePlan;

export const exhaustiveEndStateFixtures = [
  testWidgetNativeChangePlan,
  nativeButtonClickPlan,
  testWidgetSyncFusionChangedPlan,
  testWidgetSyncFusionItemsChangedPlan,
  customEventDispatchPlan,
  filteringMutationPlan,
  requestSinkAndResponsePlan,
  chainedRequestResponseContinuityPlan,
  parallelRequestPlan,
  ajaxPartialValidationRootPlan,
  ajaxPartialAddressFragmentPlan,
  serverPushPlan,
  signalRPlan,
] satisfies EndStatePlan[];
