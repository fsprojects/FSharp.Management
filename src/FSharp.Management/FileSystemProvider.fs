/// originally from https://gist.github.com/1241061
module internal FSharp.Management.FilesTypeProvider

open Samples.FSharp.ProvidedTypes
open FSharp.Management.Helper
open System
open System.IO

let createFileProperties (dir:DirectoryInfo,dirNodeType:ProvidedTypeDefinition,relative) =
    try
        for file in dir.EnumerateFiles() do
            try
                let path = 
                    match relative with
                    | Some sourcePath -> sourcePath + file.Name
                    | None -> file.FullName
                
                let pathField = ProvidedLiteralField(file.Name,typeof<string>,path)
                pathField.AddXmlDoc(sprintf "Path to '%s'" path)
                dirNodeType.AddMember pathField
            with
            | exn -> ()
    with
    | exn -> ()

let rec annotateDirectoryNode (ownerType: ProvidedTypeDefinition) (dir: DirectoryInfo) propertyName withParent relative () =
    ownerType.HideObjectMethods <- true
    ownerType.AddXmlDoc(sprintf "A strongly typed interface to '%s'" dir.FullName)

    let path = 
        match relative with
        | Some sourcePath -> sourcePath
        | None -> dir.FullName

    let pathField = ProvidedLiteralField("Path",typeof<string>,path)
    pathField.AddXmlDoc(sprintf "Path to '%s'" path)
    ownerType.AddMember pathField

    createFileProperties(dir,ownerType,relative)
    let typeSet = System.Collections.Generic.HashSet()
    
    if withParent && dir.Parent <> null then
        try
            let path =
                match relative with
                | Some sourcePath -> Some(sourcePath + "../")
                | None -> None
            ownerType.AddMember (createDirectoryNode typeSet dir.Parent "Parent" withParent path ())
        with
        | exn -> ()

    try
        for subDir in dir.EnumerateDirectories() do
            try
                let path =
                    match relative with
                    | Some sourcePath -> Some(sourcePath + subDir.Name + "/")
                    | None -> None
                ownerType.AddMemberDelayed (createDirectoryNode typeSet subDir subDir.Name false path)
            with
            | exn -> ()
    with
    | exn -> ()

    ownerType 

and createDirectoryNode typeSet (dir: DirectoryInfo) propertyName =
    annotateDirectoryNode (nestedType<obj> typeSet propertyName) dir propertyName

let watch dir ctx =
    let lastChanged = ref None
    let watcher = 
        new FileSystemWatcher(
                Path.GetFullPath dir, IncludeSubdirectories = true,
                NotifyFilter = (NotifyFilters.CreationTime ||| 
                                NotifyFilters.LastWrite ||| 
                                NotifyFilters.Size ||| 
                                NotifyFilters.DirectoryName |||
                                NotifyFilters.FileName))
    let onChanged = (fun _ -> 
        match !lastChanged with
        | Some time when DateTime.Now - time <= TimeSpan.FromSeconds 1. -> ()
        | _ -> lastChanged := Some DateTime.Now; ctx.OnChanged())
    watcher.Changed.Add onChanged
    watcher.Deleted.Add onChanged
    watcher.Renamed.Add onChanged
    watcher.Created.Add onChanged
    watcher.Error.Add onChanged
    watcher.EnableRaisingEvents <- true
    ctx.Disposing.Add watcher.Dispose

let createRootType typeName (dir: DirectoryInfo) withParent relative ctx =
    watch dir.FullName ctx
    annotateDirectoryNode (erasedType<obj> thisAssembly rootNamespace typeName) dir dir.FullName withParent relative ()
    
let createRelativePathSystem (resolutionFolder: string) =
    let dir = new DirectoryInfo(resolutionFolder)
    createRootType "RelativePath" dir true (Some "")

let createTypedFileSystem ctx =  
    let typedFileSystem = erasedType<obj> thisAssembly rootNamespace "FileSystem"

    typedFileSystem.DefineStaticParameters(
        parameters = [ProvidedStaticParameter("path", typeof<string>)], 
        instantiationFunction = (fun typeName parameterValues ->
            match parameterValues with 
            | [| :? string as path |] ->
                let dir = new DirectoryInfo(path)
                createRootType typeName dir false None ctx))

    typedFileSystem

