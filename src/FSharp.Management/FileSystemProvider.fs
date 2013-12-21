/// originally from https://gist.github.com/1241061
module internal FSharp.Management.FilesTypeProvider

open Samples.FSharp.ProvidedTypes
open FSharp.Management.Helper
open System
open System.IO

let nestedTypeWithoutNiceName<'T> typeSet typeName = 
    let uniqueName originalName (set:System.Collections.Generic.HashSet<_>) =
        let mutable name = originalName
        while set.Contains name do 
          let mutable lastLetterPos = String.length name - 1
          while Char.IsDigit name.[lastLetterPos] && lastLetterPos > 0 do
            lastLetterPos <- lastLetterPos - 1
          if lastLetterPos = name.Length - 1 then
            name <- name + "2"
          elif lastLetterPos = 0 then
            name <- (UInt64.Parse name + 1UL).ToString()
          else
            let number = name.Substring(lastLetterPos + 1)
            name <- name.Substring(0, lastLetterPos + 1) + (UInt64.Parse number + 1UL).ToString()
        set.Add name |> ignore
        name
    ProvidedTypeDefinition(uniqueName typeName typeSet, Some typeof<'T>)

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
    annotateDirectoryNode (nestedTypeWithoutNiceName<obj> typeSet propertyName) dir propertyName

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

