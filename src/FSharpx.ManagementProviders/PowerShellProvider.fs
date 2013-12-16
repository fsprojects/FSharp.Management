namespace FSharpx.TypeProviders.PowerShellProvider

module Inference =
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

module HostedRuntime = 
    open System
    open System.Management.Automation
    open System.Management.Automation.Runspaces

    type IPSRuntime =
        abstract member AllCmdlets  : unit -> Inference.PSCommandLet[]
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
                           let wrapper = Inference.createPSCommandLet cmdlet
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
                let tyOfObj = Inference.getTypeOfObjects tys result

                let collectionConverter = 
                    typedefof<Inference.CollectionConverter<_>>.MakeGenericType(tyOfObj)
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
                            |> Inference.CollectionConverter<PSObject>.Convert
                            |> List.toArray
                            |> Array.map (fun x->x?Text :?> string)
                    sprintf "<summary><para>%s</para></summary>"
                        (String.Join("</para><para>", lines |> Array.map (fun s->s.Replace("<","").Replace(">",""))))
                | _ -> String.Empty

module ExternalRuntime =
    open System
    open System.IO
    open System.ServiceModel
    open System.ServiceModel.Description
    open System.Runtime.Serialization
    open HostedRuntime

    [<DataContract>]
    type PSCmdLetInfoDTO() = 
        [<field : DataMember>]
        member val RawName = "" with get, set

        [<field : DataMember>]
        member val ResultObjectTypes   = Array.empty<string> with get, set
        [<field : DataMember>]
        member val ParametersNames     = Array.empty<string> with get, set
        [<field : DataMember>]
        member val ParametersTypes     = Array.empty<string> with get, set

    [<ServiceContract>]
    type IPSRuntimeService =
        [<OperationContract>]
        abstract member GetAllCmdlets  : unit -> PSCmdLetInfoDTO[]
        [<OperationContract>]
        abstract member GetXmlDoc      : commandName : string -> string


    [<ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)>]
    type PSRuntimeService(snapIns:string[]) =
        let psRuntime = PSRuntimeHosted(snapIns) :> IPSRuntime
        let serializeType = (fun (t:Type) -> t.AssemblyQualifiedName)
        interface IPSRuntimeService with
            member this.GetAllCmdlets() =
                psRuntime.AllCmdlets()
                |> Array.map (fun cmdlet ->
                    let paramNames, paramTypes =
                        cmdlet.ParametersInfo |> Array.unzip
                    PSCmdLetInfoDTO(
                        RawName = cmdlet.RawName,
                        ResultObjectTypes = (cmdlet.ResultObjectTypes |> Array.map serializeType),
                        ParametersNames = paramNames,
                        ParametersTypes = (paramTypes |> Array.map serializeType)
                    )
                 )
            member this.GetXmlDoc rawName =
                psRuntime.GetXmlDoc(rawName)

    [<Literal>] 
    let ExternalPowerShellHost          = "net.pipe://localhost/"
    [<Literal>] 
    let ExternalPowerShellServiceName   = "PowerShellRuntimeForTypeProvider"

    let getNetNamedPipeBinding() = 
        let binding = NetNamedPipeBinding()
        let maxSize = 50000000
        binding.MaxBufferPoolSize <- (int64)maxSize
        binding.MaxBufferSize <- maxSize
        binding.MaxReceivedMessageSize <- (int64)maxSize
        binding.ReaderQuotas.MaxArrayLength <- maxSize
        binding.ReaderQuotas.MaxBytesPerRead <- maxSize
        binding.ReaderQuotas.MaxStringContentLength <- maxSize
        binding


    type PSRuntimeServiceClient(serviceUrl:string, snapIns:string[]) =
        inherit ClientBase<IPSRuntimeService>(
            new ServiceEndpoint(
                ContractDescription.GetContract(typeof<IPSRuntimeService>), 
                getNetNamedPipeBinding(), 
                EndpointAddress(serviceUrl)))
        interface IPSRuntimeService with
            member this.GetAllCmdlets() =
                this.Channel.GetAllCmdlets()
            member this.GetXmlDoc rawName =
                this.Channel.GetXmlDoc(rawName)


    /// PowerShell runtime executed in external 64bit process
    type PSRuntimeExternal(snapIns:string[]) =
        let psProcess, serviceUrl =
            let pr = new System.Diagnostics.Process()
            pr.StartInfo.UseShellExecute <- false
            pr.StartInfo.CreateNoWindow  <- true

            let fullPath = System.Reflection.Assembly.GetAssembly(typeof<PSRuntimeExternal>).Location
            let directory = Path.GetDirectoryName( fullPath )
            let externalRuntime = Path.Combine(directory, "FSharpx.ManagementProviders.PowerShell.ExternalRuntime.exe") // TODO: update it

            pr.StartInfo.FileName  <- externalRuntime
            pr.StartInfo.Arguments <- String.Join(" ", snapIns)

            pr.StartInfo.RedirectStandardInput  <- true
            pr.StartInfo.RedirectStandardOutput <- true
            pr.Start() |> ignore
            let url = pr.StandardOutput.ReadLine()
            pr, url
        let client = new PSRuntimeServiceClient(serviceUrl, snapIns)
        let clientService = client :> IPSRuntimeService
        let unSerializeType = 
            (fun typeName -> 
                let ty = Type.GetType(typeName)
                if (ty = null) then
                    failwithf "Type does not found '%s'" typeName
                ty)

        interface IDisposable with
            member this.Dispose() =
                try
                    client.Close()
                finally
                    psProcess.StandardInput.WriteLine()
                    psProcess.WaitForExit()

        interface IPSRuntime with
            member this.AllCmdlets() =
                clientService.GetAllCmdlets()
                |> Array.map (fun dto ->
                    let resultObjectTypes = (dto.ResultObjectTypes |> Array.map unSerializeType)
                    let paramTypes = (dto.ParametersTypes |> Array.map unSerializeType)
                    {
                        RawName = dto.RawName
                        Name    = dto.RawName;
                        ResultObjectTypes = resultObjectTypes
                        ResultType = (resultObjectTypes |> Inference.buildResultType)
                        ParametersInfo = (Array.zip (dto.ParametersNames) (paramTypes))
                    }
                )
            member this.Execute(rawName,parameters:obj seq) =
                failwith "Not implemented"
            member this.GetXmlDoc rawName =
                clientService.GetXmlDoc(rawName)

module Runtime = 
    open System
    open HostedRuntime
    open ExternalRuntime

    // PowerShell runtime resolver
    let private runtimes = ref Map.empty<string[] * bool, IPSRuntime>
    let Current(snapIns, is64bitRequired, isDesignTime) =
        let key = (snapIns, isDesignTime)
        if (not <| Map.containsKey key !runtimes) then
            let value = 
                if (is64bitRequired && not(Environment.Is64BitProcess)) then
                    if (isDesignTime) 
                    then new PSRuntimeExternal(snapIns) :> IPSRuntime
                    else failwith "You should compile your code as x64 application"
                else 
                    PSRuntimeHosted(snapIns) :> IPSRuntime
            runtimes := (!runtimes |> Map.add key value)
        (!runtimes).[key]

[<AutoOpen>]
module Provider = 
    open System
    open System.Collections.Generic
    open System.Reflection
    open System.IO
    open System.Diagnostics
    open System.Threading
    open Samples.FSharp.ProvidedTypes
    open Microsoft.FSharp.Core.CompilerServices
    open System.Management.Automation
    open System.Management.Automation.Runspaces
    open FSharpx.TypeProviders.Helper

    [<TypeProvider>]
    type public PowerShellProvider(cfg:TypeProviderConfig) as this =
        inherit TypeProviderForNamespaces()

        // Get the assembly and namespace used to house the provided types
        let baseTy = typeof<obj>
        let staticParams = [ProvidedStaticParameter("PSSnapIns", typeof<string>); //, parameterDefaultValue = "")] // String list will be much better here
                            ProvidedStaticParameter("Is64BitRequired", typeof<bool>);]

        // Expose all available cmdlets as methods
        let shell = ProvidedTypeDefinition(thisAssembly, rootNamespace, "PowerShellProvider", Some(baseTy))
        let helpText =
            """<summary>Typed representation of a PowerShell runspace</summary>
               <param name='PSSnapIns'>List of PSSnapIn that will be added at the start separated by semicolon.</param>
               <param name='Is64BitRequired'>Mark that 64bit runtime should be used for PowerShell</param>"""
        do shell.AddXmlDoc helpText
        do shell.DefineStaticParameters(
            parameters=staticParams,
            instantiationFunction=(fun typeName parameterValues ->
                let psSnapIns = parameterValues.[0] :?> string
                let is64bitRequired = parameterValues.[1] :?> bool

                let pty = ProvidedTypeDefinition(thisAssembly, rootNamespace, typeName, Some(baseTy))
                pty.AddMembersDelayed(fun() ->
                   [let runtime = Runtime.Current(psSnapIns.Split(';'), is64bitRequired, true)
                    for cmdlet in runtime.AllCmdlets() do
                        let paramList = 
                           [for (name, ty) in cmdlet.ParametersInfo ->
                                let newTy, defValue = 
                                    match ty with
                                    | _ when ty = typeof<System.Management.Automation.SwitchParameter>->
                                        typeof<bool>, box false
                                    | _ when ty.IsValueType ->
                                        ty, box None //System.Activator.CreateInstance(ty)
                                    | _ -> ty, null
                                ProvidedParameter(Inference.toCamelCase name, newTy, optionalValue=defValue)]
                        let paramCount = paramList.Length
                        let pm = 
                            ProvidedMethod(
                                methodName = cmdlet.Name,
                                parameters = paramList,
                                returnType = cmdlet.ResultType,
                                IsStaticMethod = true,
                                InvokeCode = 
                                    fun args -> 
                                        if args.Length <> paramCount then
                                            failwithf "Expected %d arguments and received %d" paramCount args.Length
                                                 
                                        let namedArgs = [0..(paramCount-1)] |> List.map (fun i -> 
                                                            Quotations.Expr.Coerce(args.[i], typeof<obj>))
                                        let namedArgs = Quotations.Expr.NewArray(typeof<obj>, namedArgs)
                                        let rawName = cmdlet.RawName;
                                    
                                        <@@ Runtime.Current(psSnapIns.Split(';'), is64bitRequired, false)
                                                   .Execute(rawName, (%%namedArgs : obj[])) @@>)
                        pm.AddXmlDocDelayed(fun() ->runtime.GetXmlDoc(cmdlet.RawName))
                        yield pm :> MemberInfo
                   ])
                pty))
        do this.AddNamespace(rootNamespace, [ shell ])