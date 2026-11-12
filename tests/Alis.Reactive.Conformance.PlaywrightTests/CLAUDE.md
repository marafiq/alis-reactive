# Alis.Reactive.Conformance.PlaywrightTests

The browser tests: each is one promise of the spec, proven by a gesture and its visible outcome in Chromium, on a
screen of the conformance app. `.claude/rules/tests.md` holds the ten rules and loads with any file here. A test is
written once, at the full bar, from its promise and before its code: it fails until the code lands, proves the
behavior when it turns green, and then fails on its planted defect (`scripts/plant-defect.sh`).

## What this project adds

- It compiles against Playwright, NUnit and the world contract (`tests/Alis.Reactive.Conformance.World`) and against
  no assembly of the framework or of the app, so a test finds an element as a user does, by its role, label or text,
  or by an id written as a literal, and expects a literal.
- A test carries one `[FailsFor]` naming its constant in `tests/Alis.Reactive.Conformance.World/Promises.g.cs`. Its
  fixture is `When` and its situation, takes its hosts by `TestFixtureSource`, and sits in the folder of the module
  whose test it is, under the screen it runs on. `Harness/` holds what every module's tests use, and `SliceTree.txt`
  orders the folders: `Harness/`, then one row of module folders, each entry a module's screens (`Schema/*`), so no
  module's tests use another's helpers.
- The build holds a test to `.claude/rules/tests.md`: the project imports
  `tests/Alis.Reactive.TestRules/Alis.Reactive.TestRules.targets`, and each test rule, `TEST001` to `TEST013`, refuses
  its defect where the test is written, with the cure in its message (`spec/REWRITE.md`, section 2.10). Until U3
  writes the last test the project says its tests are still being written; after that a promise no test carries
  fails the build.
- Each test runs in a world of its own: `ConformanceFixture` opens it, and `TestWorld` seeds it through the world
  contract's typed builders and keeps the errors its page's console holds, which a test reads with `Assert.That`.
  Teardown ends the world and fails a gate still parked.
- A refusal where the view renders is read as its developer reads it: the heading of the page ASP.NET shows for
  an exception, the exception's name, a colon and the whole message, found by its role and its exact name.
  `ValuesAndSources/Refusals/WhenALiteralIsPastTheNumbersJavaScriptHolds.cs` is the pattern, the page that renders
  beside it in a second world. A message names a component's member as its declaration does, the class, a dot and
  the member.
- A bench cell's test is written from its cell's promise in `spec/Reference/native.json`, in `Vendors/Bench/`:
  `Vendors/Bench/WhenAStepReadsAComponentsProperty.cs` is the pattern. The page's own element is found by the id
  the promise names, a button by its role and name, and what the component shows by its role: its heading, and a
  line the promise names, a status named for the line with its first letter a capital (the raise line is the
  status Raise, the count of raises the status Raises, the payload line the status Payload, the held line the
  status Held; a peer's ratio is the status Ratio inside the region Peer). A name that opens another's is matched whole. An outcome
  that an element stays empty is read after the status that shows the handler returned, and stands beside the
  opposite case that fills it (`Vendors/Bench/WhenAnEventsPayloadHoldsAPromiseWhereAValueWasDeclared.cs`). A cell that holds the settle gate
  arms it before its gesture and reads the held state before its release; its refused case runs ungated and reads
  the status Settle first (`Vendors/Bench/WhenAStepAwaitsAComponentsMethod.cs`). A value picked in the component
  is two presses on its listbox named Value, the down arrow and Enter
  (`Vendors/Bench/WhenAComponentsMemberIsSetAsAnElementsText.cs`). A request a promise states is read as it leaves the
  page: the test waits for it around its gesture and asserts its method, the end of its address, the path and
  query the promise states, since its origin is the host's, and its body
  (`Vendors/Bench/WhenAGetSendsAComponentsMemberInItsQuery.cs`). The opposite case runs in a second world, seeded through a
  typed builder of the world contract where the promise changes what the component holds.
- A run opens one browser for every fixture of the assembly and closes it, with every host, at its end:
  `Harness/Run.cs` applies a test action of NUnit's to the assembly, since a setup fixture covers its own namespace
  alone and a module's tests sit in the module's.
- A test whose page asks for Syncfusion's script, an address that ends in `"/ej2.min.js"`, fails at its teardown,
  which names the address: a core test's page holds no component library, and `ConformanceFixture` reads every
  request of a world's page and of a person's. The last command below plants that script's link on the World area's
  pages and reads the refusal; the same plant with another script's name passes.
- A test of an async lane holds the machine at one named TLA+ action: it arms the gate before its gesture, waits
  until the harness reports it held, asserts the held state, and releases the held gate that wait returned. It never
  provokes an ordering by timing, by a stubbed network or by a control on the page. A test that holds one action at
  two endpoints arms a hold for each, naming the endpoint by its constant of the world contract, and releases each
  on its own (`Harness/WhenAGateIsHeldAtTwoEndpoints.cs`).
- `Harness/measured.json` holds what 20 green runs measured: the budget of one test, four times the slowest test,
  and the two waits, each four times the slowest step of its kind, a navigation's and every other step's. The runner
  writes it (`--measure`) and fails a run that holds a test over the budget. `Harness/Waits.cs` sets the waits on
  every browser context and on every expectation, so a test sets no wait of its own; in a measured run it sets none
  and records each step from Playwright's trace. The file also holds the count of tests a run takes at once, which
  `--measure-parallelism` measures: 20 runs at each of 1, 2, 4 and 8, and a count above one is kept only when its
  runs are green and took no more than the runs at one divided by 1.6. Fixtures run beside each other at that count,
  a fixture's tests one after another, and the budget is measured at it. A hand never edits the file: the runner
  refuses a budget or a wait that is not four times what it measured, a count its wall times do not keep, and a
  budget measured at another count.
- `shard-weights.json` holds the seconds each shard unit took in one run, which `--measure` writes: a module folder,
  a slice under FusionComponents, and the host variants' tests as one unit. CI runs a pool in shards (`--pool`,
  `--shard`), its units placed longest first, each on the lightest shard; a unit with no weight is placed last.
- `Harness/WhenAGateHoldsAnAnswer.cs` is the pattern: the page, the gesture and the outcome are in the test's body,
  and the opposite case stands beside it.

## Bad tests, each beside the rule it breaks

Read from main at 7b9e987cc and from the redesign at 65fdf118d, in tests/Alis.Reactive.PlaywrightTests and
tests/Alis.Reactive.Framework.PlaywrightTests. Each passed there, and each is refused here.

Rule 7, no tautology: the id the code computes. Redesign,
Components/AppLevel/PharmacyDelivery/WhenTheNightNurseReceivesADelivery.cs line 35; 51 files of the redesign and 14
of main do it. The generator that writes the input's id also finds it, so a wrong id passes. Here the test names the
input by its label, and the tests of InputsAndIds expect each derived id as a literal.

```csharp illustration
private ILocator ReceivedBy => Page.Locator($"#{IdGenerator.For<PharmacyDeliveryModel>(m => m.ReceivedBy)}");
```

Rules 3 and 4, real collaborators and an observed outcome: script in the page. Main,
Components/Native/WhenButtonFires.cs line 69. The test writes the element's text itself, which no user can, to make
its second assertion mean something. Here the opposite case is a gesture the page offers.

```csharp illustration
await admitStatus.EvaluateAsync("el => el.textContent = 'reset'");
await Page.Locator("#btn-admit").ClickAsync();
await Expect(admitStatus).ToHaveTextAsync("Admit Resident clicked", new() { Timeout = 3000 });
```

Rule 8, deterministic: a wait on time. Main,
Components/Fusion/InPlaceEditor/WhenInPlaceEditorQuickEditBlocksOnValidationRule.cs line 33. It sleeps 2.8 seconds
and then reads a flag, so it is slow when the code is right and green when a slow machine sends the request late.
Here the test expects the state the refusal shows, and the request that was held back is read from what the harness
collected.

```csharp illustration
await inner.PressAsync("Enter");
await Page.WaitForTimeoutAsync(2800);
Assert.That(postFired, Is.False, "FluentValidator must abort the POST when Value violates GreaterThan(0).");
```

Rule 9, readable as the specification: a branch on a one-shot read. Main,
Components/Fusion/Kanban/WhenUsingFusionKanban.cs line 138. The test does not know which page it is on, so it
proves neither. Here a test has no branch, and the page's state is the promise's own.

```csharp illustration
if (await Page.Locator("#care-kanban_dialog_wrapper.e-popup-open").IsVisibleAsync())
    await Page.Keyboard.PressAsync("Escape");
```

Rule 7 again: an expected value the test computes from the page. Redesign,
Components/Fusion/Schedule/WhenUsingFusionSchedule.cs line 424. The count comes from a one-shot read of the summary
it then asserts, through a pattern that matches any prefix. Here the world is seeded, so the count is a literal.

```csharp illustration
var before = await ShiftsNeedingCoverage();
await Expect(Summary).ToHaveTextAsync(new Regex($@": {before - 1} of 21 shifts"), new() { Timeout = 15000 });
```

Rule 3 again: a stubbed network. Redesign,
Components/AppLevel/PharmacyDelivery/WhenTheNightNurseReceivesADelivery.cs line 157; 19 files of the redesign and 8
of main route a request. The test's own semaphore decides when the answer leaves, so the server's part of the
promise never runs. Here the app's endpoint holds its own answer at a gate, and the test releases it.

```csharp illustration
await Page.RouteAsync(ManifestUrl, async route =>
{
    await manifestReleased.WaitAsync();
    await route.ContinueAsync();
});
```

Rule 7 a third time: a partial match. Redesign, Conditions/WhenAConditionAsksTheUser.cs line 24, in
tests/Alis.Reactive.Framework.PlaywrightTests; 61 files of the redesign and 73 of main do it. Text before or after
the expected words passes. Here the whole text is expected.

```csharp illustration
await Expect(Dialog).ToContainTextAsync("Discharge the resident?");
```

## Commands

```bash
scripts/playwright.sh --unit U2
scripts/playwright.sh --unit U3
scripts/playwright.sh WhenTwoWorldsRun
scripts/playwright.sh --measure-parallelism --all
scripts/playwright.sh --measure --all
scripts/playwright.sh --pool core --shard "1/1"
scripts/plant-defect.sh --test Harness.WhenTwoWorldsRun_each_world_lists_only_the_residents_it_seeded --in tests/Alis.Reactive.Conformance.App/World/WorldArea.cs --replace "Of(key)" --with "Of(string.Empty)"
scripts/plant-defect.sh --test M14_F1.X19_3_0 --in Alis.Reactive.Assets/runtime/Schema/read-plan.ts --replace "plan.version === pageVersion" --with "plan.version > 0"
scripts/plant-defect.sh --test Harness.WhenAGateIsHeldAtTwoEndpoints_each_endpoints_answer_waits_for_its_own_release --in tests/Alis.Reactive.Conformance.App/World/WorldData.cs --replace '(order.Gate, order.At)] = new' --with '(order.Gate, Anywhere)] = new'
scripts/plant-defect.sh --test Harness.WhenTwoWorldsRun_each_world_lists_only_the_residents_it_seeded --in tests/Alis.Reactive.Conformance.App/World/WorldArea.cs --replace '<title>{Text(title)}</title>' --with '<title>{Text(title)}</title><script src=\"/js/ej2.min.js\"></script>'
```
