cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
echo "--- runtime alone"
git grep -c -E 'addEventListener[(]"submit"|"submit"|preventDefault|requestSubmit' f2986ea66 -- 'Alis.Reactive.Assets/runtime/*.ts' > .landing/ContainerOnly/scratch/runtime-submit.txt
wc -l .landing/ContainerOnly/scratch/runtime-submit.txt
echo "--- status"
git status --short
echo "--- cites"
grep -n -E '^- D9[.]2 |^- D9[.]14 |^- D9[.]15 |^- D14[.]8 |^- D16[.]5 |^- D16[.]13 |^- .N14[.]3. |^- .N18[.]3. |^- .N18[.]13. |^- D19[.]7 |^- .X9[.]2. |^- .X9[.]14. |^- .X9[.]15. |^- .X9[.]16. |^- .X16[.]15. |^- .X16[.]18. |^- .N9[.]2. |^- .N13[.]1. |^- .N16[.]3. ' .landing/ContainerOnly/scratch/DESIGN.md | grep -o -E '^[0-9]+:- .?[A-Z][0-9]+[.][0-9]+'
echo "--- landing folder"
ls .landing/ContainerOnly/scratch/.. 
