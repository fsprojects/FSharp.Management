module FSharp.Management.PowerShellProvider.ExternalRuntime

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
    member val IsMandatory         = Array.empty<bool> with get, set
    [<field : DataMember>]
    member val ParametersTypes     = Array.empty<string> with get, set

[<ServiceContract>]
type IPSRuntimeService =
    [<OperationContract>]
    abstract member GetAllCmdlets  : unit -> PSCmdLetInfoDTO[]
    [<OperationContract>]
    abstract member GetXmlDoc      : commandName : string -> string


[<ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)>]
type PSRuntimeService(snapIns:string[], modules:string[]) =
    let psRuntimeHosted = new PSRuntimeHosted(snapIns, modules)
    let psRuntime = psRuntimeHosted :> IPSRuntime
    let serializeType = (fun (t:Type) -> t.AssemblyQualifiedName)
    interface IPSRuntimeService with
        member __.GetAllCmdlets() =
            psRuntime.AllCommands()
            |> Array.map (fun cmdlet ->
                let paramNames, isMandaroty, paramTypes =
                    cmdlet.ParametersInfo |> Array.unzip3
                PSCmdLetInfoDTO(
                    RawName = cmdlet.Name,
                    ResultObjectTypes = (cmdlet.ResultObjectTypes |> Array.map serializeType),
                    ParametersNames = paramNames,
                    IsMandatory = isMandaroty,
                    ParametersTypes = (paramTypes |> Array.map serializeType)))
        member __.GetXmlDoc rawName =
            psRuntime.GetXmlDoc(rawName)
    interface IDisposable with
        member __.Dispose() =
            (psRuntimeHosted :> IDisposable).Dispose()

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


type PSRuntimeServiceClient(serviceUrl: string) =
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
type PSRuntimeExternal(snapIns: string[], modules: string[]) =
    let psProcess, serviceUrl =
        let pr = new System.Diagnostics.Process()
        pr.StartInfo.UseShellExecute <- false
        pr.StartInfo.CreateNoWindow  <- true

        let fullPath = System.Reflection.Assembly.GetAssembly(typeof<PSRuntimeExternal>).Location
        let directory = Path.GetDirectoryName( fullPath )
        let externalRuntime = Path.Combine(directory, "FSharp.Management.PowerShell.ExternalRuntime.exe")

        pr.StartInfo.FileName  <- externalRuntime

        let parameters = Array.append snapIns (Array.map (sprintf "+%s") modules)
        pr.StartInfo.Arguments <- String.Join(" ", parameters)

        pr.StartInfo.RedirectStandardInput  <- true
        pr.StartInfo.RedirectStandardOutput <- true
        pr.Start() |> ignore
        let url = pr.StandardOutput.ReadLine()
        pr, url
    let client = new PSRuntimeServiceClient(serviceUrl)
    let clientService = client :> IPSRuntimeService
    let unSerializeType =
        (fun typeName ->
            let ty = Type.GetType(typeName)
            if (ty = null) then
                failwithf "Type does not found '%s'" typeName
            ty)

    interface IDisposable with
        member __.Dispose() =
            try
                client.Close()
            finally
                psProcess.StandardInput.WriteLine()
                psProcess.WaitForExit()

    interface IPSRuntime with
        member __.Runspace = failwith "Not implemented"
        member __.AllCommands() =
            clientService.GetAllCmdlets()
            |> Array.map (fun dto ->
                let resultObjectTypes = (dto.ResultObjectTypes |> Array.map unSerializeType)
                let paramTypes = (dto.ParametersTypes |> Array.map unSerializeType)
                {
                    Name = dto.RawName
                    UniqueID    = dto.RawName;
                    ResultObjectTypes = resultObjectTypes
                    ResultType = (resultObjectTypes |> TypeInference.buildResultType)
                    ParametersInfo = (Array.zip3 (dto.ParametersNames) (dto.IsMandatory) (paramTypes))
                })
        member __.Execute(_, _) = failwith "Not implemented"
        member __.GetXmlDoc rawName = clientService.GetXmlDoc(rawName)
