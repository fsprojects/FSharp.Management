namespace FSharp.Management

open System
open System.IO
open System.Reflection

[<AutoOpen>]
/// Common identifiers for specific folders on the system
module PathIdentifiers =
    /// Folders specific to the current User
    type UserPath =
        | RoamingApplicationData = 26
        | LocalApplicationData = 28
        | Desktop = 16
        | Documents = 5
        | Pictures = 39
        | Videos = 14
        | Favorites = 6
        | Startup = 7

    // Folders shared across all users of the system
    type SharedPath = 
        | ApplicationData = 35
        | Documents = 46
        | Desktop = 25
        | Music = 53
        | Pictures = 54
        | Video = 55

    /// Common system folders
    type SystemPath = 
        | Windows = 36
        | SystemX86 = 41
        | System = 37
        | Temp = -1

    /// Folders related to the currently executing application
    type ApplicationPath =
        | EntryPointLocation = 0
        | EntryPointShadowCopiedLocation = 1
        | FSharpManagementLocation = 2
        | FSharpManagementShadowCopiedLocation = 3

[<RequireQualifiedAccess>]
/// Module containing functions for working with common folder paths
module CommonFolders =
    let private getSpecialDirectory (pathID : int) =
        Environment.GetFolderPath(enum<Environment.SpecialFolder>(pathID))

    /// Get a folder from the set of current-user specific folders
    let GetUser (path : UserPath) =
        getSpecialDirectory <| int path

    /// Get a folder related to the folders shared between users of the system
    let GetSharedUser (path : SharedPath) =
        getSpecialDirectory <| int path

    /// Get a system folder
    let GetSystem (path : SystemPath) =
        match path with
        | SystemPath.Temp -> System.IO.Path.GetTempPath()
        | _ -> getSpecialDirectory <| int path

    /// Get a folder specific to the currently executing application
    let GetApplication (appPath : ApplicationPath) =
        let (codeBasePath, path) = 
            match appPath with
            | ApplicationPath.EntryPointLocation -> 
                try
                    (true, Assembly.GetEntryAssembly().CodeBase)
                with
                | exn -> failwith "Unable to load entry assembly location"
            | ApplicationPath.EntryPointShadowCopiedLocation -> 
                try
                    (false, Assembly.GetEntryAssembly().Location)
                with
                | exn -> failwith "Unable to load entry assembly location"
            | ApplicationPath.FSharpManagementLocation -> (true, Assembly.GetExecutingAssembly().CodeBase)
            | ApplicationPath.FSharpManagementShadowCopiedLocation -> (false, Assembly.GetExecutingAssembly().Location)
            | _ -> failwith "Unknown path specified"

        match codeBasePath with
        | true ->
            let uri = UriBuilder(path)
            let unesc = Uri.UnescapeDataString uri.Path
            Path.GetDirectoryName unesc
        | false ->
            Path.GetDirectoryName(path)
