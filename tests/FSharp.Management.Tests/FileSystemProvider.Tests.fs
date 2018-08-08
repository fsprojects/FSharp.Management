module FSharp.Management.Tests.FileSystemProviderTests

open FSharp.Management
open Expecto
open System.Reflection

#if WINDOWS
type Users = FileSystem<"C:\\Users">
type RelativeUsers = FileSystem<"C:\\Users", "C:\\Users">
#else
type usr = FileSystem<"/usr">
type relative_usr = FileSystem<"/usr", "/usr">
#endif

type Relative = RelativePath<"">
type RelativeToBin = RelativePath<"bin">
type RelativeToBuild = RelativePath<"bin/Release/net462">

let sep = System.IO.Path.DirectorySeparatorChar
let appendSep str = sprintf "%s%c" str sep
let (</>) a b = System.IO.Path.Combine(a, b)

let [<Tests>] fileSystemTests =
    testList "FileSystemProvider Tests" [

#if WINDOWS            
        test "Can create type for users path" {
            Expect.equal Users.Path @"C:\Users\" ""
        }
        test "Can access the default users path" {
            Expect.equal Users.Default.Path @"C:\Users\Default\" ""
        }
        
        test "Can access the users path via relative path" {
            Expect.equal RelativeUsers.Path "." ""
        }
        test "Can access the default users path via relative path" {
            Expect.equal RelativeUsers.Default.Path @"Default\" ""
        }
#else
        test "Can create type for usr path" {
            Expect.equal usr.Path (appendSep "/usr") ""
        }
        test "Can access the /usr/local path" {
            Expect.equal usr.local.Path (appendSep "/usr/local") ""
        }
        test "Can access the usr path via relative path" {
            Expect.equal relative_usr.Path "." ""
        }
        test "Can access the default users path via relative path" {
            Expect.equal relative_usr.local.Path @"local/" ""
        }
#endif        
        
        test "Can access the bin folder within the project" {
            Expect.equal Relative.bin.Path (appendSep @"bin") ""
        }
        test "Can access the bin folder via \".\" in RelativePath provider" {
            Expect.equal RelativePath<".">.bin.Path (appendSep @"bin") ""
        }
        test "Can access a relative path" {
            Expect.equal Relative.Path @"." ""
        }
        test "Can access a relative subfolder" {
            Expect.equal Relative.bin.Path (appendSep @"bin") ""
        }
        test "Can access a relative subfolder relative to .\\bin" {
            Expect.equal RelativeToBin.Release.Path (appendSep @"Release") ""
        }
        test "Can access a relative file" {
            Expect.equal Relative.``WMI.Tests.fs`` @"WMI.Tests.fs" ""
        }
        test "Can access a parent dir" {
            Expect.equal Relative.``..``.Path (appendSep @"..") ""
        }
        test "Can access a parent's parent dir" {
            Expect.equal Relative.``..``.``..``.Path (appendSep (".." </> "..")) ""
        }

        // something about the new build folder location has this test really confused
        // test "Can access solution files using RelativePath provider" {
        //     let fsDocPath = RelativeToBuild.``..``.``..``.``..``.``..``.``..``.docs.tools.generate.fsx
        //     let buildFolder = CommonFolders.GetApplication ApplicationPath.FSharpManagementLocation

        //     let path = System.IO.Path.GetFullPath(System.IO.Path.Combine(buildFolder, fsDocPath))

        //     Expect.equal (System.IO.File.Exists(path)) true (sprintf "path %s was expected to exist but did not. Base Build Folder was %s, and generated relative path was %s" path buildFolder fsDocPath)
        // }
    ]