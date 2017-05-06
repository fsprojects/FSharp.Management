module FSharp.Management.Tests.CommonFoldersTests

open FSharp.Management
open Expecto

let [<Tests>] userDirectoryTests =
    testList "User Directory tests" [
        test "User Roaming AppData path should match Environment.GetFolderPath" {
            let env = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData)
            Expect.equal (CommonFolders.GetUser UserPath.RoamingApplicationData) env ""
        }
        test "User Local AppData path should match Environment.GetFolderPath" {
            let env = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)
            Expect.equal (CommonFolders.GetUser UserPath.LocalApplicationData) env ""
        }
        test "User desktop folder should match Environment.GetFolderPath" {
            let env = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop)
            Expect.equal (CommonFolders.GetUser UserPath.Desktop) env ""
        }
    ]

let [<Tests>] sharedUserTests =
    testList "Shared User Tests" [
        test "Shared AppData path should match Environment.GetFolderPath" {
            let env = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData)
            Expect.equal (CommonFolders.GetSharedUser SharedPath.ApplicationData) env ""
        }
        test "Shared music folder should match Environment.GetFolderPath" {
            let env = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonMusic)
            Expect.equal (CommonFolders.GetSharedUser SharedPath.Music) env ""
        }
    ]

let [<Tests>] systemTests =
    testList "System tests" [
        test "System temp folder should match Path class" {
            let pathFolder = System.IO.Path.GetTempPath()
            Expect.equal (CommonFolders.GetSystem SystemPath.Temp) pathFolder ""
        }
        test "Windows system folder should contain kernel32.dll" {
            let sysFolder = CommonFolders.GetSystem SystemPath.System
            let file = System.IO.Path.Combine(sysFolder, "kernel32.dll")
            Expect.equal (System.IO.File.Exists file) true ""
        }
    ]

let [<Tests>] applicationTests =
    testList "Application Tests" [
        test "FSharpManangement path should contain tests\\FSharp.Manangement.Tests\\bin" {
            let path = CommonFolders.GetApplication ApplicationPath.FSharpManagementLocation
            Expect.stringContains path "tests\\FSharp.Management.Tests\\bin" ""
        }
        test "FSharpManangement shadow copied path should contain FSharp.Manangement.Tests" {
            let path = CommonFolders.GetApplication ApplicationPath.FSharpManagementShadowCopiedLocation
            Expect.stringContains path "FSharp.Management.Tests" ""
        }
    ]

// Unfortunatley, most test runners fail to load the entry point assembly properly.  This may be VS, or nunit, etc, but requires
// special care to test properly
//[<Test>]
//let ``Entry location should contain nunit.exe``() =
//    let path = CommonFolders.GetApplication ApplicationPath.EntryPointLocation
//    let file = System.IO.Path.Combine(path, "nunit.ee")
//    System.IO.File.Exists file |> should equal true
