cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
git grep -n -E 'addEventListener[(]"submit"|"submit"|preventDefault|requestSubmit' f2986ea66 -- 'Alis.Reactive.Native/*' 'tests/Alis.Reactive.Conformance.App/*.ts'
echo "--- AdmissionFileInput"
git show f2986ea66:tests/Alis.Reactive.Conformance.App/Care/MoveIn/AdmissionFileInput.cs
echo "--- Documents.cshtml file input in a container"
git grep -n -E 'BeginRequestContainer|Render<|File[(]|InputField' f2986ea66 -- tests/Alis.Reactive.Conformance.App/Care/MoveIn/Documents.cshtml
echo "--- core API InputField members"
git grep -n -E 'RenderingAndDelivery[.]InputField<TModel, TValue>[.]' f2986ea66 -- Alis.Reactive/PublicAPI/net10.0/PublicAPI.Unshipped.txt
echo "--- IInputComponent file inputs in app/native/fusion"
git grep -l -E 'IFormFile' f2986ea66 -- 'tests/Alis.Reactive.Conformance.App/*.cshtml' > .landing/ContainerOnly/scratch/views-iformfile.txt
wc -l .landing/ContainerOnly/scratch/views-iformfile.txt
git grep -n -E 'File|file' f2986ea66 -- 'tests/Alis.Reactive.Conformance.App/Care/AdmissionClinical/CarePlanUpload.cshtml'
