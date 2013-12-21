(*** hide ***)
#I "../../bin"

(**
The FileSystem type provider
============================

This tutorial shows the use of the file system type provider. 
It allows to browse your file system via Intellisense and provides compile time checks for directories and files.
The FileSystem type provider invalidates itself whenever any child dirs/files changed in any way.
*)

// reference the type provider dll
#r "FSharp.Management.dll"
open FSharp.Management

// Let the type provider do it's work
type Users = FileSystem<"C:\\Users\\">

// now you have typed access to your filesystem
Users.``All Users``.Path 
// [fsi:val it : string = "C:\Users\All Users"]

(**

![alt text](img/FileSystemProvider.png "Intellisense for the file system")

Relative paths
--------------

For web frameworks it's interesting to reference resources like images.
With the help of the FileSystemProvider we can browse the project via Intellisense and get compile time safety for relative paths.

![alt text](img/RelativeFileSystemProvider.png "Intellisense for the current subfolders")

*)

// reference the type provider dll
#r "FSharp.Management.dll"
open FSharp.Management

// browse the project
RelativePath.Parent.files.img.``PowerShellProvider.png``
// [fsi:val it : string = "../docs/files/PowerShellProvider.png"]