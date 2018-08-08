module internal FSharp.Management.StringReaderProvider

open ProviderImplementation.ProvidedTypes
open FSharp.Management.Helper
open System.IO

let createType typeName filePath =
    let typedStringReader = erasedType<obj> thisAssembly rootNamespace typeName

    let content = File.ReadAllText(filePath)

    let contentField = ProvidedField.Literal("Content", typeof<string>, content)
    contentField.AddXmlDoc(sprintf "Content of '%s'" filePath)
    typedStringReader.AddMember contentField

    typedStringReader

let createTypedStringReader (resolutionFolder: string) =
    let typedStringReader = erasedType<obj> thisAssembly rootNamespace "StringReader"

    typedStringReader.DefineStaticParameters(
        parameters =
            [ ProvidedStaticParameter("path", typeof<string>) ],
        instantiationFunction = (fun typeName parameterValues ->
            match parameterValues with
            | [| :? string as path |] ->
                let filePath =
                    match Path.IsPathRooted(path) with
                    | false -> Path.Combine(resolutionFolder, path)
                    | true -> path
                if not <| File.Exists(filePath) then
                    failwithf "Specified file [%s] could not be found" path
                createType typeName filePath
            | _ -> failwith "Wrong static parameters to type provider"))

    typedStringReader