module FSharp.Management.Tests.RegistryProviderTests

open FSharp.Management
open NUnit.Framework
open FsUnitTyped

[<Test>]
let ``Can create type for HKEY_CURRENT_USER``() =
    Registry.HKEY_CURRENT_USER.Path
    |> shouldEqual "HKEY_CURRENT_USER"

[<Test>]
let ``Can create subtype for HKEY_LOCAL_MACHINE``() =
    Registry.HKEY_LOCAL_MACHINE.SOFTWARE.Path
    |> shouldEqual @"HKEY_LOCAL_MACHINE\SOFTWARE"