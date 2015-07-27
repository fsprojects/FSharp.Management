(*** hide ***)
#I "../../bin"

(**
FSharp.Management
===========================

The FSharp.Management project contains various type providers for the management of the machine.

* [FileSystem](FileSystemProvider.html)
* [Registry](RegistryProvider.html)
* [WMI](WMIProvider.html)
* [PowerShell](PowerShellProvider.html)
* [SystemTimeZonesProvider](SystemTimeZonesProvider.html)

In addition, a set of utilities for dealing with common paths on the system at runtime are provided.

* [CommonFolders](CommonFolders.html)

<div class="row">
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      The library can be <a href="https://nuget.org/packages/FSharp.Management">installed from NuGet</a>:
      <pre>PM> Install-Package FSharp.Management</pre>
    </div>
  </div>
  <div class="span1"></div>
</div>

Example
-------

This example demonstrates the use of the FileSystem type provider:

*)
// reference the type provider dll
#r "FSharp.Management.dll"
open FSharp.Management

// Let the type provider do it's work
type Users = FileSystem<"C:\\Users\\">

// now you have typed access to your filesystem and you can browse it via Intellisense
Users.AllUsers.Path
// [fsi:val it : string = "C:\Users\All Users"]

(**

Contributing and copyright
--------------------------

The project is hosted on [GitHub][gh] where you can [report issues][issues], fork
the project and submit pull requests. If you're adding new public API, please also
consider adding [samples][content] that can be turned into a documentation. You might
also want to read [library design notes][readme] to understand how it works.

The library is available under Public Domain license, which allows modification and
redistribution for both commercial and non-commercial purposes. For more information see the
[License file][license] in the GitHub repository.

  [content]: https://github.com/fsprojects/FSharp.Management/tree/master/docs/content
  [gh]: https://github.com/fsprojects/FSharp.Management
  [issues]: https://github.com/fsprojects/FSharp.Management/issues
  [readme]: https://github.com/fsprojects/FSharp.Management/blob/master/README.md
  [license]: https://github.com/fsprojects/FSharp.Management/blob/master/LICENSE.txt
*)
