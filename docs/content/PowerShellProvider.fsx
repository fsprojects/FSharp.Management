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
#r "FSharp.Management.PowerShell.dll"

open FSharp.Management

// Let the type provider infer the signatures of available cmdlets
type PS = PowerShellProvider< "Microsoft.PowerShell.Management;Microsoft.PowerShell.Core" >

// now you have typed access to your PowerShell and you can browse it via Intellisense
PS.``Get-EventLog``(logName="Application", entryType=[|"Error"|], newest=2)
// [fsi:val it :]
// [fsi:  Option<Choice<List<Diagnostics.EventLog>,List<Diagnostics.EventLogEntry>,]
// [fsi:                List<string>>> =]
// [fsi:  Some]
// [fsi:    (Choice2Of3]
// [fsi:       [System.Diagnostics.EventLogEntry]
// [fsi:          {Category = "(0)";]
// [fsi:           CategoryNumber = 0s;]
// [fsi:           Container = null;]
// [fsi:           Data = [|126uy; 0uy; 0uy; 0uy|];]
// [fsi:           EntryType = Error;]
// [fsi:           EventID = 1023;]
// [fsi:           Index = 214869;]
// [fsi:           InstanceId = 3221226495L;]
// [fsi:           MachineName = "xxx";]
// [fsi:           Message = "The description for Event ID '-1073740801' in Source 'Perflib' cannot ..."]
// [fsi:           ReplacementStrings = [|"rdyboost"; "4"|];]
// [fsi:           Site = null;]
// [fsi:           Source = "Perflib";]
// [fsi:           TimeGenerated = 7/12/2015 12:14:55 AM;]
// [fsi:           TimeWritten = 7/12/2015 12:14:55 AM;]
// [fsi:           UserName = null;};]
// [fsi:        System.Diagnostics.EventLogEntry]
// [fsi:          {Category = "(0)";]
// [fsi:           CategoryNumber = 0s;]
// [fsi:           Container = null;]
// [fsi:           Data = [||];]
// [fsi:           EntryType = Error;]
// [fsi:           EventID = 513;]
// [fsi:           Index = 214760;]
// [fsi:           InstanceId = 513L;]
// [fsi:           MachineName = "xxx";]
// [fsi:           Message = "Cryptographic Services failed while ...";]
// [fsi:           ReplacementStrings = [|""|];]
// [fsi:           Site = null;]
// [fsi:           Source = "Microsoft-Windows-CAPI2";]
// [fsi:           TimeGenerated = 7/11/2015 11:50:42 AM;]
// [fsi:           TimeWritten = 7/11/2015 11:50:42 AM;]
// [fsi:           UserName = null;}])]


(**

![alt text](img/PowerShellProvider.png "Intellisense for the PowerShell")

Manage Windows services
--------------------
*)
#r "System.ServiceProcess.dll"

let service =
    match PS.``Get-Service``(name=[|"Windows Search"|]) with
    | Some(services) when services.Length = 1 ->
        services.Head
    | _ -> failwith "Choice is ambiguous or service not found"
// [fsi:val service : ServiceProcess.ServiceController =]
// [fsi:  System.ServiceProcess.ServiceController]
// [fsi:    {CanPauseAndContinue = false;]
// [fsi:     CanShutdown = true;]
// [fsi:     CanStop = true;]
// [fsi:     Container = null;]
// [fsi:     DependentServices = [|System.ServiceProcess.ServiceController;]
// [fsi:                           System.ServiceProcess.ServiceController|];]
// [fsi:     DisplayName = "Windows Search";]
// [fsi:     MachineName = ".";]
// [fsi:     ServiceHandle = SafeServiceHandle;]
// [fsi:     ServiceName = "WSearch";]
// [fsi:     ServiceType = Win32OwnProcess;]
// [fsi:     ServicesDependedOn = [|System.ServiceProcess.ServiceController|];]
// [fsi:     Site = null;]
// [fsi:     Status = Running;}]

PS.``Start-Service``(inputObject=[|service|])

(**
Working with Snapins
--------------------
*)
// get all registered PowerShell Snapins
PS.``Get-PSSnapin``(registered=true)
// [fsi:val it : Option<List<PSSnapInInfo>> =]
// [fsi:  Some]
// [fsi:    [WDeploySnapin3.0]
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


(**
Working with Script Modules files 
-------------------- 

 - The powershell script execution should be enable on the system, make sure the execution policy is appropriately set
 - The exported function in the module file needs to have the OutputType attribute if they return a value

 This [module](..\..\tests\FSharp.Management.Tests\testModule.psm1) defition can be referenced like so 

*)

let [<Literal>]ModuleFile = __SOURCE_DIRECTORY__ + @"\..\..\tests\FSharp.Management.Tests\testModule.psm1" 
type PSFileModule =  PowerShellProvider< ModuleFile >
PSFileModule.doSomething(test="testString")
// [fsi:val it : Option<List<string>> = Some ["testString"]]