/// originally from https://gist.github.com/1241061
module internal FSharp.Management.FilesTypeProvider

open Samples.FSharp.ProvidedTypes
open FSharp.Management.Helper
open System
open System.IO

type PathType =
    | Directory
    | File

let getPathType path =
    if Directory.Exists(path) then Directory
    else if File.Exists(path) then File
    else failwith (sprintf "Path [%s] does not exist" path)

let fixDirectoryPath path =
    if path = "" then
        "."
    else if path.EndsWith(Path.AltDirectorySeparatorChar.ToString()) || path.EndsWith(Path.DirectorySeparatorChar.ToString()) then path
    else path + Path.DirectorySeparatorChar.ToString()

let fixPath path =
    let t = getPathType path
    match t with
    | File -> path
    | Directory -> fixDirectoryPath path

let GetRelativePath fromPath toPath =
    let fromUri = Uri(fixPath fromPath)
    let toUri = Uri(fixPath toPath)

    let relative = fromUri.MakeRelativeUri toUri
    let path = Uri.UnescapeDataString(relative.ToString())
    path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)

let createFileProperties (dir:DirectoryInfo,dirNodeType:ProvidedTypeDefinition,relative) =
    try
        for file in dir.EnumerateFiles() do
            try
                let path = 
                    match relative with
                    | Some sourcePath -> GetRelativePath sourcePath file.FullName
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
        | Some sourcePath -> fixDirectoryPath <| GetRelativePath sourcePath dir.FullName
        | None -> fixDirectoryPath dir.FullName

    let pathField = ProvidedLiteralField("Path",typeof<string>,path)
    pathField.AddXmlDoc(sprintf "Path to '%s'" path)
    ownerType.AddMember pathField

    createFileProperties(dir,ownerType,relative)
    let typeSet = System.Collections.Generic.HashSet()
    
    if withParent && dir.Parent <> null then
        try
            let path =
                match relative with
                | Some sourcePath -> Some((fixDirectoryPath sourcePath) + "..\\")
                | None -> None
            ownerType.AddMemberDelayed (createDirectoryNode typeSet dir.Parent "Parent" withParent relative)
        with
        | exn -> ()

    try
        for subDir in dir.EnumerateDirectories() do
            try
                ownerType.AddMemberDelayed (createDirectoryNode typeSet subDir subDir.Name false relative)
            with
            | exn -> ()
    with
    | exn -> ()

    ownerType 

and createDirectoryNode typeSet (dir: DirectoryInfo) propertyName relative =
    annotateDirectoryNode (ProvidedTypeDefinition(propertyName, Some typeof<obj>)) dir propertyName relative

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
    
    watcher.Deleted.Add onChanged
    watcher.Renamed.Add onChanged
    watcher.Created.Add onChanged
    watcher.Error.Add onChanged
    watcher.EnableRaisingEvents <- true
    ctx.Disposing.Add watcher.Dispose

let createRootType typeName (dir: DirectoryInfo) withParent relative ctx =
    watch dir.FullName ctx
    annotateDirectoryNode (erasedType<obj> thisAssembly rootNamespace typeName) dir dir.FullName withParent relative ()
    
let createRelativePathSystem (resolutionFolder: string) ctx =
    let dir = new DirectoryInfo(resolutionFolder)
    createRootType "RelativePath" dir true (Some resolutionFolder) ctx

let createTypedFileSystem ctx =  
    let typedFileSystem = erasedType<obj> thisAssembly rootNamespace "FileSystem"

    typedFileSystem.DefineStaticParameters(
        parameters = [ ProvidedStaticParameter("path", typeof<string>); ProvidedStaticParameter("relativeTo", typeof<string>, "")], 
        instantiationFunction = (fun typeName parameterValues ->
            match parameterValues with
            | [| :? string as path; :? string as relativePath |] -> 
                let dir = new DirectoryInfo(path)
                let relative = 
                    match relativePath with
                    | "" -> None
                    | _ -> Some relativePath
                createRootType typeName dir false relative ctx
            | _ -> failwith "Wrong static parameters to type provider"))
            
    typedFileSystem
