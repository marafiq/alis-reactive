# Handoff

Written at `date`: Tue Nov 17 23:41:41 EST 2026

## Where it stands
- Branch: detached HEAD
- HEAD: 8391f0be5 spec: STATE at a3c26b5cd
- Full check: No .work/logs/spec-check-*.log exists.
- .work/last-run.txt: missing

## Landed since the commit before the last spec: STATE commit
8391f0be5 spec: STATE at a3c26b5cd

## Open
open: 19 promises with no browser test, 7 runtime cases not built, 14 doors not implemented
feature promises: 522; named by a browser test: 503; with no browser test: 19
  FusionAppLevelObjects  The confirm                                  M23_F2.M23_X2_0
  FusionAppLevelObjects  The confirm                                  M23_F2.M23_X3_0
  FusionAppLevelObjects  The page's toast                             M23_F1.M23_X1_0
  FusionComponents       FusionBreadcrumb                             M21_F6.M21_X1002_0
  FusionComponents       FusionGrid                                   M21_F23.M21_X201_0
  FusionComponents       FusionGrid                                   M21_F23.M21_X201_1
  FusionComponents       FusionGrid                                   M21_F23.M21_X201_2
  FusionComponents       FusionGrid                                   M21_F23.M21_X203_0
  FusionComponents       FusionGrid                                   M21_F23.M21_X203_1
  FusionComponents       FusionGrid                                   M21_F23.M21_X203_2
  FusionComponents       FusionGrid                                   M21_F23.M21_X205_0
  FusionComponents       FusionGrid                                   M21_F23.M21_X205_1
  FusionComponents       FusionMenu                                   M21_F30.M21_X406_0
  FusionComponents       FusionSidebar                                M21_F40.M21_X1024_0
  FusionComponents       FusionSmartPasteButton                       M21_F42.M21_X803_0
  FusionComponents       FusionSmartPasteButton                       M21_F42.M21_X803_1
  FusionComponents       FusionSmartPasteButton                       M21_F42.M21_X804_0
  FusionComponents       FusionToolbar                                M21_F51.M21_X1032_0
  PathsAndWire           Paths and wire                               M15_F1.X3_7_0
runtime cases not built:
Alis.Reactive.Assets/runtime/Conditions/operators.ts:43
Alis.Reactive.Assets/runtime/InputsAndIds/model.ts:240
Alis.Reactive.Assets/runtime/InputsAndIds/model.ts:248
Alis.Reactive.Assets/runtime/InputsAndIds/model.ts:264
Alis.Reactive.Assets/runtime/RequestsAndResponses/request.ts:273
Alis.Reactive.Assets/runtime/Validation/forms.ts:194
Alis.Reactive.Assets/runtime/Validation/forms.ts:231
doors not implemented:
Alis.Reactive.Fusion/FusionAppLevelObjects/FusionConfirm/FusionConfirmHtml.Net10.cs:16
Alis.Reactive.Fusion/FusionAppLevelObjects/FusionConfirm/FusionConfirmHtml.Net10.cs:25
Alis.Reactive.Fusion/FusionAppLevelObjects/FusionConfirm/FusionConfirmHtml.Net48.cs:16
Alis.Reactive.Fusion/FusionAppLevelObjects/FusionConfirm/FusionConfirmHtml.Net48.cs:25
Alis.Reactive.Fusion/FusionAppLevelObjects/FusionToast/FusionToastHtml.Net10.cs:16
Alis.Reactive.Fusion/FusionAppLevelObjects/FusionToast/FusionToastHtml.Net48.cs:16
Alis.Reactive.Fusion/FusionComponents/FusionBreadcrumb/FusionBreadcrumbHtml.Net10.cs:21
Alis.Reactive.Fusion/FusionComponents/FusionBreadcrumb/FusionBreadcrumbHtml.Net48.cs:21
Alis.Reactive.Fusion/FusionComponents/FusionMenu/FusionMenuHtml.Net10.cs:21
Alis.Reactive.Fusion/FusionComponents/FusionMenu/FusionMenuHtml.Net48.cs:21
Alis.Reactive.Fusion/FusionComponents/FusionSidebar/FusionSidebarHtml.Net10.cs:21
Alis.Reactive.Fusion/FusionComponents/FusionSidebar/FusionSidebarHtml.Net48.cs:21
Alis.Reactive.Fusion/FusionComponents/FusionToolbar/FusionToolbarHtml.Net10.cs:21
Alis.Reactive.Fusion/FusionComponents/FusionToolbar/FusionToolbarHtml.Net48.cs:21

## Unlanded work
- ../..: base 65fdf118d1498a8285db84a43c4da81dfaa0ea3e, 1 paths changed
- ../../.claude/worktrees/agent-a00a1711dcf7a6d00: base 59e79a595b39149b9e3369bdf11d1a5c37c5bead, 2 paths changed
- ../../.claude/worktrees/agent-a087381ed4e704c09: base 6ab77b717b9db3bc8dc57933e772d63f9878db35, 2 paths changed
- ../../.claude/worktrees/agent-a0a6202526eebf18b: base fa22f3166cb7addcaa5ae7cd9abbf89ac4f0c229, 2 paths changed
- ../../.claude/worktrees/agent-a0b5a2b70b8200fcc: base 6ab77b717b9db3bc8dc57933e772d63f9878db35, 2 paths changed
- ../../.claude/worktrees/agent-a103f190a25764213: base 877b149575d08a5340e0f14846f6f1e74bd88bff, 1 paths changed
- ../../.claude/worktrees/agent-a31851ac03d5b7c69: base 3016f85152e7c323786ad364474624a345a118d6, 2 paths changed
- ../../.claude/worktrees/agent-a3244be516d978539: base eb7529f7767ada3ebc969383d995dec9554aabbf, 2 paths changed
- ../../.claude/worktrees/agent-a40504c60401d4690: base 6ab77b717b9db3bc8dc57933e772d63f9878db35, 2 paths changed
- ../../.claude/worktrees/agent-a4244fc1e4500b1d0: base 8f0230e4e5dd2476463174172a6b66589aaf7cba, 2 paths changed
- ../../.claude/worktrees/agent-a5420607c80e0052e: base 6ab77b717b9db3bc8dc57933e772d63f9878db35, 2 paths changed
- ../../.claude/worktrees/agent-a740a6fa394f23b56: base 1543e8059d3bcea4d0653958a88c282a738f5534, 2 paths changed
- ../../.claude/worktrees/agent-a8ca7b2d3c1b780df: base 052b1677d7c5c42a730075f9c5ab1fcedcb9a4a2, 2 paths changed
- ../../.claude/worktrees/agent-a9363882f208aaa73: base 877b149575d08a5340e0f14846f6f1e74bd88bff, 1 paths changed
- ../../.claude/worktrees/agent-a9d20255edb96665c: base 8404ad585a88e058efcbb12a55ac4382231bbef9, 1 paths changed
- ../../.claude/worktrees/agent-aa607ef8a7d3ee58c: base b65005f6ba04255eabaee48d95e41a93ff4c7e8d, 2 paths changed
- ../../.claude/worktrees/agent-aa694ffd126f9fb76: base 2124bb5946c03032694f13f2528ee51907460b55, 2 paths changed
- ../../.claude/worktrees/agent-aa982faf0f983745f: base abc039af4b338302027c8f01e98380081467a8d5, 2 paths changed
- ../../.claude/worktrees/agent-aaa3eefcaef1ffb35: base 6815082a45a1108acd446265cd1c7025da1b6b6a, 2 paths changed
- ../../.claude/worktrees/agent-ab1dfb883841a9aa8: base 4694cb88751e84bab1c37318df192a45f1b89ab9, 2 paths changed
- ../../.claude/worktrees/agent-ab639ef412d90afbd: base 94b1238751fb2efba51cff4a9dfc46df5138617d, 2 paths changed
- ../../.claude/worktrees/agent-ab975fa2e855a2456: base 4e1fa93950491f0ab8f7a933f6e221846fd13852, 2 paths changed
- ../../.claude/worktrees/agent-ac079d5955b8b90a4: base 08c4d79c453001eed575b7d5b03ece9642d0f79a, 2 paths changed
- ../../.claude/worktrees/agent-af9101f80704f4b02: base 5bb48b782d3cfecdd19a9d0de5ea17b835521084, 2 paths changed
- ../../.claude/worktrees/wf_09af784e-883-1: base c90a816647aaff403c0e14e87d5631d780603efd, 2 paths changed
- ../../.claude/worktrees/wf_09af784e-883-2: base 247caa844237a8e88a1947a99bcc8ce6ce3506a1, 2 paths changed
- ../../.claude/worktrees/wf_15f82345-f50-1: base 1cb15f8648c8318bd6bc78d43512e89ab3588642, 2 paths changed
- ../../.claude/worktrees/wf_15f82345-f50-2: base 1cb15f8648c8318bd6bc78d43512e89ab3588642, 3 paths changed
- ../../.claude/worktrees/wf_15f82345-f50-3: base 1cb15f8648c8318bd6bc78d43512e89ab3588642, 2 paths changed
- ../../.claude/worktrees/wf_28531daa-606-1: base 7663072c1aa33f48b7bef3616b771e010e3cf88d, 2 paths changed
- ../../.claude/worktrees/wf_28531daa-606-2: base 5f3f9a4f1857d7dc474a5800fd5167d06b4cb046, 2 paths changed
- ../../.claude/worktrees/wf_55072de0-a21-1: base db492f7245cadfc310ec44f48a52bf9ab6373dea, 2 paths changed
- ../../.claude/worktrees/wf_55072de0-a21-2: base bcb37787a31d4b63d7003efbcee965756cb4968f, 2 paths changed
- ../../.claude/worktrees/wf_86ddefea-935-1: base 57ad9ea743e57512906f28782987bef5d55963db, 2 paths changed
- ../../.claude/worktrees/wf_86ddefea-935-2: base e0b171551f39a94547d93cdcee9b3eede0f2aa06, 2 paths changed
- ../../.claude/worktrees/wf_86ddefea-935-3: base 06a686ba2e3911b0231cd0eb0b42443efe06150b, 2 paths changed
- ../../.claude/worktrees/wf_86ddefea-935-4: base c63a11befdf0cf6e66541327999ed1708164d6bb, 2 paths changed
- ../../.claude/worktrees/wf_86ddefea-935-5: base 2e97adc452f78858cf618de95cb186008bc9151f, 2 paths changed
- ../../.claude/worktrees/wf_bef18f5c-129-1: base b5fb0a6bca2fd7965b1124f17016e05cf9644fae, 2 paths changed
- ../../.claude/worktrees/wf_bef18f5c-129-2: base e830401824f19e8ce49101f9dee34686835733b5, 2 paths changed
- ../../.claude/worktrees/wf_bef18f5c-129-3: base 7cf5a9c78e895ec6d142c6fca50cd2aa7a1a9bd0, 2 paths changed
- ../integrate-slices: base 4fcf042cbc5aac8dee796182f54d886123451c9b, 2 paths changed
- ../opus-b: base 649c6bd8f5406a46c88fff7827566abf9de21132, 19 paths changed
- ../opus-fields: base 44cf5f3b215a2817f3752ca7ad6ea91c66593fc3, 86 paths changed
- ../opus-rows: base 0dba7e32dac04599bd19a3d0a14c8be217a11bb2, 32 paths changed
- ../opus-u1: base c1f3a26aebd239016e2a0c0d381277ab68781482, 3 paths changed
- ../opus-u19b: base dd5eebbf5705208304ec60243fb6e977e2d49a4e, 73 paths changed
- ../opus-u19b/.worktrees/check-plants-e1lkm273/tree0: base dd5eebbf5705208304ec60243fb6e977e2d49a4e, 62 paths changed
- ../opus-u19b/.worktrees/check-plants-e1lkm273/tree1: base dd5eebbf5705208304ec60243fb6e977e2d49a4e, 62 paths changed
- ../opus-u19b/.worktrees/check-plants-e1lkm273/tree2: base dd5eebbf5705208304ec60243fb6e977e2d49a4e, 62 paths changed
- ../opus-u19b/.worktrees/check-plants-e1lkm273/tree3: base dd5eebbf5705208304ec60243fb6e977e2d49a4e, 62 paths changed
- ../opus-u19c: base 35090a20951737c407ee1845b30ec76ac335b57f, 86 paths changed
- ../opus-u2: base 51cd34230aec20c1cf97353ff730aa98357ac91e, 20 paths changed
- ../opus-u26b: base b786d224e98013bcaa3e9cfc08894a8f59f53f86, 27 paths changed
- ../opus-u26c: base 8b9f94562305d05d8f95e8cba01ad5f47c788598, 17 paths changed
- ../opus-u26d: base 76ca0e0b362169290a47f9b84d9c948f0d92418f, 18 paths changed
- ../opus-u26e: base cc75d21291c43b20be9a4ac49afcc75035308d05, 28 paths changed
- ../opus-u26f: base cc75d21291c43b20be9a4ac49afcc75035308d05, 14 paths changed
- ../opus-u26g: base 0bffecf95f4435078a90ad13682ac2c1f114c140, 28 paths changed
- ../opus-u26h: base fcfca56c5915a01abc54d0a47394113c81bf0141, 23 paths changed
- ../opus-u26h/.worktrees/check-plants-761ejgt6/tree0: base fcfca56c5915a01abc54d0a47394113c81bf0141, 2 paths changed
- ../opus-u26h/.worktrees/check-plants-761ejgt6/tree1: base fcfca56c5915a01abc54d0a47394113c81bf0141, 2 paths changed
- ../opus-u26h/.worktrees/check-plants-761ejgt6/tree2: base fcfca56c5915a01abc54d0a47394113c81bf0141, 2 paths changed
- ../opus-u26h/.worktrees/check-plants-761ejgt6/tree3: base fcfca56c5915a01abc54d0a47394113c81bf0141, 2 paths changed
- ../opus-u3: base 8391f0be5043b4d809bfa3a0fdb486b761a91b0e, 2 paths changed
- ../opus-u4: base 3eb49e2dcbc3d396a3bfa9271b2deb390970c1d1, 17 paths changed
- ../opus-upload: base 7c1e23c8b9933cdd590c8524cb817554fd807646, 18 paths changed
- ../opus-upload2: base 0bffecf95f4435078a90ad13682ac2c1f114c140, 37 paths changed
- ../opus-x1738: base 44cf5f3b215a2817f3752ca7ad6ea91c66593fc3, 21 paths changed
- ../rewrite-next: base e4fa6904120dbfffc5050a63801055494e99672a, 1 paths changed
- ../rewrite/.worktrees/check-plants-e7stabxu/tree0: base 8b9f94562305d05d8f95e8cba01ad5f47c788598, 6 paths changed
- ../rewrite/.worktrees/check-plants-e7stabxu/tree1: base 8b9f94562305d05d8f95e8cba01ad5f47c788598, 6 paths changed
- ../rewrite/.worktrees/check-plants-e7stabxu/tree2: base 8b9f94562305d05d8f95e8cba01ad5f47c788598, 6 paths changed
- ../rewrite/.worktrees/check-plants-e7stabxu/tree3: base 8b9f94562305d05d8f95e8cba01ad5f47c788598, 6 paths changed

## The owner's latest decisions
spec/CLAUDE.md:1848: - The owner, 2026-11-17, answering the lead's ask to mark a refused check's stop in the plan, verbatim: "yes
spec/CLAUDE.md:1858: - The owner, 2026-11-17, answering the lead's two asks on request shapes, verbatim: "i will go with your calls."
spec/CLAUDE.md:1865: - The owner, 2026-11-17, answering the lead's ask on the hub's client, verbatim: "yes on hub.clinet." Read by the
spec/CLAUDE.md:1870: - The owner, 2026-11-17, answering the lead's ask on a container's read of a member the app's JSON options leave
spec/CLAUDE.md:1874: - The owner, 2026-11-17, answering the lead's ask on GHSA-fc5q-39xc-vjpf, which failed every landing's
spec/CLAUDE.md:1880: - The owner, 2026-11-17, answering the lead's ask on FusionUploader's FilesChanged, raised by the slice's code
spec/CLAUDE.md:1890: - The owner, 2026-11-17 (verbatim in `spec/NOTES.md`): "Rewrite must apply wherever it applies. That's the point of
spec/CLAUDE.md:1901: - The owner, 2026-11-17, answering the lead's ask 4 (X17.38), verbatim: "4- yes approved". Read by the lead: a class,
spec/CLAUDE.md:1905: - The owner, 2026-11-17, answering the lead's ask B (an input bound to a whole object, a list of objects or a

## Next
<!-- next -->
In work: unit 3, piece 1, the shape kind `enum` for `as` (the owner's yes, 2026-11-17), built in `.worktrees/opus-u3`;
it lands and is pushed when its proof lines stand.

Then the harness, the one goal (the owner, 2026-11-17: "the whole harness has to be reviewed"); nothing else opens
until each line holds, by a tool where a tool can hold it:
1. One start page: the quality bar and each rule's home first, then how the work is done (root CLAUDE.md, "How you work").
2. What done means for a unit, checked by scripts/land-unit.sh: the gate, the full browser run compared test by test,
   and a planted-defect line for each promise the unit adds.
3. Agents: one brief form in the repo (scope, base, files, what done means, the proving commands, the reply's size);
   one build at a time, a read-only consult beside it.
4. The handoff generated at every push and before every compaction by a hook the repo tracks; a status is
   scripts/open.py's output.
5. CI green on GitHub's machines, the MVC 5 job running its build. At 421d7227e, the last run that finished, these
   failed: checks (after npm audit), playwright fusion (35 of 86), delivery-net10 (one architecture test) and
   verify-net48 (msbuild not found). How C# is written for net10 and net48, true of the code.
6. The conformance app's coverage of the core's public doors as a number, with no Fusion or Native component on a
   core page; the doors no passing core test's page calls listed, then closed.
7. The runtime's TypeScript and the C# core read against the rules by an independent inspector; each finding re-read
   at its source, then fixed or refused with its reason.

Behind the harness, the framework's open list: a list of files in a request form (D14.3; the runtime throws notBuilt);
a validation check on a text or a file body (a view reaches it and no line gives it a meaning: refused where the view
renders); the seven operators no C# writes leave the grammar (the owner's yes, 2026-11-17), with the table "After 1.0"
in spec/NOTES.md; the plan contract narrowed to what the C# writes. Then unit 3's second piece (the Fusion template
pages), and Fusion's own: an equality of two members in a template, the grid's validation door and exports,
SmartPaste, U27.
<!-- /next -->
