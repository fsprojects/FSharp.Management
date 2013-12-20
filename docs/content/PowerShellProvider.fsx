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
PS.``Get-EventLog``(logName="Application", entryType=[|"Error"|], newest=2)
// [fsi:val it :]
// [fsi:  Choice<List<string>,List<System.Diagnostics.EventLogEntry>,]
// [fsi:         List<System.Diagnostics.EventLog>,]
// [fsi:         List<System.Management.Automation.PSObject>> =]
// [fsi:  Choice2Of4]
// [fsi:    [System.Diagnostics.EventLogEntry]
// [fsi:       {Category = "(0)";]
// [fsi:        CategoryNumber = 0s;]
// [fsi:        Container = null;]
// [fsi:        Data = [|126uy; 0uy; 0uy; 0uy|];]
// [fsi:        EntryType = Error;]
// [fsi:        EventID = 1023;]
// [fsi:        Index = 3861;]
// [fsi:        InstanceId = 3221226495L;]
// [fsi:        MachineName = "xxxxxxxxx";]
// [fsi:        Message = "The description for Event ID '-1073740801' in Source 'Perflib' cannot be found.  The local computer may not have the necessary registry information or message DLL files to display the message, or you may not have permission to access them.  The following information is part of the event:'rdyboost', '4'";]
// [fsi:        ReplacementStrings = [|"rdyboost"; "4"|];]
// [fsi:        Site = null;]
// [fsi:        Source = "Perflib";]
// [fsi:        TimeGenerated = 12/20/2013 11:08:02 PM;]
// [fsi:        TimeWritten = 12/20/2013 11:08:02 PM;]
// [fsi:        UserName = null;};]
// [fsi:     System.Diagnostics.EventLogEntry]
// [fsi:       {Category = "Logging/Recovery";]
// [fsi:        CategoryNumber = 3s;]
// [fsi:        Container = null;]
// [fsi:        Data = [||];]
// [fsi:        EntryType = Error;]
// [fsi:        EventID = 454;]
// [fsi:        Index = 3521;]
// [fsi:        InstanceId = 454L;]
// [fsi:        MachineName = "xxxxxxxxx";]
// [fsi:        Message = "taskhostex (2416) IndexedDb: Database recovery/restore failed with unexpected error -1216.";]
// [fsi:        ReplacementStrings = [|"taskhostex"; "2416"; "IndexedDb: "; "-1216"|];]
// [fsi:        Site = null;]
// [fsi:        Source = "ESENT";]
// [fsi:        TimeGenerated = 12/20/2013 1:31:59 AM;]
// [fsi:        TimeWritten = 12/20/2013 1:31:59 AM;]
// [fsi:        UserName = null;}]]

(**

![alt text](img/PowerShellProvider.png "Intellisense for the PowerShell")

Manage Windows services
--------------------
*)
#r "System.ServiceProcess.dll"
let service = 
    match PS.``Get-Service``(name=[|"Windows Search"|]) with
    | Choice1Of2(services) when services.Length = 1 ->
        services.Head
    | _ -> failwith "Choice is ambiguous or service not found"
// [fsi:val service : System.ServiceProcess.ServiceController =]
// [fsi:  System.ServiceProcess.ServiceController]
// [fsi:    {CanPauseAndContinue = false;]
// [fsi:     CanShutdown = true;]
// [fsi:     CanStop = true;]
// [fsi:     Container = null;]
// [fsi:     DependentServices = [|System.ServiceProcess.ServiceController;]
// [fsi:                           System.ServiceProcess.ServiceController|];]
// [fsi:     DisplayName = "Windows Search";]
// [fsi:     MachineName = ".";]
// [fsi:     ServiceHandle = ?;]
// [fsi:     ServiceName = "WSearch";]
// [fsi:     ServiceType = Win32OwnProcess;]
// [fsi:     ServicesDependedOn = [|System.ServiceProcess.ServiceController|];]
// [fsi:     Site = null;]
// [fsi:     Status = Running;}]
// [fsi:  System.ServiceProcess.ServiceController]]
PS.``Start-Service``(inputObject=[|service|])

(**
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