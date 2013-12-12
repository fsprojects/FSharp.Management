(*** hide ***)
#I "../../bin"

(**
FSharpx.ManagementProviders
===========================

Documentation

<div class="row">
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      The F# ProjectTemplate library can be <a href="https://nuget.org/packages/FSharpx.ManagementProviders">installed from NuGet</a>:
      <pre>PM> Install-Package FSharpx.ManagementProviders</pre>
    </div>
  </div>
  <div class="span1"></div>
</div>

<img src="img/logo.png" alt="F# Project" style="float:right;width:150px;margin:10px" />

Example
-------

This example demonstrates the use of the FileSystem type provider:

*)
// reference the type provider dll
#r "FSharpx.ManagementProviders.dll"
open FSharpx

// Let the type provider do it's work
type Users = FileSystem<"C:\\Users\\">

// now you have typed access to your filesystem and you can browse it via Intellisense
Users.AllUsers.Path 
// [fsi:val it : string = "C:\Users\All Users"]

(**
Some more info

Samples & documentation
-----------------------

The library comes with comprehensible documentation.

 * [API Reference](reference/index.html) contains automatically generated documentation for all types, modules
   and functions in the library. This includes additional brief samples on using most of the
   functions.
 
Contributing and copyright
--------------------------

The project is hosted on [GitHub][gh] where you can [report issues][issues], fork 
the project and submit pull requests. If you're adding new public API, please also 
consider adding [samples][content] that can be turned into a documentation. You might
also want to read [library design notes][readme] to understand how it works.

The library is available under Public Domain license, which allows modification and 
redistribution for both commercial and non-commercial purposes. For more information see the 
[License file][license] in the GitHub repository. 

  [content]: https://github.com/forki/FSharpx.ManagementProviders/tree/master/docs/content
  [gh]: https://github.com/forki/FSharpx.ManagementProviders
  [issues]: https://github.com/forki/FSharpx.ManagementProviders/issues
  [readme]: https://github.com/forki/FSharpx.ManagementProviders/blob/master/README.md
  [license]: https://github.com/forki/FSharpx.ManagementProviders/blob/master/LICENSE.txt
*)
