(*** hide ***)
#I "../../bin"

(**
The Registry type provider
==========================

This tutorial shows the use of the registry type provider.
*)

// reference the type provider dll
#r "FSharp.Management.dll"
open FSharp.Management

// use the registry type provider to get typed access to your registry and browse it via Intellisense
Registry.HKEY_CURRENT_USER.Path 
// [fsi:val it : string = "HKEY_CURRENT_USER"]

Registry.HKEY_LOCAL_MACHINE.SOFTWARE.Microsoft.Path
// [fsi:val it : string = "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft"]

(**

![alt text](img/RegistryProvider.png "Intellisense for the registry")

*)