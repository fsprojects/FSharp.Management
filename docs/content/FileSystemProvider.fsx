(*** hide ***)
#I "../../bin"

(**
The FileSystem type provider
============================

This tutorial shows the use of the file system type provider.
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

![alt text](img/FileSystemProvider.png "Intellisense for the file system")

*)