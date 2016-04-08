module FSharp.Management.Tests.FileSystemProviderTests

open FSharp.Management
open NUnit.Framework
open FsUnitTyped

type Users = FileSystem<"C:\\Users">
type RelativeUsers = FileSystem<"C:\\Users", "C:\\Users">

type Relative = RelativePath<"">
type RelativeToBin = RelativePath<"bin">
type RelativeToBuild = RelativePath<"bin\\Release">

[<Test>]
let ``Can create type for users path``() =
    Users.Path |> shouldEqual @"C:\Users\"

[<Test>]
let ``Can access the default users path``() =
    Users.Default.Path |> shouldEqual @"C:\Users\Default\"

[<Test>]
let ``Can access the users path via relative path``() =
    RelativeUsers.Path |> shouldEqual "."

[<Test>]
let ``Can access the default users path via relative path``() =
    RelativeUsers.Default.Path |> shouldEqual @"Default\"

[<Test>]
let ``Can access the bin folder within the project``() =
    Relative.bin.Path |> shouldEqual @"bin\"

[<Test>]
let ``Can access the bin folder via \".\" in RelativePath provider``() =
    RelativePath<".">.bin.Path |> shouldEqual @"bin\"

[<Test>]
let ``Can access a relative path``() =
    Relative.Path |> shouldEqual @"."

[<Test>]
let ``Can access a relative subfolder``() =
    Relative.bin.Path |> shouldEqual @"bin\"

[<Test>]
let ``Can access a relative subfolder relative to .\\bin``() =
    RelativeToBin.Release.Path |> shouldEqual @"Release\"

[<Test>]
let ``Can access a relative file``() =
    Relative.``WMI.Tests.fs`` |> shouldEqual @"WMI.Tests.fs"

[<Test>]
let ``Can access a parent dir``() =
    Relative.``..``.Path |> shouldEqual @"..\"

[<Test>]
let ``Can access a parent's parent dir``() =
    Relative.``..``.``..``.Path |> shouldEqual @"..\..\"

[<Test>]
let ``Can access solution files using RelativePath provider``() =
    let fsDocPath = RelativeToBuild.``..``.``..``.``..``.``..``.docs.content.``FileSystemProvider.fsx``
    let buildFolder = CommonFolders.GetApplication ApplicationPath.FSharpManagementLocation

    let path = System.IO.Path.GetFullPath(System.IO.Path.Combine(buildFolder, fsDocPath))

    System.IO.File.Exists(path) |> shouldEqual true