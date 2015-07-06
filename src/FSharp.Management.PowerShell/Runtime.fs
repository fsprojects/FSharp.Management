module FSharp.Management.PowerShellProvider.Runtime

open System
open HostedRuntime
open ExternalRuntime

// PowerShell runtime resolver
let private runtimes = ref Map.empty<string[] * string[] * bool, IPSRuntime>
let Current(snapIns, modules, is64bitRequired, isDesignTime) =
    let key = (snapIns, modules, isDesignTime)
    if (not <| Map.containsKey key !runtimes) then
        let value =
            if (is64bitRequired && not(Environment.Is64BitProcess)) then
                if (isDesignTime)
                then new PSRuntimeExternal(snapIns, modules) :> IPSRuntime
                else failwith "You should compile your code as x64 application"
            else
                PSRuntimeHosted(snapIns, modules) :> IPSRuntime
        runtimes := (!runtimes |> Map.add key value)
    (!runtimes).[key]
