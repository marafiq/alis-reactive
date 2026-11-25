cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
git grep -n -E 'BeginRequestForm\(' f2986ea66 -- '*.cshtml' > .landing/ContainerOnly/scratch/views-beginform.txt
git grep -l -E 'BeginRequestForm\(' f2986ea66 -- '*.cshtml' > .landing/ContainerOnly/scratch/views-beginform-files.txt
git grep -n -E 'BeginRequestContainer\(' f2986ea66 -- '*.cshtml' > .landing/ContainerOnly/scratch/views-begincontainer.txt
git grep -l -E 'BeginRequestContainer\(' f2986ea66 -- '*.cshtml' > .landing/ContainerOnly/scratch/views-begincontainer-files.txt
git grep -n -E '[.]Form\(' f2986ea66 -- '*.cshtml' > .landing/ContainerOnly/scratch/views-formcall.txt
git grep -l -E '[.]Form\(' f2986ea66 -- '*.cshtml' > .landing/ContainerOnly/scratch/views-formcall-files.txt
git grep -n -E '[.]Form([^(A-Za-z]|$)' f2986ea66 -- '*.cshtml' > .landing/ContainerOnly/scratch/views-formhandle.txt
git grep -l -E '[.]Form([^(A-Za-z]|$)' f2986ea66 -- '*.cshtml' > .landing/ContainerOnly/scratch/views-formhandle-files.txt
git grep -n -E '(Multipart|UrlEncoded|Json)\([a-zA-Z]+[.](Form|Container),' f2986ea66 -- '*.cshtml' > .landing/ContainerOnly/scratch/views-bodydoor-container.txt
git grep -n -E '[.]Inputs\(' f2986ea66 -- '*.cshtml' > .landing/ContainerOnly/scratch/views-inputs.txt
git grep -l -E '<form' f2986ea66 -- '*.cshtml' > .landing/ContainerOnly/scratch/views-rawform-files.txt
git grep -n -E '<form|BeginForm\(' f2986ea66 -- '*.cshtml' > .landing/ContainerOnly/scratch/views-rawform.txt
wc -l .landing/ContainerOnly/scratch/views-beginform.txt .landing/ContainerOnly/scratch/views-beginform-files.txt .landing/ContainerOnly/scratch/views-begincontainer.txt .landing/ContainerOnly/scratch/views-begincontainer-files.txt .landing/ContainerOnly/scratch/views-formcall.txt .landing/ContainerOnly/scratch/views-formcall-files.txt .landing/ContainerOnly/scratch/views-formhandle.txt .landing/ContainerOnly/scratch/views-formhandle-files.txt .landing/ContainerOnly/scratch/views-bodydoor-container.txt .landing/ContainerOnly/scratch/views-inputs.txt .landing/ContainerOnly/scratch/views-rawform-files.txt .landing/ContainerOnly/scratch/views-rawform.txt
git ls-tree -r --name-only f2986ea66 > .landing/ContainerOnly/scratch/tree.txt
grep -c -E '[.]cshtml$' .landing/ContainerOnly/scratch/tree.txt
grep -E '[.]cshtml$' .landing/ContainerOnly/scratch/tree.txt | grep -v -c -E '^tests/Alis.Reactive.Conformance.App/'
