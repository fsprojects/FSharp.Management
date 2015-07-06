module FSharp.Management.PowerShellProvider.TypeInference

open System
open System.Management.Automation

type PSCommandLet =
    {
        Name    : string
        UniqueID: string

        ResultObjectTypes : Type[]
        ResultType        : Type
        ParametersInfo    : (string*bool*Type)[]
    }

let private getOutputTypesBasic(command:CommandInfo) =
    let types =
        command.OutputType
        |> Seq.map (fun ty -> ty.Type)
        |> Seq.filter (fun x-> x<>null) //TODO: verify this case
        |> Seq.toArray
    match types with
    | _ when 0 < types.Length && types.Length <= 7
        -> types |> Some
    | _ -> None

let getOutputTypes (command:CommandInfo) =
    let resultType = getOutputTypesBasic command
    defaultArg resultType Array.empty

let buildResultType (possibleTypes:Type[]) =
    let listOfTy ty = typedefof<list<_>>.MakeGenericType([|ty|])
    let tys = possibleTypes |> Array.map listOfTy
    let choise =
        match tys.Length with
        | 1 -> tys.[0]
        | 2 -> typedefof<Choice<_,_>>.MakeGenericType(tys)
        | 3 -> typedefof<Choice<_,_,_>>.MakeGenericType(tys)
        | 4 -> typedefof<Choice<_,_,_,_>>.MakeGenericType(tys)
        | 5 -> typedefof<Choice<_,_,_,_,_>>.MakeGenericType(tys)
        | 6 -> typedefof<Choice<_,_,_,_,_,_>>.MakeGenericType(tys)
        | 7 -> typedefof<Choice<_,_,_,_,_,_,_>>.MakeGenericType(tys)
        | _ -> typeof<PSObject> //TODO: test it
               //failwithf "Unexpected number of result types '%d'" (tys.Length) //listOfTy typeof<PSObject>
    typedefof<Option<_>>.MakeGenericType(choise)

let getParameterProperties (parameterSet: CommandParameterSetInfo) =
    match parameterSet.Parameters with
    | null -> [||]
    | parameters ->
        parameters
        |> Seq.map (fun p-> p.Name, p.IsMandatory, p.ParameterType)
        |> Seq.toArray

let toCamelCase s =
    if (String.IsNullOrEmpty(s) || not <| Char.IsLetter(s.[0]) || Char.IsLower(s.[0]))
        then s
        else sprintf "%c%s" (Char.ToLower(s.[0])) (s.Substring(1))

let getTypeOfObjects (types:Type[]) (collection:PSObject seq) =
    let applicableTypes =
        types |> Array.filter (fun ty ->
            collection |> Seq.map(fun x->x.BaseObject) |> Seq.forall (ty.IsInstanceOfType))
    match applicableTypes with
    | [|ty|] -> Some(ty)
    | _ -> None

type CollectionConverter<'T> =
    static member Convert (objSeq:obj seq) =
        objSeq |> Seq.cast<'T> |> Seq.toList

let createPSCommandLet (command:CommandInfo) (parameterSet:CommandParameterSetInfo) =
    let resultObjectTypes = command |> getOutputTypes
    {
        Name                = command.Name
        UniqueID            = command.Name + parameterSet.Name;
        ResultObjectTypes   = resultObjectTypes
        ResultType          = buildResultType resultObjectTypes
        ParametersInfo      = getParameterProperties parameterSet
    }