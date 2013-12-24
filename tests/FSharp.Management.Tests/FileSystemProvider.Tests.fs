module FSharp.Management.Tests.FileSystemProviderTests

open FSharp.Management
open NUnit.Framework
open FsUnit

type Users = FileSystem<"C:\\Users">
type RelativeUsers = FileSystem<"C:\\Users", "C:\\Users">

type RelativePathL = RelativePath<"">
type RelativePathB = RelativePath<"bin">

[<Test>]
let ``Can create type for users path``() = 
    Users.Path |> should equal @"C:\Users\"

[<Test>] 
let ``Can access the default users path``() =
    Users.Default.Path |> should equal @"C:\Users\Default\"

[<Test>]
let ``Can access the users path via relative path``() =
    RelativeUsers.Path |> should equal "."

[<Test>]
let ``Can access the default users path via relative path``() =
    RelativeUsers.Default.Path |> should equal @"Default\"

[<Test>]
let ``Can access the bin folder within the project``() =
    RelativePath<"">.bin.Path |> should equal @"bin\"

[<Test>]
let ``Can access a relative path``() =
    RelativePathL.Path |> should equal @"."

[<Test>] 
let ``Can access a relative subfolder``() =
    RelativePathL.bin.Path |> should equal @"bin\"
    RelativePathB.Debug.Path |> should equal @"Debug\"

[<Test>] 
let ``Can access a relative file``() =
    RelativePathL.``WMI.Tests.fs`` |> should equal @"WMI.Tests.fs"

[<Test>] 
let ``Can access a parent dir``() =
    RelativePathL.Parent.Path |> should equal @"..\"

[<Test>] 
let ``Can access a parent's parent dir``() =
    RelativePathL.Parent.Parent.Path |> should equal @"..\..\"
