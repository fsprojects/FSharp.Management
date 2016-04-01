module FSharp.Management.PowerShellProvider.Types

type PsCmdletResult<'TSuccess,'TFailure> = 
    | Success of 'TSuccess
    | Failure of 'TFailure