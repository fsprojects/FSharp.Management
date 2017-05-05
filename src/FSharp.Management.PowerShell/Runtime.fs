module FSharp.Management.PowerShellProvider.Runtime

open System
open HostedRuntime
open ExternalRuntime

let private runtimes = ref Map.empty<string[] * string[] * bool, IPSRuntime>

/// PowerShell runtime resolver
let Current(snapIns, modules, is64bitRequired, isDesignTime) =
    let key = (snapIns, modules, isDesignTime)
    if (not <| Map.containsKey key !runtimes) then
        let value =
            if (is64bitRequired && not(Environment.Is64BitProcess)) then
                if (isDesignTime)
                then new PSRuntimeExternal(snapIns, modules) :> IPSRuntime
                else failwith "You should compile your code as x64 application"
            else
                new PSRuntimeHosted(snapIns, modules) :> IPSRuntime
        runtimes := (!runtimes |> Map.add key value)
    (!runtimes).[key]

/// Release external PowerShell runtimes
let disposeAll() =
    (!runtimes)
    |> Seq.iter (fun kvPair ->
        match kvPair.Value with
        | :? IDisposable as x -> x.Dispose()
        | _ -> ignore()
        )