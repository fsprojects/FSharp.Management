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
        member __.GetAllCmdlets() =
            psRuntime.AllCmdlets()
            |> Array.map (fun cmdlet ->
                let paramNames, paramTypes =
                    cmdlet.ParametersInfo |> Array.unzip
                PSCmdLetInfoDTO(
                    RawName = cmdlet.RawName,
                    ResultObjectTypes = (cmdlet.ResultObjectTypes |> Array.map serializeType),
                    ParametersNames = paramNames,
                    ParametersTypes = (paramTypes |> Array.map serializeType)))
        member __.GetXmlDoc rawName =
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


type PSRuntimeServiceClient(serviceUrl: string, _snapIns: string[]) =
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
type PSRuntimeExternal(snapIns: string[]) =
    let psProcess, serviceUrl =
        let pr = new System.Diagnostics.Process()
        pr.StartInfo.UseShellExecute <- false
        pr.StartInfo.CreateNoWindow  <- true

        let fullPath = System.Reflection.Assembly.GetAssembly(typeof<PSRuntimeExternal>).Location
        let directory = Path.GetDirectoryName( fullPath )
        let externalRuntime = Path.Combine(directory, "FSharp.Management.PowerShell.ExternalRuntime.exe") // TODO: update it

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
        member __.Dispose() =
            try
                client.Close()
            finally
                psProcess.StandardInput.WriteLine()
                psProcess.WaitForExit()

    interface IPSRuntime with
        member __.AllCmdlets() =
            clientService.GetAllCmdlets()
            |> Array.map (fun dto ->
                let resultObjectTypes = (dto.ResultObjectTypes |> Array.map unSerializeType)
                let paramTypes = (dto.ParametersTypes |> Array.map unSerializeType)
                {
                    RawName = dto.RawName
                    Name    = dto.RawName;
                    ResultObjectTypes = resultObjectTypes
                    ResultType = (resultObjectTypes |> TypeInference.buildResultType)
                    ParametersInfo = (Array.zip (dto.ParametersNames) (paramTypes))
                })
        member __.Execute(_rawName, _parameters: obj seq) = failwith "Not implemented"
        member __.GetXmlDoc rawName = clientService.GetXmlDoc(rawName)
