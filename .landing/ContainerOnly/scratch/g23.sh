cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
git grep -n -E 'AriaRole[.]Group[)]' f2986ea66 -- 'tests/Alis.Reactive.Conformance.PlaywrightTests/*.cs' > .landing/ContainerOnly/scratch/tests-group-unnamed.txt
wc -l .landing/ContainerOnly/scratch/tests-group-unnamed.txt
git grep -n -E 'AriaRole[.]Group,' f2986ea66 -- 'tests/Alis.Reactive.Conformance.PlaywrightTests/*.cs' > .landing/ContainerOnly/scratch/tests-group-named.txt
wc -l .landing/ContainerOnly/scratch/tests-group-named.txt
git grep -n -i -E 'form' f2986ea66 -- 'Alis.Reactive.Fusion/FusionComponents/FusionUploader/*' 'Alis.Reactive.Fusion/FusionComponents/FusionRichTextEditor/*' > .landing/ContainerOnly/scratch/fusion-uploader-rte-form.txt
wc -l .landing/ContainerOnly/scratch/fusion-uploader-rte-form.txt
git show f2986ea66:tests/Alis.Reactive.Conformance.PlaywrightTests/PartialsAndSlots/MoveIn/WhenReceptionSignsInAVisitor.cs > .landing/ContainerOnly/scratch/WhenReceptionSignsInAVisitor.cs
grep -n -E 'PostData' .landing/ContainerOnly/scratch/WhenReceptionSignsInAVisitor.cs
git show f2986ea66:tests/Alis.Reactive.Conformance.PlaywrightTests/Vendors/Bench/WhenAFormSendsAHiddenInputBesideAComponentsMember.cs > .landing/ContainerOnly/scratch/WhenAFormSendsAHiddenInput.cs
grep -n -E 'sent[.]|Assert' .landing/ContainerOnly/scratch/WhenAFormSendsAHiddenInput.cs
