# Alis.Reactive

Reactive pages for ASP.NET MVC, written in your views as fluent, typed C#. The C# says what a page
does (react to an event, send a request, check a form on both sides, load a partial, show a branch);
the framework renders it as a plan that its browser runtime runs. Views carry no JavaScript.

It is for teams building server-rendered MVC applications, on ASP.NET Core MVC (.NET 10) or ASP.NET
MVC 5 (.NET Framework 4.8), who want dynamic pages without hand-written scripts, and who want to bring
any JavaScript component (Syncfusion, native inputs, their own) through one typed declaration.

## This branch

This branch rewrites the framework from its specification. The design is in `spec/`, and `spec/REWRITE.md` lists
the units it is built in; each unit adds the documentation of the doors it lands to this file. The released
packages are built from `main`.

## Working on the framework

`scripts/doctor.sh` checks the tools, `scripts/build.sh` builds, and `scripts/playwright.sh` runs the browser
tests one selector names (`scripts/playwright.sh --help` lists them). `CLAUDE.md` holds the rules every contributor and coding agent follows.
