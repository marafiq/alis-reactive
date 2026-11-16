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

## Unicode CLDR data

On .NET Framework 4.8, AlisReactive names a view's time zone to the browser by its IANA id, which it reads from a
table generated from Unicode CLDR's `common/supplemental/windowsZones.xml` at release-49
(`spec/probes/cldr/windowsZones.xml.txt`, pinned by `spec/probes/cldr/windowsZones.lock`). CLDR releases that file
under the Unicode License V3, whose notice follows.

```text
COPYRIGHT AND PERMISSION NOTICE

Copyright © 2001–2026 Unicode, Inc.

NOTICE TO USER: Carefully read the following legal agreement. BY
DOWNLOADING, INSTALLING, COPYING OR OTHERWISE USING DATA FILES, AND/OR
SOFTWARE, YOU UNEQUIVOCALLY ACCEPT, AND AGREE TO BE BOUND BY, ALL OF THE
TERMS AND CONDITIONS OF THIS AGREEMENT. IF YOU DO NOT AGREE, DO NOT
DOWNLOAD, INSTALL, COPY, DISTRIBUTE OR USE THE DATA FILES OR SOFTWARE.

Permission is hereby granted, free of charge, to any person obtaining a
copy of data files and any associated documentation (the "Data Files") or
software and any associated documentation (the "Software") to deal in the
Data Files or Software without restriction, including without limitation
the rights to use, copy, modify, merge, publish, distribute, and/or sell
copies of the Data Files or Software, and to permit persons to whom the
Data Files or Software are furnished to do so, provided that either (a)
this copyright and permission notice appear with all copies of the Data
Files or Software, or (b) this copyright and permission notice appear in
associated Documentation.

THE DATA FILES AND SOFTWARE ARE PROVIDED "AS IS", WITHOUT WARRANTY OF ANY
KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT OF
THIRD PARTY RIGHTS.

IN NO EVENT SHALL THE COPYRIGHT HOLDER OR HOLDERS INCLUDED IN THIS NOTICE
BE LIABLE FOR ANY CLAIM, OR ANY SPECIAL INDIRECT OR CONSEQUENTIAL DAMAGES,
OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS,
WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION,
ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THE DATA
FILES OR SOFTWARE.

Except as contained in this notice, the name of a copyright holder shall
not be used in advertising or otherwise to promote the sale, use or other
dealings in these Data Files or Software without prior written
authorization of the copyright holder.
```
