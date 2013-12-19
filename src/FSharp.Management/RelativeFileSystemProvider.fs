/// originally from https://gist.github.com/1241061
module internal FSharp.Management.RelativeFilesTypeProvider

open Samples.FSharp.ProvidedTypes
open FSharp.Management.Helper
open System.IO

let createFileProperties (dir:DirectoryInfo,dirNodeType:ProvidedTypeDefinition) =
    for file in dir.EnumerateFiles() do
        let pathField = ProvidedLiteralField(file.Name,typeof<string>,file.FullName)
        pathField.AddXmlDoc(sprintf "Full path to '%s'" file.FullName)
        dirNodeType.AddMember pathField


let rec createDirectoryNode (dir:DirectoryInfo) () =
    let dirNodeType = runtimeType<obj> dir.Name
    dirNodeType.HideObjectMethods <- true
    dirNodeType.AddXmlDoc(sprintf "A strongly typed interface to '%s'" dir.FullName)
    let pathField = ProvidedLiteralField("Path",typeof<string>,dir.FullName)
    pathField.AddXmlDoc(sprintf "Full path to '%s'" dir.FullName)
    dirNodeType.AddMember pathField

    createFileProperties(dir,dirNodeType)

    for subDir in dir.EnumerateDirectories() do
        dirNodeType.AddMemberDelayed (createDirectoryNode(subDir))

    dirNodeType


let createRelativePathSystem(resolutionFolder: string) = 
    let relativePathType = erasedType<obj> thisAssembly rootNamespace "RelativePath"

    let dir = new DirectoryInfo(resolutionFolder)

    createFileProperties(dir,relativePathType)
    for subDir in dir.EnumerateDirectories() do
        relativePathType.AddMemberDelayed (createDirectoryNode(subDir))

    relativePathType