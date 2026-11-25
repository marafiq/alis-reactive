cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
echo "--- DESIGN lines on what main's Form(formId) did"
git grep -n -E 'GatherBuilder[.]cs:1[4-6][0-9]|FormData\(|form[.]elements|all-registered|every registered input' f2986ea66 -- spec/Core/DESIGN.md spec/BRIEF.md
echo "--- X10.23"
git show f2986ea66:spec/Core/DESIGN.md > .landing/ContainerOnly/scratch/DESIGN.md
grep -n -A3 -E '^- .X10[.]23. ' .landing/ContainerOnly/scratch/DESIGN.md
echo "--- R lines naming forms"
grep -c -E '.R[0-9]+[.][0-9]+. .*([Ff]orm)' .landing/ContainerOnly/scratch/DESIGN.md
