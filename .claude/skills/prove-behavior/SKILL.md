---
name: prove-behavior
description: Prove that a Playwright test fails for the defect it names, by choosing that defect, planting it with scripts/plant-defect.sh and reading the verdict. Use when a test is written or changed, before a behavior is reported done, and when asked whether a test proves anything.
---

# Prove a behavior

A test keeps the rules in `.claude/rules/tests.md`. This skill is how to show it keeps rule 5 (it fails for the
defect it names) and rule 6 (both sides of the promise). `scripts/plant-defect.sh` does the mechanics: it replaces
one text found once, rebuilds the project's assets, runs the filtered tests, says whether the named test failed,
and restores the file on every exit, an interrupt included.

```bash
scripts/plant-defect.sh --project poc --filter "FullyQualifiedName~Properties" --test a_wrong_code_leaves_the_board_locked --in poc/runtime/src/execute.ts --replace "<text found once>" --with "<the defect>"
```

## Choose the defect

- A defect is a mistake an implementation could make in the code that owns the meaning: the C# that states it or
  the runtime that carries it out. The test project and the test app's components are the arrange, never the place
  for a defect; the tool refuses the test project.
- Each side of a promise gets its own defect. For the side where something happens, the defect stops it: the write
  lands nowhere, the value arrives as text. For the side where it must not, the defect lets it happen: the pipeline
  goes on past a missing member, a wrong code unlocks.
- Before the run, write down which test the defect turns red and which it leaves green. A test that turns red
  without being predicted shares a cause you did not see, or the defect is wider than the promise.
- No two tests fail together on every defect. Two that do prove one promise twice: find the defect that fails
  one and leaves the other green, or make them one test.

## Read the verdict

- **caught**: read the failure message the tool prints. It must be the test's own assertion missing the outcome the
  promise names, such as expected 'Fully staffed' but was 'Short staffed'. A timeout waiting for the page, a
  missing element or every test red means the defect broke the page, not the promise: choose a narrower one.
  One case is a catch with a timeout: the promise is that a view renders a read, and the only way to break it is
  to refuse the view. Then the page never boots; the evidence is the refusal's own text in the screenshot the test
  host keeps on failure, with every other test passing.
- **ESCAPED**: the test does not fail for its defect. Either the assertion is one the defect also satisfies, so the
  test gets a positive control or an exact value, or the defect changes nothing anyone can observe. Then the planted
  code is not a behavior: record the finding in `spec/NOTES.md`, and the code either gains a meaning a real case
  raises or leaves.
- A refusal or "no test ran" is no verdict. A defect the build refuses is held by the compiler and needs no test; a
  filter that runs no test proves nothing.

## Report

The proof is the tool's own lines from this run, a `[plant] caught:` line for each defect, and the clean run's
result line from `scripts/playwright.sh` after the restore. An item not exercised names who owns it, a later POC or
the `verify-net48` CI run; one this unit can reach is closed in it, not listed. Two failed attempts at one proof:
stop and research.
