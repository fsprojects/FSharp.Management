(*** hide ***)
#I "../../bin"

(**
The CommonFolders module
============================

This tutorial shows the use of the CommonFolders module. 
It simplifies runtime access to various common folder locations on the system, including 
folders related to the current user, the shared user folders, system wide folders, and
folders specific to the currently executing process.

This is particularly useful when combined with the RelativePath type provider for working
with project data and deployed.  For example, the following copies a file from the project's
doc\contents to the user's roaming application data folder.
*)

// Reference the type provider dll
#r "FSharp.Management.dll"
open FSharp.Management
open System.IO

// Get the currently executing user's application data folder
let userAppData = CommonFolders.GetUser UserPath.RoamingApplicationData

// Let the type provider do it's work - grab the docs subfolder
type Docs = RelativePath<"docs">

// now you have typed access to your filesystem
let currentFile = Docs.content.CommonFolders.fsx

currentFile
// [fsi:val it : string = "content\CommonFolders.fsx"]

// Get the location of the installed executable
let exePath = CommonFolders.GetApplication ApplicationPath.EntryPointLocation

// Build the source and target path - This assumes the exe is in bin\Release or similar
let sourcePath = Path.Combine(exePath, "..\\..", currentFile)
let targetPath = Path.Combine(userAppData, "SomeCompany\\SomeProgram", currentFile)

// Make sure it exists on the system
Directory.CreateDirectory(Path.GetDirectoryName(targetPath))
// [fsi:val it : System.IO.DirectoryInfo = ]
// [fsi:  content {Attributes = Directory; ]
// [fsi:  ... ]

// Copy the source to the target location
File.Copy(sourcePath, targetPath)
// [fsi:val it : unit = () ]

// File is now in the appropriate user's [Application data]\SomeCompany\SomeProgram\content\CommonFolders.fsx

(**

The CommonFolders module supports pulling data from various locations, based on any provided 
UserPath, SharedPath, SystemPath, or ApplicationPath.  Note that many of these paths are wrappers
around Environment.GetFolderPath, though a few pull from other APIs.

Some examples of getting common folders include:
*)

// Get the currently executing user's application data folder, both roaming and local profile
let userAppData = CommonFolders.GetUser UserPath.RoamingApplicationData
let userLocalAppData = CommonFolders.GetUser UserPath.LocalApplicationData

// Get the user's Desktop
let userDesktop = CommonFolders.GetUser UserPath.Desktop

// Get user's pictures
let userDesktop = CommonFolders.GetUser UserPath.Pictures

// Get the Windows install folder:
let windows = CommonFolders.GetSystem SystemPath.Windows

// Get the temp folder
let windows = CommonFolders.GetSystem SystemPath.Temp

// Get the shared documents folder
let sharedAppData = CommonFolders.GetSharedUser SharedPath.Documents
