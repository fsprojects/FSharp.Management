(*** hide ***)
#I "../../bin"

(**
The WMI type provider
=====================

This tutorial shows the use of the [Windows Management Instrumentation (WMI)](http://msdn.microsoft.com/en-us/library/aa394582(v=vs.85).aspx) type provider.
*)

// reference the type provider dll
#r "System.Management.dll"
#r "FSharp.Management.WMI.dll"

open FSharp.Management

// Let the type provider infer the local machine
type Local = WmiProvider<"localhost">
let data = Local.GetDataContext()

(**

![alt text](img/WMIProvider.png "Intellisense for WMI")

*)

// Add a handler to watch WMI queries getting executed (optional)
data.QueryExecuted.Add(printfn "Query executed: %s")

// list all local drives
[for d in data.Win32_DiskDrive -> d.Name, d.Description]
// [fsi:Query executed: select * from Win32_DiskDrive]
// [fsi:val it : (string * string) list =]
// [fsi:  [("\\.\PHYSICALDRIVE0", "Laufwerk"); ("\\.\PHYSICALDRIVE1", "Laufwerk")]

// Access some WMI data from the data connection
[for d in data.CIM_DiskDrive -> 
    [for c in d.Capabilities -> c.Is_SMART_Notification]]
// [fsi:Query executed: select * from CIM_DiskDrive]
// [fsi:val it : bool list list = [[false; false]; [false; false]]]
