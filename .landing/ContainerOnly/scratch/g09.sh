cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
echo "--- tests that open the native-set pages (files)"
git grep -l -E 'IntakeAdmit|SupplyCodes|HttpBesideHidden|IntakeDocuments|CarePages[.]Reception|ReceptionDocuments|DoseSave|DoseIntake|MoveInPages[.]Reception|Reception\b' f2986ea66 -- 'tests/Alis.Reactive.Conformance.PlaywrightTests/*.cs'
echo "--- sentences.json Form"
git grep -c -E 'Form' f2986ea66 -- spec/Fluent/sentences.json
git grep -n -E '"(concept|door|name)": "[^"]*Form[^"]*"' f2986ea66 -- spec/Fluent/sentences.json
echo "--- X10.23 and X9.2/X9.14 ids in DESIGN"
git grep -n -E '^- .X10[.]23. ' f2986ea66 -- spec/Core/DESIGN.md
echo "--- condensed Input after Inputs (BodiedParts/QueriedParts receivers)"
git grep -c -E 'CondensedRequests[.]Input<.*this Alis[.]Reactive[.]TriggersAndPipelines[.](BodiedParts|QueriedParts)<TRequest>' f2986ea66 -- Alis.Reactive/PublicAPI/net10.0/PublicAPI.Unshipped.txt
echo "--- move-in ids near 675"
git grep -n -E '^#+ .*(Reception|reception)|^[|] MI[.][0-9.]+ .*[Rr]eception' f2986ea66 -- spec/Reference/move-in.md
echo "--- BRIEF principle 6"
git grep -n -E '^6[.] |^- P6|principle 6' f2986ea66 -- spec/BRIEF.md
echo "--- Net48 twin views among BeginRequestForm"
grep -c -E 'Net48[.]cshtml' .landing/ContainerOnly/scratch/views-beginform-files.txt
echo "--- test names with Form in PlaywrightTests (files)"
grep -c -E '^tests/Alis.Reactive.Conformance.PlaywrightTests/.*Form.*[.]cs$' .landing/ContainerOnly/scratch/tree.txt
echo "--- features.json lines whose text says a form (not form data), by file"
git grep -c -E '(^|[^a-zA-Z-])(native )?forms?( |,|[.]|\x27|:|;|[)])' f2986ea66 -- 'spec/*/features.json' 'spec/Reference/app.json' 'spec/Reference/cells.json'
