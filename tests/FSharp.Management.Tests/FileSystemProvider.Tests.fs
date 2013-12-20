module FSharp.Management.Tests.FileSystemProviderTests

open FSharp.Management
open NUnit.Framework
open FsUnit

type Users = FileSystem<"C:\\Users\\">

type RelativeUsers = FileSystem<"C:\\Users", "C:\\Users">

[<Test>] 
let ``Can create type for users path``() = 
    Users.Path |> should equal @"C:\Users\"

[<Test>] 
let ``Can access the default users path``() = 
    Users.Default.Path |> should equal @"C:\Users\Default"

[<Test>]
let ``Can access the default users path via relative path``() =
    RelativeUsers.Default.Path |> should equal @".\Default"

[<Test>]
let ``Can access the bin folder within the project``() =
    RelativePath.Bin.Path |> should equal @".\bin"

// Access relative path
RelativePath.Bin.Path |> ignore

// access a file
RelativePath.``WMI.Tests.fs`` |> ignore

// access parent
RelativePath.Parent.Path |> ignore

RelativePath.Parent.Parent.Path |> ignore