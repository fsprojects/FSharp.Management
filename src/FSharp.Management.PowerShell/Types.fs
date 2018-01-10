[<AutoOpen>]
module FSharp.Management.PowerShellProviderTypes

open System.Management.Automation
open System.Collections.ObjectModel

type PsCmdletResult<'TSuccess,'TFailure> = 
    | Success of 'TSuccess
    | Failure of 'TFailure

type PsExecutionResult =
    | Result of Collection<PSObject>
    | Error of ErrorRecord
