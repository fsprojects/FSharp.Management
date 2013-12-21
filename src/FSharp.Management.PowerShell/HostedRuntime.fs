module FSharp.Management.PowerShellProvider.HostedRuntime

open System
open System.Management.Automation
open System.Management.Automation.Runspaces

type IPSRuntime =
    abstract member AllCmdlets  : unit -> TypeInference.PSCommandLet[]
    abstract member Execute     : string * obj seq -> obj
    abstract member GetXmlDoc   : string -> string

/// PowerShell runtime built-in current process
type PSRuntimeHosted(snapIns:string[]) =
    let runSpace = 
        let config = RunspaceConfiguration.Create(); 
        for snapIn in snapIns do
            if (not <| String.IsNullOrEmpty(snapIn)) then 
                let info, ex = config.AddPSSnapIn(snapIn)
                if ex <> null then
                    failwithf "AddPSSnapInException: %s" ex.Message
        let rs = RunspaceFactory.CreateRunspace(config)
        rs.Open()
        rs
    let cmdlets =
        runSpace.RunspaceConfiguration.Cmdlets
        |> Seq.map (fun cmdlet ->
                        let wrapper = TypeInference.createPSCommandLet cmdlet
                        wrapper.RawName, wrapper)
        |> Map.ofSeq
    interface IPSRuntime with
        member this.AllCmdlets() =
            cmdlets |> Map.toSeq |> Seq.map snd |> Seq.toArray
        member this.Execute(rawName,parameters:obj seq) =
            // Create command
            let cmdlet = cmdlets.[rawName]
            let command = Command(cmdlet.RawName)
            parameters |> Seq.iteri (fun i value->
                let key,ty = cmdlet.ParametersInfo.[i]
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
            let tyOfObj = TypeInference.getTypeOfObjects tys result

            let collectionConverter = 
                typedefof<TypeInference.CollectionConverter<_>>.MakeGenericType(tyOfObj)
            let objectCollection = 
                if (tyOfObj = typeof<PSObject>) then box result 
                else result |> Seq.map (fun x->x.BaseObject) |> box
            let typedCollection = 
                collectionConverter.GetMethod("Convert").Invoke(null, [|objectCollection|])

            if (tys.Length = 1) 
            then typedCollection
            else let ind = tys |> Array.findIndex (fun x-> x = tyOfObj)
                 let funcName = sprintf "NewChoice%dOf%d" (ind+1) (tys.Length)
                 cmdlet.ResultType.GetMethod(funcName).Invoke(null, [|typedCollection|])
        member this.GetXmlDoc(rawName:string) =
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