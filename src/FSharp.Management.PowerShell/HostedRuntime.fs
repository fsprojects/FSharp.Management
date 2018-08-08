module FSharp.Management.PowerShellProvider.HostedRuntime

open TypeInference

open System
open System.Management.Automation
open System.Management.Automation.Runspaces
open System.Threading
open System.Security.Principal      

type IPSRuntime =
    abstract member AllCommands  : unit -> PSCommandSignature[]
    abstract member Execute     : string * obj seq -> obj
    abstract member GetXmlDoc   : string -> string
    abstract member Runspace   : System.Management.Automation.Runspaces.Runspace

/// PowerShell runtime built into the current process
type PSRuntimeHosted(snapIns:string[], modules:string[]) =
    let runSpace =
        try
            let initState = InitialSessionState.CreateDefault()
            initState.AuthorizationManager <- new Microsoft.PowerShell.PSAuthorizationManager("Microsoft.PowerShell")
            
            // Import SnapIns
            for snapIn in snapIns do
                if not <| String.IsNullOrEmpty(snapIn) then
                    let _, ex = initState.ImportPSSnapIn(snapIn)
                    if ex <> null then
                        failwithf "ImportPSSnapInExceptions: %s" ex.Message

            // Import modules
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
        //Get-Command -CommandType @("cmdlet","function") -ListImported
        PowerShell.Create(Runspace=runSpace)
          .AddCommand("Get-Command")
          .AddParameter("CommandType", ["cmdlet"; "function"])  // Get only cmdlets and functions (without aliases)
          .AddParameter("ListImported")                         // Get only commands imported into current runtime
          .Invoke()
        |> Seq.map (fun x ->
            match x.BaseObject with
            | :? CommandInfo as ci -> ci
            | w -> failwithf "Unsupported type of command: %A" w)
        |> Seq.toArray
    let commands =
        try
            commandInfos
            |> Seq.collect (fun cmd ->
                match cmd with
                | :? CmdletInfo 
                | :? FunctionInfo ->
                    if cmd.ParameterSets.Count > 0 then
                        seq {
                            // Generate function for each command's parameter set
                            for pSet in cmd.ParameterSets do
                                let cmdSignature = getPSCommandSignature cmd pSet
                                yield cmdSignature.UniqueID, cmdSignature
                        }
                    else
                        failwithf "Command is not loaded: %A" cmd
                | _ -> failwithf "Unexpected type of command: %A" cmd
            )
            |> Map.ofSeq
        with
        | e -> failwithf "Could not load command: %s\n%s" e.Message e.StackTrace
    let allCommands = commands |> Map.toSeq |> Seq.map snd |> Seq.toArray

    let getXmlDoc(cmdName:string) =
        let result =
            PowerShell.Create(Runspace=runSpace)
                .AddCommand("Get-Help")
                .AddParameter("Name", cmdName)
                .Invoke()
            |> Seq.toArray

        let (?) (this : PSObject) (prop : string) : obj =
            let prop = this.Properties |> Seq.find (fun p -> p.Name = prop)
            prop.Value
        match result with
        | [|help|] ->
            let lines =
                let description = (help?description :?> obj [])
                if description = null then [||]
                else
                    description
                    |> CollectionConverter<PSObject>.Convert
                    |> List.toArray
                    |> Array.map (fun x->x?Text :?> string)
            sprintf "<summary><para>%s</para></summary>"
                (String.Join("</para><para>", lines |> Array.map (fun s->s.Replace("<","").Replace(">",""))))
        | _ -> String.Empty
    let xmlDocs = System.Collections.Generic.Dictionary<_,_>()

    interface IPSRuntime with
        member __.Runspace = runSpace
        member __.AllCommands() = allCommands
        member __.Execute(uniqueId, parameters:obj seq) =
            let cmd = commands.[uniqueId]

            // Create and execute PowerShell command
            let ps = PowerShell.Create(Runspace=runSpace).AddCommand(cmd.Name)
            parameters |> Seq.iteri (fun i value->
                let key, _,ty = cmd.ParametersInfo.[i]
                match ty with
                | _ when ty = typeof<System.Management.Automation.SwitchParameter>->
                    if (unbox<bool> value) then
                        ps.AddParameter(key) |> ignore
                | _ when ty.IsValueType ->
                    if (value <> System.Activator.CreateInstance(ty))
                    then ps.AddParameter(key, value) |> ignore
                | _ ->
                    if (value <> null)
                    then ps.AddParameter(key, value) |> ignore
            )
            let result = ps.Invoke()           

            // Infer type of the result
            match getTypeOfObjects cmd.ResultObjectTypes result with
            | None -> 
                if ps.Streams.Error.Count > 0 then                       
                    let errors = ps.Streams.Error |> Seq.cast<ErrorRecord> |> List.ofSeq   
                    cmd.ResultType.GetMethod("NewFailure").Invoke(null, [|errors|])
                else                    
                    let boxedResult = if result.Count > 0 then
                                            box (new PSObject(result))
                                        else
                                            box None

                    cmd.ResultType.GetMethod("NewSuccess").Invoke(null, [|boxedResult|])    // Result of execution is empty object

            | Some(tyOfObj) ->
                let collectionConverter =
                    typedefof<CollectionConverter<_>>.MakeGenericType(tyOfObj)
                let collectionObj =
                    if (tyOfObj = typeof<PSObject>) then box result
                    else result |> Seq.map (fun x->x.BaseObject) |> box
                let typedCollection =
                    collectionConverter.GetMethod("Convert").Invoke(null, [|collectionObj|])

                let choise =
                    if (cmd.ResultObjectTypes.Length = 1)
                    then typedCollection
                    else let ind = cmd.ResultObjectTypes |> Array.findIndex (fun x-> x = tyOfObj)
                         let funcName = sprintf "NewChoice%dOf%d" (ind+1) (cmd.ResultObjectTypes.Length)
                         cmd.ResultType.GetGenericArguments().[0] // GenericTypeArguments in .NET 4.5
                            .GetMethod(funcName).Invoke(null, [|typedCollection|])

                cmd.ResultType.GetMethod("NewSuccess").Invoke(null, [|choise|])                
        member __.GetXmlDoc (cmdName:string) =
            if not <| xmlDocs.ContainsKey cmdName
                then xmlDocs.Add(cmdName, getXmlDoc cmdName)
            xmlDocs.[cmdName]

    interface IDisposable with
        member __.Dispose () = runSpace.Dispose()