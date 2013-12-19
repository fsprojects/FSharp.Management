/// originally from https://gist.github.com/1241061
module internal FSharp.Management.FilesTypeProvider

open Samples.FSharp.ProvidedTypes
open FSharp.Management.Helper
open System.IO

let createFileProperties (dir:DirectoryInfo,dirNodeType:ProvidedTypeDefinition) =
    try
        for file in dir.EnumerateFiles() do
            try
                let pathField = ProvidedLiteralField(file.Name,typeof<string>,file.FullName)
                pathField.AddXmlDoc(sprintf "Full path to '%s'" file.FullName)
                dirNodeType.AddMember pathField
            with
            | exn -> ()
    with
    | exn -> ()

let rec annotateDirectoryNode (ownerType:ProvidedTypeDefinition,dir:DirectoryInfo,propertyName,withParent) =
    ownerType.HideObjectMethods <- true
    ownerType.AddXmlDoc(sprintf "A strongly typed interface to '%s'" dir.FullName)

    let pathField = ProvidedLiteralField("Path",typeof<string>,dir.FullName)
    pathField.AddXmlDoc(sprintf "Full path to '%s'" dir.FullName)
    ownerType.AddMember pathField

    createFileProperties(dir,ownerType)
    
    if withParent && dir.Parent <> null then
        try
            ownerType.AddMember (createDirectoryNode(dir.Parent,"Parent",withParent) ())
        with
        | exn -> ()

    try
        for subDir in dir.EnumerateDirectories() do
            try
                ownerType.AddMemberDelayed (createDirectoryNode(subDir,subDir.Name,false))
            with
            | exn -> ()
    with
    | exn -> ()

    ownerType 

and createDirectoryNode (dir:DirectoryInfo,propertyName,withParent) () =
    annotateDirectoryNode (runtimeType<obj> propertyName,dir,propertyName,withParent)

let createRelativePathSystem(resolutionFolder: string) =
    let dir = new DirectoryInfo(resolutionFolder)
    annotateDirectoryNode (erasedType<obj> thisAssembly rootNamespace "RelativePath",dir,dir.FullName,true)

let createTypedFileSystem() =  
    let typedFileSystem = erasedType<obj> thisAssembly rootNamespace "FileSystem"

    typedFileSystem.DefineStaticParameters(
        parameters = [ProvidedStaticParameter("path", typeof<string>)], 
        instantiationFunction = (fun typeName parameterValues ->
            match parameterValues with 
            | [| :? string as path |] ->
                let dir = new DirectoryInfo(path)
                annotateDirectoryNode (erasedType<obj> thisAssembly rootNamespace typeName,dir,dir.FullName,false)))

    typedFileSystem