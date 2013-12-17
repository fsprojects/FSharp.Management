(*** hide ***)
#I "../../bin"

(**
The WMI type provider
=====================

This tutorial shows the use of the WMI type provider.
*)

// reference the type provider dll
#r "System.Management.dll"
#r "FSharp.Management.WMI.dll"

open FSharp.Management

// Let the type provider infer the local machine
type Local = WmiProvider<"localhost">
let data = Local.GetDataContext()

// Add a handler to watch WMI queries getting executed (optional)
data.QueryExecuted.Add(printfn "Query executed: %s")

// list all local drives
[for b in data.Win32_DiskDrive -> b.Name, b.Description]
// [fsi:Query executed: select * from Win32_DiskDrive]
// [fsi:val it : (string * string) list =]
// [fsi:  [("\\.\PHYSICALDRIVE0", "Laufwerk"); ("\\.\PHYSICALDRIVE1", "Laufwerk")]

// Access some WMI data from the data connection
[for dd in data.CIM_DiskDrive -> 
    [for c in dd.Capabilities -> c.Is_SMART_Notification]]
// [fsi:Query executed: select * from CIM_DiskDrive]
// [fsi:val it : bool list list = [[false; false]; [false; false]]]