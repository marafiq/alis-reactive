# Feature flag rollout (example)

An ALIS admin screen for one feature flag at a time, built with Alis.Reactive on .NET 10.
It implements the "Feature Flag Rollout" design: the flag list beside one flag's form,
mobile first, with fake data held in memory.

Hierarchy: **app → site → company → community.** Every flag belongs to one app (ALIS web,
Admin, Daemons); the same key can exist in several apps as separate flags.

## Run it

From the repo root:

```bash
npm install
npm run build:all
dotnet run --project examples/feature-flags
```

Open http://localhost:5230. The app serves the framework bundles straight from
`Alis.Reactive.Assets/dist` and Syncfusion from `node_modules/@syncfusion/ej2`, like the
sandbox, so it always runs the current framework source. Set `Syncfusion:LicenseKey` in
configuration to hide the trial banner. Restarting the app resets every flag.

## What's on the screen

| Part | How it is built |
|------|-----------------|
| App tabs, Everyone / Some / Off filter, search | `NativeRadioGroup` and `NativeTextBox`; any change reloads the list partial |
| Flag rows with the 7-site strip | Server-rendered; the whole row is a `NativeButton` hit area that loads the flag |
| Flag switch, Everyone | `FusionSwitch` |
| Sites, Companies, Communities, Keep off for | `FusionMultiSelect`, checkbox mode, filtering, grouping |
| Who has it (count, site strip, each community and why) | A partial recomputed by the server from the posted draft |
| Save bar (`9 → 30 communities`) | A partial recomputed on every change |
| Other apps: open the twin or copy here | `GET /flags/new?copyFrom=` into the same form |
| History, For developers, Name and description | Server-rendered; folding is a class toggle |
| Copy a code snippet | A small `clipboard` plugin (`wwwroot/js/flags-plugins.js`) |
| What's on for a community? | `NativeDrawer` with a filtering, grouped `FusionDropDownList` |
| Save / Create | `IncludeAll` POST with `Validate<FlagEditValidator>` / `Validate<NewFlagValidator>`; a key already used in the app is a server-only rule |

## How it fits together

The page is a shell. Each part is a partial view injected into a slot with `Into(...)`:

```
Index (FlagsPageModel)            ── flags-changed → reload the list for an app
├─ #flag-list   _FlagList          app tabs · check button · search · filter · rows
└─ #flag-detail _FlagDetail (FlagEditModel)
   ├─ #key-field     _KeyField        new flags: key filled in from the name
   ├─ #who-has-it    _WhoHasIt        recomputed from the draft
   ├─ #dev-snippets  _DevSnippets     new flags: snippets follow the key
   └─ #save-summary  _SaveSummary     recomputed from the draft
```

Every input on the form reacts the same way (`FlagPipelines.RefreshDraft`): show the save
bar, then post the whole form (`IncludeAll`) and inject the answers. The browser never
works out who has a flag; the server does, with the same rule the SDK uses
(`Domain/FlagRule.cs`): a community has the flag when the flag is on, it is picked
(everyone, or the community, its company or its site) and nothing covering it is kept off.

Views carry no JavaScript. The only script the app adds is the clipboard plugin.

## Fake data

`Domain/Hierarchy.cs` seeds 7 sites, 9 companies and 31 communities; `Domain/FlagStore.cs`
seeds 14 flags across the three apps with their history.
