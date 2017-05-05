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
// [fsi:  PowerShellProvider.Types.PsCmdletResult<Choice<List<System.Diagnostics.EventLog>,]
// [fsi:                                                 List<System.Diagnostics.EventLogEntry>,]
// [fsi:                                                 List<string>>,]
// [fsi:                                          List<System.Management.Automation.ErrorRecord>>]
// [fsi:= Success]
// [fsi:    (Choice2Of3]
// [fsi:       [System.Diagnostics.EventLogEntry]
// [fsi:          {Category = "(0)";]
// [fsi:           CategoryNumber = 0s;]
// [fsi:           Container = null;]
// [fsi:           Data = [||];]
// [fsi:           EntryType = Error;]
// [fsi:           EventID = 1022;]
// [fsi:           Index = 72795;]
// [fsi:           InstanceId = 1022L;]
// [fsi:           MachineName = "xxx";]
// [fsi:           Message = ".NET Runtime version 4.0.30319.0 - Loading profiler failed during ...";]
// [fsi:           ReplacementStrings = [|".NET Runtime version 4.0.30319.0 - Loading profiler failed during ..."|];]
// [fsi:           Site = null;]
// [fsi:           Source = ".NET Runtime";]
// [fsi:           TimeGenerated = 4/8/2016 7:46:57 PM;]
// [fsi:           TimeWritten = 4/8/2016 7:46:57 PM;]
// [fsi:           UserName = "xxx";};]
// [fsi:        System.Diagnostics.EventLogEntry]
// [fsi:          {Category = "(0)";]
// [fsi:           CategoryNumber = 0s;]
// [fsi:           Container = null;]
// [fsi:           Data = [||];]
// [fsi:           EntryType = Error;]
// [fsi:           EventID = 1022;]
// [fsi:           Index = 72794;]
// [fsi:           InstanceId = 1022L;]
// [fsi:           MachineName = "xxx";]
// [fsi:           Message = ".NET Runtime version 4.0.30319.0 - Loading profiler failed during ...";]
// [fsi:           ReplacementStrings = [|".NET Runtime version 4.0.30319.0 - Loading profiler failed during ..."|];]
// [fsi:           Site = null;]
// [fsi:           Source = ".NET Runtime";]
// [fsi:           TimeGenerated = 4/8/2016 7:46:25 PM;]
// [fsi:           TimeWritten = 4/8/2016 7:46:25 PM;]
// [fsi:           UserName = "xxx";}])]

(**

![alt text](img/PowerShellProvider.png "Intellisense for the PowerShell")

Manage Windows services
--------------------
*)
#r "System.ServiceProcess.dll"

let service =
    match PS.``Get-Service``(name=[|"Windows Search"|]) with
    | Success(services) when services.Length = 1 ->
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
// [fsi:val it :]
// [fsi:  PsCmdletResult<List<System.Management.Automation.PSSnapInInfo>,]
// [fsi:                 List<System.Management.Automation.ErrorRecord>> =]
// [fsi:  Success]
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
Working with Script Module files
--------------------

 - The `PowerShell` script execution should be enable on the system, make sure the execution policy is appropriately set
   (Example: `set-executionpolicy remotesigned` Note that `x86` and `x64` runtimes have different execution policy settings)
 - The exported function in the module file needs to have the `OutputType` attribute
   if they return a value


 This following module definition

```PowerShell
function doSomething {
    [OutputType([string])]
    param (
        [string] $test
    )
    return $test
}

export-moduleMember -function doSomething
```

 can be referenced like so

*)

let [<Literal>]ModuleFile =
    __SOURCE_DIRECTORY__ + @"\..\..\tests\FSharp.Management.Tests\testModule.psm1"

type PSFileModule =  PowerShellProvider< ModuleFile >

PSFileModule.doSomething(test="testString")
// [fsi:val it : ]
// [fsi:  PsCmdletResult<List<string>,List<System.Management.Automation.ErrorRecord>>]
// [fsi:= Success ["testString"]]



(**
Parallel commands execution
--------------------

 - The "CustomRunspace" method will create a separate runspace where you can execute all provided commands. The runspace is closed on disposing
*)

use runspace = new PSFileModule.CustomRunspace()
runspace.doSomething(test="testString")

(**
This can be used to execute commands in parallel
*)

["testString1"; "testString2"; "testString3"]
    |> Seq.map (fun testString -> async {
                    use runspace = new PSFileModule.CustomRunspace()
                    runspace.doSomething(test=testString)
               })
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
        