module FSharp.Management.Tests.FileSystemProviderTests

open FSharp.Management
open NUnit.Framework
open FsUnit

type Users = FileSystem<"C:\\Users\\">

[<Test>] 
let ``Can create type for users path``() = 
    Users.Path |> should equal @"C:\Users\"

[<Test>] 
let ``Can access the default users path``() = 
    Users.Default.Path |> should equal @"C:\Users\Default"

[<Test>] 
let ``Can access a relative path``() = 
    RelativePath.Bin.Path |> should equal "bin/"

[<Test>] 
let ``Can access a relative file``() =
    RelativePath.``WMI.Tests.fs`` |> should equal "WMI.Tests.fs"

[<Test>] 
let ``Can access a parent dir``() =
    RelativePath.Parent.Path |> should equal "../"

[<Test>] 
let ``Can access a parent's parent dir``() =
    RelativePath.Parent.Parent.Path |> should equal "../../"