cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
echo "--- A1 views: BeginRequestForm lines per tree (count of files:lines summed by awk)"
git grep -c -E 'BeginRequestForm\(' f2986ea66 -- '*.cshtml' | awk -F: '{n+=$NF; f+=1} END {print f " files, " n " lines"}'
echo "--- A1b by top folder"
git grep -c -E 'BeginRequestForm\(' f2986ea66 -- '*.cshtml' | awk -F: '{split($2,p,"/"); k=p[1]"/"p[2]; n[k]+=$NF; f[k]+=1} END {for (k in n) print k ": " f[k] " files, " n[k] " lines"}'
echo "--- A2 views: BeginRequestContainer lines"
git grep -c -E 'BeginRequestContainer\(' f2986ea66 -- '*.cshtml' | awk -F: '{split($2,p,"/"); k=p[1]"/"p[2]; n[k]+=$NF; f[k]+=1} END {for (k in n) print k ": " f[k] " files, " n[k] " lines"}'
echo "--- A3 views: [.]Form( calls and [.]Form handle reads"
git grep -c -E '[.]Form\(' f2986ea66 -- '*.cshtml' | awk -F: '{split($2,p,"/"); k=p[1]"/"p[2]; n[k]+=$NF; f[k]+=1} END {for (k in n) print k ": " f[k] " files, " n[k] " lines"}'
echo "--- A4 views: [.]Form not followed by paren (handle reads incl. inside calls)"
git grep -c -E '[.]Form([^(A-Za-z]|$)' f2986ea66 -- '*.cshtml' | awk -F: '{split($2,p,"/"); k=p[1]"/"p[2]; n[k]+=$NF; f[k]+=1} END {for (k in n) print k ": " f[k] " files, " n[k] " lines"}'
echo "--- A5 everything else (.cs, .md, .json, .sh, .tla ...) naming BeginRequestForm|RequestForm|RequestFormBlock, by file"
git grep -c -E 'BeginRequestForm|RequestForm|RequestFormBlock' f2986ea66 -- . ':!*.cshtml'
