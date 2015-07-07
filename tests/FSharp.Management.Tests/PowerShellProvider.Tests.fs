module FSharp.Management.Tests.PowerShellProvider

open FSharp.Management
open NUnit.Framework
open FsUnit

let [<Literal>]Modules = "Microsoft.PowerShell.Management;Microsoft.PowerShell.Core"

type PS = PowerShellProvider< Modules >

[<Test>]
let ``Get system drive``() =
    match PS.``Get-Item``(path=[|@"C:\"|]) with
    | Some(Choice5Of5 dirs) ->
        dirs |> should haveLength 1
        (Seq.head dirs).FullName |> shouldEqual @"C:\"
    | _ -> failwith "Unexpected result"

[<Test>]
let ``Check that process `testtest` does not started``() =
    match PS.``Get-Process``(name=[|"testtest"|]) with
    | None -> ignore()
    | _ -> failwith "Unexpected result"

[<Test>]
let ``Get list of registered snapins``() =
    match PS.``Get-PSSnapin``(registered = true) with
    | Some(snapins) ->
        snapins.IsEmpty |> should be False
    | _ -> failwith "Unexpected result"


type PS64 = PowerShellProvider< Modules, Is64BitRequired=true >

[<Test>]
let ``Get system drive x64``() =
    match PS64.``Get-Item``(path=[|@"C:\"|]) with
    | Some(Choice5Of5 dirs) ->
        dirs |> should haveLength 1
        (Seq.head dirs).FullName |> shouldEqual @"C:\"
    | _ -> failwith "Unexpected result"

[<Test>]
let ``Check that process `testtest` does not started x64``() =
    match PS64.``Get-Process``(name=[|"testtest"|]) with
    | None -> ignore()
    | _ -> failwith "Unexpected result"

[<Test>]
let ``Get list of registered snapins x64``() =
    match PS64.``Get-PSSnapin``(registered = true) with
    | Some(snapins) ->
        snapins.IsEmpty |> should be False
    | _ -> failwith "Unexpected result"
