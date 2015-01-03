open FSharp.Management.PowerShellProvider.ExternalRuntime

open System
open System.ServiceModel
open System.ServiceModel.Description

[<EntryPoint>]
let main argv = 
    if (Environment.Version.Major < 4)
        then failwith ".NET runtime should be version 4+"
    if (not <| Environment.Is64BitProcess)
        then failwith "Process should be 64bit"

    let snapIns = argv
    let psRuntimeService = PSRuntimeService(snapIns)
    let serviceHost = 
        new ServiceHost(psRuntimeService, [|Uri(ExternalPowerShellHost)|])
    serviceHost.AddServiceEndpoint(
        typeof<IPSRuntimeService>, 
        getNetNamedPipeBinding(), 
        ExternalPowerShellServiceName) 
      |> ignore

    serviceHost.Open()

    for endpoint in serviceHost.Description.Endpoints do
        printfn "%s" (endpoint.Address.Uri.AbsoluteUri)

    Console.ReadLine() |> ignore
    0 // return an integer exit code