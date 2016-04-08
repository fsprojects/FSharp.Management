[<AutoOpen>]
module FSharp.Management.PowerShellProviderTypes

type PsCmdletResult<'TSuccess,'TFailure> = 
    | Success of 'TSuccess
    | Failure of 'TFailure