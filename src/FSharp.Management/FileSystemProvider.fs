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

let watch dir (ctx : Context) =    
    let watcher = 
        new FileSystemWatcher(
                Path.GetFullPath dir, IncludeSubdirectories = false,
                NotifyFilter = (NotifyFilters.CreationTime ||| 
                                NotifyFilters.LastWrite ||| 
                                NotifyFilters.Size ||| 
                                NotifyFilters.DirectoryName |||
                                NotifyFilters.FileName))
    let onChanged = (fun _ -> ctx.Trigger())
    
    try
        watcher.Deleted.Add onChanged
        watcher.Renamed.Add onChanged
        watcher.Created.Add onChanged
        watcher.Error.Add onChanged
        watcher.EnableRaisingEvents <- true
        ctx.Disposing.Add watcher.Dispose
    with
    | exn ->
        watcher.Dispose()

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

let rec annotateDirectoryNode (ownerType: ProvidedTypeDefinition) (dir: DirectoryInfo) propertyName withParent relative ctx () =
    watch dir.FullName ctx
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
            ownerType.AddMemberDelayed (createDirectoryNode typeSet dir.Parent ".." withParent relative ctx)
        with
        | exn -> ()

    try
        for subDir in dir.EnumerateDirectories() do
            try
                let name = subDir.Name // Pull out name first to verify permissions
                let fullName = subDir.FullName
                ownerType.AddMemberDelayed (createDirectoryNode typeSet subDir name false relative ctx)
            with
            | exn -> ()
    with
    | exn -> ()

    ownerType 

and createDirectoryNode typeSet (dir: DirectoryInfo) propertyName withParent relative ctx =
    annotateDirectoryNode (ProvidedTypeDefinition(propertyName, Some typeof<obj>)) dir propertyName withParent relative ctx

let createRootType typeName (dir: DirectoryInfo) withParent relative ctx =
    annotateDirectoryNode (erasedType<obj> thisAssembly rootNamespace typeName) dir dir.FullName withParent relative ctx ()
    
let createRelativePathSystem (resolutionFolder: string) ctx =
    let relativeFileSystem = erasedType<obj> thisAssembly rootNamespace "RelativePath"

    relativeFileSystem.DefineStaticParameters(
        parameters = [ ProvidedStaticParameter("relativeTo", typeof<string>)], 
        instantiationFunction = (fun typeName parameterValues ->
            match parameterValues with
            | [| :? string as relativePath |] ->                 
                let folder = 
                    match relativePath with
                    | "" | "." -> Path.GetFullPath(resolutionFolder)
                    | _ -> Path.GetFullPath(Path.Combine(resolutionFolder, relativePath))
                match Directory.Exists(folder) with
                | false -> failwith (sprintf "Specified directory [%s] could not be found" folder)
                | true -> createRootType typeName (new DirectoryInfo(folder)) true (Some folder) ctx
            | _ -> failwith "Wrong static parameters to type provider"))
            
    relativeFileSystem

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
                match Directory.Exists(path) with
                | false -> failwith (sprintf "Specified directory [%s] could not be found" path)
                | true -> createRootType typeName dir false relative ctx
            | _ -> failwith "Wrong static parameters to type provider"))
            
    typedFileSystem
