(*** hide ***)
#I "../../bin"
#I @"C:\Program Files (x86)\Reference Assemblies\Microsoft\WindowsPowerShell\3.0"

(**
The PowerShell type provider
============================

Requirements:

- .NET 4.5
- PowerShell 3.0

This tutorial shows the use of the PowerShell type provider.
*)

// reference the type provider dll
#r "System.Management.Automation.dll"
#r "FSharp.Management.dll"

open FSharp.Management

// Let the type provider infer the signatures of available cmdlets
type PS = PowerShellProvider< PSSnapIns="", Is64BitRequired=false >

// now you have typed access to your PowerShell and you can browse it via Intellisense
PS.``Get-Process``(name=[|"devenv"|])
// [fsi:val it :]
// [fsi:  Choice<List<System.Diagnostics.Process>,]
// [fsi:         List<System.Diagnostics.FileVersionInfo>,]
// [fsi:         List<System.Diagnostics.ProcessModule>,]
// [fsi:         List<System.Management.Automation.PSObject>> =]
// [fsi:  Choice1Of4 [System.Diagnostics.Process (devenv)]]

(**

![alt text](img/PowerShellProvider.png "Intellisense for the PowerShell")

Working with Snapins
--------------------
*)

// get all registered PowerShell Snapins
PS.``Get-PSSnapin``(registered=true)
// [fsi:val it :]
// [fsi:  Choice<List<System.Management.Automation.PSSnapInInfo>,]
// [fsi:         List<System.Management.Automation.PSObject>> =]
// [fsi:  Choice1Of2]
// [fsi:    [MSDeploySnapin]
// [fsi:       {ApplicationBase = "C:\Program Files\IIS\Microsoft Web Deploy V3\";]
// [fsi:        AssemblyName = "Microsoft.Web.Deployment.PowerShell, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";]
// [fsi:        Description = "This is a PowerShell snap-in that contains cmdlets for managing Microsoft Web Deployment infrastructure.";]
// [fsi:        Formats = seq [];]
// [fsi:        IsDefault = false;]
// [fsi:        LogPipelineExecutionDetails = false;]
// [fsi:        ModuleName = "Microsoft.Web.Deployment.PowerShell.dll";]
// [fsi:        Name = "MSDeploySnapin";]
// [fsi:        PSVersion = 2.0;]
// [fsi:        Types = seq [];]
// [fsi:        Vendor = "Microsoft";]
// [fsi:        Version = 9.0.0.0;};]
// [fsi:     WDeploySnapin3.0]
// [fsi:       {ApplicationBase = "C:\Program Files\IIS\Microsoft Web Deploy V3\";]
// [fsi:        AssemblyName = "Microsoft.Web.Deployment.PowerShell, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";]
// [fsi:        Description = "This is a PowerShell snap-in that contains cmdlets for managing Microsoft Web Deployment infrastructure.";]
// [fsi:        Formats = seq [];]
// [fsi:        IsDefault = false;]
// [fsi:        LogPipelineExecutionDetails = false;]
// [fsi:        ModuleName = "Microsoft.Web.Deployment.PowerShell.dll";]
// [fsi:        Name = "WDeploySnapin3.0";]
// [fsi:        PSVersion = 2.0;]
// [fsi:        Types = seq [];]
// [fsi:        Vendor = "Microsoft";]
// [fsi:        Version = 9.0.0.0;}]]