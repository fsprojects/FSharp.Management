/// originally from https://gist.github.com/1241061
module internal FSharp.Management.FilesTypeProvider

open Samples.FSharp.ProvidedTypes
open FSharp.Management.Helper
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

let rec annotateDirectoryNode (ownerType:ProvidedTypeDefinition,dir:DirectoryInfo,propertyName,withParent,relative) =
    ownerType.HideObjectMethods <- true
    ownerType.AddXmlDoc(sprintf "A strongly typed interface to '%s'" dir.FullName)

    let path = 
        match relative with
        | Some sourcePath -> sourcePath + dir.Name + "/"
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
            ownerType.AddMember (createDirectoryNode(typeSet,dir.Parent,"Parent",withParent,path) ())
        with
        | exn -> ()

    try
        for subDir in dir.EnumerateDirectories() do
            try
                let path =
                    match relative with
                    | Some sourcePath -> Some(sourcePath + dir.Name + "/")
                    | None -> None
                ownerType.AddMemberDelayed (createDirectoryNode(typeSet,subDir,subDir.Name,false,path))
            with
            | exn -> ()
    with
    | exn -> ()

    ownerType 

and createDirectoryNode (typeSet,dir:DirectoryInfo,propertyName,withParent,relative) () =
    annotateDirectoryNode (nestedType<obj> typeSet propertyName,dir,propertyName,withParent,relative)

let createRelativePathSystem(resolutionFolder: string) =
    let dir = new DirectoryInfo(resolutionFolder)
    annotateDirectoryNode (erasedType<obj> thisAssembly rootNamespace "RelativePath",dir,dir.FullName,true,Some "")

let createTypedFileSystem() =  
    let typedFileSystem = erasedType<obj> thisAssembly rootNamespace "FileSystem"

    typedFileSystem.DefineStaticParameters(
        parameters = [ProvidedStaticParameter("path", typeof<string>)], 
        instantiationFunction = (fun typeName parameterValues ->
            match parameterValues with 
            | [| :? string as path |] ->
                let dir = new DirectoryInfo(path)
                annotateDirectoryNode (erasedType<obj> thisAssembly rootNamespace typeName,dir,dir.FullName,false,None)))

    typedFileSystem