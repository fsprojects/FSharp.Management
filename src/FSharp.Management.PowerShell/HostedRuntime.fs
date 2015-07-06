module FSharp.Management.PowerShellProvider.HostedRuntime

open System
open System.Management.Automation
open System.Management.Automation.Runspaces
open System.Threading
open System.Security.Principal

open TypeInference

type IPSRuntime =
    abstract member AllCmdlets  : unit -> PSCommandLet[]
    abstract member Execute     : string * obj seq -> obj
    abstract member GetXmlDoc   : string -> string

/// PowerShell runtime built-in current process
type PSRuntimeHosted(snapIns:string[], modules:string[]) =
//  let modules = [|"Azure"; "ActiveDirectory"|]
//  let snapIns = [|""|]
    let runSpace =
        try
            let initState = InitialSessionState.CreateDefault()

            for snapIn in snapIns do
                if not <| String.IsNullOrEmpty(snapIn) then
                    let _, ex = initState.ImportPSSnapIn(snapIn)
                    if ex <> null then
                        failwithf "ImportPSSnapInExceptions: %s" ex.Message

            let modules = modules |> Array.filter (String.IsNullOrWhiteSpace >> not)
            if not <| Array.isEmpty modules then
                initState.ImportPSModule(modules);

            let rs = RunspaceFactory.CreateRunspace(initState)
            Thread.CurrentPrincipal <- GenericPrincipal(GenericIdentity("PowerShellTypeProvider"), null)
            rs.Open()
            rs
        with
        | e -> failwithf "Could not create PowerShell Runspace: '%s'" e.Message

    let commandInfos =
        let ps  = PowerShell.Create(Runspace=runSpace)
        //Get-Command -CommandType @("cmdlet","function") -ListImported
        ps.AddCommand("Get-Command")
          .AddParameter("CommandType", ["cmdlet"; "function"])
          .AddParameter("ListImported")
          .Invoke()
        |> Seq.toArray

    let commands =
        try
            commandInfos
            |> Seq.map (fun x ->
                match x.BaseObject with
                | :? CommandInfo as ci -> ci
                | w -> failwithf "Unsupported type of command: %A" w)
            |> Seq.map (fun cmd ->
                match cmd with
                | :? CmdletInfo | :? FunctionInfo->
                    if cmd.ParameterSets.Count > 0 then
                        seq {
                            for pSet in cmd.ParameterSets do
                                let wrapper = createPSCommandLet cmd pSet
                                yield wrapper.UniqueID, wrapper
                        }
                    else
                        failwith "Cmdlet/Function is not loaded: %A" cmd
                | _ -> failwithf "Unexpected type of command: %A" cmd
            )
            |> Seq.concat
            |> Map.ofSeq
        with
        | e -> failwithf "Could not load cmdlets: %s\n%s" e.Message e.StackTrace

    interface IPSRuntime with
        member __.AllCmdlets() =
            commands |> Map.toSeq |> Seq.map snd |> Seq.toArray
        member __.Execute(uniqueId, parameters:obj seq) =
            // Create command
            let cmdlet = commands.[uniqueId]
            //TODO: Execute commands using PowerShell class
            let command = Command(cmdlet.Name)
            parameters |> Seq.iteri (fun i value->
                let key, _,ty = cmdlet.ParametersInfo.[i]
                match ty with
                | _ when ty = typeof<System.Management.Automation.SwitchParameter>->
                    if (unbox<bool> value) then
                        command.Parameters.Add(CommandParameter(key))
                | _ when ty.IsValueType ->
                    if (value <> System.Activator.CreateInstance(ty))
                    then command.Parameters.Add(CommandParameter(key, value))
                | _ ->
                    if (value <> null)
                    then command.Parameters.Add(CommandParameter(key, value))
            )
            // Execute
            let pipeline = runSpace.CreatePipeline()
            pipeline.Commands.Add(command)
            let result = pipeline.Invoke()

            // Format result
            let tys = cmdlet.ResultObjectTypes
            let tyOfObjOpt = TypeInference.getTypeOfObjects tys result

            match tyOfObjOpt with
            | None -> box None
            | Some(tyOfObj) ->
                let collectionConverter =
                    typedefof<TypeInference.CollectionConverter<_>>.MakeGenericType(tyOfObj)
                let objectCollection =
                    if (tyOfObj = typeof<PSObject>) then box result
                    else result |> Seq.map (fun x->x.BaseObject) |> box
                let typedCollection =
                    collectionConverter.GetMethod("Convert").Invoke(null, [|objectCollection|])

                let choise =
                    if (tys.Length = 1)
                    then typedCollection
                    else let ind = tys |> Array.findIndex (fun x-> x = tyOfObj)
                         let funcName = sprintf "NewChoice%dOf%d" (ind+1) (tys.Length)
                         cmdlet.ResultType.GenericTypeArguments.[0]
                            .GetMethod(funcName).Invoke(null, [|typedCollection|])

                cmdlet.ResultType.GetMethod("Some").Invoke(null, [|choise|])

        member __.GetXmlDoc(rawName:string) =
            // Create command
            let command = Command("Get-Help")
            command.Parameters.Add(CommandParameter("Name", rawName))
            // Execute
            let pipeline = runSpace.CreatePipeline()
            pipeline.Commands.Add(command)
            let result = pipeline.Invoke() |> Seq.toArray
            // Format result
            let (?) (this : PSObject) (prop : string) : obj =
                let prop = this.Properties |> Seq.find(fun p -> p.Name = prop)
                prop.Value
            match result with
            | [|help|] ->
                let lines =
                    let description = (help?description :?> obj [])
                    if description = null then [||]
                    else
                        description
                        |> TypeInference.CollectionConverter<PSObject>.Convert
                        |> List.toArray
                        |> Array.map (fun x->x?Text :?> string)
                sprintf "<summary><para>%s</para></summary>"
                    (String.Join("</para><para>", lines |> Array.map (fun s->s.Replace("<","").Replace(">",""))))
            | _ -> String.Empty