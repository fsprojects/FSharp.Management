module FSharp.Management.Tests.PowerShellProvider

open FSharp.Management
open NUnit.Framework
open FsUnit

type PS = PowerShellProvider< PSSnapIns="", Is64BitRequired=false >

[<Test>] 
let ``Get system drive``() = 
    let items = PS.``Get-Item`` [|@"C:\"|] 
    items |> should haveLength 1
    let dir = items.Head.BaseObject :?> System.IO.DirectoryInfo
    dir.FullName |> shouldEqual @"C:\"

[<Test>]
let ``Check that process `testtest` does not started``() =
    match PS.``Get-Process``(name = [|"testtest"|]) with
    | Choice4Of4(processes) -> 
        processes |> should be Empty
    | _ -> failwith "Unexpected result"
    
[<Test>]
let ``Get list of registered snapins``() = 
    match PS.``Get-PSSnapin``(registered = true) with
    | Choice1Of2(snapins) -> 
        snapins.IsEmpty |> should be False
    | _ -> failwith "Unexpected result"
