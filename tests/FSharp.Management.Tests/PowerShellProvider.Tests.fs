module FSharp.Management.Tests.PowerShellProvider

open FSharp.Management
open NUnit.Framework
open FsUnitTyped

let [<Literal>]Modules = "Microsoft.PowerShell.Management;Microsoft.PowerShell.Core"

type PS = PowerShellProvider< Modules >

[<Test>]
let ``Get system drive``() =
    match PS.``Get-Item``(path=[|@"C:\"|]) with
    | Success(Choice5Of5 dirs) ->
        dirs |> shouldHaveLength 1
        (Seq.head dirs).FullName |> shouldEqual @"C:\"
    | _ -> failwith "Unexpected result"

[<Test>]
let ``Check that process `testtest` does not started``() =
    match PS.``Get-Process``(name=[|"testtest"|]) with
    | Failure(_) -> ignore()
    | _ -> failwith "Unexpected result"

[<Test>]
let ``Get list of registered snapins``() =
    match PS.``Get-PSSnapin``(registered = true) with
    | Success(snapins) ->
        snapins.IsEmpty |> shouldEqual false
    | _ -> failwith "Unexpected result"

[<Test>]
let ``Get random number from range`` () =
    match PS.``Get-Random``(minimum = 0, maximum = 10) with
    | Success(Choice1Of3 [value]) when value >=0 && value <= 10 -> ()
    | _ -> failwith "Unexpected result"

[<Test>]
let ``Get events from event log`` () =
    match PS.``Get-EventLog``(logName="Application", entryType=[|"Error"|], newest=2) with
    | Success(Choice2Of3 entries) ->
        entries |> shouldHaveLength 2
    | _ -> failwith "Unexpected result"

// This Cmdlet has an empty OutputType
[<Test>]
let ``Get help message`` () =
    match PS.``Get-Help``() with
    | Success(resultObj) ->
        resultObj.GetType() |> shouldEqual typeof<System.Management.Automation.PSObject>
    | _ -> failwith "Unexpected result"

//This Cmdlet has a typed OutputType, but doesn't actually return anything
[<Test>]
let ``Change location`` () =
    match PS.``Set-Location``(path="""C:\Temp""") with
    | Success (resultObj) ->
        match box resultObj with
        | null -> ignore()
        | _ -> failwith "Unexpected result"
    | _ -> failwith "Unexpected result"

//let [<Literal>]ModuleFile = __SOURCE_DIRECTORY__ + @"\testModule.psm1"
//type PSFileModule =  PowerShellProvider< ModuleFile >
//
//[<Test>]
//let ``Call a function defined in a module file`` () =
//    let testString = "testString"
//    match PSFileModule.doSomething(test=testString) with
//    | Success(stringList) ->
//        stringList |> shouldHaveLength 1
//        stringList
//        |> Seq.head
//        |> shouldEqual testString
//    | _ -> failwith "Unexpected result"
//
//type PS64 = PowerShellProvider< Modules, Is64BitRequired=true >


//[<Test>]
//let ``Get system drive x64``() =
//    match PS64.``Get-Item``(path=[|@"C:\"|]) with
//    | Success(Choice5Of5 dirs) ->
//        dirs |> shouldHaveLength 1
//        (Seq.head dirs).FullName |> shouldEqual @"C:\"
//    | _ -> failwith "Unexpected result"
//
//[<Test>]
//let ``Check that process `testtest` does not started x64``() =
//    match PS64.``Get-Process``(name=[|"testtest"|]) with
//    | Failure(_) -> ignore()
//    | _ -> failwith "Unexpected result"
//
//[<Test>]
//let ``Get list of registered snapins x64``() =
//    match PS64.``Get-PSSnapin``(registered = true) with
//    | Success(snapins) ->
//        snapins.IsEmpty |> shouldEqual false
//    | _ -> failwith "Unexpected result"

//let modules = "ActiveDirectory;AppBackgroundTask;AppLocker;Appx;AssignedAccess;Azure;BestPractices;BitLocker;BranchCache;CimCmdlets;ClusterAwareUpdating;DFSN;DFSR;Defender;DhcpServer;DirectAccessClientComponents;Dism;DnsClient;DnsServer;FailoverClusters;GroupPolicy;Hyper-V;ISE;International;IpamServer;IscsiTarget;Kds;MMAgent;Microsoft.PowerShell.Core;Microsoft.PowerShell.Diagnostics;Microsoft.PowerShell.Host;Microsoft.PowerShell.Management;Microsoft.PowerShell.Security;Microsoft.PowerShell.Utility;Microsoft.WSMan.Management;MsDtc;NFS;NetAdapter;NetConnection;NetEventPacketCapture;NetLbfo;NetNat;NetQos;NetSecurity;NetSwitchTeam;NetTCPIP;NetWNV;NetworkConnectivityStatus;NetworkLoadBalancingClusters;NetworkTransition;PKI;PSDesiredStateConfiguration;PSDiagnostics;PSScheduledJob;PSWorkflow;PcsvDevice;PrintManagement;RemoteAccess;RemoteDesktop;ScheduledTasks;SecureBoot;ServerManager;ServerManagerTasks;SmbShare;SmbWitness;StartScreen;Storage;TLS;TroubleshootingPack;TrustedPlatformModule;UpdateServices;VpnClient;Wdac;WebAdministration;WindowsDeveloperLicense;WindowsErrorReporting;WindowsSearch;iSCSI".Split([|';'|])