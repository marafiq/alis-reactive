### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
TEST001 | Alis.Reactive.TestRules | Error | A browser test's project references only what a test takes
TEST002 | Alis.Reactive.TestRules | Error | A browser test takes only the listed members of Playwright
TEST003 | Alis.Reactive.TestRules | Error | A test waits for the state it names, never for time
TEST004 | Alis.Reactive.TestRules | Error | A test is never skipped, repeated, ordered or parameterized
TEST005 | Alis.Reactive.TestRules | Error | A test has no branch and no loop
TEST006 | Alis.Reactive.TestRules | Error | Assert.That reads what the browser reports beside the page
TEST008 | Alis.Reactive.TestRules | Error | A page is matched by its whole text, never by a part
TEST011 | Alis.Reactive.TestRules | Error | Only a test method asserts
TEST012 | Alis.Reactive.TestRules | Error | A test carries the one promise it proves
TEST013 | Alis.Reactive.TestRules | Error | A test performs its gesture itself
