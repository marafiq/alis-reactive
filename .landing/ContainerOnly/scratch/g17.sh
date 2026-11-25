cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
echo "--- ContainerInsideABody"
git show f2986ea66:spec/api/HttpSurface/refused/ContainerInsideABody.CS1929.cs.txt | grep -n -E 'Refused|Json|Multipart|UrlEncoded|Inputs|Form'
echo "--- FormInJson"
git show f2986ea66:spec/api/HttpSurface/refused/FormInJson.CS1929.cs.txt | grep -n -E 'Refused|Json|Multipart|UrlEncoded|Inputs|Form'
echo "--- FormOfContainer"
git show f2986ea66:spec/api/HttpSurface/refused/FormOfContainer.CS1503.cs.txt | grep -n -E 'Refused|Json|Multipart|UrlEncoded|Inputs|Form'
echo "--- test locators on a form element (lines)"
git grep -n -E 'Locator[(]"form|AriaRole[.]Form|"form#|"form\[|form >|> form' f2986ea66 -- 'tests/Alis.Reactive.Conformance.PlaywrightTests/*.cs' > .landing/ContainerOnly/scratch/tests-form-locators.txt
wc -l .landing/ContainerOnly/scratch/tests-form-locators.txt
echo "--- css keyed on form / fieldset elements (lines)"
git grep -n -E '(^|[ ,>+~])(form|fieldset)([ ,{.#:>+~]|$)' f2986ea66 -- '*.css' ':!poc/*' ':!spec/*' ':!*/wwwroot/lib/*' ':!node_modules/*' > .landing/ContainerOnly/scratch/css-form.txt
wc -l .landing/ContainerOnly/scratch/css-form.txt
grep -c -E 'fieldset' .landing/ContainerOnly/scratch/css-form.txt
echo "--- Playwright test files named Form"
grep -E '^tests/Alis.Reactive.Conformance.PlaywrightTests/.*Form.*[.]cs$' .landing/ContainerOnly/scratch/tree.txt > .landing/ContainerOnly/scratch/tests-named-form.txt
wc -l .landing/ContainerOnly/scratch/tests-named-form.txt
echo "--- features naming BeginRequestForm pages: page-doors"
git grep -c -E 'BeginRequestForm|RequestForm|"Form"' f2986ea66 -- spec/Reference/page-doors.json spec/Reference/app.json spec/Reference/matrix.json spec/Reference/native.json
