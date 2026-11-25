cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
echo "--- rule files"
git grep -n -B1 -A2 -E 'BeginRequestForm' f2986ea66 -- tests/Alis.Reactive.Conformance.App/CLAUDE.md
git grep -n -A2 -E 'submission set leaves as' f2986ea66 -- tests/Alis.Reactive.Conformance.PlaywrightTests/CLAUDE.md
git grep -n -B1 -A1 -E 'BeginRequestForm' f2986ea66 -- spec/REWRITE.md
echo "--- module.md lines"
git grep -n -A2 -E 'one body door, .Form[(]form[)]|its .Form[(]formId[)]. to|.Form[(]form[)]., .Inputs[(]container[)]. and .Input. each' f2986ea66 -- spec/RequestsAndResponses/module.md
git grep -n -B1 -A1 -E 'and its .Form. and .Inputs.' f2986ea66 -- spec/TypeKnowledge/module.md
echo "--- sentences.json"
git grep -n -E 'Form' f2986ea66 -- spec/Fluent/sentences.json
echo "--- cell http.beside.hidden"
git grep -n -E 'http[.]beside[.]hidden|beside[.]hidden' f2986ea66 -- spec/Reference spec/Vendors spec/CLAUDE.md
echo "--- AdmissionFileInput declaration"
git grep -n -E 'class AdmissionFileInput' f2986ea66 -- tests
echo "--- analyzers naming Form door"
git grep -n -E '"Form"|"Inputs"|BeginRequestForm|RequestForm' f2986ea66 -- Alis.Reactive.Analyzers tests/Alis.Reactive.AnalyzerTests tests/Alis.Reactive.ArchitectureTests
echo "--- absence counts: submit listener; Container.kind read"
git grep -c -E 'addEventListener[(]"submit"|"submit"|preventDefault|requestSubmit' f2986ea66 -- 'Alis.Reactive.Assets/runtime/*.ts' 'Alis.Reactive.Native/*' 'tests/Alis.Reactive.Conformance.App/*.ts' 'tests/Alis.Reactive.Conformance.App/*.js'
git grep -c -E 'addEventListener' f2986ea66 -- 'Alis.Reactive.Assets/runtime/*.ts' ':!*.test.ts'
