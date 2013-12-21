module FSharp.Management.PowerShellProvider.TypeInference

open System
open System.Management.Automation
open System.Management.Automation.Runspaces

type PSCommandLet = 
    {
        RawName : string
        Name    : string

        ResultObjectTypes : Type[]
        ResultType        : Type
        ParametersInfo    : (string*Type)[]
    }

let private getOutputTypesBasedOnTheAttributes(cmdlet:CmdletConfigurationEntry) =
    let types =
        cmdlet.ImplementingType.GetCustomAttributes(false) 
        |> Seq.filter (fun x-> x :? OutputTypeAttribute)
        |> Seq.cast<OutputTypeAttribute>
        |> Seq.fold (fun state (attr:OutputTypeAttribute) ->
            attr.Type 
            |> Array.map (fun x -> x.Type)
            |> Array.append state
            ) [||]
    match types with
    | _ when types |> Seq.exists (fun x -> x = null) // OutputTypeAttribute instantiated by string[]
        -> None 
    | _ when 0 < types.Length && types.Length <= 7
        -> types |> Array.rev |> Some
    | _ -> None

let private getOutputTypesFromSharePointCmdlets(cmdlet:CmdletConfigurationEntry) =
    let baseTy = cmdlet.ImplementingType.BaseType
    let isSPassembly = 
        [|"Microsoft.Office.Server.Search.PowerShell"; "Microsoft.SharePoint.PowerShell"|]
        |> Seq.map (fun namePref -> baseTy.FullName.StartsWith(namePref))
        |> Seq.fold (fun state value -> state || value) false
    if (not <| isSPassembly) then None
    else
        match baseTy.GenericTypeArguments with
        | [|x|] -> Some([|x|])
        | _ -> None

let getOutputTypes (cmdlet:CmdletConfigurationEntry) =
    let resultType =
        [getOutputTypesBasedOnTheAttributes;
            getOutputTypesFromSharePointCmdlets]
        |> List.map (fun f-> f cmdlet)
        |> List.fold (fun state x ->
            match state, x with
            | (Some(_),_) -> state
            | _ -> x
        ) None
    Array.append
        (defaultArg resultType Array.empty)
        [|typeof<PSObject>|]

let buildResultType possibleTypes = 
    let listOfTy ty = typedefof<list<_>>.MakeGenericType([|ty|])
    let tys = possibleTypes |> Array.map listOfTy
    match tys.Length with
    | 1 -> tys.[0]
    | 2 -> typedefof<Choice<_,_>>.MakeGenericType(tys)
    | 3 -> typedefof<Choice<_,_,_>>.MakeGenericType(tys)
    | 4 -> typedefof<Choice<_,_,_,_>>.MakeGenericType(tys)
    | 5 -> typedefof<Choice<_,_,_,_,_>>.MakeGenericType(tys)
    | 6 -> typedefof<Choice<_,_,_,_,_,_>>.MakeGenericType(tys)
    | 7 -> typedefof<Choice<_,_,_,_,_,_,_>>.MakeGenericType(tys)
    | _ -> failwithf "Unexpected number of result types '%d'" (tys.Length) //listOfTy typeof<PSObject>

let getParameterProperties (cmdlet: CmdletConfigurationEntry) =
    cmdlet.ImplementingType.GetProperties()
    |> Array.choose (fun p ->
            if p.GetCustomAttributes (typeof<ParameterAttribute>, false) |> Array.isEmpty
            then None
            else Some (p.Name, p.PropertyType))

let toCamelCase s =
    if (String.IsNullOrEmpty(s) || not <| Char.IsLetter(s.[0]) || Char.IsLower(s.[0]))
        then s
        else sprintf "%c%s" (Char.ToLower(s.[0])) (s.Substring(1))

let getTypeOfObjects (types:Type[]) (collection:PSObject seq) = 
    let applicableTypes =
        types |> Array.filter (fun ty ->
            collection |> Seq.map(fun x->x.BaseObject) |> Seq.forall (ty.IsInstanceOfType))
    match applicableTypes with
    | [|ty|] -> ty
    | _ -> typeof<PSObject>

type CollectionConverter<'T> =
    static member Convert (objSeq:obj seq) = 
        objSeq |> Seq.cast<'T> |> Seq.toList

let createPSCommandLet(cmdlet:CmdletConfigurationEntry) =
    let resultObjectTypes = cmdlet |> getOutputTypes
    {
        RawName = cmdlet.Name
        Name    = cmdlet.Name;//.Replace("-","")
        ResultObjectTypes = resultObjectTypes
        ResultType = (resultObjectTypes |> buildResultType)
        ParametersInfo = (cmdlet |> getParameterProperties)
    }