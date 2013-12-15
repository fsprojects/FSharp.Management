(*** hide ***)
#I "../../bin"

(**
The PowerShell type provider
============================

Requirements:

- .NET 4.5
- PowerShell 3.0

This tutorial shows the use of the PowerShell type provider.
*)

// reference the type provider dll
#r @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Management.Automation\v4.0_3.0.0.0__31bf3856ad364e35\System.Management.Automation.dll"
#r "FSharpx.ManagementProviders.dll"
open FSharpx

// Let the type provider to inference signatures of available cmdlets
type PS = PowerShellProvider< PSSnapIns="", Is64BitRequired=false >

// now you have typed access to your filesystem and you can browse it via Intellisense
PS.``Get-Process``(name=[|"devenv"|])
// [fsi:val it :
//  Choice<List<System.Diagnostics.Process>,
//         List<System.Diagnostics.FileVersionInfo>,
//         List<System.Diagnostics.ProcessModule>,
//         List<System.Management.Automation.PSObject>> =
//  Choice1Of4
//    [System.Diagnostics.Process (devenv)
//       {...}]]

(**

![alt text](img/PowerShellProvider.png "Intellisense for the PowerShell")

*)

(**
#### "SharePoint 2013 Management" Sample ####

Also you can load non default SnapIns with cmdlets that you need, but you need to specify it directly.
Use `PSSnapIns` as semicolon separated string to list of all SnapIns that need to be loaded in the beginning
If some of your SnapIns require `64bit` runtime, please specify it directly.
*)
#r @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Management.Automation\v4.0_3.0.0.0__31bf3856ad364e35\System.Management.Automation.dll"
#r "FSharpx.ManagementProviders.dll"
#r "System.ServiceModel.dll"
#r "Microsoft.Sharepoint.dll"
open FSharpx

type PS64 = PowerShellProvider<PSSnapIns="Microsoft.SharePoint.PowerShell", Is64BitRequired=true >
